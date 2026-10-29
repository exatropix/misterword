using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace Misterword
{
    /// <summary>
    /// Télécharge un dictionnaire (Misterword-dictionnaire-xx.zip) depuis la dernière version publiée sur GitHub
    /// et le décompresse dans le dossier du jeu (ou, à défaut de droits d'écriture, dans le dossier des données).
    /// </summary>
    static class Telechargement
    {
        public static string Adresse(string code)
        {
            return "https://github.com/" + MiseAJour.Depot + "/releases/latest/download/Misterword-dictionnaire-" + code + ".zip";
        }

        /// <summary>
        /// Lance le téléchargement en arrière-plan. progression(pourcent) est appelé à chaque palier de 10 %,
        /// fin(erreur) à la fin : erreur vaut null si tout s'est bien passé. Les rappels arrivent sur le fil de l'interface.
        /// </summary>
        public static void Lancer(string code, string dossierJeu, string dossierDonnees, Action<int> progression, Action<string> fin)
        {
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; } catch { }
            string tmp = Path.Combine(Path.GetTempPath(), "Misterword-dictionnaire-" + code + ".zip");
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "Misterword";
            int palier = -1;
            wc.DownloadProgressChanged += (s, e) =>
            {
                int p = e.ProgressPercentage / 10 * 10;
                if (p > palier && p < 100) { palier = p; if (progression != null) progression(p); }
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                wc.Dispose();
                string err = null;
                if (e.Cancelled) err = "annulé";
                else if (e.Error != null) err = e.Error.Message;
                else
                {
                    try { Installer(tmp, code, dossierJeu, dossierDonnees); }
                    catch (Exception ex) { err = ex.Message; }
                }
                try { File.Delete(tmp); } catch { }
                fin(err);
            };
            try { wc.DownloadFileAsync(new Uri(Adresse(code)), tmp); }
            catch (Exception ex) { fin(ex.Message); }
        }

        static void Installer(string zip, string code, string dossierJeu, string dossierDonnees)
        {
            try { Extraire(zip, code, dossierJeu); }
            catch (UnauthorizedAccessException) { Extraire(zip, code, dossierDonnees); }
        }

        /// <summary>Ne garde que les fichiers du dossier dictionnaires\code du zip, quel que soit le niveau où il se trouve.</summary>
        static void Extraire(string zip, string code, string racine)
        {
            string cible = Path.Combine(racine, "dictionnaires", code);
            int n = 0;
            using (var z = ZipFile.OpenRead(zip))
            {
                foreach (var e in z.Entries)
                {
                    if (string.IsNullOrEmpty(e.Name)) continue;
                    string chemin = e.FullName.Replace('\\', '/');
                    string marque = "dictionnaires/" + code + "/";
                    int i = chemin.IndexOf(marque, StringComparison.OrdinalIgnoreCase);
                    string rel = i >= 0 ? chemin.Substring(i + marque.Length) : e.Name;
                    if (rel.Contains("..")) continue;
                    string dest = Path.Combine(cible, rel.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    e.ExtractToFile(dest, true);
                    n++;
                }
            }
            if (!File.Exists(Path.Combine(cible, "mots.txt"))) throw new InvalidDataException(L.T("le fichier téléchargé ne contient pas de dictionnaire."));
        }
    }
}
