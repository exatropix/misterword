using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    enum Jeu { Aucun, Mots, Couleurs, Taquin, MotsCroises, Sudoku }

    partial class Fenetre : Form
    {
        readonly Random rnd = new Random();
        readonly string dossierJeu, dossierDonnees;
        Dictionnaire dico;
        string langueMots = "fr";               // langue des mots de Misterword
        static readonly string[] Langues = { "fr", "en", "es" };
        bool majAuto = true;                    // rechercher une mise à jour au démarrage, une fois par jour
        string majDate;
        Statistiques stats;
        bool clair;                       // annoncer les résultats en clair, en plus des bips
        bool indices;                     // Mistermind : résultat couleur par couleur, comme Misterword

        readonly MenuStrip menu = new MenuStrip();
        ToolStripMenuItem menuFichier, itemClair, itemIndices;

        // Page d'accueil, comme une page vierge de traitement de texte
        readonly TextBox page = new TextBox();

        // Zone de jeu : saisie + liste des propositions
        readonly TableLayoutPanel zoneJeu = new TableLayoutPanel();
        readonly Label etiquetteSaisie = new Label { AutoSize = true };
        readonly TextBox saisieMot = new TextBox();
        readonly ZoneCode saisieCode = new ZoneCode();
        readonly ListBox liste = new ListBox();
        readonly Panel dessin = new Panel();
        readonly Label etat = new Label { AutoSize = true, Dock = DockStyle.Fill };
        readonly Label lListe = new Label { AutoSize = true };

        Jeu jeu = Jeu.Aucun;
        PartieMot partieMot;
        PartieCouleurs partieCouleurs;
        int longueurChoisie;              // 0 = au hasard
        bool avanceChoisi;
        bool finie, comptee;
        int pos = -1;                     // lettre ou couleur lue dans la liste

        // Taquin
        readonly ZoneTaquin zoneTaquin = new ZoneTaquin();
        readonly TableLayoutPanel panneauTaquin = new TableLayoutPanel();
        readonly Label etatTaquin = new Label { AutoSize = true, Dock = DockStyle.Fill };
        PartieTaquin partieTaquin;
        readonly Dictionary<int, RecordsTaquin> records = new Dictionary<int, RecordsTaquin>();
        int tailleTaquin = 4;
        RecordsTaquin Record { get { return records[tailleTaquin]; } }
        static string NiveauTaquin(int t) { return t == 3 ? L.T("facile") : t == 4 ? L.T("normal") : L.T("expert"); }
        DateTime debutTaquin;
        bool chronoLance;

        readonly Timer minuterie = new Timer();
        Action differee;

        public Fenetre()
        {
            dossierJeu = AppDomain.CurrentDomain.BaseDirectory;
            dossierDonnees = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Misterword");
            stats = Statistiques.Charger(Path.Combine(dossierDonnees, "stats.txt"));
            clair = LireOption("clair", false);
            indices = LireOption("indices", true);
            L.Initialiser(LireTexte("langue"));
            langueMots = LireTexte("mots") ?? L.Code;
            majAuto = LireOption("maj", true);
            majDate = LireTexte("majdate");

            Text = "Misterword";
            ClientSize = new Size(820, 620);
            MinimumSize = new Size(600, 450);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 11f);

            minuterie.Tick += (s, e) => { minuterie.Stop(); var a = differee; differee = null; if (a != null) a(); };

            ConstruireMenu();
            ConstruirePage();
            ConstruireZoneJeu();
            ConstruireTaquin();
            ConstruireGrilles();
            Controls.Add(zoneJeu);
            Controls.Add(panneauTaquin);
            Controls.Add(panneauMC);
            Controls.Add(panneauSudoku);
            Controls.Add(page);
            Controls.Add(menu);
            MainMenuStrip = menu;
            zoneJeu.Visible = false;
            panneauTaquin.Visible = false;
            panneauMC.Visible = false;
            panneauSudoku.Visible = false;
            records[3] = RecordsTaquin.Charger(Path.Combine(dossierDonnees, "taquin3.txt"));
            records[4] = RecordsTaquin.Charger(Path.Combine(dossierDonnees, "taquin.txt"));
            records[5] = RecordsTaquin.Charger(Path.Combine(dossierDonnees, "taquin5.txt"));
        }

        // ================= Construction =================

        void ConstruireMenu()
        {
            menuFichier = new ToolStripMenuItem(L.T("&Fichier"));
            var nouvelle = new ToolStripMenuItem(L.T("&Nouvelle partie"));
            var mots = new ToolStripMenuItem(L.T("&Misterword"));
            for (int l = Dictionnaire.LongueurMin; l <= Dictionnaire.LongueurMax; l++)
            {
                int ll = l;
                mots.DropDownItems.Add(Item((l == 10 ? "1&0" : "&" + l) + L.T(" lettres"), Keys.None, () => NouveauMot(ll)));
            }
            mots.DropDownItems.Add(Item(L.T("Au &hasard"), Keys.None, () => NouveauMot(0)));
            nouvelle.DropDownItems.Add(mots);
            var couleurs = new ToolStripMenuItem(L.T("Mister&mind"));
            couleurs.DropDownItems.Add(Item(L.T("&Facile : 4 couleurs sur 6"), Keys.None, () => NouveauCode(false)));
            couleurs.DropDownItems.Add(Item(L.T("&Expert : 5 couleurs sur 8"), Keys.None, () => NouveauCode(true)));
            nouvelle.DropDownItems.Add(couleurs);
            var taquin = new ToolStripMenuItem(L.T("&Taquin"));
            taquin.DropDownItems.Add(Item(L.T("&Facile : 3 sur 3, lettres A à H"), Keys.None, () => NouveauTaquin(3)));
            taquin.DropDownItems.Add(Item(L.T("&Normal : 4 sur 4, lettres A à O"), Keys.None, () => NouveauTaquin(4)));
            taquin.DropDownItems.Add(Item(L.T("&Expert : 5 sur 5, lettres A à X"), Keys.None, () => NouveauTaquin(5)));
            nouvelle.DropDownItems.Add(taquin);
            var mc = new ToolStripMenuItem(L.T("Mots &croisés"));
            mc.DropDownItems.Add(Item(L.T("&Facile : grille de 5 sur 5"), Keys.None, () => NouveauxMotsCroises(5)));
            mc.DropDownItems.Add(Item(L.T("&Moyen : grille de 7 sur 7"), Keys.None, () => NouveauxMotsCroises(7)));
            mc.DropDownItems.Add(Item(L.T("&Difficile : grille de 9 sur 9"), Keys.None, () => NouveauxMotsCroises(9)));
            mc.DropDownItems.Add(Item(L.T("&Expert : grille de 11 sur 11"), Keys.None, () => NouveauxMotsCroises(11)));
            nouvelle.DropDownItems.Add(mc);
            var su = new ToolStripMenuItem(L.T("&Sudoku"));
            su.DropDownItems.Add(Item(L.T("&Facile"), Keys.None, () => NouveauSudoku(1)));
            su.DropDownItems.Add(Item(L.T("&Moyen"), Keys.None, () => NouveauSudoku(2)));
            su.DropDownItems.Add(Item(L.T("&Difficile"), Keys.None, () => NouveauSudoku(3)));
            su.DropDownItems.Add(Item(L.T("&Expert"), Keys.None, () => NouveauSudoku(4)));
            nouvelle.DropDownItems.Add(su);
            menuFichier.DropDownItems.Add(nouvelle);
            menuFichier.DropDownItems.Add(Item(L.T("&Rejouer au même jeu"), Keys.Control | Keys.N, Rejouer));
            menuFichier.DropDownItems.Add(Item(L.T("Sau&vegarder la partie"), Keys.Control | Keys.S, () => Sauvegarder(true)));
            menuFichier.DropDownItems.Add(Item(L.T("Re&prendre la partie sauvegardée"), Keys.Control | Keys.O, () => Reprendre(true)));
            menuFichier.DropDownItems.Add(Item(L.T("&Abandonner et voir la solution"), Keys.Control | Keys.R, Abandonner));
            menuFichier.DropDownItems.Add(Item(L.T("Ré&capitulatif"), Keys.F3, () => Parole.Dire(Recapitulatif())));
            menuFichier.DropDownItems.Add(Item(L.T("&Statistiques"), Keys.F2, AnnoncerStatistiques));
            menuFichier.DropDownItems.Add(new ToolStripSeparator());
            itemClair = Item(L.T("Annoncer les résultats en c&lair"), Keys.F4, BasculerClair);
            itemClair.Checked = clair;
            menuFichier.DropDownItems.Add(itemClair);
            itemIndices = Item(L.T("Mistermind : &indice couleur par couleur"), Keys.F5, BasculerIndices);
            itemIndices.Checked = indices;
            menuFichier.DropDownItems.Add(itemIndices);
            menuFichier.DropDownItems.Add(new ToolStripSeparator());
            menuFichier.DropDownItems.Add(Item(L.T("&Quitter"), Keys.Control | Keys.Q, Close));

            var aide = new ToolStripMenuItem(L.T("&Aide"));
            aide.DropDownItems.Add(Item(L.T("&Aide et règles"), Keys.F1, () => Aide.Afficher(this)));
            aide.DropDownItems.Add(Item(L.T("Écouter les &bips"), Keys.None, ExpliquerBips));
            aide.DropDownItems.Add(new ToolStripSeparator());
            aide.DropDownItems.Add(Item(L.T("Rechercher une &mise à jour"), Keys.None, () => RechercherMaj(true)));
            aide.DropDownItems.Add(ItemCoche(L.T("Rechercher les mises à jour au &démarrage"), majAuto, BasculerMajAuto));
            aide.DropDownItems.Add(new ToolStripSeparator());
            aide.DropDownItems.Add(Item(L.T("À &propos"), Keys.None, APropos));

            var langue = new ToolStripMenuItem(L.T("&Langue"));
            var inter = new ToolStripMenuItem(L.T("&Interface"));
            inter.DropDownItems.Add(ItemCoche("&Français", L.Code == "fr", () => ChangerInterface("fr")));
            inter.DropDownItems.Add(ItemCoche("&English", L.Code == "en", () => ChangerInterface("en")));
            inter.DropDownItems.Add(ItemCoche("&Español", L.Code == "es", () => ChangerInterface("es")));
            var mots2 = new ToolStripMenuItem(L.T("&Langue des mots (Misterword, mots croisés)"));
            foreach (var c in Langues)
            {
                string cc = c;
                string nom = c == "fr" ? L.T("&Français") : c == "en" ? L.T("&Anglais") : L.T("&Espagnol");
                if (!DicoInstalle(c)) nom += L.T(" (non installé)");
                mots2.DropDownItems.Add(ItemCoche(nom, langueMots == c, () => ChangerMots(cc)));
            }
            var tele = new ToolStripMenuItem(L.T("&Télécharger un dictionnaire"));
            foreach (var c in Langues)
            {
                string cc = c;
                string nom = c == "fr" ? L.T("&Français") : c == "en" ? L.T("&Anglais") : L.T("&Espagnol");
                nom += DicoInstalle(c) ? L.T(" (installé, mettre à jour)") : L.T(" (non installé)");
                tele.DropDownItems.Add(Item(nom, Keys.None, () => Telecharger(cc, null)));
            }
            langue.DropDownItems.Add(inter);
            langue.DropDownItems.Add(mots2);
            langue.DropDownItems.Add(tele);

            menu.Items.Add(menuFichier);
            menu.Items.Add(langue);
            menu.Items.Add(aide);
            menu.MenuDeactivate += (s, e) => BeginInvoke(new Action(() => { if (ActiveControl == null || !ActiveControl.Visible) FocusPrincipal(); }));
        }

        static ToolStripMenuItem ItemCoche(string texte, bool coche, Action a)
        {
            var i = Item(texte, Keys.None, a);
            i.Checked = coche;
            return i;
        }

        static ToolStripMenuItem Item(string texte, Keys raccourci, Action a)
        {
            var i = new ToolStripMenuItem(texte);
            if (raccourci != Keys.None) i.ShortcutKeys = raccourci;
            i.Click += (s, e) => a();
            return i;
        }

        void ConstruirePage()
        {
            page.Multiline = true;
            page.ReadOnly = true;
            page.Dock = DockStyle.Fill;
            page.BackColor = Color.White;
            page.BorderStyle = BorderStyle.None;
            page.Font = new Font("Segoe UI", 13f);
            page.AccessibleName = "Misterword";
            page.Text = L.T("Misterword\r\n\r\nAppuyez sur Alt ou F10 pour ouvrir le menu Fichier, puis choisissez Nouvelle partie.\r\nF1 : aide.");
        }

        void ConstruireZoneJeu()
        {
            var t = zoneJeu;
            t.Dock = DockStyle.Fill; t.ColumnCount = 2; t.RowCount = 5; t.Padding = new Padding(8);
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            saisieMot.AccessibleName = L.T("Votre proposition");
            saisieMot.CharacterCasing = CharacterCasing.Upper;
            saisieMot.Dock = DockStyle.Fill;
            saisieMot.Font = new Font("Consolas", 18f, FontStyle.Bold);
            saisieMot.KeyDown += ToucheSaisieMot;

            saisieCode.Dock = DockStyle.Fill;
            saisieCode.BackColor = Color.FromArgb(40, 40, 40);
            saisieCode.NomCouleur = i => PartieCouleurs.Palette[i].Nom;
            saisieCode.Teinte = i => PartieCouleurs.Palette[i].Teinte;
            saisieCode.Indice = c => partieCouleurs == null ? -1 : partieCouleurs.Indice(c);
            saisieCode.Valider = ProposerCode;
            saisieCode.GotFocus += (s, e) => PlusTard(150, () => Parole.Dire(L.T("Votre combinaison. ") + saisieCode.Relire(), false));

            lListe.Text = L.T("Propositions :");
            liste.AccessibleName = L.T("Propositions");
            liste.Dock = DockStyle.Fill;
            liste.IntegralHeight = false;
            liste.KeyDown += ToucheListe;
            liste.SelectedIndexChanged += (s, e) => { pos = -1; if (liste.Focused) BipsLigne(); };
            liste.Enter += (s, e) =>
            {
                if (liste.Items.Count == 0) { PlusTard(100, () => Parole.Dire(L.T("Propositions, liste vide."), false)); return; }
                if (liste.SelectedIndex < 0) liste.SelectedIndex = liste.Items.Count - 1;
                pos = -1;
                BipsLigne();
            };

            dessin.Dock = DockStyle.Fill;
            dessin.Paint += (s, e) => { if (jeu == Jeu.Mots) DessinerMots(e.Graphics); else if (jeu == Jeu.Couleurs) DessinerCouleurs(e.Graphics); };
            dessin.Resize += (s, e) => dessin.Invalidate();
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dessin, true, null);

            t.Controls.Add(etiquetteSaisie, 0, 0); t.SetColumnSpan(etiquetteSaisie, 2);
            var hote = new Panel { Dock = DockStyle.Fill, Margin = new Padding(3) };
            hote.Controls.Add(saisieMot); hote.Controls.Add(saisieCode);
            t.Controls.Add(hote, 0, 1); t.SetColumnSpan(hote, 2);
            t.Controls.Add(lListe, 0, 2); t.SetColumnSpan(lListe, 2);
            t.Controls.Add(liste, 0, 3);
            t.Controls.Add(dessin, 1, 3);
            t.Controls.Add(etat, 0, 4); t.SetColumnSpan(etat, 2);
            hote.TabIndex = 0; saisieMot.TabIndex = 0; saisieCode.TabIndex = 1; liste.TabIndex = 2;
        }

        // ================= Démarrage et outils =================

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            page.Focus();
            page.SelectionStart = 0; page.SelectionLength = 0;
            if (!DicoInstalle(langueMots)) langueMots = Langues.FirstOrDefault(DicoInstalle) ?? langueMots;
            ChargerDico(false);
            if (File.Exists(CheminSauvegarde))
            {
                var r = MessageBox.Show(this, L.T("Une partie sauvegardée existe : ") + DecrireSauvegarde() + L.T(".\n\nVoulez-vous la reprendre ?"), L.T("Reprendre la partie"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes && Reprendre(false)) { PlusTardMaj(); return; }
            }
            if (!Langues.Any(DicoInstalle))
            {
                string code = Langues.Contains(L.Code) ? L.Code : "fr";
                ProposerTelechargement(code, L.T("Aucun dictionnaire n'est installé : Misterword et les mots croisés en ont besoin. Le Mistermind, le Taquin et le Sudoku restent jouables sans dictionnaire."), null);
            }
            PlusTard(600, () => Parole.Dire(L.T("Misterword. Alt ou F10 pour ouvrir le menu, puis Nouvelle partie."), false));
            var t = new Timer { Interval = 6000 };
            t.Tick += (s, e2) => { t.Stop(); t.Dispose(); MajAuDemarrage(); };
            t.Start();
        }

        // ================= Recherche de mise à jour =================

        void RechercherMaj(bool manuel)
        {
            if (manuel) Parole.Dire(L.T("Recherche d'une mise à jour…"));
            MiseAJour.Rechercher(r =>
            {
                try { BeginInvoke(new Action(() => ResultatMaj(r, manuel))); } catch { }
            });
        }

        void ResultatMaj(MiseAJour.Resultat r, bool manuel)
        {
            if (r.Reussi) { majDate = DateTime.Today.ToString("yyyy-MM-dd"); EcrireOptions(); }
            if (r.Reussi && r.NouvelleVersion)
            {
                string msg = L.F("Une nouvelle version est disponible : {0}. Vous avez la version {1}.", r.Version, MiseAJour.VersionActuelle);
                if (!manuel)
                {
                    // Au démarrage, on prévient sans interrompre la partie par une fenêtre.
                    Parole.Dire(msg + " " + L.T("Pour la télécharger : menu Aide, Rechercher une mise à jour."), false);
                    return;
                }
                var rep = MessageBox.Show(this, msg + "\n\n" + L.T("Ouvrir la page de téléchargement ?"), L.T("Mise à jour"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (rep == DialogResult.Yes) MiseAJour.OuvrirPage(r.Page);
                return;
            }
            if (!manuel) return;
            if (r.Reussi) Parole.Dire(L.F("Vous avez la dernière version, la {0}.", MiseAJour.VersionActuelle));
            else Parole.Dire(L.T("Impossible de vérifier les mises à jour. Vérifiez votre connexion à Internet."));
        }

        void BasculerMajAuto()
        {
            majAuto = !majAuto;
            EcrireOptions();
            ReconstruireMenu();
            Parole.Dire(majAuto ? L.T("Recherche des mises à jour au démarrage activée.") : L.T("Recherche des mises à jour au démarrage désactivée."));
        }

        void PlusTardMaj()
        {
            var t = new Timer { Interval = 8000 };
            t.Tick += (s, e2) => { t.Stop(); t.Dispose(); MajAuDemarrage(); };
            t.Start();
        }

        void MajAuDemarrage()
        {
            if (majAuto && majDate != DateTime.Today.ToString("yyyy-MM-dd")) RechercherMaj(false);
        }


        // ================= Langues et dictionnaires =================

        /// <summary>Dossier d'un dictionnaire : dictionnaires\fr, dictionnaires\en, dictionnaires\es.</summary>
        string DossierDico(string code) { return Path.Combine(dossierJeu, "dictionnaires", code); }

        string FichierMots(string code)
        {
            string f = Path.Combine(DossierDico(code), "mots.txt");
            if (File.Exists(f)) return f;
            f = Path.Combine(DossierDicoDonnees(code), "mots.txt");
            if (File.Exists(f)) return f;
            f = ChercherDico(code);
            if (f != null) return f;
            if (code == "fr" && File.Exists(Path.Combine(dossierJeu, "mots.txt"))) return Path.Combine(dossierJeu, "mots.txt");   // ancienne installation
            return null;
        }

        readonly Dictionary<string, string> dicosTrouves = new Dictionary<string, string>();

        /// <summary>
        /// Le dictionnaire a pu être décompressé un niveau trop bas (par exemple dans un dossier portant le nom du zip) :
        /// on cherche un dossier dictionnaires\code\mots.txt n'importe où dans le dossier du jeu.
        /// </summary>
        string ChercherDico(string code)
        {
            string r;
            if (dicosTrouves.TryGetValue(code, out r)) return r;
            r = null;
            try
            {
                foreach (var f in Directory.EnumerateFiles(dossierJeu, "mots.txt", SearchOption.AllDirectories))
                {
                    var dir = new DirectoryInfo(Path.GetDirectoryName(f));
                    if (dir.Name.Equals(code, StringComparison.OrdinalIgnoreCase) && dir.Parent != null && dir.Parent.Name.Equals("dictionnaires", StringComparison.OrdinalIgnoreCase)) { r = f; break; }
                }
            }
            catch { }
            if (r != null) dicosTrouves[code] = r;
            return r;
        }

        bool DicoInstalle(string code) { return FichierMots(code) != null; }

        static string NomLangue(string code) { return code == "fr" ? L.T("français") : code == "en" ? L.T("anglais") : L.T("espagnol"); }

        bool ChargerDico(bool annoncer)
        {
            string f = FichierMots(langueMots);
            if (f == null) { dico = null; return false; }
            Cursor = Cursors.WaitCursor;
            try
            {
                dico = Dictionnaire.Charger(f, Path.Combine(Path.GetDirectoryName(f), "mots_secrets.txt"));
                if (dico.Nombre == 0) throw new Exception(L.T("le fichier ne contient aucun mot de 5 à 10 lettres."));
            }
            catch (Exception ex)
            {
                dico = null;
                Cursor = Cursors.Default;
                MessageBox.Show(this, L.T("Le dictionnaire mots.txt est introuvable ou illisible :\n") + ex.Message, "Misterword", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Cursor = Cursors.Default;
            if (annoncer) Parole.Dire(L.F("Mots en {0}.", NomLangue(langueMots)));
            return true;
        }

        void ChangerMots(string code)
        {
            if (!DicoInstalle(code))
            {
                ProposerTelechargement(code, L.F("Le dictionnaire {0} n'est pas installé.", NomLangue(code)), () => { if (DicoInstalle(code)) ChangerMots(code); });
                return;
            }
            langueMots = code;
            EcrireOptions();
            ChargerDico(true);
            ReconstruireMenu();
        }

        void ChangerInterface(string code)
        {
            L.Code = code;
            EcrireOptions();
            ReconstruireMenu();
            page.Text = L.T("Misterword\r\n\r\nAppuyez sur Alt ou F10 pour ouvrir le menu Fichier, puis choisissez Nouvelle partie.\r\nF1 : aide.");
            saisieMot.AccessibleName = L.T("Votre proposition");
            liste.AccessibleName = L.T("Propositions");
            lListe.Text = L.T("Propositions :");
            saisieCode.AccessibleName = L.T("Votre combinaison");
            zoneTaquin.AccessibleName = L.T("Taquin");
            if (jeu == Jeu.Mots && partieMot != null)
            {
                etiquetteSaisie.Text = L.T("Votre proposition (") + partieMot.Longueur + L.T(" lettres, commence par ") + partieMot.PremiereLettre + ") :";
                MajEtatMot();
            }
            else if (jeu == Jeu.Couleurs && partieCouleurs != null)
            {
                etiquetteSaisie.Text = L.T("Votre combinaison (") + ListeCouleursDispo() + ") :";
                MajEtatCode();
            }
            else if (jeu == Jeu.Taquin && partieTaquin != null) MajEtatTaquin();
            if (jeu == Jeu.Mots || jeu == Jeu.Couleurs) { RemplirListe(); dessin.Invalidate(); }
            Parole.Dire(code == "fr" ? "Interface en français." : code == "en" ? "Interface in English." : "Interfaz en español.");
        }

        void ReconstruireMenu()
        {
            menu.Items.Clear();
            ConstruireMenu();
        }

        void PlusTard(int ms, Action a)
        {
            minuterie.Stop();
            differee = a;
            minuterie.Interval = Math.Max(1, ms);
            minuterie.Start();
        }

        void FocusPrincipal()
        {
            if (jeu == Jeu.Aucun) page.Focus();
            else if (jeu == Jeu.Mots) saisieMot.Focus();
            else if (jeu == Jeu.Taquin) zoneTaquin.Focus();
            else if (jeu == Jeu.MotsCroises) { if (!listeDefs.Focused) zoneMC.Focus(); }
            else if (jeu == Jeu.Sudoku) zoneSudoku.Focus();
            else saisieCode.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F10)
            {
                menuFichier.ShowDropDown();
                if (menuFichier.DropDownItems.Count > 0) menuFichier.DropDownItems[0].Select();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (PartieEnCours)
            {
                var r = MessageBox.Show(this, L.T("Voulez-vous sauvegarder la partie en cours avant de quitter ?"), L.T("Quitter Misterword"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) { e.Cancel = true; FocusPrincipal(); return; }
                if (r == DialogResult.Yes && !Sauvegarder(false)) { e.Cancel = true; return; }
            }
            base.OnFormClosing(e);
            if (e.Cancel) return;
            stats.Sauver();
            Bips.Arreter();
        }

        bool LireOption(string nom, bool defaut)
        {
            try
            {
                string f = Path.Combine(dossierDonnees, "options.txt");
                if (!File.Exists(f)) return defaut;
                var l = File.ReadAllLines(f);
                if (l.Contains(nom + "=1")) return true;
                if (l.Contains(nom + "=0")) return false;
                return defaut;
            }
            catch { return defaut; }
        }

        string LireTexte(string nom)
        {
            try
            {
                string f = Path.Combine(dossierDonnees, "options.txt");
                if (!File.Exists(f)) return null;
                foreach (var l in File.ReadAllLines(f))
                    if (l.StartsWith(nom + "=")) return l.Substring(nom.Length + 1);
            }
            catch { }
            return null;
        }

        void EcrireOptions()
        {
            try
            {
                Directory.CreateDirectory(dossierDonnees);
                File.WriteAllText(Path.Combine(dossierDonnees, "options.txt"), "clair=" + (clair ? "1" : "0") + "\r\nindices=" + (indices ? "1" : "0")
                    + "\r\nlangue=" + L.Code + "\r\nmots=" + langueMots + "\r\nmaj=" + (majAuto ? "1" : "0") + "\r\nmajdate=" + majDate + "\r\n");
            }
            catch { }
        }

        static string Essais(int n) { return n + (L.Pl(n) ? L.T(" essais") : L.T(" essai")); }
        static string Restants(int n) { return Essais(n) + (L.Pl(n) ? L.T(" restants.") : L.T(" restant.")); }

        void Afficher(Jeu j)
        {
            Bips.Arreter();
            minuterie.Stop();
            CompterSiAbandon();
            jeu = j;
            finie = false; comptee = false; pos = -1;
            page.Visible = false;
            zoneJeu.Visible = j == Jeu.Mots || j == Jeu.Couleurs;
            panneauTaquin.Visible = j == Jeu.Taquin;
            panneauMC.Visible = j == Jeu.MotsCroises;
            panneauSudoku.Visible = j == Jeu.Sudoku;
            saisieMot.Visible = j == Jeu.Mots;
            saisieCode.Visible = j == Jeu.Couleurs;
            liste.Items.Clear();
            dessin.BackColor = j == Jeu.Mots ? Color.FromArgb(20, 40, 90) : Color.FromArgb(70, 45, 25);
            dessin.Invalidate();
        }

        // ================= Commandes du menu =================

        void Rejouer()
        {
            if (jeu == Jeu.Mots) NouveauMot(longueurChoisie);
            else if (jeu == Jeu.Couleurs) NouveauCode(avanceChoisi);
            else if (jeu == Jeu.Taquin) NouveauTaquin(tailleTaquin);
            else if (jeu == Jeu.MotsCroises) NouveauxMotsCroises(tailleMC);
            else if (jeu == Jeu.Sudoku) NouveauSudoku(niveauSudoku);
            else Parole.Dire(L.T("Choisissez d'abord un jeu dans le menu Fichier, Nouvelle partie."));
        }

        void Abandonner()
        {
            if (jeu == Jeu.Aucun || finie) { Parole.Dire(L.T("Aucune partie en cours.")); return; }
            if (jeu == Jeu.Taquin)
            {
                finie = true;
                etatTaquin.Text = L.T("Partie abandonnée. Ctrl+N pour rejouer.");
                int d0 = Bips.Defaite();
                PlusTard(d0, () => Parole.Dire(L.T("Partie abandonnée, après ") + Depl(partieTaquin.Deplacements) + L.T(". Ctrl+N pour rejouer.")));
                return;
            }
            if (jeu == Jeu.MotsCroises || jeu == Jeu.Sudoku) { AbandonnerGrille(); return; }
            Compter(false);
            finie = true;
            string sol = jeu == Jeu.Mots ? L.T("Le mot était ") + partieMot.Secret : L.T("La combinaison était : ") + PartieCouleurs.Noms(partieCouleurs.Secret);
            liste.Items.Add(L.T("Abandon. ") + sol + ".");
            liste.SelectedIndex = liste.Items.Count - 1;
            etat.Text = sol + L.T(". Ctrl+N pour rejouer.");
            dessin.Invalidate();
            int d = Bips.Defaite();
            string dire = sol + (jeu == Jeu.Mots ? ". " + Epeler(partieMot.Secret) : "") + L.T(". Ctrl+N pour rejouer.");
            PlusTard(d, () => Parole.Dire(dire));
        }

        void AnnoncerStatistiques()
        {
            Parole.Dire(stats.Mots.Decrire("Misterword") + " " + stats.Couleurs.Decrire("Mistermind") + " " + DecrireRecordsTaquin() + " " + DecrireRecordsGrilles());
        }

        void BasculerClair()
        {
            clair = !clair;
            itemClair.Checked = clair;
            EcrireOptions();
            RemplirListe();
            Parole.Dire(clair ? L.T("Résultats annoncés en clair, en plus des bips.") : L.T("Résultats donnés par les bips seulement."));
        }

        void BasculerIndices()
        {
            indices = !indices;
            itemIndices.Checked = indices;
            EcrireOptions();
            RemplirListe();
            dessin.Invalidate();
            Parole.Dire(indices
                ? L.T("Mistermind : chaque couleur reçoit son bip, aigu bien placée, moyen mal placée, grave absente.")
                : L.T("Mistermind, règle classique : seulement le nombre de couleurs bien placées et mal placées."));
        }

        void ExpliquerBips()
        {
            var notes = new List<KeyValuePair<int, int>>
            {
                new KeyValuePair<int,int>(Bips.Aigu, 300), new KeyValuePair<int,int>(0, 500),
                new KeyValuePair<int,int>(Bips.Moyen, 300), new KeyValuePair<int,int>(0, 500),
                new KeyValuePair<int,int>(Bips.Grave, 300),
            };
            int d = Bips.Jouer(notes);
            PlusTard(d + 100, () => Parole.Dire(L.T("Aigu : bien placé. Moyen : mal placé. Grave : absent.")));
        }

        void APropos()
        {
            MessageBox.Show(this,
                L.T("Misterword, version 4.1\n\nMisterword, dans le style de Motus, Mistermind, le jeu des couleurs, le Taquin, les Mots croisés et le Sudoku.\nJouable entièrement au clavier, avec NVDA ou JAWS.\n\nDictionnaire : Grammalecte / Dicollecte (MPL 2.0) et « an-array-of-french-words » (MIT).\nCommunication avec NVDA : nvdaControllerClient, de NV Access (LGPL).\n\nDéfinitions des mots croisés écrites pour le jeu. Publié par Exatropic, sans restriction de droits."),
                L.T("À propos de Misterword"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        string Recapitulatif()
        {
            if (jeu == Jeu.Aucun) return L.T("Aucune partie. Alt ou F10 pour ouvrir le menu.");
            if (jeu == Jeu.Taquin) return RecapTaquin();
            if (jeu == Jeu.MotsCroises) return RecapMC();
            if (jeu == Jeu.Sudoku) return RecapSudoku();
            if (jeu == Jeu.Mots)
            {
                if (finie) return L.T("Partie terminée. Le mot était ") + partieMot.Secret + L.T(". Ctrl+N pour rejouer.");
                var motif = partieMot.Motif();
                string s = L.T("Mot de ") + partieMot.Longueur + L.T(" lettres : ") + string.Join(", ", motif.Select(c => c == '.' ? L.T("blanc") : c.ToString())) + ". ";
                var mal = partieMot.LettresMalPlacees().ToList();
                var abs = partieMot.LettresAbsentes().ToList();
                if (mal.Count > 0) s += L.T("Présentes mais mal placées : ") + string.Join(", ", mal) + ". ";
                if (abs.Count > 0) s += L.T("Absentes : ") + string.Join(", ", abs) + ". ";
                return s + Restants(partieMot.Restants);
            }
            if (finie) return L.T("Partie terminée. La combinaison était : ") + PartieCouleurs.Noms(partieCouleurs.Secret) + L.T(". Ctrl+N pour rejouer.");
            return NomNiveau() + ". " + Restants(partieCouleurs.Restants) + L.T(" Couleurs : ") + ListeCouleursDispo() + L.T(". En cours : ") + saisieCode.Relire();
        }

        void CompterSiAbandon()
        {
            // quitter une partie commencée pour une autre compte comme une partie perdue
            if (jeu != Jeu.Mots && jeu != Jeu.Couleurs || finie || comptee) return;
            int n = jeu == Jeu.Mots ? (partieMot == null ? 0 : partieMot.Essais.Count) : (partieCouleurs == null ? 0 : partieCouleurs.Essais.Count);
            if (n > 0) Compter(false);
        }

        void Compter(bool gagne)
        {
            if (comptee || (jeu != Jeu.Mots && jeu != Jeu.Couleurs)) return;
            comptee = true;
            if (jeu == Jeu.Mots) stats.Mots.Enregistrer(gagne, partieMot.Essais.Count, gagne ? partieMot.Points() : 0);
            else stats.Couleurs.Enregistrer(gagne, partieCouleurs.Essais.Count, gagne ? partieCouleurs.Points() : 0);
            stats.Sauver();
        }

        // ================= La liste des propositions =================

        string TexteLigne(int i)
        {
            if (jeu == Jeu.Mots)
            {
                var e = partieMot.Essais[i];
                return (i + 1) + ". " + e.Mot + (clair ? " : " + DetailMot(e) + "." : "");
            }
            if (indices && clair)
                return L.T("Essai ") + (i + 1) + " : " + DetailCode(i) + ".";
            return L.T("Essai ") + (i + 1) + " : " + PartieCouleurs.Noms(partieCouleurs.Essais[i]) + (clair ? ". " + Majuscule(Resultat(partieCouleurs.Reponses[i])) + "." : "");
        }

        int NombreEssais { get { return jeu == Jeu.Mots ? partieMot.Essais.Count : jeu == Jeu.Couleurs ? partieCouleurs.Essais.Count : 0; } }

        void RemplirListe()
        {
            if (jeu == Jeu.Aucun) return;
            int sel = liste.SelectedIndex, posGarde = pos;
            var fin = Enumerable.Range(0, liste.Items.Count).Skip(NombreEssais).Select(i => liste.Items[i]).ToList();
            liste.BeginUpdate();
            liste.Items.Clear();
            for (int i = 0; i < NombreEssais; i++) liste.Items.Add(TexteLigne(i));
            foreach (var f in fin) liste.Items.Add(f);
            if (sel >= 0 && sel < liste.Items.Count) liste.SelectedIndex = sel;
            liste.EndUpdate();
            pos = posGarde;
        }

        void AjouterLigne(string texte)
        {
            liste.Items.Add(texte);
            liste.SelectedIndex = liste.Items.Count - 1;
        }

        /// <summary>Mistermind : en arrivant sur une ligne, un bip aigu par couleur bien placée, un moyen par couleur mal placée, un grave si aucune.</summary>
        void BipsLigne()
        {
            if (jeu != Jeu.Couleurs) return;
            int i = liste.SelectedIndex;
            if (i < 0 || i >= partieCouleurs.Essais.Count) return;
            Bips.JouerMarques(BipsCode(i));
        }

        /// <summary>Les bips d'un essai : couleur par couleur si l'option est cochée, sinon le résumé classique.</summary>
        Marque[] BipsCode(int i)
        {
            return indices ? partieCouleurs.Positions[i] : BipsResume(partieCouleurs.Reponses[i]);
        }

        static string NomMarqueF(Marque m)
        {
            return m == Marque.BienPlacee ? L.T("bien placée") : m == Marque.MalPlacee ? L.T("mal placée") : L.T("absente");
        }

        string DetailCode(int i)
        {
            var c = partieCouleurs.Essais[i]; var m = partieCouleurs.Positions[i];
            return string.Join(", ", Enumerable.Range(0, c.Length).Select(k => PartieCouleurs.Palette[c[k]].Nom + " " + NomMarqueF(m[k])));
        }

        static Marque[] BipsResume(Marque[] r)
        {
            var b = r.Where(x => x != Marque.Absente).ToArray();
            return b.Length == 0 ? new[] { Marque.Absente } : b;
        }

        void ToucheListe(object sender, KeyEventArgs e)
        {
            if (e.Control || e.Alt) return;
            int i = liste.SelectedIndex;
            if (e.KeyCode == Keys.Down && i == liste.Items.Count - 1)
            {
                e.Handled = true; e.SuppressKeyPress = true;
                if (jeu == Jeu.Mots) { saisieMot.Focus(); Parole.Dire(L.T("Votre proposition")); }
                else saisieCode.Focus();
                return;
            }
            if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right) return;
            e.Handled = true; e.SuppressKeyPress = true;
            if (i < 0 || i >= NombreEssais) { Parole.Dire(i < 0 ? L.T("Aucune proposition.") : liste.Items[i].ToString()); return; }
            int longueur = jeu == Jeu.Mots ? partieMot.Longueur : partieCouleurs.Longueur;
            if (e.KeyCode == Keys.Right) { if (pos < longueur - 1) pos++; else System.Media.SystemSounds.Beep.Play(); }
            else { if (pos > 0) pos--; else { pos = 0; System.Media.SystemSounds.Beep.Play(); } }

            if (jeu == Jeu.Mots)
            {
                var es = partieMot.Essais[i];
                Bips.JouerMarques(new[] { es.Marques[pos] });
                Parole.Dire(es.Mot[pos] + (clair ? ", " + NomMarque(es.Marques[pos]) : ""));
            }
            else
            {
                string nom = PartieCouleurs.Palette[partieCouleurs.Essais[i][pos]].Nom;
                if (indices)
                {
                    var m = partieCouleurs.Positions[i][pos];
                    Bips.JouerMarques(new[] { m });
                    Parole.Dire(nom + (clair ? ", " + NomMarqueF(m) : ""));
                }
                else Parole.Dire(nom);   // règle classique : la couleur seule, sans bip
            }
        }

        // ================= Sauvegarde de la partie =================

        string CheminSauvegarde { get { return Path.Combine(dossierDonnees, "partie.sav"); } }

        bool PartieEnCours
        {
            get
            {
                if (finie) return false;
                if (jeu == Jeu.Taquin) return partieTaquin != null;
                if (jeu == Jeu.MotsCroises) return partieMC != null && partieMC.CasesRemplies > 0;
                if (jeu == Jeu.Sudoku) return partieSudoku != null;
                if (jeu == Jeu.Mots) return partieMot != null && partieMot.Essais.Count > 0;
                if (jeu == Jeu.Couleurs) return partieCouleurs != null && partieCouleurs.Essais.Count > 0;
                return false;
            }
        }

        static string Code(int[] c) { return string.Concat(c.Select(i => (char)('0' + i))); }
        int[] Decode(string s, PartieCouleurs modele) { return s.Select(ch => char.IsDigit(ch) ? ch - '0' : modele.Indice(ch)).ToArray(); }

        bool Sauvegarder(bool annoncer)
        {
            if (jeu == Jeu.Aucun || finie)
            {
                if (annoncer) Parole.Dire(L.T("Aucune partie en cours à sauvegarder."));
                return false;
            }
            var l = new List<string>();
            if (jeu == Jeu.Mots)
            {
                l.Add("jeu=mots");
                l.Add("langue=" + langueMots);
                l.Add("longueur=" + longueurChoisie);
                l.Add("secret=" + partieMot.Secret);
                l.Add("essais=" + string.Join(",", partieMot.Essais.Select(e => e.Mot)));
            }
            else if (jeu == Jeu.Couleurs)
            {
                l.Add("jeu=couleurs");
                l.Add("avance=" + (partieCouleurs.Avance ? 1 : 0));
                l.Add("secret=" + Code(partieCouleurs.Secret));
                l.Add("essais=" + string.Join(",", partieCouleurs.Essais.Select(Code)));
            }
            else if (jeu == Jeu.MotsCroises || jeu == Jeu.Sudoku) SauverGrille(l);
            else
            {
                l.Add("jeu=taquin");
                l.Add("taille=" + partieTaquin.Taille);
                l.Add("grille=" + partieTaquin.Texte().Replace(' ', '_'));
                l.Add("deplacements=" + partieTaquin.Deplacements);
                l.Add("secondes=" + SecondesTaquin);
                l.Add("ligne=" + zoneTaquin.Ligne);
                l.Add("colonne=" + zoneTaquin.Colonne);
            }
            try
            {
                Directory.CreateDirectory(dossierDonnees);
                File.WriteAllLines(CheminSauvegarde, l);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.T("Impossible de sauvegarder la partie :\n") + ex.Message, "Misterword", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (annoncer) Parole.Dire(L.T("Partie sauvegardée. Vous pourrez la reprendre au prochain lancement, ou avec Ctrl+O."));
            return true;
        }

        Dictionary<string, string> LireSauvegarde()
        {
            var d = new Dictionary<string, string>();
            foreach (var ligne in File.ReadAllLines(CheminSauvegarde))
            {
                int i = ligne.IndexOf('=');
                if (i > 0) d[ligne.Substring(0, i)] = ligne.Substring(i + 1);
            }
            return d;
        }

        string DecrireSauvegarde()
        {
            try
            {
                var d = LireSauvegarde();
                switch (d["jeu"])
                {
                    case "mots": return L.T("Misterword, mot de ") + d["secret"].Length + L.T(" lettres");
                    case "couleurs": return L.T("Mistermind ") + (d["avance"] == "1" ? L.T("expert") : L.T("facile"));
                    case "taquin": return L.T("Taquin ") + NiveauTaquin(int.Parse(d["taille"]));
                    case "motscroises": return L.T("Mots croisés ") + NiveauMC(int.Parse(d["taille"]));
                    case "sudoku": return L.T("Sudoku ") + PartieSudoku.NomNiveau(int.Parse(d["niveau"]));
                }
            }
            catch { }
            return L.T("partie");
        }

        static int Entier(Dictionary<string, string> d, string cle)
        {
            string v; int n;
            return d.TryGetValue(cle, out v) && int.TryParse(v, out n) ? n : 0;
        }

        bool Reprendre(bool annoncerAbsence)
        {
            if (!File.Exists(CheminSauvegarde))
            {
                if (annoncerAbsence) Parole.Dire(L.T("Aucune partie sauvegardée."));
                return false;
            }
            if (annoncerAbsence && PartieEnCours)
            {
                var r = MessageBox.Show(this, L.T("La partie en cours sera abandonnée. Reprendre quand même la partie sauvegardée (") + DecrireSauvegarde() + ") ?", L.T("Reprendre la partie"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) { FocusPrincipal(); return false; }
            }
            try
            {
                var d = LireSauvegarde();
                string essais; d.TryGetValue("essais", out essais);
                var liste = string.IsNullOrEmpty(essais) ? new string[0] : essais.Split(',');
                switch (d["jeu"])
                {
                    case "mots":
                        {
                            string lg;
                            if (d.TryGetValue("langue", out lg) && lg != langueMots && DicoInstalle(lg)) { langueMots = lg; ChargerDico(false); EcrireOptions(); ReconstruireMenu(); }
                        }
                        NouveauMot(Entier(d, "longueur"), d["secret"], liste);
                        break;
                    case "couleurs":
                        {
                            bool avance = d["avance"] == "1";
                            var modele = new PartieCouleurs(avance, rnd);
                            NouveauCode(avance, Decode(d["secret"], modele), liste.Select(x => Decode(x, modele)).ToList());
                            break;
                        }
                    case "taquin":
                        NouveauTaquin(Entier(d, "taille"), d["grille"].Replace('_', ' '), Entier(d, "deplacements"), Entier(d, "secondes"), Entier(d, "ligne"), Entier(d, "colonne"));
                        break;
                    case "motscroises":
                    case "sudoku":
                        ReprendreGrille(d["jeu"], d);
                        break;
                    default: throw new Exception("jeu inconnu");
                }
                try { File.Delete(CheminSauvegarde); } catch { }
                return true;
            }
            catch
            {
                MessageBox.Show(this, L.T("La sauvegarde est illisible."), "Misterword", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                try { File.Delete(CheminSauvegarde); } catch { }
                return false;
            }
        }

        // ================= Taquin =================

        void ConstruireTaquin()
        {
            var t = panneauTaquin;
            t.Dock = DockStyle.Fill; t.ColumnCount = 1; t.RowCount = 2; t.Padding = new Padding(8);
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            zoneTaquin.Dock = DockStyle.Fill;
            zoneTaquin.ApresGlissement = GlissementTaquin;
            t.Controls.Add(zoneTaquin, 0, 0);
            t.Controls.Add(etatTaquin, 0, 1);
        }

        string DecrireRecordsTaquin()
        {
            var l = new List<string>();
            foreach (int t in new[] { 3, 4, 5 })
                if (records[t].MeilleurTemps >= 0)
                    l.Add(L.T("niveau ") + NiveauTaquin(t) + " : " + RecordsTaquin.Duree(records[t].MeilleurTemps) + L.T(" en ") + records[t].DeplacementsDuRecord + L.T(" déplacements"));
            return l.Count == 0 ? L.T("Taquin : aucun temps enregistré.") : L.T("Records du taquin, ") + string.Join(" ; ", l) + ".";
        }

        void NouveauTaquin(int taille, string grilleReprise = null, int deplacements = 0, int secondes = 0, int ligne = 0, int colonne = 0)
        {
            tailleTaquin = taille;
            Afficher(Jeu.Taquin);
            partieTaquin = new PartieTaquin(taille);
            bool test = Environment.GetEnvironmentVariable("MISTERWORD_TEST_TAQUIN") != null;
            if (grilleReprise == null || !partieTaquin.Reprendre(grilleReprise, deplacements))
                partieTaquin.Melanger(test ? new Random(42) : rnd, test ? 2 : taille * taille * 20);
            zoneTaquin.Partie = partieTaquin;
            zoneTaquin.Ligne = Math.Min(ligne, taille - 1); zoneTaquin.Colonne = Math.Min(colonne, taille - 1);
            chronoLance = false;
            if (grilleReprise != null && (deplacements > 0 || secondes > 0)) { chronoLance = true; debutTaquin = DateTime.Now.AddSeconds(-secondes); }
            MajEtatTaquin();
            zoneTaquin.Invalidate();
            zoneTaquin.Focus();
            int d = Bips.Nouveau();
            if (grilleReprise != null)
            {
                PlusTard(d, () => Parole.Dire(L.T("Partie reprise. Taquin ") + NiveauTaquin(taille) + ", " + Depl(partieTaquin.Deplacements) + ", "
                    + L.T("temps écoulé : ") + RecordsTaquin.Duree(secondes) + ". " + zoneTaquin.LireTout() + L.T(" Vous êtes sur ") + zoneTaquin.DecrireCase(zoneTaquin.Ligne, zoneTaquin.Colonne, false) + "."));
                return;
            }
            PlusTard(d, () => Parole.Dire(L.T("Taquin ") + NiveauTaquin(taille) + ", " + taille + L.T(" sur ") + taille + L.T(". Remettez les lettres dans l'ordre, de A à ") + partieTaquin.DerniereLettre + L.T(", ligne par ligne, la case vide en bas à droite. ")
                + L.T("Flèches pour choisir une lettre, Entrée pour la faire glisser vers la case vide. T lit tout le tableau, V va à la case vide. ")
                + zoneTaquin.LireTout() + L.T(" Vous êtes sur ") + zoneTaquin.DecrireCase(0, 0, false) + "."));
        }

        void MajEtatTaquin()
        {
            etatTaquin.Text = L.T("Taquin ") + NiveauTaquin(tailleTaquin) + "   —   " + Depl(partieTaquin.Deplacements)
                + (Record.MeilleurTemps >= 0 ? L.T("   —   record : ") + RecordsTaquin.Duree(Record.MeilleurTemps) : "");
        }

        static string Depl(int n) { return n + (L.Pl(n) ? L.T(" déplacements") : L.T(" déplacement")); }

        int SecondesTaquin { get { return chronoLance ? (int)(DateTime.Now - debutTaquin).TotalSeconds : 0; } }

        string RecapTaquin()
        {
            if (finie) return L.T("Partie terminée. Ctrl+N pour rejouer.");
            string s = L.T("Taquin ") + NiveauTaquin(tailleTaquin) + ", " + Depl(partieTaquin.Deplacements) + ". ";
            s += chronoLance ? L.T("Temps écoulé : ") + RecordsTaquin.Duree(SecondesTaquin) + ". " : L.T("Le chronomètre démarre au premier déplacement. ");
            if (Record.MeilleurTemps >= 0) s += L.T("Record : ") + RecordsTaquin.Duree(Record.MeilleurTemps) + ".";
            return s;
        }

        void GlissementTaquin(List<char> lettres, string sens)
        {
            if (finie) return;
            if (!chronoLance) { chronoLance = true; debutTaquin = DateTime.Now; }
            int d = Bips.Glissement(lettres.Count);
            MajEtatTaquin();
            lettres.Reverse();   // la lettre choisie d'abord
            string quoi = string.Join(", ", lettres);
            if (!partieTaquin.EstRangee())
            {
                PlusTard(d, () => Parole.Dire(quoi + " " + sens + "."));
                return;
            }
            finie = true;
            int secondes = SecondesTaquin;
            int coups = partieTaquin.Deplacements;
            PlusTard(d + 100, () =>
            {
                int v = Bips.Victoire();
                string bravo = L.T("Bravo ! Toutes les lettres sont dans l'ordre, en ") + RecordsTaquin.Duree(secondes) + L.T(" et ") + Depl(coups) + ".";
                Parole.Dire(bravo);
                PlusTard(v + 2500, () => ProposerRecord(secondes, coups, bravo));
            });
        }

        void ProposerRecord(int secondes, int coups, string bravo)
        {
            var r = MessageBox.Show(this, bravo + L.T("\n\nVoulez-vous enregistrer votre temps ?"), L.T("Taquin réussi"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            string msg;
            if (r == DialogResult.Yes)
            {
                int ancien = Record.MeilleurTemps;
                Record.Enregistrees++;
                if (Record.MoinsDeDeplacements < 0 || coups < Record.MoinsDeDeplacements) Record.MoinsDeDeplacements = coups;
                if (ancien < 0 || secondes < ancien)
                {
                    Record.MeilleurTemps = secondes; Record.DeplacementsDuRecord = coups;
                    msg = ancien < 0 ? L.T("Temps enregistré. C'est votre premier record : ") + RecordsTaquin.Duree(secondes) + "."
                                     : L.T("Nouveau record ! ") + RecordsTaquin.Duree(secondes) + L.T(", contre ") + RecordsTaquin.Duree(ancien) + L.T(" auparavant.");
                    Bips.Victoire();
                }
                else msg = L.T("Temps enregistré. Le record reste de ") + RecordsTaquin.Duree(ancien) + ".";
                Record.Sauver();
            }
            else msg = L.T("Temps non enregistré.");
            MajEtatTaquin();
            etatTaquin.Text = L.T("Réussi en ") + RecordsTaquin.Duree(secondes) + ", " + Depl(coups) + L.T(". Ctrl+N pour rejouer.");
            zoneTaquin.Focus();
            PlusTard(300, () => Parole.Dire(msg + L.T(" Ctrl+N pour rejouer.")));
        }

        // ================= Misterword =================

        void NouveauMot(int longueur, string secretRepris = null, IList<string> essaisRepris = null)
        {
            if (dico == null)
            {
                string code = Langues.Contains(langueMots) ? langueMots : L.Code;
                ProposerTelechargement(code, Langues.Any(DicoInstalle) ? L.F("Le dictionnaire {0} n'est pas installé.", NomLangue(code))
                    : L.T("Aucun dictionnaire n'est installé. Le Mistermind, le Taquin et le Sudoku restent jouables sans dictionnaire."), () => { if (dico != null) NouveauMot(longueur); });
                return;
            }
            longueurChoisie = longueur;
            int l = longueur > 0 ? longueur : rnd.Next(Dictionnaire.LongueurMin, Dictionnaire.LongueurMax + 1);
            string secret = secretRepris ?? dico.Tirer(l, rnd);
            string impose = Environment.GetEnvironmentVariable("MISTERWORD_TEST_MOT");
            if (!string.IsNullOrEmpty(impose) && secretRepris == null) secret = Texte.Normaliser(impose);
            if (secret == null) { Parole.Dire(L.T("Aucun mot de ") + l + L.T(" lettres dans le dictionnaire.")); return; }
            Afficher(Jeu.Mots);
            partieMot = new PartieMot(secret, dico);
            etiquetteSaisie.Text = L.T("Votre proposition (") + partieMot.Longueur + L.T(" lettres, commence par ") + partieMot.PremiereLettre + ") :";
            saisieMot.Text = "";
            saisieMot.MaxLength = partieMot.Longueur;
            if (essaisRepris != null)
            {
                foreach (var m in essaisRepris) { partieMot.Proposer(m); AjouterLigne(TexteLigne(partieMot.Essais.Count - 1)); }
                dessin.Invalidate();
            }
            MajEtatMot();
            saisieMot.Focus();
            if (secretRepris != null)
            {
                int d0 = Bips.Nouveau();
                PlusTard(d0, () => Parole.Dire(L.T("Partie reprise. Misterword, mot de ") + partieMot.Longueur + L.T(" lettres, commençant par ") + partieMot.PremiereLettre + ". "
                    + (partieMot.Essais.Count > 0 ? partieMot.Essais.Count + (L.Pl(partieMot.Essais.Count) ? L.T(" propositions faites, ") : L.T(" proposition faite, ")) : "") + Restants(partieMot.Restants) + L.T(" Flèche haut pour relire vos propositions.")));
                return;
            }
            int d = Bips.Nouveau();
            PlusTard(d, () => Parole.Dire(L.T("Misterword. Mot de ") + partieMot.Longueur + L.T(" lettres, commençant par ") + partieMot.PremiereLettre + ". " + PartieMot.EssaisMax + L.T(" essais. Tapez votre mot puis Entrée.")));
        }

        void MajEtatMot()
        {
            etat.Text = L.T("Mot : ") + string.Join(" ", partieMot.Motif()) + "   —   " + Restants(partieMot.Restants);
        }

        static string NomMarque(Marque m)
        {
            return m == Marque.BienPlacee ? L.T("bien placé") : m == Marque.MalPlacee ? L.T("mal placé") : L.T("absent");
        }

        static string DetailMot(EssaiMot e)
        {
            return string.Join(", ", e.Mot.Select((c, i) => c + " " + NomMarque(e.Marques[i])));
        }

        static string Epeler(string m) { return string.Join(", ", m.ToCharArray()); }
        static string Majuscule(string s) { return s.Length == 0 ? s : char.ToUpper(s[0]) + s.Substring(1); }

        void ToucheSaisieMot(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A) { e.SuppressKeyPress = true; saisieMot.SelectAll(); return; }
            if (e.Control || e.Alt) return;
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; e.Handled = true; ProposerMot(); }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true; e.Handled = true;
                if (liste.Items.Count == 0) Parole.Dire(L.T("Aucune proposition pour l'instant."));
                else { liste.SelectedIndex = liste.Items.Count - 1; liste.Focus(); }
            }
        }

        void ProposerMot()
        {
            if (finie) { Parole.Dire(L.T("Partie terminée. Ctrl+N pour rejouer, ou Alt pour le menu.")); return; }
            string mot = Texte.Normaliser(saisieMot.Text);
            if (mot.Length == 0) { Parole.Dire(Recapitulatif()); return; }
            if (mot.Length == partieMot.Longueur - 1 && mot[0] != partieMot.PremiereLettre) mot = partieMot.PremiereLettre + mot;
            var refus = partieMot.Verifier(mot);
            if (refus != Refus.Aucun)
            {
                string msg;
                switch (refus)
                {
                    case Refus.Longueur: msg = mot + L.T(" a ") + mot.Length + L.T(" lettres. Il en faut ") + partieMot.Longueur + "."; break;
                    case Refus.PremiereLettre: msg = L.T("Le mot doit commencer par ") + partieMot.PremiereLettre + "."; break;
                    case Refus.DejaPropose: msg = mot + L.T(" a déjà été proposé."); break;
                    case Refus.Inconnu: msg = mot + L.T(" n'est pas dans le dictionnaire."); break;
                    default: msg = L.T("Proposition refusée."); break;
                }
                int d0 = Bips.Erreur();
                saisieMot.SelectAll();
                PlusTard(d0, () => Parole.Dire(msg + L.T(" Essai non compté.")));
                return;
            }

            var essai = partieMot.Proposer(mot);
            int n = partieMot.Essais.Count;
            AjouterLigne(TexteLigne(n - 1));
            saisieMot.Text = "";
            MajEtatMot();
            dessin.Invalidate();
            string detail = clair ? DetailMot(essai) + ". " : "";

            int duree = Bips.JouerMarques(essai.Marques);
            if (partieMot.Gagnee)
            {
                int pts = partieMot.Points();
                Compter(true); finie = true;
                AjouterLigne(L.T("Gagné ! ") + mot + L.T(" trouvé en ") + Essais(n) + ", " + pts + L.T(" points."));
                etat.Text = L.T("Trouvé : ") + mot + L.T(" en ") + Essais(n) + ", " + pts + L.T(" points. Ctrl+N pour rejouer.");
                string a = L.T("Bravo ! Vous avez trouvé ") + mot + L.T(" en ") + Essais(n) + ". " + pts + L.T(" points. Ctrl+N pour rejouer.");
                PlusTard(duree + 150, () => { int v = Bips.Victoire(); PlusTard(v, () => Parole.Dire(a)); });
            }
            else if (partieMot.Finie)
            {
                Compter(false); finie = true;
                AjouterLigne(L.T("Perdu. Le mot était ") + partieMot.Secret + ".");
                etat.Text = L.T("Perdu. Le mot était ") + partieMot.Secret + L.T(". Ctrl+N pour rejouer.");
                string a = detail + L.T("Perdu ! Le mot était ") + partieMot.Secret + ". " + Epeler(partieMot.Secret) + L.T(". Ctrl+N pour rejouer.");
                PlusTard(duree + 150, () => { int v = Bips.Defaite(); PlusTard(v, () => Parole.Dire(a)); });
            }
            else
            {
                string a = detail + Restants(partieMot.Restants);
                PlusTard(duree + 100, () => Parole.Dire(a));
            }
        }

        void DessinerMots(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (partieMot == null) return;
            int L = partieMot.Longueur, R = PartieMot.EssaisMax;
            int taille = Math.Max(12, Math.Min((dessin.Width - 16) / L, (dessin.Height - 16) / R) - 4);
            int x0 = (dessin.Width - L * (taille + 4)) / 2, y0 = (dessin.Height - R * (taille + 4)) / 2;
            using (var police = new Font("Consolas", taille * 0.5f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var bleu = new SolidBrush(Color.FromArgb(30, 80, 170)))
            using (var rouge = new SolidBrush(Color.FromArgb(210, 40, 40)))
            using (var jaune = new SolidBrush(Color.FromArgb(240, 200, 30)))
            using (var bord = new Pen(Color.FromArgb(120, 160, 230)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                var motif = partieMot.Motif();
                for (int r = 0; r < R; r++)
                    for (int c = 0; c < L; c++)
                    {
                        var rc = new Rectangle(x0 + c * (taille + 4), y0 + r * (taille + 4), taille, taille);
                        g.FillRectangle(bleu, rc);
                        g.DrawRectangle(bord, rc);
                        string lettre = null;
                        if (r < partieMot.Essais.Count)
                        {
                            var es = partieMot.Essais[r];
                            if (es.Marques[c] == Marque.BienPlacee) g.FillRectangle(rouge, rc);
                            else if (es.Marques[c] == Marque.MalPlacee) g.FillEllipse(jaune, Rectangle.Inflate(rc, -2, -2));
                            lettre = es.Mot[c].ToString();
                        }
                        else if (r == partieMot.Essais.Count && !finie && motif[c] != '.') lettre = motif[c].ToString();
                        if (lettre != null) g.DrawString(lettre, police, Brushes.White, rc, fmt);
                    }
            }
        }

        // ================= Mistermind =================

        string NomNiveau()
        {
            return partieCouleurs.Avance ? L.T("Mistermind expert, 5 couleurs sur 8") : L.T("Mistermind facile, 4 couleurs sur 6");
        }

        string ListeCouleursDispo()
        {
            return string.Join(", ", partieCouleurs.Disponibles.Select(c => c.Lettre + " " + c.Nom));
        }

        void NouveauCode(bool avance, int[] secretRepris = null, IList<int[]> essaisRepris = null)
        {
            avanceChoisi = avance;
            Afficher(Jeu.Couleurs);
            partieCouleurs = new PartieCouleurs(avance, rnd, secretRepris);
            saisieCode.NbCouleurs = partieCouleurs.NbCouleurs;
            saisieCode.Vider(partieCouleurs.Longueur);
            etiquetteSaisie.Text = L.T("Votre combinaison (") + ListeCouleursDispo() + ") :";
            if (essaisRepris != null)
            {
                foreach (var c in essaisRepris) { partieCouleurs.Proposer(c); AjouterLigne(TexteLigne(partieCouleurs.Essais.Count - 1)); }
                dessin.Invalidate();
            }
            MajEtatCode();
            int d = Bips.Nouveau();
            minuterie.Stop();
            saisieCode.Focus();
            if (secretRepris != null)
            {
                PlusTard(d, () => Parole.Dire(L.T("Partie reprise. ") + NomNiveau() + ". " + Restants(partieCouleurs.Restants) + L.T(" Couleurs : ") + ListeCouleursDispo() + L.T(". Tab pour relire vos propositions.")));
                return;
            }
            PlusTard(d, () => Parole.Dire(NomNiveau() + ". " + PartieCouleurs.EssaisMax + L.T(" essais. Couleurs : ") + ListeCouleursDispo() + L.T(". Tapez votre combinaison puis Entrée.")));
        }

        void MajEtatCode()
        {
            etat.Text = NomNiveau() + "   —   " + Restants(partieCouleurs.Restants);
        }

        static string Resultat(Marque[] r)
        {
            int b = r.Count(x => x == Marque.BienPlacee), m = r.Count(x => x == Marque.MalPlacee);
            if (b == 0 && m == 0) return L.T("aucune couleur juste");
            var parts = new List<string>();
            if (b > 0) parts.Add(b + (L.Pl(b) ? L.T(" bien placées") : L.T(" bien placée")));
            if (m > 0) parts.Add(m + (L.Pl(m) ? L.T(" mal placées") : L.T(" mal placée")));
            return string.Join(", ", parts);
        }

        void ProposerCode()
        {
            if (finie) { Parole.Dire(L.T("Partie terminée. Ctrl+N pour rejouer, ou Alt pour le menu.")); return; }
            if (!saisieCode.Complete)
            {
                int vides = saisieCode.Cases.Count(c => c < 0);
                int d0 = Bips.Erreur();
                PlusTard(d0, () => Parole.Dire(L.T("Combinaison incomplète : ") + vides + (L.Pl(vides) ? L.T(" cases vides. ") : L.T(" case vide. ")) + saisieCode.Relire()));
                return;
            }
            var code = (int[])saisieCode.Cases.Clone();
            var r = partieCouleurs.Proposer(code);
            int n = partieCouleurs.Essais.Count;
            AjouterLigne(TexteLigne(n - 1));
            saisieCode.Vider(partieCouleurs.Longueur);
            MajEtatCode();
            dessin.Invalidate();
            string res = clair ? (indices ? DetailCode(n - 1) : Majuscule(Resultat(r))) + ". " : "";

            int duree = Bips.JouerMarques(BipsCode(n - 1));
            if (partieCouleurs.Gagnee)
            {
                int pts = partieCouleurs.Points();
                Compter(true); finie = true;
                AjouterLigne(L.T("Gagné ! Trouvé en ") + Essais(n) + ", " + pts + L.T(" points."));
                etat.Text = L.T("Trouvé en ") + Essais(n) + ", " + pts + L.T(" points. Ctrl+N pour rejouer.");
                string a = L.T("Bravo ! Combinaison trouvée en ") + Essais(n) + " : " + PartieCouleurs.Noms(code) + ". " + pts + L.T(" points. Ctrl+N pour rejouer.");
                PlusTard(duree + 150, () => { int v = Bips.Victoire(); PlusTard(v, () => Parole.Dire(a)); });
            }
            else if (partieCouleurs.Finie)
            {
                Compter(false); finie = true;
                string sol = PartieCouleurs.Noms(partieCouleurs.Secret);
                AjouterLigne(L.T("Perdu. La combinaison était : ") + sol + ".");
                etat.Text = L.T("Perdu. La combinaison était : ") + sol + L.T(". Ctrl+N pour rejouer.");
                string a = res + L.T("Perdu ! La combinaison était : ") + sol + L.T(". Ctrl+N pour rejouer.");
                PlusTard(duree + 150, () => { int v = Bips.Defaite(); PlusTard(v, () => Parole.Dire(a)); });
            }
            else
            {
                string a = res + Restants(partieCouleurs.Restants);
                PlusTard(duree + 100, () => Parole.Dire(a));
            }
        }

        void DessinerCouleurs(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (partieCouleurs == null) return;
            int L = partieCouleurs.Longueur, R = PartieCouleurs.EssaisMax;
            int h = Math.Max(10, (dessin.Height - 16) / (R + 1) - 4);
            int d = Math.Max(8, Math.Min(h, (dessin.Width - 40) / (L + 2) - 6));
            int x0 = 10, y0 = 8;
            using (var vide = new SolidBrush(Color.FromArgb(50, 30, 15)))
            {
                for (int r = 0; r < R; r++)
                {
                    int y = y0 + r * (h + 4);
                    for (int c = 0; c < L; c++)
                    {
                        var rc = new Rectangle(x0 + c * (d + 6), y, d, d);
                        if (r < partieCouleurs.Essais.Count)
                            using (var b = new SolidBrush(PartieCouleurs.Palette[partieCouleurs.Essais[r][c]].Teinte)) g.FillEllipse(b, rc);
                        else g.FillEllipse(vide, rc);
                    }
                    if (r < partieCouleurs.Reponses.Count)
                    {
                        var rep = indices ? partieCouleurs.Positions[r] : partieCouleurs.Reponses[r];
                        int p = Math.Max(4, d / 3);
                        int xp = x0 + L * (d + 6) + 10;
                        for (int k = 0; k < rep.Length; k++)
                        {
                            var rc = indices ? new Rectangle(xp + k * (p + 3), y + (d - p) / 2, p, p)
                                             : new Rectangle(xp + (k % 3) * (p + 3), y + (k / 3) * (p + 3), p, p);
                            if (rep[k] == Marque.BienPlacee) g.FillEllipse(Brushes.Red, rc);
                            else if (rep[k] == Marque.MalPlacee) g.FillEllipse(Brushes.White, rc);
                            else g.DrawEllipse(Pens.Black, rc);
                        }
                    }
                }
                if (finie)
                {
                    int y = y0 + R * (h + 4) + 4;
                    for (int c = 0; c < L; c++)
                        using (var b = new SolidBrush(PartieCouleurs.Palette[partieCouleurs.Secret[c]].Teinte))
                            g.FillEllipse(b, new Rectangle(x0 + c * (d + 6), y, d, d));
                }
            }
        }
    }
}
