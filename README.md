# Misterword

![Téléchargements](https://img.shields.io/github/downloads/exatropix/misterword/total?label=T%C3%A9l%C3%A9chargements)

**Cinq jeux de lettres et de logique, entièrement au clavier, accessibles avec NVDA et JAWS. Pour Windows 10 et 11. En français, en anglais et en espagnol.**

Par Exatropic.

## Les jeux

- **Misterword**, dans le style de Motus : trouvez un mot de 5 à 10 lettres en 6 essais. La première lettre est donnée. Après chaque essai, un bip par lettre : aigu si elle est bien placée, moyen si elle est mal placée, grave si elle est absente.
- **Mistermind**, le jeu des couleurs : retrouvez une combinaison de 4 couleurs parmi 6 (facile) ou de 5 parmi 8 (expert), en 10 essais.
- **Le Taquin** : remettez les lettres dans l'ordre en les faisant glisser vers la case vide. Trois niveaux, de 3 sur 3 à 5 sur 5, avec chronomètre et records.
- **Les Mots croisés** : une grille nouvelle à chaque partie, de 5 sur 5 à 11 sur 11, en français, en anglais ou en espagnol. Flèches pour se déplacer, Entrée lit la définition, Tab ouvre la liste des définitions, Ctrl+E vérifie le mot avec un bip par lettre.
- **Le Sudoku** : grille de 9 sur 9, quatre niveaux, solution unique. L, C et B lisent la ligne, la colonne et le bloc ; P donne les chiffres encore possibles.

Tout passe par votre lecteur d'écran, avec votre voix habituelle. Les propositions restent dans une liste que l'on relit à tout moment, lettre par lettre ou couleur par couleur. On peut sauvegarder une partie et la reprendre plus tard. Le jeu vérifie seul s'il existe une nouvelle version.

## Télécharger

1. Le jeu : [Misterword.zip](https://github.com/exatropix/misterword/releases/latest/download/Misterword.zip)
2. Un dictionnaire au moins, pour Misterword et les mots croisés. Le jeu propose de le télécharger tout seul au premier lancement (menu Langue, Télécharger un dictionnaire). Vous pouvez aussi le prendre ici :
   [français](https://github.com/exatropix/misterword/releases/latest/download/Misterword-dictionnaire-fr.zip),
   [anglais](https://github.com/exatropix/misterword/releases/latest/download/Misterword-dictionnaire-en.zip),
   [espagnol](https://github.com/exatropix/misterword/releases/latest/download/Misterword-dictionnaire-es.zip)

Décompressez le jeu dans un dossier, puis le ou les dictionnaires dans ce même dossier. Lancez Misterword.exe. Si Windows affiche « Windows a protégé votre ordinateur », choisissez « Informations complémentaires », puis « Exécuter quand même » : le programme n'est pas signé.

Le Mistermind, le Taquin et le Sudoku fonctionnent sans dictionnaire.

## Les commandes essentielles

- Alt ou F10 : le menu. Fichier, Nouvelle partie, puis le jeu et son niveau.
- Tab : passer de la saisie à la liste des propositions.
- Dans la liste : flèches haut et bas pour changer de ligne, gauche et droite pour lire lettre par lettre.
- Ctrl+S sauvegarder, Ctrl+O reprendre, Ctrl+N rejouer, F3 récapitulatif, F2 statistiques, F1 aide complète.
- Menu Langue : la langue de l'interface et celle des mots, réglables séparément, et le téléchargement des dictionnaires.

## English

Five keyboard word and logic games for NVDA and JAWS users, on Windows 10 and 11: Misterword (a Lingo-style word game), Mistermind (the colour game), a slide puzzle, a Crossword (clues in English, French or Spanish) and Sudoku. Interface in French, English and Spanish. Download the game, unzip it and run Misterword.exe: it offers to download a dictionary by itself. Switch the interface to English in the Langue menu.

## Español

Cinco juegos de letras y de lógica con el teclado, accesibles con NVDA y JAWS, para Windows 10 y 11: Misterword (al estilo de Lingo), Mistermind (el juego de los colores), un puzle deslizante, un Crucigrama (definiciones en español, francés o inglés) y el Sudoku. Interfaz en francés, inglés y español. Descargue el juego, descomprímalo y ejecute Misterword.exe: le propone descargar un diccionario. Cambie la interfaz al español en el menú Langue.

## Compiler

Le code source est dans le dossier `source` (C#, Windows Forms, .NET Framework 4.8). Par exemple :
`csc /target:winexe /out:Misterword.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll source\*.cs`
Placez à côté de l'exécutable les fichiers nvdaControllerClient32.dll et nvdaControllerClient64.dll, fournis dans le zip du jeu.

## Licences

- Le code de ce projet est versé dans le domaine public (voir LICENSE).
- Dictionnaire français : Grammalecte / Dicollecte (MPL 2.0) et an-array-of-french-words (MIT).
- Dictionnaires anglais et espagnol : listes de mots de Letterpress, paquets word-list et an-array-of-spanish-words (MIT).
- Définitions des mots croisés (français, anglais, espagnol) écrites pour le jeu, versées dans le domaine public.
- Communication avec NVDA : nvdaControllerClient, de NV Access (LGPL, voir LICENSE-NVDA.txt).
