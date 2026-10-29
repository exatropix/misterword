using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    /// <summary>Un mot placé dans la grille, avec sa définition.</summary>
    public class MotPlace
    {
        public string Mot, Definition;
        public int Ligne, Colonne;     // première lettre
        public bool Horizontal;
        public int Numero;
        public int Longueur { get { return Mot.Length; } }
        public int L(int i) { return Horizontal ? Ligne : Ligne + i; }
        public int C(int i) { return Horizontal ? Colonne + i : Colonne; }
        public bool Contient(int l, int c)
        {
            return Horizontal ? l == Ligne && c >= Colonne && c < Colonne + Longueur
                              : c == Colonne && l >= Ligne && l < Ligne + Longueur;
        }
    }

    /// <summary>
    /// Grille de mots croisés générée automatiquement : des mots qui se croisent dans un carré de N cases,
    /// les autres cases étant noires. Chaque mot vient du fichier de définitions.
    /// </summary>
    public class PartieMotsCroises
    {
        public readonly int Taille;
        public readonly char[,] Solution;     // '\0' = case noire
        public readonly char[,] Saisie;       // ' ' = vide
        public readonly List<MotPlace> Mots = new List<MotPlace>();
        public int Indices;                   // lettres révélées

        public PartieMotsCroises(int taille)
        {
            Taille = taille;
            Solution = new char[taille, taille];
            Saisie = new char[taille, taille];
            for (int l = 0; l < taille; l++) for (int c = 0; c < taille; c++) Saisie[l, c] = ' ';
        }

        public bool Noire(int l, int c) { return Solution[l, c] == '\0'; }
        public bool Dans(int l, int c) { return l >= 0 && c >= 0 && l < Taille && c < Taille; }

        public static Dictionary<string, string> ChargerDefinitions(string chemin)
        {
            var d = new Dictionary<string, string>();
            foreach (var ligne in File.ReadLines(chemin))
            {
                int i = ligne.IndexOf('|');
                if (i <= 0) continue;
                string mot = Texte.Normaliser(ligne.Substring(0, i));
                string def = ligne.Substring(i + 1).Trim();
                if (mot.Length >= 3 && def.Length > 0 && !d.ContainsKey(mot)) d[mot] = def;
            }
            return d;
        }

        /// <summary>Nombre de mots visé selon la taille de la grille.</summary>
        static int Cible(int taille) { return taille <= 5 ? 6 : taille <= 7 ? 10 : taille <= 9 ? 15 : 21; }

        /// <summary>Génère la meilleure grille parmi plusieurs essais.</summary>
        public static PartieMotsCroises Generer(int taille, Dictionary<string, string> defs, Random rnd)
        {
            var pool = defs.Keys.Where(m => m.Length <= taille).ToList();
            PartieMotsCroises meilleure = null;
            double meilleurScore = -1;
            for (int essai = 0; essai < 40; essai++)
            {
                var p = new PartieMotsCroises(taille);
                p.Remplir(pool, defs, rnd, Cible(taille));
                int lettres = 0;
                for (int l = 0; l < taille; l++) for (int c = 0; c < taille; c++) if (!p.Noire(l, c)) lettres++;
                double score = p.Mots.Count * 10 + lettres;
                if (score > meilleurScore) { meilleurScore = score; meilleure = p; }
                if (p.Mots.Count >= Cible(taille)) break;
            }
            meilleure.Numeroter();
            return meilleure;
        }

        void Remplir(List<string> pool, Dictionary<string, string> defs, Random rnd, int cible)
        {
            // Premier mot : long, horizontal, sur une ligne au hasard
            var longs = pool.Where(m => m.Length >= Taille - 2).ToList();
            if (longs.Count == 0) longs = pool;
            string premier = longs[rnd.Next(longs.Count)];
            int ligne = rnd.Next(Taille);
            int col = rnd.Next(Taille - premier.Length + 1);
            Placer(premier, defs[premier], ligne, col, true);

            var utilises = new HashSet<string> { premier };
            for (int tentative = 0; tentative < 1500 && Mots.Count < cible; tentative++)
            {
                string mot = pool[rnd.Next(pool.Count)];
                if (utilises.Contains(mot)) continue;
                int bestCroise = 0, bl = 0, bc = 0; bool bh = true;
                for (int h = 0; h < 2; h++)
                {
                    bool horiz = h == 0;
                    for (int l = 0; l < Taille; l++)
                        for (int c = 0; c < Taille; c++)
                        {
                            int croise = Evaluer(mot, l, c, horiz);
                            if (croise > bestCroise || (croise == bestCroise && croise > 0 && rnd.Next(3) == 0))
                            { bestCroise = croise; bl = l; bc = c; bh = horiz; }
                        }
                }
                if (bestCroise > 0)
                {
                    Placer(mot, defs[mot], bl, bc, bh);
                    utilises.Add(mot);
                }
            }
        }

        /// <summary>Nombre de croisements si le mot peut être placé ici sans former de mot parasite, sinon 0.</summary>
        int Evaluer(string mot, int l, int c, bool horiz)
        {
            int dl = horiz ? 0 : 1, dc = horiz ? 1 : 0;
            int fl = l + dl * (mot.Length - 1), fc = c + dc * (mot.Length - 1);
            if (!Dans(l, c) || !Dans(fl, fc)) return 0;
            // cases juste avant et juste après le mot : vides
            if (Dans(l - dl, c - dc) && !Noire(l - dl, c - dc)) return 0;
            if (Dans(fl + dl, fc + dc) && !Noire(fl + dl, fc + dc)) return 0;
            int croise = 0;
            for (int i = 0; i < mot.Length; i++)
            {
                int x = l + dl * i, y = c + dc * i;
                char s = Solution[x, y];
                if (s != '\0')
                {
                    if (s != mot[i]) return 0;
                    // la lettre existante ne doit pas déjà appartenir à un mot dans le même sens
                    if (Mots.Any(m => m.Horizontal == horiz && m.Contient(x, y))) return 0;
                    croise++;
                }
                else
                {
                    // nouvelle lettre : pas de voisin sur les côtés
                    int px = dc, py = dl;   // direction perpendiculaire
                    if (Dans(x + px, y + py) && !Noire(x + px, y + py)) return 0;
                    if (Dans(x - px, y - py) && !Noire(x - px, y - py)) return 0;
                }
            }
            if (croise == mot.Length) return 0;   // le mot existerait déjà en entier
            return croise;
        }

        void Placer(string mot, string def, int l, int c, bool horiz)
        {
            var m = new MotPlace { Mot = mot, Definition = def, Ligne = l, Colonne = c, Horizontal = horiz };
            for (int i = 0; i < mot.Length; i++) Solution[m.L(i), m.C(i)] = mot[i];
            Mots.Add(m);
        }

        /// <summary>Numérotation classique : de gauche à droite et de haut en bas ; deux mots qui commencent sur la même case partagent le numéro.</summary>
        public void Numeroter()
        {
            int n = 0;
            for (int l = 0; l < Taille; l++)
                for (int c = 0; c < Taille; c++)
                {
                    var ici = Mots.Where(m => m.Ligne == l && m.Colonne == c).ToList();
                    if (ici.Count == 0) continue;
                    n++;
                    foreach (var m in ici) m.Numero = n;
                }
            Mots.Sort((a, b) => a.Horizontal != b.Horizontal ? (a.Horizontal ? -1 : 1) : a.Numero.CompareTo(b.Numero));
        }

        public MotPlace MotEn(int l, int c, bool horiz)
        {
            return Mots.FirstOrDefault(m => m.Horizontal == horiz && m.Contient(l, c));
        }

        public string SaisieMot(MotPlace m)
        {
            var s = new char[m.Longueur];
            for (int i = 0; i < m.Longueur; i++) s[i] = Saisie[m.L(i), m.C(i)];
            return new string(s);
        }

        public bool MotJuste(MotPlace m) { return SaisieMot(m) == m.Mot; }

        public bool Terminee
        {
            get
            {
                for (int l = 0; l < Taille; l++)
                    for (int c = 0; c < Taille; c++)
                        if (!Noire(l, c) && Saisie[l, c] != Solution[l, c]) return false;
                return true;
            }
        }

        public int CasesBlanches { get { int n = 0; for (int l = 0; l < Taille; l++) for (int c = 0; c < Taille; c++) if (!Noire(l, c)) n++; return n; } }
        public int CasesRemplies { get { int n = 0; for (int l = 0; l < Taille; l++) for (int c = 0; c < Taille; c++) if (!Noire(l, c) && Saisie[l, c] != ' ') n++; return n; } }
        public int Erreurs { get { int n = 0; for (int l = 0; l < Taille; l++) for (int c = 0; c < Taille; c++) if (!Noire(l, c) && Saisie[l, c] != ' ' && Saisie[l, c] != Solution[l, c]) n++; return n; } }

        // ---- Sauvegarde : une ligne par mot « mot;ligne;colonne;H|V;définition » et la saisie
        public string[] Exporter()
        {
            var l = new List<string>();
            foreach (var m in Mots) l.Add(m.Mot + ";" + m.Ligne + ";" + m.Colonne + ";" + (m.Horizontal ? "H" : "V") + ";" + m.Definition.Replace(";", ","));
            return l.ToArray();
        }

        public string SaisieTexte()
        {
            var s = new System.Text.StringBuilder();
            for (int l = 0; l < Taille; l++) for (int c = 0; c < Taille; c++) s.Append(Saisie[l, c] == ' ' ? '.' : Saisie[l, c]);
            return s.ToString();
        }

        public static PartieMotsCroises Importer(int taille, IEnumerable<string> mots, string saisie)
        {
            var p = new PartieMotsCroises(taille);
            foreach (var ligne in mots)
            {
                var t = ligne.Split(new[] { ';' }, 5);
                if (t.Length < 5) continue;
                p.Placer(t[0], t[4], int.Parse(t[1]), int.Parse(t[2]), t[3] == "H");
            }
            if (saisie != null && saisie.Length == taille * taille)
                for (int i = 0; i < saisie.Length; i++) p.Saisie[i / taille, i % taille] = saisie[i] == '.' ? ' ' : saisie[i];
            p.Numeroter();
            return p;
        }
    }

    /// <summary>La grille de mots croisés au clavier.</summary>
    class ZoneMotsCroises : Control
    {
        public PartieMotsCroises Partie;
        public int Ligne, Colonne;
        public bool Horizontal = true;
        public Func<bool> Clair = () => false;
        public Action Rempli;            // appelé après chaque lettre : le jeu vérifie si la grille est finie
        public Action<string> Annoncer;  // pour les messages plus longs (définition)

        public ZoneMotsCroises()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = true;
            AccessibleName = L.T("Grille de mots croisés");
            AccessibleRole = AccessibleRole.Table;
            BackColor = Color.FromArgb(30, 30, 30);
        }

        protected override bool IsInputKey(Keys k)
        {
            Keys b = k & Keys.KeyCode;
            if ((k & Keys.Alt) != 0) return false;
            if ((k & Keys.Control) != 0) return b == Keys.E || b == Keys.G || b == Keys.I || b == Keys.L;
            return b != Keys.Tab && b != Keys.F6;
        }
        protected override bool IsInputChar(char c) { return true; }

        public static string Coord(int l, int c) { return ((char)('A' + l)).ToString() + (c + 1); }

        string NomCase(int l, int c)
        {
            if (Partie.Noire(l, c)) return L.T("case noire");
            char s = Partie.Saisie[l, c];
            return s == ' ' ? L.T("vide") : s.ToString();
        }

        public MotPlace MotCourant
        {
            get
            {
                if (Partie == null || Partie.Noire(Ligne, Colonne)) return null;
                return Partie.MotEn(Ligne, Colonne, Horizontal) ?? Partie.MotEn(Ligne, Colonne, !Horizontal);
            }
        }

        public static string NomSens(bool h) { return h ? L.T("horizontal") : L.T("vertical"); }

        public string LettresDe(MotPlace m)
        {
            return string.Join(", ", Partie.SaisieMot(m).Select(ch => ch == ' ' ? L.T("blanc") : ch.ToString()));
        }

        public string DecrireMot(MotPlace m)
        {
            if (m == null) return "";
            return m.Numero + " " + NomSens(m.Horizontal) + ", " + m.Longueur + L.T(" lettres : ") + m.Definition + ". " + LettresDe(m) + ".";
        }

        void AnnoncerCase(bool avecPosition)
        {
            string s = NomCase(Ligne, Colonne);
            if (avecPosition) s = Coord(Ligne, Colonne) + ", " + s;
            Parole.Dire(s);
        }

        void Bord() { System.Media.SystemSounds.Beep.Play(); }

        /// <summary>Après un déplacement vers un autre mot, on annonce le début de sa définition.</summary>
        MotPlace dernierMot;

        void Deplacer(int dl, int dc)
        {
            int l = Ligne + dl, c = Colonne + dc;
            if (!Partie.Dans(l, c)) { Bord(); AnnoncerCase(true); return; }
            Ligne = l; Colonne = c;
            if (dl != 0 && dc == 0) { if (Partie.MotEn(l, c, false) != null) Horizontal = false; }
            else if (dc != 0) { if (Partie.MotEn(l, c, true) != null) Horizontal = true; }
            var m = MotCourant;
            string s = Coord(l, c) + ", " + NomCase(l, c);
            if (m != null && m != dernierMot) s += ". " + m.Numero + " " + NomSens(m.Horizontal);
            dernierMot = m;
            Parole.Dire(s);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Partie == null || e.Alt) return;
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.E: VerifierMot(); break;
                    case Keys.G: VerifierGrille(); break;
                    case Keys.I: Indice(); break;
                    case Keys.L: { var m = MotCourant; Parole.Dire(m == null ? NomCase(Ligne, Colonne) : LettresDe(m)); break; }
                    default: return;
                }
                e.Handled = true; e.SuppressKeyPress = true; Invalidate(); return;
            }
            switch (e.KeyCode)
            {
                case Keys.Left: Deplacer(0, -1); break;
                case Keys.Right: Deplacer(0, 1); break;
                case Keys.Up: Deplacer(-1, 0); break;
                case Keys.Down: Deplacer(1, 0); break;
                case Keys.Home: { var m = MotCourant; if (m != null) { Ligne = m.Ligne; Colonne = m.Colonne; } AnnoncerCase(true); break; }
                case Keys.End: { var m = MotCourant; if (m != null) { Ligne = m.L(m.Longueur - 1); Colonne = m.C(m.Longueur - 1); } AnnoncerCase(true); break; }
                case Keys.Space:
                    {
                        if (!Partie.Noire(Ligne, Colonne) && Partie.MotEn(Ligne, Colonne, !Horizontal) != null) Horizontal = !Horizontal;
                        var m = MotCourant;
                        if (m != null) { Horizontal = m.Horizontal; Annoncer(DecrireMot(m)); }
                        else Parole.Dire(NomCase(Ligne, Colonne));
                        break;
                    }
                case Keys.Enter:
                    { var m = MotCourant; if (m != null) Annoncer(DecrireMot(m)); else Parole.Dire(NomCase(Ligne, Colonne)); break; }
                case Keys.Back:
                    {
                        if (!Partie.Noire(Ligne, Colonne) && Partie.Saisie[Ligne, Colonne] == ' ') Reculer();
                        if (!Partie.Noire(Ligne, Colonne)) { Partie.Saisie[Ligne, Colonne] = ' '; Parole.Dire(L.T("Effacé") + ", " + Coord(Ligne, Colonne)); }
                        break;
                    }
                case Keys.Delete:
                    if (!Partie.Noire(Ligne, Colonne)) { Partie.Saisie[Ligne, Colonne] = ' '; Parole.Dire(L.T("Effacé")); }
                    break;
                default: return;
            }
            e.Handled = true; e.SuppressKeyPress = true;
            Invalidate();
        }

        void Reculer()
        {
            int dl = Horizontal ? 0 : 1, dc = Horizontal ? 1 : 0;
            int l = Ligne - dl, c = Colonne - dc;
            if (Partie.Dans(l, c) && !Partie.Noire(l, c)) { Ligne = l; Colonne = c; }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
            if (Partie == null || char.IsControl(e.KeyChar) || e.KeyChar == ' ') return;
            string n = Texte.Normaliser(e.KeyChar.ToString());
            if (n.Length != 1) return;
            if (Partie.Noire(Ligne, Colonne)) { Bord(); Parole.Dire(L.T("case noire")); return; }
            Partie.Saisie[Ligne, Colonne] = n[0];
            string s = n;
            // avancer dans le sens du mot
            int dl = Horizontal ? 0 : 1, dc = Horizontal ? 1 : 0;
            int l = Ligne + dl, c = Colonne + dc;
            var m = Partie.MotEn(Ligne, Colonne, Horizontal);
            if (m != null && m.Contient(l, c)) { Ligne = l; Colonne = c; }
            else if (m != null && Partie.SaisieMot(m).IndexOf(' ') < 0) s += ". " + L.T("Mot complet");
            Parole.Dire(s);
            Invalidate();
            if (Rempli != null) Rempli();
        }

        void VerifierMot()
        {
            var m = MotCourant;
            if (m == null) { Parole.Dire(L.T("Placez-vous sur un mot.")); return; }
            var marques = new List<Marque>();
            string saisie = Partie.SaisieMot(m);
            for (int i = 0; i < m.Longueur; i++)
                marques.Add(saisie[i] == ' ' ? Marque.MalPlacee : saisie[i] == m.Mot[i] ? Marque.BienPlacee : Marque.Absente);
            Bips.JouerMarques(marques);
            int justes = marques.Count(x => x == Marque.BienPlacee), fausses = marques.Count(x => x == Marque.Absente), vides = marques.Count(x => x == Marque.MalPlacee);
            if (justes == m.Longueur) Parole.Dire(L.T("Mot juste."));
            else Parole.Dire(L.F("Lettres justes : {0}, fausses : {1}, vides : {2}.", justes, fausses, vides));
        }

        void VerifierGrille()
        {
            int err = Partie.Erreurs, rem = Partie.CasesRemplies, tot = Partie.CasesBlanches;
            if (err == 0) Parole.Dire(L.F("Aucune erreur. {0} cases remplies sur {1}.", rem, tot));
            else { Bips.Erreur(); Parole.Dire((L.Pl(err) ? L.F("{0} lettres fausses.", err) : L.F("{0} lettre fausse.", err)) + " " + L.F("{0} cases remplies sur {1}.", rem, tot)); }
        }

        void Indice()
        {
            if (Partie.Noire(Ligne, Colonne)) { Parole.Dire(L.T("case noire")); return; }
            char juste = Partie.Solution[Ligne, Colonne];
            string ou = "";
            if (Partie.Saisie[Ligne, Colonne] == juste)
            {
                // case déjà juste : on révèle la première case fausse ou vide du mot courant
                var m = MotCourant;
                int k = -1;
                if (m != null) for (int i = 0; i < m.Longueur; i++) if (Partie.Saisie[m.L(i), m.C(i)] != m.Mot[i]) { k = i; break; }
                if (k < 0) { Parole.Dire(m != null ? L.T("Ce mot est déjà juste.") : L.F("{0} est déjà juste.", juste)); return; }
                Ligne = m.L(k); Colonne = m.C(k);
                juste = Partie.Solution[Ligne, Colonne];
                ou = ", " + Coord(Ligne, Colonne);
            }
            Partie.Saisie[Ligne, Colonne] = juste;
            Partie.Indices++;
            Parole.Dire(L.F("Indice : {0}", juste) + ou + ".");
            Invalidate();
            if (Rempli != null) Rempli();
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);
            if (Partie == null) return;
            int n = Partie.Taille;
            int t = Math.Max(16, Math.Min(Width, Height) / n - 2);
            int x0 = (Width - n * t) / 2, y0 = (Height - n * t) / 2;
            var mc = MotCourant;
            using (var police = new Font("Segoe UI", t * 0.5f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var petite = new Font("Segoe UI", Math.Max(7, t * 0.22f), GraphicsUnit.Pixel))
            using (var surligne = new SolidBrush(Color.FromArgb(255, 245, 190)))
            using (var cadre = new Pen(Color.OrangeRed, 3))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int l = 0; l < n; l++)
                    for (int c = 0; c < n; c++)
                    {
                        var r = new Rectangle(x0 + c * t, y0 + l * t, t - 1, t - 1);
                        if (Partie.Noire(l, c)) { g.FillRectangle(Brushes.Black, r); continue; }
                        g.FillRectangle(mc != null && mc.Contient(l, c) ? surligne : Brushes.White, r);
                        var debut = Partie.Mots.FirstOrDefault(m => m.Ligne == l && m.Colonne == c);
                        if (debut != null) g.DrawString(debut.Numero.ToString(), petite, Brushes.Gray, r.X + 1, r.Y);
                        if (Partie.Saisie[l, c] != ' ') g.DrawString(Partie.Saisie[l, c].ToString(), police, Brushes.Black, r, fmt);
                        if (Focused && l == Ligne && c == Colonne) g.DrawRectangle(cadre, Rectangle.Inflate(r, -1, -1));
                    }
            }
        }
    }
}
