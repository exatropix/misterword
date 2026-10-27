using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    /// <summary>Le jeu de taquin : une grille de 3, 4 ou 5 cases de côté, remplie de lettres dans l'ordre alphabétique, avec une case vide.</summary>
    public class PartieTaquin
    {
        public readonly int Taille;
        public readonly char[,] Grille;   // ' ' = case vide
        public int LigneVide, ColonneVide;
        public int Deplacements;

        public PartieTaquin(int taille = 4) { Taille = taille; Grille = new char[taille, taille]; Ranger(); }

        /// <summary>Dernière lettre du jeu : H en 3 sur 3, O en 4 sur 4, X en 5 sur 5.</summary>
        public char DerniereLettre { get { return (char)('A' + Taille * Taille - 2); } }

        public char Attendue(int l, int c)
        {
            int i = l * Taille + c;
            return i == Taille * Taille - 1 ? ' ' : (char)('A' + i);
        }

        void Ranger()
        {
            for (int l = 0; l < Taille; l++)
                for (int c = 0; c < Taille; c++) Grille[l, c] = Attendue(l, c);
            LigneVide = ColonneVide = Taille - 1;
        }

        /// <summary>Mélange par des coups légaux à partir de la position rangée : la grille obtenue a toujours une solution.</summary>
        public void Melanger(Random rnd, int coups = 300)
        {
            Ranger();
            int dl0 = 0, dc0 = 0;
            for (int k = 0; k < coups || EstRangee(); k++)
            {
                var possibles = new List<int[]>();
                foreach (var d in new[] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } })
                {
                    int l = LigneVide + d[0], c = ColonneVide + d[1];
                    if (l < 0 || l >= Taille || c < 0 || c >= Taille) continue;
                    if (d[0] == -dl0 && d[1] == -dc0) continue;   // pas de retour immédiat
                    possibles.Add(d);
                }
                var ch = possibles[rnd.Next(possibles.Count)];
                int nl = LigneVide + ch[0], nc = ColonneVide + ch[1];
                Grille[LigneVide, ColonneVide] = Grille[nl, nc];
                Grille[nl, nc] = ' ';
                LigneVide = nl; ColonneVide = nc;
                dl0 = ch[0]; dc0 = ch[1];
            }
            Deplacements = 0;
        }

        /// <summary>La grille ligne par ligne, une case vide notée par une espace.</summary>
        public string Texte()
        {
            var sb = new System.Text.StringBuilder();
            for (int l = 0; l < Taille; l++) for (int c = 0; c < Taille; c++) sb.Append(Grille[l, c]);
            return sb.ToString();
        }

        /// <summary>Remet une grille sauvegardée. Refuse une grille incomplète ou invalide.</summary>
        public bool Reprendre(string texte, int deplacements)
        {
            if (texte == null || texte.Length != Taille * Taille) return false;
            var attendues = Enumerable.Range(0, Taille * Taille).Select(i => i == Taille * Taille - 1 ? ' ' : (char)('A' + i)).OrderBy(x => x);
            if (!texte.OrderBy(x => x).SequenceEqual(attendues)) return false;
            for (int i = 0; i < texte.Length; i++)
            {
                Grille[i / Taille, i % Taille] = texte[i];
                if (texte[i] == ' ') { LigneVide = i / Taille; ColonneVide = i % Taille; }
            }
            Deplacements = deplacements;
            return true;
        }

        public bool EstRangee()
        {
            for (int l = 0; l < Taille; l++)
                for (int c = 0; c < Taille; c++) if (Grille[l, c] != Attendue(l, c)) return false;
            return true;
        }

        /// <summary>
        /// Fait glisser la lettre (l, c) vers la case vide, avec toutes celles qui la séparent de la case vide.
        /// Renvoie les lettres déplacées (dans l'ordre où elles bougent) et le sens, ou null si la case n'est pas alignée avec la case vide.
        /// </summary>
        public List<char> Glisser(int l, int c, out string sens)
        {
            sens = null;
            if (l == LigneVide && c == ColonneVide) return null;
            var bougees = new List<char>();
            if (l == LigneVide)
            {
                int pas = c < ColonneVide ? 1 : -1;          // sens du déplacement des lettres
                sens = pas > 0 ? L.T("vers la droite") : L.T("vers la gauche");
                for (int k = ColonneVide; k != c; k -= pas)
                {
                    Grille[l, k] = Grille[l, k - pas];
                    bougees.Add(Grille[l, k]);
                }
                Grille[l, c] = ' ';
            }
            else if (c == ColonneVide)
            {
                int pas = l < LigneVide ? 1 : -1;
                sens = pas > 0 ? L.T("vers le bas") : L.T("vers le haut");
                for (int k = LigneVide; k != l; k -= pas)
                {
                    Grille[k, c] = Grille[k - pas, c];
                    bougees.Add(Grille[k, c]);
                }
                Grille[l, c] = ' ';
            }
            else return null;
            LigneVide = l; ColonneVide = c;
            Deplacements++;
            return bougees;
        }
    }

    /// <summary>Records du taquin : meilleur temps et nombre de parties enregistrées.</summary>
    public class RecordsTaquin
    {
        public int MeilleurTemps = -1;      // en secondes, -1 = aucun
        public int DeplacementsDuRecord;
        public int MoinsDeDeplacements = -1;
        public int Enregistrees;
        readonly string chemin;

        RecordsTaquin(string chemin) { this.chemin = chemin; }

        public static RecordsTaquin Charger(string chemin)
        {
            var r = new RecordsTaquin(chemin);
            try
            {
                if (File.Exists(chemin))
                    foreach (var l in File.ReadAllLines(chemin))
                    {
                        var p = l.Split('='); int v;
                        if (p.Length != 2 || !int.TryParse(p[1], out v)) continue;
                        if (p[0] == "temps") r.MeilleurTemps = v;
                        else if (p[0] == "deplacementsrecord") r.DeplacementsDuRecord = v;
                        else if (p[0] == "moinsdeplacements") r.MoinsDeDeplacements = v;
                        else if (p[0] == "enregistrees") r.Enregistrees = v;
                    }
            }
            catch { }
            return r;
        }

        public void Sauver()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(chemin));
                File.WriteAllLines(chemin, new[] { "temps=" + MeilleurTemps, "deplacementsrecord=" + DeplacementsDuRecord, "moinsdeplacements=" + MoinsDeDeplacements, "enregistrees=" + Enregistrees });
            }
            catch { }
        }

        public static string Duree(int s)
        {
            int m = s / 60, r = s % 60;
            string t = "";
            if (m > 0) t = m + (L.Pl(m) ? L.T(" minutes") : L.T(" minute"));
            if (r > 0 || m == 0) t += (t.Length > 0 ? " " : "") + r + (L.Pl(r) ? L.T(" secondes") : L.T(" seconde"));
            return t;
        }

        public string Decrire()
        {
            if (MeilleurTemps < 0) return L.T("Taquin : aucun temps enregistré.");
            return L.T("Taquin : record ") + Duree(MeilleurTemps) + L.T(", en ") + DeplacementsDuRecord + L.T(" déplacements. Le moins de déplacements : ") + MoinsDeDeplacements + ". " + Enregistrees + (L.Pl(Enregistrees) ? L.T(" parties enregistrées.") : L.T(" partie enregistrée."));
        }
    }

    /// <summary>La grille du taquin, au clavier. Elle parle par le lecteur d'écran et se dessine pour une personne voyante.</summary>
    class ZoneTaquin : Control
    {
        public PartieTaquin Partie;
        public int Ligne, Colonne;
        public Action<List<char>, string> ApresGlissement;   // lettres déplacées, sens

        public ZoneTaquin()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = true;
            AccessibleName = L.T("Taquin");
            AccessibleRole = AccessibleRole.Table;
            BackColor = Color.FromArgb(60, 40, 25);
        }

        protected override bool IsInputKey(Keys k)
        {
            if ((k & (Keys.Control | Keys.Alt)) != 0) return false;
            Keys b = k & Keys.KeyCode;
            return b != Keys.Tab && b != Keys.F6;
        }
        protected override bool IsInputChar(char c) { return true; }

        static string NomCase(char ch) { return ch == ' ' ? L.T("vide") : ch.ToString(); }

        public string DecrireCase(int l, int c, bool avecPosition)
        {
            string s = NomCase(Partie.Grille[l, c]);
            if (avecPosition) s += L.T(", ligne ") + (l + 1) + L.T(", colonne ") + (c + 1);
            return s;
        }

        public string LireLigne(int l)
        {
            return L.T("Ligne ") + (l + 1) + " : " + string.Join(", ", Enumerable.Range(0, Partie.Taille).Select(c => NomCase(Partie.Grille[l, c])));
        }

        public string LireColonne(int c)
        {
            return L.T("Colonne ") + (c + 1) + " : " + string.Join(", ", Enumerable.Range(0, Partie.Taille).Select(l => NomCase(Partie.Grille[l, c])));
        }

        public string LireTout()
        {
            return string.Join(". ", Enumerable.Range(0, Partie.Taille).Select(LireLigne)) + ".";
        }

        void Annoncer()
        {
            Parole.Dire(DecrireCase(Ligne, Colonne, false));
        }

        void Bord() { System.Media.SystemSounds.Beep.Play(); }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Partie == null || e.Control || e.Alt) return;
            int n = Partie.Taille;
            switch (e.KeyCode)
            {
                case Keys.Left: if (Colonne > 0) { Colonne--; Annoncer(); } else { Bord(); Annoncer(); } break;
                case Keys.Right: if (Colonne < n - 1) { Colonne++; Annoncer(); } else { Bord(); Annoncer(); } break;
                case Keys.Up: if (Ligne > 0) { Ligne--; Annoncer(); } else { Bord(); Annoncer(); } break;
                case Keys.Down: if (Ligne < n - 1) { Ligne++; Annoncer(); } else { Bord(); Annoncer(); } break;
                case Keys.Home: Colonne = 0; Annoncer(); break;
                case Keys.End: Colonne = n - 1; Annoncer(); break;
                case Keys.PageUp: Ligne = 0; Annoncer(); break;
                case Keys.PageDown: Ligne = n - 1; Annoncer(); break;
                case Keys.Enter:
                case Keys.Space:
                    Deplacer(); break;
                case Keys.L: Parole.Dire(LireLigne(Ligne)); break;
                case Keys.C: Parole.Dire(LireColonne(Colonne)); break;
                case Keys.T: Parole.Dire(LireTout()); break;
                case Keys.P: Parole.Dire(DecrireCase(Ligne, Colonne, true)); break;
                case Keys.V:
                    Ligne = Partie.LigneVide; Colonne = Partie.ColonneVide;
                    Parole.Dire(L.T("Case vide, ligne ") + (Ligne + 1) + L.T(", colonne ") + (Colonne + 1));
                    break;
                default: return;
            }
            e.Handled = true; e.SuppressKeyPress = true;
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e) { e.Handled = true; }

        void Deplacer()
        {
            if (Partie.Grille[Ligne, Colonne] == ' ') { Bips.Erreur(); Parole.Dire(L.T("C'est la case vide. Choisissez une lettre sur sa ligne ou sa colonne.")); return; }
            string sens;
            var bougees = Partie.Glisser(Ligne, Colonne, out sens);
            if (bougees == null)
            {
                Bips.Erreur();
                Parole.Dire(Partie.Grille[Ligne, Colonne] + L.T(" n'est ni sur la ligne ni sur la colonne de la case vide. V pour aller à la case vide."));
                return;
            }
            // le curseur suit la lettre choisie : après le glissement, la lettre choisie est à côté de son ancienne case, dans le sens du mouvement
            if (sens == L.T("vers la droite")) Colonne++;
            else if (sens == L.T("vers la gauche")) Colonne--;
            else if (sens == L.T("vers le bas")) Ligne++;
            else if (sens == L.T("vers le haut")) Ligne--;
            Invalidate();
            if (ApresGlissement != null) ApresGlissement(bougees, sens);
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            if (Partie == null) return;
            int n = Partie.Taille;
            int t = Math.Max(20, Math.Min(Width, Height) / n - 10);
            int x0 = (Width - n * (t + 8)) / 2, y0 = (Height - n * (t + 8)) / 2;
            using (var police = new Font("Segoe UI", t * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var bois = new SolidBrush(Color.FromArgb(230, 200, 140)))
            using (var cadre = new Pen(Color.White, 4))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int l = 0; l < n; l++)
                    for (int c = 0; c < n; c++)
                    {
                        var r = new Rectangle(x0 + c * (t + 8), y0 + l * (t + 8), t, t);
                        char ch = Partie.Grille[l, c];
                        if (ch != ' ')
                        {
                            g.FillRectangle(bois, r);
                            g.DrawString(ch.ToString(), police, Brushes.SaddleBrown, r, fmt);
                        }
                        if (Focused && l == Ligne && c == Colonne) g.DrawRectangle(cadre, Rectangle.Inflate(r, 3, 3));
                    }
            }
        }
    }
}
