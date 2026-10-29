using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    /// <summary>Mots croisés et Sudoku : la partie de la fenêtre qui leur est propre.</summary>
    partial class Fenetre
    {
        // ---------- Mots croisés
        readonly TableLayoutPanel panneauMC = new TableLayoutPanel();
        readonly ZoneMotsCroises zoneMC = new ZoneMotsCroises();
        readonly ListBox listeDefs = new ListBox();
        readonly Label etatMC = new Label { AutoSize = true, Dock = DockStyle.Fill };
        PartieMotsCroises partieMC;
        int tailleMC = 7;
        readonly Dictionary<string, Dictionary<string, string>> definitionsPar = new Dictionary<string, Dictionary<string, string>>();

        // ---------- Sudoku
        readonly TableLayoutPanel panneauSudoku = new TableLayoutPanel();
        readonly ZoneSudoku zoneSudoku = new ZoneSudoku();
        readonly Label etatSudoku = new Label { AutoSize = true, Dock = DockStyle.Fill };
        PartieSudoku partieSudoku;
        int niveauSudoku = 1;

        // ---------- Chronomètre et records communs aux deux grilles
        DateTime debutGrille;
        readonly Dictionary<string, RecordsTaquin> recordsGrilles = new Dictionary<string, RecordsTaquin>();

        static string NiveauMC(int t) { return t == 5 ? L.T("facile") : t == 7 ? L.T("moyen") : t == 9 ? L.T("difficile") : L.T("expert"); }

        RecordsTaquin RecordGrille(string cle)
        {
            RecordsTaquin r;
            if (!recordsGrilles.TryGetValue(cle, out r)) recordsGrilles[cle] = r = RecordsTaquin.Charger(Path.Combine(dossierDonnees, cle + ".txt"));
            return r;
        }

        int SecondesGrille { get { return (int)(DateTime.Now - debutGrille).TotalSeconds; } }

        void ConstruireGrilles()
        {
            // Mots croisés : la grille à gauche, les définitions à droite ; Tab passe de l'une à l'autre.
            var t = panneauMC;
            t.Dock = DockStyle.Fill; t.ColumnCount = 2; t.RowCount = 3; t.Padding = new Padding(8);
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lDefs.Text = L.T("Définitions :");
            zoneMC.Dock = DockStyle.Fill;
            zoneMC.Rempli = VerifierFinMC;
            zoneMC.Annoncer = s => Parole.Dire(s);
            zoneMC.Clair = () => clair;
            listeDefs.Dock = DockStyle.Fill;
            listeDefs.IntegralHeight = false;
            listeDefs.AccessibleName = L.T("Définitions");
            listeDefs.KeyDown += ToucheDefs;
            listeDefs.DoubleClick += (s, e) => AllerAuMot();
            t.Controls.Add(lDefs, 1, 0);
            t.Controls.Add(zoneMC, 0, 1);
            t.Controls.Add(listeDefs, 1, 1);
            t.Controls.Add(etatMC, 0, 2); t.SetColumnSpan(etatMC, 2);
            zoneMC.TabIndex = 0; listeDefs.TabIndex = 1;

            var s2 = panneauSudoku;
            s2.Dock = DockStyle.Fill; s2.ColumnCount = 1; s2.RowCount = 2; s2.Padding = new Padding(8);
            s2.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            s2.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            zoneSudoku.Dock = DockStyle.Fill;
            zoneSudoku.Rempli = VerifierFinSudoku;
            s2.Controls.Add(zoneSudoku, 0, 0);
            s2.Controls.Add(etatSudoku, 0, 1);
        }
        readonly Label lDefs = new Label { AutoSize = true };

        // ======================= Mots croisés =======================

        /// <summary>Définitions des mots croisés pour une langue : dictionnaires\xx\definitions.txt, à côté de mots.txt.</summary>
        Dictionary<string, string> Definitions(string code)
        {
            Dictionary<string, string> d;
            if (definitionsPar.TryGetValue(code, out d)) return d;
            var m = FichierMots(code);
            if (m == null) return null;
            string f = Path.Combine(Path.GetDirectoryName(m), "definitions.txt");
            if (!File.Exists(f)) return null;
            try { d = PartieMotsCroises.ChargerDefinitions(f); } catch { return null; }
            if (d.Count < 20) return null;
            definitionsPar[code] = d;
            return d;
        }

        void NouveauxMotsCroises(int taille, PartieMotsCroises reprise = null, int secondes = 0, int ligne = -1, int colonne = 0, bool horiz = true)
        {
            Dictionary<string, string> defs = null;
            if (reprise == null)
            {
                string code = Langues.Contains(langueMots) ? langueMots : L.Code;
                defs = Definitions(code);
                if (defs == null)
                {
                    ProposerTelechargement(code, L.F("Les mots croisés en {0} ont besoin des définitions du dictionnaire {0}, qui n'est pas installé ou date d'une version plus ancienne.", NomLangue(code)),
                        () => { if (Definitions(code) != null) NouveauxMotsCroises(taille); });
                    return;
                }
            }
            tailleMC = taille;
            Afficher(Jeu.MotsCroises);
            partieMC = reprise ?? PartieMotsCroises.Generer(taille, defs, rnd);
            zoneMC.Partie = partieMC;
            var premier = partieMC.Mots.First();
            zoneMC.Ligne = premier.Ligne; zoneMC.Colonne = premier.Colonne; zoneMC.Horizontal = premier.Horizontal;
            if (ligne >= 0 && partieMC.Dans(ligne, colonne) && !partieMC.Noire(ligne, colonne))
            {
                zoneMC.Ligne = ligne; zoneMC.Colonne = colonne;
                zoneMC.Horizontal = partieMC.MotEn(ligne, colonne, horiz) != null ? horiz : !horiz;
            }
            debutGrille = DateTime.Now.AddSeconds(-secondes);
            RemplirDefs();
            MajEtatMC();
            zoneMC.Invalidate();
            zoneMC.Focus();
            int nbH = partieMC.Mots.Count(m => m.Horizontal), nbV = partieMC.Mots.Count - nbH;
            int dl = Bips.Nouveau();
            string intro = (reprise != null ? L.T("Partie reprise. ") : "")
                + L.F("Mots croisés {0}, {1} sur {1}. {2} mots : {3} horizontaux, {4} verticaux.", NiveauMC(taille), taille, partieMC.Mots.Count, nbH, nbV)
                + " " + L.T("Flèches pour vous déplacer, tapez les lettres. Entrée lit la définition, Espace change de sens, Tab va à la liste des définitions.")
                + " " + zoneMC.DecrireMot(zoneMC.MotCourant);
            PlusTard(dl, () => Parole.Dire(intro));
        }

        void RemplirDefs()
        {
            int sel = listeDefs.SelectedIndex;
            listeDefs.BeginUpdate();
            listeDefs.Items.Clear();
            foreach (var m in partieMC.Mots)
                listeDefs.Items.Add(m.Numero + " " + ZoneMotsCroises.NomSens(m.Horizontal) + " (" + m.Longueur + ") : " + m.Definition
                    + (partieMC.MotJuste(m) && clair ? " — " + L.T("trouvé") : ""));
            if (sel >= 0 && sel < listeDefs.Items.Count) listeDefs.SelectedIndex = sel;
            else if (listeDefs.Items.Count > 0) listeDefs.SelectedIndex = 0;
            listeDefs.EndUpdate();
        }

        void ToucheDefs(object sender, KeyEventArgs e)
        {
            if (partieMC == null || e.Control || e.Alt) return;
            int i = listeDefs.SelectedIndex;
            if (i < 0 || i >= partieMC.Mots.Count) return;
            var m = partieMC.Mots[i];
            if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; AllerAuMot(); }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                e.Handled = true; e.SuppressKeyPress = true;
                Parole.Dire(zoneMC.LettresDe(m));
            }
        }

        void AllerAuMot()
        {
            int i = listeDefs.SelectedIndex;
            if (partieMC == null || i < 0 || i >= partieMC.Mots.Count) return;
            var m = partieMC.Mots[i];
            zoneMC.Ligne = m.Ligne; zoneMC.Colonne = m.Colonne; zoneMC.Horizontal = m.Horizontal;
            zoneMC.Focus();
            zoneMC.Invalidate();
            PlusTard(200, () => Parole.Dire(ZoneMotsCroises.Coord(m.Ligne, m.Colonne) + ". " + zoneMC.DecrireMot(m)));
        }

        void MajEtatMC()
        {
            if (partieMC == null) return;
            int justes = partieMC.Mots.Count(partieMC.MotJuste);
            var rec = RecordGrille("motscroises" + tailleMC);
            etatMC.Text = L.F("Mots croisés {0}   —   {1} mots trouvés sur {2}", NiveauMC(tailleMC), justes, partieMC.Mots.Count)
                + (rec.MeilleurTemps >= 0 ? L.T("   —   record : ") + RecordsTaquin.Duree(rec.MeilleurTemps) : "");
        }

        void VerifierFinMC()
        {
            if (finie || partieMC == null) return;
            MajEtatMC();
            if (clair) RemplirDefs();
            if (!partieMC.Terminee) return;
            finie = true;
            int secondes = SecondesGrille;
            string bravo = L.F("Bravo ! Grille terminée en {0}.", RecordsTaquin.Duree(secondes))
                + Indices(partieMC.Indices);
            PlusTard(400, () =>
            {
                int v = Bips.Victoire();
                Parole.Dire(bravo);
                if (partieMC.Indices == 0) PlusTard(v + 2500, () => ProposerTempsGrille("motscroises" + tailleMC, secondes, bravo, zoneMC, etatMC));
                else etatMC.Text = bravo + L.T(" Ctrl+N pour rejouer.");
            });
        }

        string RecapMC()
        {
            if (finie) return L.T("Partie terminée. Ctrl+N pour rejouer.");
            int justes = partieMC.Mots.Count(partieMC.MotJuste);
            return L.F("Mots croisés {0} : {1} mots trouvés sur {2}, {3} cases remplies sur {4}. Temps écoulé : {5}.",
                NiveauMC(tailleMC), justes, partieMC.Mots.Count, partieMC.CasesRemplies, partieMC.CasesBlanches, RecordsTaquin.Duree(SecondesGrille));
        }

        // ======================= Sudoku =======================

        void NouveauSudoku(int niveau, PartieSudoku reprise = null, int secondes = 0, int ligne = -1, int colonne = 0)
        {
            niveauSudoku = niveau;
            Afficher(Jeu.Sudoku);
            partieSudoku = reprise ?? PartieSudoku.Generer(niveau, rnd);
            zoneSudoku.Partie = partieSudoku;
            // première case vide
            zoneSudoku.Ligne = 0; zoneSudoku.Colonne = 0;
            for (int k = 0; k < 81; k++) if (partieSudoku.Grille[k / 9, k % 9] == 0) { zoneSudoku.Ligne = k / 9; zoneSudoku.Colonne = k % 9; break; }
            if (ligne >= 0 && ligne < 9 && colonne >= 0 && colonne < 9) { zoneSudoku.Ligne = ligne; zoneSudoku.Colonne = colonne; }
            debutGrille = DateTime.Now.AddSeconds(-secondes);
            MajEtatSudoku();
            zoneSudoku.Invalidate();
            zoneSudoku.Focus();
            int dl = Bips.Nouveau();
            string intro = (reprise != null ? L.T("Partie reprise. ") : "")
                + L.F("Sudoku {0}, {1} cases à remplir.", PartieSudoku.NomNiveau(niveau), partieSudoku.Vides)
                + " " + L.T("Flèches pour vous déplacer, chiffres de 1 à 9 pour remplir. L, C et B lisent la ligne, la colonne et le bloc.")
                + " " + zoneSudoku.Position(zoneSudoku.Ligne, zoneSudoku.Colonne) + ".";
            PlusTard(dl, () => Parole.Dire(intro));
        }

        void MajEtatSudoku()
        {
            if (partieSudoku == null) return;
            var rec = RecordGrille("sudoku" + niveauSudoku);
            etatSudoku.Text = L.F("Sudoku {0}   —   {1} cases vides", PartieSudoku.NomNiveau(niveauSudoku), partieSudoku.Vides)
                + (rec.MeilleurTemps >= 0 ? L.T("   —   record : ") + RecordsTaquin.Duree(rec.MeilleurTemps) : "");
        }

        void VerifierFinSudoku()
        {
            if (finie || partieSudoku == null) return;
            MajEtatSudoku();
            if (!partieSudoku.Terminee) return;
            finie = true;
            int secondes = SecondesGrille;
            string bravo = L.F("Bravo ! Sudoku terminé en {0}.", RecordsTaquin.Duree(secondes))
                + Indices(partieSudoku.Indices);
            PlusTard(400, () =>
            {
                int v = Bips.Victoire();
                Parole.Dire(bravo);
                if (partieSudoku.Indices == 0) PlusTard(v + 2500, () => ProposerTempsGrille("sudoku" + niveauSudoku, secondes, bravo, zoneSudoku, etatSudoku));
                else etatSudoku.Text = bravo + L.T(" Ctrl+N pour rejouer.");
            });
        }

        string RecapSudoku()
        {
            if (finie) return L.T("Partie terminée. Ctrl+N pour rejouer.");
            return L.F("Sudoku {0} : {1} cases vides. Temps écoulé : {2}.", PartieSudoku.NomNiveau(niveauSudoku), partieSudoku.Vides, RecordsTaquin.Duree(SecondesGrille));
        }

        // ======================= Commun =======================

        static string Indices(int n) { return n == 0 ? "" : " " + (L.Pl(n) ? L.F("{0} indices utilisés.", n) : L.F("{0} indice utilisé.", n)); }

        /// <summary>Propose d'enregistrer le temps et annonce un éventuel record (seulement sans indice).</summary>
        void ProposerTempsGrille(string cle, int secondes, string bravo, Control zone, Label etatLabel)
        {
            var rec = RecordGrille(cle);
            var r = MessageBox.Show(this, bravo + L.T("\n\nVoulez-vous enregistrer votre temps ?"), "Misterword", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            string msg;
            if (r == DialogResult.Yes)
            {
                int ancien = rec.MeilleurTemps;
                rec.Enregistrees++;
                if (ancien < 0 || secondes < ancien)
                {
                    rec.MeilleurTemps = secondes;
                    msg = ancien < 0 ? L.T("Temps enregistré. C'est votre premier record : ") + RecordsTaquin.Duree(secondes) + "."
                                     : L.T("Nouveau record ! ") + RecordsTaquin.Duree(secondes) + L.T(", contre ") + RecordsTaquin.Duree(ancien) + L.T(" auparavant.");
                    Bips.Victoire();
                }
                else msg = L.T("Temps enregistré. Le record reste de ") + RecordsTaquin.Duree(ancien) + ".";
                rec.Sauver();
            }
            else msg = L.T("Temps non enregistré.");
            etatLabel.Text = bravo + L.T(" Ctrl+N pour rejouer.");
            zone.Focus();
            PlusTard(300, () => Parole.Dire(msg + L.T(" Ctrl+N pour rejouer.")));
        }

        void AbandonnerGrille()
        {
            finie = true;
            if (jeu == Jeu.MotsCroises)
            {
                for (int l = 0; l < partieMC.Taille; l++) for (int c = 0; c < partieMC.Taille; c++) if (!partieMC.Noire(l, c)) partieMC.Saisie[l, c] = partieMC.Solution[l, c];
                zoneMC.Invalidate();
                etatMC.Text = L.T("Partie abandonnée : la solution est affichée. Ctrl+N pour rejouer.");
            }
            else
            {
                Array.Copy(partieSudoku.Solution, partieSudoku.Grille, 81);
                zoneSudoku.Invalidate();
                etatSudoku.Text = L.T("Partie abandonnée : la solution est affichée. Ctrl+N pour rejouer.");
            }
            int d0 = Bips.Defaite();
            PlusTard(d0, () => Parole.Dire(L.T("Partie abandonnée : la solution est remplie dans la grille, vous pouvez la parcourir. Ctrl+N pour rejouer.")));
        }

        string DecrireRecordsGrilles()
        {
            var l = new List<string>();
            foreach (int t in new[] { 5, 7, 9, 11 })
            {
                var r = RecordGrille("motscroises" + t);
                if (r.MeilleurTemps >= 0) l.Add(L.T("mots croisés ") + NiveauMC(t) + " : " + RecordsTaquin.Duree(r.MeilleurTemps));
            }
            for (int n = 1; n <= 4; n++)
            {
                var r = RecordGrille("sudoku" + n);
                if (r.MeilleurTemps >= 0) l.Add(L.T("sudoku ") + PartieSudoku.NomNiveau(n) + " : " + RecordsTaquin.Duree(r.MeilleurTemps));
            }
            return l.Count == 0 ? L.T("Mots croisés et Sudoku : aucun temps enregistré.") : L.T("Records, ") + string.Join(" ; ", l) + ".";
        }

        void SauverGrille(List<string> l)
        {
            if (jeu == Jeu.MotsCroises)
            {
                l.Add("jeu=motscroises");
                l.Add("taille=" + partieMC.Taille);
                l.Add("secondes=" + SecondesGrille);
                l.Add("saisie=" + partieMC.SaisieTexte());
                l.Add("indices=" + partieMC.Indices);
                l.Add("ligne=" + zoneMC.Ligne);
                l.Add("colonne=" + zoneMC.Colonne);
                l.Add("horizontal=" + (zoneMC.Horizontal ? 1 : 0));
                foreach (var m in partieMC.Exporter()) l.Add("mot=" + m);
            }
            else
            {
                l.Add("jeu=sudoku");
                l.Add("niveau=" + niveauSudoku);
                l.Add("secondes=" + SecondesGrille);
                l.Add("indices=" + partieSudoku.Indices);
                l.Add("ligne=" + zoneSudoku.Ligne);
                l.Add("colonne=" + zoneSudoku.Colonne);
                l.Add("grille=" + partieSudoku.Exporter());
            }
        }

        void ReprendreGrille(string jeuSauve, Dictionary<string, string> d)
        {
            if (jeuSauve == "motscroises")
            {
                var mots = File.ReadAllLines(CheminSauvegarde).Where(x => x.StartsWith("mot=")).Select(x => x.Substring(4)).ToList();
                var p = PartieMotsCroises.Importer(Entier(d, "taille"), mots, d.ContainsKey("saisie") ? d["saisie"] : null);
                if (p.Mots.Count == 0) throw new Exception();
                p.Indices = Entier(d, "indices");
                NouveauxMotsCroises(p.Taille, p, Entier(d, "secondes"), d.ContainsKey("ligne") ? Entier(d, "ligne") : -1, Entier(d, "colonne"), Entier(d, "horizontal") == 1);
            }
            else
            {
                int n = Math.Max(1, Math.Min(4, Entier(d, "niveau")));
                var p = PartieSudoku.Importer(n, d["grille"]);
                p.Indices = Entier(d, "indices");
                NouveauSudoku(n, p, Entier(d, "secondes"), d.ContainsKey("ligne") ? Entier(d, "ligne") : -1, Entier(d, "colonne"));
            }
        }
    }
}
