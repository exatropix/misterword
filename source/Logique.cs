using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Misterword
{
    /// <summary>Résultat d'une lettre (ou d'une couleur) proposée.</summary>
    public enum Marque { Absente = 0, MalPlacee = 1, BienPlacee = 2 }

    public static class Correction
    {
        /// <summary>
        /// Règle commune à Motus et au Mastermind : on marque d'abord les éléments bien placés,
        /// puis, parmi ceux qui restent, les éléments présents ailleurs (sans compter deux fois le même).
        /// </summary>
        public static Marque[] Comparer<T>(IList<T> secret, IList<T> proposition)
        {
            int n = secret.Count;
            var res = new Marque[n];
            var restants = new Dictionary<T, int>();
            for (int i = 0; i < n; i++)
            {
                if (proposition[i].Equals(secret[i])) res[i] = Marque.BienPlacee;
                else
                {
                    int c; restants.TryGetValue(secret[i], out c); restants[secret[i]] = c + 1;
                }
            }
            for (int i = 0; i < n; i++)
            {
                if (res[i] == Marque.BienPlacee) continue;
                int c;
                if (restants.TryGetValue(proposition[i], out c) && c > 0)
                {
                    res[i] = Marque.MalPlacee;
                    restants[proposition[i]] = c - 1;
                }
            }
            return res;
        }
    }

    public static class Texte
    {
        /// <summary>Majuscules, sans accents, sans espaces ni tirets.</summary>
        public static string Normaliser(string s)
        {
            if (s == null) return "";
            // Le Ñ espagnol est une lettre à part entière : on le protège avant d'ôter les accents.
            string d = s.Trim().Replace('ñ', '\u0001').Replace('Ñ', '\u0001').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
            {
                if (c == '\u0001') { sb.Append('Ñ'); continue; }
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (c == 'œ' || c == 'Œ') { sb.Append("OE"); continue; }
                if (c == 'æ' || c == 'Æ') { sb.Append("AE"); continue; }
                char u = char.ToUpperInvariant(c);
                if (u >= 'A' && u <= 'Z') sb.Append(u);
            }
            return sb.ToString();
        }
    }

    // ===================== MISTERWORD (Motus) =====================

    public class Dictionnaire
    {
        readonly HashSet<string> mots = new HashSet<string>();
        readonly Dictionary<int, List<string>> secrets = new Dictionary<int, List<string>>();
        public const int LongueurMin = 5, LongueurMax = 10;

        public int Nombre { get { return mots.Count; } }

        public static Dictionnaire Charger(string fichierMots, string fichierSecrets)
        {
            var d = new Dictionnaire();
            foreach (var l in File.ReadLines(fichierMots))
            {
                string m = Texte.Normaliser(l);
                if (m.Length >= LongueurMin && m.Length <= LongueurMax) d.mots.Add(m);
            }
            // Mots à deviner : la liste dédiée si elle existe, sinon le dictionnaire sans les formes trop rares.
            IEnumerable<string> source;
            bool dedie = fichierSecrets != null && File.Exists(fichierSecrets);
            if (dedie) source = File.ReadLines(fichierSecrets).Select(Texte.Normaliser);
            else source = d.mots.Where(BonSecret);
            foreach (var m in source)
            {
                if (m.Length < LongueurMin || m.Length > LongueurMax) continue;
                if (dedie) d.mots.Add(m);
                List<string> l;
                if (!d.secrets.TryGetValue(m.Length, out l)) d.secrets[m.Length] = l = new List<string>();
                l.Add(m);
            }
            return d;
        }

        public static Dictionnaire DepuisListe(IEnumerable<string> liste)
        {
            var d = new Dictionnaire();
            foreach (var m in liste) d.mots.Add(m);
            foreach (var m in d.mots.Where(BonSecret))
            {
                List<string> l;
                if (!d.secrets.TryGetValue(m.Length, out l)) d.secrets[m.Length] = l = new List<string>();
                l.Add(m);
            }
            return d;
        }

        static readonly string[] finsEcartees = { "S", "Z", "X", "ENT", "AIT", "AIENT", "ERA", "IRA", "ERAI", "IRAI", "ASSE", "ISSE", "AMES", "ATES", "ASSENT", "IRENT", "ERENT", "IONS" };

        /// <summary>Écarte des mots à deviner les pluriels et la plupart des formes conjuguées.</summary>
        public static bool BonSecret(string m)
        {
            foreach (var f in finsEcartees) if (m.EndsWith(f)) return false;
            return true;
        }

        public bool Contient(string m) { return mots.Contains(m); }

        public string Tirer(int longueur, Random rnd)
        {
            List<string> l;
            if (!secrets.TryGetValue(longueur, out l) || l.Count == 0)
            {
                var tous = mots.Where(m => m.Length == longueur).ToList();
                if (tous.Count == 0) return null;
                return tous[rnd.Next(tous.Count)];
            }
            return l[rnd.Next(l.Count)];
        }
    }

    public class EssaiMot
    {
        public string Mot;
        public Marque[] Marques;
        public bool Gagne { get { return Marques.All(x => x == Marque.BienPlacee); } }
    }

    public enum Refus { Aucun, Longueur, PremiereLettre, Inconnu, DejaPropose, PartieFinie }

    public class PartieMot
    {
        public const int EssaisMax = 6;
        public readonly string Secret;
        public readonly List<EssaiMot> Essais = new List<EssaiMot>();
        readonly Dictionnaire dico;

        public PartieMot(string secret, Dictionnaire d) { Secret = secret; dico = d; }

        public int Longueur { get { return Secret.Length; } }
        public char PremiereLettre { get { return Secret[0]; } }
        public bool Gagnee { get { return Essais.Count > 0 && Essais[Essais.Count - 1].Gagne; } }
        public bool Finie { get { return Gagnee || Essais.Count >= EssaisMax; } }
        public int Restants { get { return EssaisMax - Essais.Count; } }

        public Refus Verifier(string mot)
        {
            if (Finie) return Refus.PartieFinie;
            if (mot.Length != Longueur) return Refus.Longueur;
            if (mot[0] != PremiereLettre) return Refus.PremiereLettre;
            if (Essais.Any(e => e.Mot == mot)) return Refus.DejaPropose;
            if (mot != Secret && !dico.Contient(mot)) return Refus.Inconnu;
            return Refus.Aucun;
        }

        public EssaiMot Proposer(string mot)
        {
            var e = new EssaiMot { Mot = mot, Marques = Correction.Comparer(Secret.ToCharArray(), mot.ToCharArray()) };
            Essais.Add(e);
            return e;
        }

        /// <summary>Lettres bien placées connues ; un point pour les cases inconnues. La première lettre est toujours donnée.</summary>
        public char[] Motif()
        {
            var m = new char[Longueur];
            for (int i = 0; i < Longueur; i++) m[i] = '.';
            m[0] = PremiereLettre;
            foreach (var e in Essais)
                for (int i = 0; i < Longueur; i++) if (e.Marques[i] == Marque.BienPlacee) m[i] = e.Mot[i];
            return m;
        }

        public IEnumerable<char> LettresMalPlacees()
        {
            var connues = new HashSet<char>(Motif());
            return Essais.SelectMany(e => e.Mot.Where((c, i) => e.Marques[i] == Marque.MalPlacee))
                         .Distinct().Where(c => !connues.Contains(c)).OrderBy(c => c);
        }

        public IEnumerable<char> LettresAbsentes()
        {
            var presentes = new HashSet<char>(Secret);
            return Essais.SelectMany(e => e.Mot).Where(c => !presentes.Contains(c)).Distinct().OrderBy(c => c);
        }

        public int Points()
        {
            if (!Gagnee) return 0;
            return Longueur * (EssaisMax + 1 - Essais.Count);
        }
    }

    // ===================== MASTERMIND =====================

    public class Couleur
    {
        public readonly char Lettre; public readonly string Nom; public readonly System.Drawing.Color Teinte;
        public Couleur(char l, string n, System.Drawing.Color t) { Lettre = l; Nom = n; Teinte = t; }
    }

    public class PartieCouleurs
    {
        static readonly Couleur[] PaletteFr =
        {
            new Couleur('R', "rouge",  System.Drawing.Color.Red),
            new Couleur('V', "vert",   System.Drawing.Color.LimeGreen),
            new Couleur('B', "bleu",   System.Drawing.Color.RoyalBlue),
            new Couleur('J', "jaune",  System.Drawing.Color.Gold),
            new Couleur('O', "orange", System.Drawing.Color.DarkOrange),
            new Couleur('N', "noir",   System.Drawing.Color.Black),
            new Couleur('M', "marron", System.Drawing.Color.SaddleBrown),
            new Couleur('G', "gris",   System.Drawing.Color.Gray),
        };
        // Chaque langue a ses couleurs, pour que chaque touche corresponde à l'initiale du nom.
        static readonly Couleur[] PaletteEn =
        {
            new Couleur('R', "red",    System.Drawing.Color.Red),
            new Couleur('G', "green",  System.Drawing.Color.LimeGreen),
            new Couleur('B', "blue",   System.Drawing.Color.RoyalBlue),
            new Couleur('Y', "yellow", System.Drawing.Color.Gold),
            new Couleur('O', "orange", System.Drawing.Color.DarkOrange),
            new Couleur('W', "white",  System.Drawing.Color.White),
            new Couleur('P', "purple", System.Drawing.Color.MediumPurple),
            new Couleur('C', "cyan",   System.Drawing.Color.Cyan),
        };
        static readonly Couleur[] PaletteEs =
        {
            new Couleur('R', "rojo",   System.Drawing.Color.Red),
            new Couleur('V', "verde",  System.Drawing.Color.LimeGreen),
            new Couleur('A', "azul",   System.Drawing.Color.RoyalBlue),
            new Couleur('N', "negro",  System.Drawing.Color.Black),
            new Couleur('B', "blanco", System.Drawing.Color.White),
            new Couleur('G', "gris",   System.Drawing.Color.Gray),
            new Couleur('M', "marrón", System.Drawing.Color.SaddleBrown),
            new Couleur('L', "lila",   System.Drawing.Color.MediumPurple),
        };
        public static Couleur[] Palette { get { return L.Code == "en" ? PaletteEn : L.Code == "es" ? PaletteEs : PaletteFr; } }

        public const int EssaisMax = 10;
        public readonly bool Avance;
        public readonly int Longueur, NbCouleurs;
        public readonly int[] Secret;
        public readonly List<int[]> Essais = new List<int[]>();
        public readonly List<Marque[]> Reponses = new List<Marque[]>();   // résultats triés : seulement les comptes
        public readonly List<Marque[]> Positions = new List<Marque[]>();  // résultat couleur par couleur, dans l'ordre

        public PartieCouleurs(bool avance, Random rnd, int[] secretImpose = null)
        {
            Avance = avance;
            Longueur = avance ? 5 : 4;
            NbCouleurs = avance ? 8 : 6;
            Secret = secretImpose ?? Enumerable.Range(0, Longueur).Select(_ => rnd.Next(NbCouleurs)).ToArray();
        }

        public IEnumerable<Couleur> Disponibles { get { return Palette.Take(NbCouleurs); } }

        /// <summary>Indice de la couleur pour une lettre tapée, ou -1.</summary>
        public int Indice(char lettre)
        {
            lettre = char.ToUpperInvariant(lettre);
            for (int i = 0; i < NbCouleurs; i++) if (Palette[i].Lettre == lettre) return i;
            return -1;
        }

        public bool Gagnee { get { return Reponses.Count > 0 && Reponses[Reponses.Count - 1].All(x => x == Marque.BienPlacee); } }
        public bool Finie { get { return Gagnee || Essais.Count >= EssaisMax; } }
        public int Restants { get { return EssaisMax - Essais.Count; } }

        public Marque[] Proposer(int[] code)
        {
            var r = Correction.Comparer(Secret, code);
            // Au Mastermind on ne dit pas quelle position est juste : on donne seulement les comptes, triés.
            var tri = r.OrderByDescending(x => (int)x).ToArray();
            Essais.Add((int[])code.Clone());
            Reponses.Add(tri);
            Positions.Add(r);
            return tri;
        }

        public static string Noms(IEnumerable<int> code)
        {
            return string.Join(", ", code.Select(i => Palette[i].Nom));
        }

        public int Points()
        {
            if (!Gagnee) return 0;
            return (EssaisMax + 1 - Essais.Count) * (Avance ? 2 : 1);
        }
    }
}
