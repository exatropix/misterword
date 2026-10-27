using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

namespace Misterword
{
    /// <summary>
    /// Fabrique les bips en mémoire (aucun fichier son nécessaire) et les joue d'un seul tenant.
    /// Aigu : bien placé. Moyen : mal placé. Grave : absent.
    /// </summary>
    static class Bips
    {
        public const int Aigu = 1047, Moyen = 659, Grave = 220;
        const int Freq = 22050;
        public static int DureeNote = 170, Silence = 70;
        static SoundPlayer lecteur;
        static MemoryStream flux;

        public static int FrequencePour(Marque m)
        {
            return m == Marque.BienPlacee ? Aigu : m == Marque.MalPlacee ? Moyen : Grave;
        }

        /// <summary>Joue une suite de notes (fréquence, durée en ms ; fréquence 0 = silence). Renvoie la durée totale en ms.</summary>
        public static int Jouer(IList<KeyValuePair<int, int>> notes)
        {
            var echantillons = new List<short>();
            int total = 0;
            foreach (var n in notes)
            {
                int nb = Freq * n.Value / 1000;
                int attaque = Math.Min(nb / 6, Freq / 200);
                for (int i = 0; i < nb; i++)
                {
                    double env = 1.0;
                    if (i < attaque) env = (double)i / attaque;
                    else if (i > nb - attaque * 2) env = Math.Max(0, (double)(nb - i) / (attaque * 2));
                    double v = n.Key <= 0 ? 0 : Math.Sin(2 * Math.PI * n.Key * i / Freq) * env * 0.45;
                    echantillons.Add((short)(v * short.MaxValue));
                }
                total += n.Value;
            }
            try
            {
                Arreter();
                flux = Wav(echantillons);
                lecteur = new SoundPlayer(flux);
                lecteur.Play();
            }
            catch { }
            return total;
        }

        public static int JouerMarques(IEnumerable<Marque> marques)
        {
            var notes = new List<KeyValuePair<int, int>>();
            foreach (var m in marques)
            {
                notes.Add(new KeyValuePair<int, int>(FrequencePour(m), DureeNote));
                notes.Add(new KeyValuePair<int, int>(0, Silence));
            }
            return Jouer(notes);
        }

        public static int Erreur()
        {
            return Jouer(new[] { N(180, 120), N(0, 40), N(150, 220) });
        }

        public static int Victoire()
        {
            return Jouer(new[] { N(523, 120), N(659, 120), N(784, 120), N(1047, 320) });
        }

        public static int Defaite()
        {
            return Jouer(new[] { N(392, 200), N(330, 200), N(262, 420) });
        }

        public static int Nouveau()
        {
            return Jouer(new[] { N(784, 90), N(0, 30), N(1047, 120) });
        }

        /// <summary>Son de glissement : un souffle bref par lettre déplacée.</summary>
        public static int Glissement(int lettres)
        {
            var e = new List<short>();
            var r = new Random(7);
            int nb = Freq * 130 / 1000, pause = Freq * 25 / 1000;
            for (int k = 0; k < Math.Max(1, lettres); k++)
            {
                double y = 0;
                for (int i = 0; i < nb; i++)
                {
                    double t = (double)i / nb;
                    double env = Math.Sin(Math.PI * t);                 // monte puis descend
                    double a = 0.06 + 0.25 * t;                        // filtre qui s'ouvre : le son « glisse »
                    y += a * ((r.NextDouble() * 2 - 1) - y);
                    e.Add((short)(y * env * 0.9 * short.MaxValue));
                }
                for (int i = 0; i < pause; i++) e.Add(0);
            }
            int max = Math.Max(1, e.Max(x => Math.Abs((int)x)));
            double g = 0.55 * short.MaxValue / max;
            for (int i = 0; i < e.Count; i++) e[i] = (short)(e[i] * g);
            try
            {
                Arreter();
                flux = Wav(e);
                lecteur = new SoundPlayer(flux);
                lecteur.Play();
            }
            catch { }
            return (int)((long)e.Count * 1000 / Freq);
        }

        public static void Arreter()
        {
            try { if (lecteur != null) lecteur.Stop(); } catch { }
        }

        static KeyValuePair<int, int> N(int f, int ms) { return new KeyValuePair<int, int>(f, ms); }

        static MemoryStream Wav(List<short> e)
        {
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            int data = e.Count * 2;
            w.Write("RIFF".ToCharArray()); w.Write(36 + data); w.Write("WAVE".ToCharArray());
            w.Write("fmt ".ToCharArray()); w.Write(16); w.Write((short)1); w.Write((short)1);
            w.Write(Freq); w.Write(Freq * 2); w.Write((short)2); w.Write((short)16);
            w.Write("data".ToCharArray()); w.Write(data);
            foreach (var s in e) w.Write(s);
            w.Flush();
            ms.Position = 0;
            return ms;
        }
    }
}
