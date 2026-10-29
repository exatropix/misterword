using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Misterword
{
    /// <summary>Téléchargement des dictionnaires depuis le logiciel.</summary>
    partial class Fenetre
    {
        bool telechargementEnCours;

        /// <summary>Demande au joueur s'il veut télécharger le dictionnaire ; apres est appelé une fois installé.</summary>
        void ProposerTelechargement(string code, string raison, Action apres)
        {
            var r = MessageBox.Show(this, raison + "\n\n" + L.F("Télécharger maintenant le dictionnaire {0} depuis la page du jeu ? (connexion Internet nécessaire)", NomLangue(code)),
                "Misterword", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes) Telecharger(code, apres);
            else FocusPrincipal();
        }

        void Telecharger(string code, Action apres)
        {
            if (telechargementEnCours) { Parole.Dire(L.T("Un téléchargement est déjà en cours, patientez.")); return; }
            telechargementEnCours = true;
            Parole.Dire(L.F("Téléchargement du dictionnaire {0}…", NomLangue(code)));
            Telechargement.Lancer(code, dossierJeu, dossierDonnees,
                p => Invoquer(() => { if (p > 0 && p % 30 == 0) Parole.Dire(p + " %"); }),
                err => Invoquer(() =>
                {
                    telechargementEnCours = false;
                    dicosTrouves.Remove(code);
                    if (err != null)
                    {
                        Bips.Erreur();
                        MessageBox.Show(this, L.F("Le dictionnaire {0} n'a pas pu être téléchargé :\n", NomLangue(code)) + err
                            + "\n\n" + L.T("Vous pouvez aussi le télécharger sur la page du jeu, puis le décompresser dans le dossier du jeu."),
                            "Misterword", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FocusPrincipal();
                        return;
                    }
                    definitionsPar.Remove(code);
                    if (code == langueMots || dico == null) { langueMots = code; EcrireOptions(); ChargerDico(false); }
                    ReconstruireMenu();
                    Bips.Nouveau();
                    Parole.Dire(L.F("Dictionnaire {0} installé.", NomLangue(code)));
                    FocusPrincipal();
                    if (apres != null) PlusTard(1800, apres);
                }));
        }

        void Invoquer(Action a)
        {
            try { if (InvokeRequired) BeginInvoke(a); else a(); } catch { }
        }

        /// <summary>Dossier de secours pour un dictionnaire téléchargé sans droit d'écriture dans le dossier du jeu.</summary>
        string DossierDicoDonnees(string code) { return Path.Combine(dossierDonnees, "dictionnaires", code); }
    }
}
