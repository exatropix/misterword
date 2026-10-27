# Misterword

![Téléchargements](https://img.shields.io/github/downloads/exatropix/misterword/total?label=T%C3%A9l%C3%A9chargements)

**Trois jeux de lettres et de logique, entièrement au clavier, accessibles avec NVDA et JAWS. Pour Windows 10 et 11. En français, en anglais et en espagnol.**

Par Exatropic.

## Les jeux

- **Misterword**, dans le style de Motus : trouvez un mot de 5 à 10 lettres en 6 essais. La première lettre est donnée. Après chaque essai, un bip par lettre : aigu si elle est bien placée, moyen si elle est mal placée, grave si elle est absente.
- **Mistermind**, le jeu des couleurs : retrouvez une combinaison de 4 couleurs parmi 6 (facile) ou de 5 parmi 8 (expert), en 10 essais.
- **Le Taquin** : remettez les lettres dans l'ordre en les faisant glisser vers la case vide. Trois niveaux, de 3 sur 3 à 5 sur 5, avec chronomètre et records.

Tout passe par votre lecteur d'écran, avec votre voix habituelle. Les propositions restent dans une liste que l'on relit à tout moment, lettre par lettre ou couleur par couleur. On peut sauvegarder une partie et la reprendre plus tard. Le jeu vérifie seul s'il existe une nouvelle version.

## Télécharger

1. Le jeu : [Misterword.zip](https://github.com/exatropix/misterword/releases/latest/download/Misterword.zip)
2. Un dictionnaire au moins, pour Misterword :
   [français](https://github.com/exatropix/misterword/releases/latest/download/Misterword-dictionnaire-fr.zip),
   [anglais](https://github.com/exatropix/misterword/releases/latest/download/Misterword-dictionnaire-en.zip),
   [espagnol](https://github.com/exatropix/misterword/releases/latest/download/Misterword-dictionnaire-es.zip)

Décompressez le jeu dans un dossier, puis le ou les dictionnaires dans ce même dossier. Lancez Misterword.exe. Si Windows affiche « Windows a protégé votre ordinateur », choisissez « Informations complémentaires », puis « Exécuter quand même » : le programme n'est pas signé.

Le Mistermind et le Taquin fonctionnent sans dictionnaire.

## Les commandes essentielles

- Alt ou F10 : le menu. Fichier, Nouvelle partie, puis le jeu et son niveau.
- Tab : passer de la saisie à la liste des propositions.
- Dans la liste : flèches haut et bas pour changer de ligne, gauche et droite pour lire lettre par lettre.
- Ctrl+S sauvegarder, Ctrl+O reprendre, Ctrl+N rejouer, F3 récapitulatif, F2 statistiques, F1 aide complète.
- Menu Langue : la langue de l'interface et celle des mots, réglables séparément.

## English

Three keyboard word and logic games for NVDA and JAWS users, on Windows 10 and 11: Misterword (a Lingo-style word game), Mistermind (the colour game) and a slide puzzle. Interface in French, English and Spanish. Download the game and at least one dictionary above, unzip them into the same folder and run Misterword.exe. Switch the interface to English in the Langue menu.

## Español

Tres juegos de letras y de lógica con el teclado, accesibles con NVDA y JAWS, para Windows 10 y 11: Misterword (al estilo de Lingo), Mistermind (el juego de los colores) y un puzle deslizante. Interfaz en francés, inglés y español. Descargue el juego y al menos un diccionario, descomprímalos en la misma carpeta y ejecute Misterword.exe. Cambie la interfaz al español en el menú Langue.

## Compiler

Le code source est dans le dossier `source` (C#, Windows Forms, .NET Framework 4.8). Par exemple :
`csc /target:winexe /out:Misterword.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll source\*.cs`
Placez à côté de l'exécutable les fichiers nvdaControllerClient32.dll et nvdaControllerClient64.dll, fournis dans le zip du jeu.

## Licences

- Le code de ce projet est versé dans le domaine public (voir LICENSE).
- Dictionnaire français : Grammalecte / Dicollecte (MPL 2.0) et an-array-of-french-words (MIT).
- Dictionnaires anglais et espagnol : listes de mots de Letterpress, paquets word-list et an-array-of-spanish-words (MIT).
- Communication avec NVDA : nvdaControllerClient, de NV Access (LGPL, voir LICENSE-NVDA.txt).
