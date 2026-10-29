using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    /// <summary>Sudoku 9 × 9 : génération d'une grille à solution unique, selon le niveau.</summary>
    public class PartieSudoku
    {
        public readonly int[,] Solution = new int[9, 9];
        public readonly int[,] Grille = new int[9, 9];    // 0 = vide
        public readonly bool[,] Donne = new bool[9, 9];
        public int Niveau;                                 // 1 facile … 4 expert
        public int Indices;

        public static string NomNiveau(int n) { return n == 1 ? L.T("facile") : n == 2 ? L.T("moyen") : n == 3 ? L.T("difficile") : L.T("expert"); }
        static int Indices_(int n) { return n == 1 ? 40 : n == 2 ? 33 : n == 3 ? 28 : 24; }   // chiffres donnés visés

        public static PartieSudoku Generer(int niveau, Random rnd)
        {
            var p = new PartieSudoku { Niveau = niveau };
            p.RemplirSolution(rnd);
            Array.Copy(p.Solution, p.Grille, 81);
            var cases = Enumerable.Range(0, 81).OrderBy(_ => rnd.Next()).ToList();
            int restants = 81, cible = Indices_(niveau);
            foreach (int k in cases)
            {
                if (restants <= cible) break;
                int l = k / 9, c = k % 9;
                int v = p.Grille[l, c];
                p.Grille[l, c] = 0;
                if (p.CompterSolutions(2) != 1) p.Grille[l, c] = v;   // la solution doit rester unique
                else restants--;
            }
            for (int l = 0; l < 9; l++) for (int c = 0; c < 9; c++) p.Donne[l, c] = p.Grille[l, c] != 0;
            return p;
        }

        void RemplirSolution(Random rnd)
        {
            var g = new int[9, 9];
            Resoudre(g, rnd, 0);
            Array.Copy(g, Solution, 81);
        }

        static bool Possible(int[,] g, int l, int c, int v)
        {
            for (int i = 0; i < 9; i++) if (g[l, i] == v || g[i, c] == v) return false;
            int bl = l / 3 * 3, bc = c / 3 * 3;
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) if (g[bl + i, bc + j] == v) return false;
            return true;
        }

        static bool Resoudre(int[,] g, Random rnd, int k)
        {
            if (k == 81) return true;
            int l = k / 9, c = k % 9;
            if (g[l, c] != 0) return Resoudre(g, rnd, k + 1);
            foreach (int v in Enumerable.Range(1, 9).OrderBy(_ => rnd.Next()))
            {
                if (!Possible(g, l, c, v)) continue;
                g[l, c] = v;
                if (Resoudre(g, rnd, k + 1)) return true;
            }
            g[l, c] = 0;
            return false;
        }

        /// <summary>Compte les solutions de la grille actuelle (on s'arrête à « limite »).</summary>
        int CompterSolutions(int limite)
        {
            var g = (int[,])Grille.Clone();
            int n = 0;
            Compter(g, ref n, limite);
            return n;
        }

        static void Compter(int[,] g, ref int n, int limite)
        {
            // case vide ayant le moins de possibilités
            int bl = -1, bc = -1, best = 10;
            for (int l = 0; l < 9; l++)
                for (int c = 0; c < 9; c++)
                {
                    if (g[l, c] != 0) continue;
                    int nb = 0;
                    for (int v = 1; v <= 9; v++) if (Possible(g, l, c, v)) nb++;
                    if (nb < best) { best = nb; bl = l; bc = c; }
                }
            if (bl < 0) { n++; return; }
            if (best == 0) return;
            for (int v = 1; v <= 9 && n < limite; v++)
            {
                if (!Possible(g, bl, bc, v)) continue;
                g[bl, bc] = v;
                Compter(g, ref n, limite);
                g[bl, bc] = 0;
            }
        }

        public bool Terminee
        {
            get { for (int l = 0; l < 9; l++) for (int c = 0; c < 9; c++) if (Grille[l, c] != Solution[l, c]) return false; return true; }
        }

        public int Vides { get { int n = 0; foreach (int v in Grille) if (v == 0) n++; return n; } }
        public int Erreurs { get { int n = 0; for (int l = 0; l < 9; l++) for (int c = 0; c < 9; c++) if (Grille[l, c] != 0 && Grille[l, c] != Solution[l, c]) n++; return n; } }

        /// <summary>Où le chiffre est déjà présent : « ligne », « colonne », « bloc » ; vide si nulle part.</summary>
        public List<string> Conflits(int l, int c, int v)
        {
            var r = new List<string>();
            for (int i = 0; i < 9; i++) if (i != c && Grille[l, i] == v) { r.Add(L.T("la ligne")); break; }
            for (int i = 0; i < 9; i++) if (i != l && Grille[i, c] == v) { r.Add(L.T("la colonne")); break; }
            int bl = l / 3 * 3, bc = c / 3 * 3;
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++)
                    if ((bl + i != l || bc + j != c) && Grille[bl + i, bc + j] == v) { if (!r.Contains(L.T("le bloc"))) r.Add(L.T("le bloc")); }
            return r;
        }

        public List<int> Candidats(int l, int c)
        {
            var r = new List<int>();
            int garde = Grille[l, c]; Grille[l, c] = 0;
            for (int v = 1; v <= 9; v++) if (Possible(Grille, l, c, v)) r.Add(v);
            Grille[l, c] = garde;
            return r;
        }

        // ---- Sauvegarde : 81 chiffres de solution, 81 de grille, 81 indicateurs « donné »
        public string Exporter()
        {
            var s = new System.Text.StringBuilder();
            foreach (int v in Solution) s.Append(v);
            s.Append(';');
            foreach (int v in Grille) s.Append(v);
            s.Append(';');
            foreach (bool d in Donne) s.Append(d ? '1' : '0');
            return s.ToString();
        }

        public static PartieSudoku Importer(int niveau, string texte)
        {
            var t = texte.Split(';');
            if (t.Length != 3 || t[0].Length != 81 || t[1].Length != 81 || t[2].Length != 81) throw new FormatException();
            var p = new PartieSudoku { Niveau = niveau };
            for (int i = 0; i < 81; i++)
            {
                p.Solution[i / 9, i % 9] = t[0][i] - '0';
                p.Grille[i / 9, i % 9] = t[1][i] - '0';
                p.Donne[i / 9, i % 9] = t[2][i] == '1';
            }
            return p;
        }
    }

    /// <summary>La grille de Sudoku au clavier.</summary>
    class ZoneSudoku : Control
    {
        public PartieSudoku Partie;
        public int Ligne, Colonne;
        public Action Rempli;

        public ZoneSudoku()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = true;
            AccessibleName = L.T("Grille de Sudoku");
            AccessibleRole = AccessibleRole.Table;
            BackColor = Color.FromArgb(235, 235, 225);
        }

        protected override bool IsInputKey(Keys k)
        {
            Keys b = k & Keys.KeyCode;
            if ((k & Keys.Alt) != 0) return false;
            if ((k & Keys.Control) != 0) return b == Keys.E || b == Keys.I;
            return b != Keys.Tab && b != Keys.F6;
        }
        protected override bool IsInputChar(char c) { return true; }

        string Valeur(int l, int c)
        {
            int v = Partie.Grille[l, c];
            if (v == 0) return L.T("vide");
            return v + (Partie.Donne[l, c] ? ", " + L.T("donné") : "");
        }

        public string Position(int l, int c) { return L.F("Ligne {0}, colonne {1}", l + 1, c + 1); }

        public string LireLigne(int l) { return L.F("Ligne {0} : ", l + 1) + string.Join(", ", Enumerable.Range(0, 9).Select(c => Court(l, c))) + "."; }
        public string LireColonne(int c) { return L.F("Colonne {0} : ", c + 1) + string.Join(", ", Enumerable.Range(0, 9).Select(l => Court(l, c))) + "."; }
        public string LireBloc(int l, int c)
        {
            int bl = l / 3 * 3, bc = c / 3 * 3;
            var v = new List<string>();
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) v.Add(Court(bl + i, bc + j));
            return L.F("Bloc {0} : ", bl + bc / 3 + 1) + string.Join(", ", v) + ".";
        }
        string Court(int l, int c) { int v = Partie.Grille[l, c]; return v == 0 ? L.T("vide") : v.ToString(); }

        void Bord() { System.Media.SystemSounds.Beep.Play(); }

        void Annoncer(bool changementBloc)
        {
            string s = Valeur(Ligne, Colonne) + ". " + Position(Ligne, Colonne);
            if (changementBloc) s += ", " + L.F("bloc {0}", Ligne / 3 * 3 + Colonne / 3 + 1);
            Parole.Dire(s);
        }

        void Aller(int l, int c)
        {
            if (l < 0 || l > 8 || c < 0 || c > 8) { Bord(); Annoncer(false); return; }
            bool bloc = l / 3 != Ligne / 3 || c / 3 != Colonne / 3;
            Ligne = l; Colonne = c;
            Annoncer(bloc);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Partie == null || e.Alt) return;
            if (e.Control)
            {
                if (e.KeyCode == Keys.E) Verifier();
                else if (e.KeyCode == Keys.I) Indice();
                else return;
                e.Handled = true; e.SuppressKeyPress = true; Invalidate(); return;
            }
            switch (e.KeyCode)
            {
                case Keys.Left: Aller(Ligne, Colonne - 1); break;
                case Keys.Right: Aller(Ligne, Colonne + 1); break;
                case Keys.Up: Aller(Ligne - 1, Colonne); break;
                case Keys.Down: Aller(Ligne + 1, Colonne); break;
                case Keys.Home: Aller(Ligne, 0); break;
                case Keys.End: Aller(Ligne, 8); break;
                case Keys.PageUp: Aller(0, Colonne); break;
                case Keys.PageDown: Aller(8, Colonne); break;
                case Keys.Back:
                case Keys.Delete:
                case Keys.D0:
                case Keys.NumPad0:
                    Effacer(); break;
                case Keys.L: Parole.Dire(LireLigne(Ligne)); break;
                case Keys.C: Parole.Dire(LireColonne(Colonne)); break;
                case Keys.B: Parole.Dire(LireBloc(Ligne, Colonne)); break;
                case Keys.P:
                    {
                        if (Partie.Grille[Ligne, Colonne] != 0 && Partie.Donne[Ligne, Colonne]) { Parole.Dire(L.T("Chiffre donné.")); break; }
                        var cand = Partie.Candidats(Ligne, Colonne);
                        Parole.Dire(cand.Count == 0 ? L.T("Aucun chiffre possible.") : L.T("Possibles : ") + string.Join(", ", cand) + ".");
                        break;
                    }
                case Keys.Space: Annoncer(true); break;
                default:
                    {
                        int v = -1;
                        if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9) v = e.KeyCode - Keys.D0;
                        else if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9) v = e.KeyCode - Keys.NumPad0;
                        if (v < 0) return;
                        Poser(v);
                        break;
                    }
            }
            e.Handled = true; e.SuppressKeyPress = true;
            Invalidate();
        }

        /// <summary>Sur un clavier AZERTY, les chiffres du haut s'obtiennent sans Maj : on accepte aussi &amp;, é, ", ', (, -, è, _, ç.</summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
            int i = "&é\"'(-è_ç".IndexOf(e.KeyChar);
            if (i >= 0 && !dejaPose) { Poser(i + 1); Invalidate(); }
            dejaPose = false;
        }
        bool dejaPose;

        void Poser(int v)
        {
            dejaPose = true;
            if (Partie.Donne[Ligne, Colonne]) { Bord(); Parole.Dire(L.T("Chiffre donné, il ne peut pas être changé.")); return; }
            Partie.Grille[Ligne, Colonne] = v;
            var conflits = Partie.Conflits(Ligne, Colonne, v);
            if (conflits.Count > 0)
            {
                Bips.Erreur();
                Parole.Dire(L.F("{0}, déjà dans {1}.", v, conflits.Count < 2 ? conflits[0] : string.Join(", ", conflits.Take(conflits.Count - 1)) + L.T(" et ") + conflits[conflits.Count - 1]));
            }
            else Parole.Dire(v.ToString());
            if (Rempli != null) Rempli();
        }

        void Effacer()
        {
            if (Partie.Donne[Ligne, Colonne]) { Bord(); Parole.Dire(L.T("Chiffre donné, il ne peut pas être changé.")); return; }
            Partie.Grille[Ligne, Colonne] = 0;
            Parole.Dire(L.T("Effacé"));
        }

        void Verifier()
        {
            int err = Partie.Erreurs, vides = Partie.Vides;
            if (err == 0) Parole.Dire(L.F("Aucune erreur. {0} cases vides.", vides));
            else { Bips.Erreur(); Parole.Dire((L.Pl(err) ? L.F("{0} chiffres faux.", err) : L.F("{0} chiffre faux.", err)) + " " + L.F("{0} cases vides.", vides)); }
        }

        void Indice()
        {
            if (Partie.Donne[Ligne, Colonne] || Partie.Grille[Ligne, Colonne] == Partie.Solution[Ligne, Colonne])
            {
                // case déjà bonne : on cherche la première case vide ou fausse
                for (int k = 0; k < 81; k++)
                {
                    int l = k / 9, c = k % 9;
                    if (Partie.Grille[l, c] != Partie.Solution[l, c]) { Ligne = l; Colonne = c; break; }
                }
            }
            if (Partie.Grille[Ligne, Colonne] == Partie.Solution[Ligne, Colonne]) { Parole.Dire(L.T("La grille est déjà juste.")); return; }
            Partie.Grille[Ligne, Colonne] = Partie.Solution[Ligne, Colonne];
            Partie.Indices++;
            Parole.Dire(L.F("Indice : {0}. {1}", Partie.Solution[Ligne, Colonne], Position(Ligne, Colonne)));
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
            int t = Math.Max(18, Math.Min(Width, Height) / 9 - 2);
            int x0 = (Width - 9 * t) / 2, y0 = (Height - 9 * t) / 2;
            using (var police = new Font("Segoe UI", t * 0.55f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var normal = new Font("Segoe UI", t * 0.55f, GraphicsUnit.Pixel))
            using (var bleu = new SolidBrush(Color.FromArgb(30, 70, 170)))
            using (var fin = new Pen(Color.Gray, 1))
            using (var epais = new Pen(Color.Black, 3))
            using (var cadre = new Pen(Color.OrangeRed, 3))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int l = 0; l < 9; l++)
                    for (int c = 0; c < 9; c++)
                    {
                        var r = new Rectangle(x0 + c * t, y0 + l * t, t, t);
                        g.FillRectangle(Brushes.White, r);
                        g.DrawRectangle(fin, r);
                        int v = Partie.Grille[l, c];
                        if (v != 0) g.DrawString(v.ToString(), Partie.Donne[l, c] ? police : normal, Partie.Donne[l, c] ? Brushes.Black : bleu, r, fmt);
                        if (Focused && l == Ligne && c == Colonne) g.DrawRectangle(cadre, Rectangle.Inflate(r, -2, -2));
                    }
                for (int i = 0; i <= 3; i++)
                {
                    g.DrawLine(epais, x0 + i * 3 * t, y0, x0 + i * 3 * t, y0 + 9 * t);
                    g.DrawLine(epais, x0, y0 + i * 3 * t, x0 + 9 * t, y0 + i * 3 * t);
                }
            }
        }
    }
}
