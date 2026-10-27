using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    /// <summary>Zone de saisie des couleurs du Mastermind : elle parle en couleurs, jamais en lettres.</summary>
    class ZoneCode : Control
    {
        public int[] Cases = new int[4];   // -1 = vide
        public int Curseur;
        public Func<int, string> NomCouleur = i => "";
        public Func<char, int> Indice = c => -1;
        public int NbCouleurs = 6;
        public Action Valider;

        public ZoneCode()
        {
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = true;
            AccessibleName = L.T("Votre combinaison");
            AccessibleRole = AccessibleRole.Client;
            Vider(4);
        }

        public void Vider(int longueur)
        {
            Cases = Enumerable.Repeat(-1, longueur).ToArray();
            Curseur = 0;
            Invalidate();
        }

        public bool Complete { get { return Cases.All(c => c >= 0); } }

        string NomCase(int i) { return Cases[i] < 0 ? L.T("vide") : NomCouleur(Cases[i]); }

        public string Relire()
        {
            if (Cases.All(c => c < 0)) return L.T("Combinaison vide.");
            return string.Join(", ", Enumerable.Range(0, Cases.Length).Select(NomCase)) + ".";
        }

        protected override bool IsInputKey(Keys k)
        {
            Keys b = k & Keys.KeyCode;
            if ((k & (Keys.Control | Keys.Alt)) != 0) return false;
            return b != Keys.Tab && b != Keys.F6;
        }

        // Toutes les lettres tapées sont pour la zone : sinon Windows les prendrait pour des raccourcis (J ouvrirait le menu Jeu).
        protected override bool IsInputChar(char c) { return true; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Control || e.Alt) return;
            int n = Cases.Length;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (Curseur > 0) Curseur--; else Sons();
                    Parole.Dire(L.T("Case ") + (Curseur + 1) + ", " + NomCase(Curseur)); break;
                case Keys.Right:
                    if (Curseur < n - 1) Curseur++; else Sons();
                    Parole.Dire(L.T("Case ") + (Curseur + 1) + ", " + NomCase(Curseur)); break;
                case Keys.Home:
                    Curseur = 0; Parole.Dire(L.T("Case 1, ") + NomCase(0)); break;
                case Keys.End:
                    Curseur = n - 1; Parole.Dire(L.T("Case ") + n + ", " + NomCase(n - 1)); break;
                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    {
                        int c = Cases[Curseur];
                        if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Down) c = c < 0 ? 0 : (c + 1) % NbCouleurs;
                        else c = c < 0 ? NbCouleurs - 1 : (c + NbCouleurs - 1) % NbCouleurs;
                        Cases[Curseur] = c;
                        Parole.Dire(NomCouleur(c));
                        break;
                    }
                case Keys.Back:
                    {
                        // efface la case courante si c'est la dernière et qu'elle est remplie, sinon la précédente
                        int i = (Curseur == n - 1 && Cases[Curseur] >= 0) ? Curseur : Curseur - 1;
                        if (i < 0) { Sons(); Parole.Dire(L.T("Début de la combinaison")); break; }
                        string ancien = NomCase(i);
                        Cases[i] = -1; Curseur = i;
                        Parole.Dire(L.T("Effacé ") + ancien);
                        break;
                    }
                case Keys.Delete:
                    if (Cases[Curseur] >= 0) { string a = NomCase(Curseur); Cases[Curseur] = -1; Parole.Dire(L.T("Effacé ") + a); }
                    else Parole.Dire(L.T("Case ") + (Curseur + 1) + L.T(" déjà vide"));
                    break;
                case Keys.Escape:
                    Vider(n); Parole.Dire(L.T("Combinaison effacée")); break;
                case Keys.Space:
                    Parole.Dire(Relire()); break;
                case Keys.Enter:
                    if (Valider != null) Valider(); break;
                default:
                    return;
            }
            e.Handled = true; e.SuppressKeyPress = true;
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (char.IsControl(e.KeyChar) || e.KeyChar == ' ') { e.Handled = true; return; }
            e.Handled = true;
            int c = Indice(e.KeyChar);
            if (c < 0)
            {
                Sons();
                Parole.Dire(char.ToUpperInvariant(e.KeyChar) + L.T(" ne correspond à aucune couleur. F3 pour la liste des couleurs."));
                return;
            }
            Cases[Curseur] = c;
            string msg = NomCouleur(c);
            if (Curseur < Cases.Length - 1) Curseur++;
            else if (Complete) msg += L.T(". Combinaison complète, Entrée pour valider");
            Parole.Dire(msg);
            Invalidate();
        }

        void Sons() { System.Media.SystemSounds.Beep.Play(); }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        public Func<int, Color> Teinte = i => Color.White;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            int d = Math.Min(Height - 10, 44);
            for (int i = 0; i < Cases.Length; i++)
            {
                var r = new Rectangle(8 + i * (d + 12), (Height - d) / 2, d, d);
                using (var b = new SolidBrush(Cases[i] < 0 ? Color.FromArgb(60, 60, 60) : Teinte(Cases[i]))) g.FillEllipse(b, r);
                if (Focused && i == Curseur) using (var p = new Pen(Color.White, 3)) g.DrawEllipse(p, Rectangle.Inflate(r, 4, 4));
            }
        }
    }

}
