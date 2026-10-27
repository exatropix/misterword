using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Misterword
{
    /// <summary>Statistiques d'un jeu, sauvegardées dans AppData\Roaming\Misterword\stats.txt.</summary>
    public class StatJeu
    {
        public int Parties, Gagnees, EssaisGagnes, Serie, MeilleureSerie, Score;

        public void Enregistrer(bool gagne, int essais, int points)
        {
            Parties++;
            if (gagne)
            {
                Gagnees++; EssaisGagnes += essais; Serie++; Score += points;
                if (Serie > MeilleureSerie) MeilleureSerie = Serie;
            }
            else Serie = 0;
        }

        public string Decrire(string nom)
        {
            if (Parties == 0) return nom + L.T(" : aucune partie terminée pour l'instant.");
            string s = nom + " : " + Parties + (L.Pl(Parties) ? L.T(" parties jouées, ") : L.T(" partie jouée, "))
                + Gagnees + (L.Pl(Gagnees) ? L.T(" gagnées") : L.T(" gagnée")) + L.T(", soit ") + (100 * Gagnees / Parties) + L.T(" pour cent. ");
            if (Gagnees > 0)
                s += L.T("En moyenne ") + ((double)EssaisGagnes / Gagnees).ToString("0.#", CultureInfo.GetCultureInfo(L.Code == "en" ? "en-GB" : L.Code == "es" ? "es-ES" : "fr-FR")) + L.T(" essais par mot trouvé. ");
            s += L.T("Score total : ") + Score + L.T(" points. Série en cours : ") + Serie + L.T(". Meilleure série : ") + MeilleureSerie + ".";
            return s;
        }
    }

    public class Statistiques
    {
        public StatJeu Mots = new StatJeu(), Couleurs = new StatJeu();
        readonly string chemin;

        public Statistiques(string chemin) { this.chemin = chemin; }

        public static Statistiques Charger(string chemin)
        {
            var s = new Statistiques(chemin);
            try
            {
                if (!File.Exists(chemin)) return s;
                foreach (var l in File.ReadAllLines(chemin))
                {
                    var p = l.Split('=');
                    if (p.Length != 2) continue;
                    var k = p[0].Split('.');
                    if (k.Length != 2) continue;
                    int v; if (!int.TryParse(p[1], out v)) continue;
                    var j = k[0] == "mots" ? s.Mots : k[0] == "couleurs" ? s.Couleurs : null;
                    if (j == null) continue;
                    switch (k[1])
                    {
                        case "parties": j.Parties = v; break;
                        case "gagnees": j.Gagnees = v; break;
                        case "essais": j.EssaisGagnes = v; break;
                        case "serie": j.Serie = v; break;
                        case "meilleure": j.MeilleureSerie = v; break;
                        case "score": j.Score = v; break;
                    }
                }
            }
            catch { }
            return s;
        }

        public void Sauver()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(chemin));
                var l = new List<string>();
                foreach (var kv in new[] { new KeyValuePair<string, StatJeu>("mots", Mots), new KeyValuePair<string, StatJeu>("couleurs", Couleurs) })
                {
                    var j = kv.Value;
                    l.Add(kv.Key + ".parties=" + j.Parties);
                    l.Add(kv.Key + ".gagnees=" + j.Gagnees);
                    l.Add(kv.Key + ".essais=" + j.EssaisGagnes);
                    l.Add(kv.Key + ".serie=" + j.Serie);
                    l.Add(kv.Key + ".meilleure=" + j.MeilleureSerie);
                    l.Add(kv.Key + ".score=" + j.Score);
                }
                File.WriteAllLines(chemin, l);
            }
            catch { }
        }
    }
}
