using System.Drawing;
using System.Windows.Forms;

namespace Misterword
{
    static class Aide
    {
        public const string Texte =
@"MISTERWORD, VERSION 4.1

Cinq jeux au clavier, pour NVDA et JAWS : Misterword, le jeu de mots dans le style de Motus, Mistermind, le jeu des couleurs, le Taquin, le jeu des lettres à remettre dans l'ordre, les Mots croisés et le Sudoku.
Tout ce que le jeu annonce passe par votre lecteur d'écran, avec votre voix habituelle. Sans lecteur d'écran, la voix de Windows prend le relais.
Appuyez sur Échap pour fermer cette aide.

LE MENU

À l'ouverture, le jeu affiche une page vide. Alt ou F10 ouvre le menu.
Menu Fichier :
- Nouvelle partie : Misterword, avec la longueur du mot (5 à 10 lettres, ou au hasard), Mistermind, facile (4 couleurs sur 6) ou expert (5 couleurs sur 8), Taquin, facile (3 sur 3), normal (4 sur 4) ou expert (5 sur 5), Mots croisés, facile (5 sur 5), moyen (7 sur 7), difficile (9 sur 9) ou expert (11 sur 11), ou Sudoku, facile, moyen, difficile ou expert.
- Rejouer au même jeu : Ctrl+N.
- Sauvegarder la partie : Ctrl+S. Reprendre la partie sauvegardée : Ctrl+O.
  En quittant pendant une partie, le jeu propose de la sauvegarder. Au lancement suivant, il propose de la reprendre. Une seule partie est gardée à la fois : la dernière sauvegardée.
- Abandonner et voir la solution : Ctrl+R.
- Récapitulatif de la partie : F3.
- Statistiques : F2.
- Annoncer les résultats en clair : F4. Sans cette option, seuls les bips vous renseignent. Avec elle, le jeu dit aussi « bien placé », « mal placé » ou « absent ».
- Mistermind, indice couleur par couleur : F5. Cochée au départ.
- Quitter : Ctrl+Q.
Menu Langue :
- Interface : la langue des menus, des annonces et de cette aide. Français, anglais ou espagnol.
- Mots de Misterword : la langue des mots à trouver. Elle peut être différente de celle de l'interface, par exemple pour jouer en espagnol avec des menus en français.
Menu Aide : cette aide (F1), Écouter les bips, Rechercher une mise à jour, Rechercher les mises à jour au démarrage, À propos.
Au démarrage, une fois par jour, le jeu vérifie sur Internet si une nouvelle version existe et vous prévient, sans rien installer. Rechercher une mise à jour propose d'ouvrir la page de téléchargement. Vos statistiques, records et parties sauvegardées sont conservés quand vous installez la nouvelle version.

LES DEUX ZONES

Pendant une partie, l'écran a deux zones. Tab passe de l'une à l'autre.
1. La zone de saisie, où vous écrivez votre mot ou votre combinaison. Entrée valide.
2. La liste des propositions. Flèche haut et flèche bas passent d'une ligne à l'autre. Flèche gauche et flèche droite lisent la ligne lettre par lettre, ou couleur par couleur.
Depuis la saisie de Misterword, flèche haut mène aussi à la dernière proposition. Flèche bas sur la dernière ligne ramène à la saisie.

LES BIPS

Bip aigu : bien placé.
Bip moyen : présent, mais mal placé.
Bip grave : absent.

MISTERWORD

Il faut trouver un mot de 5 à 10 lettres, en 6 essais au plus. La première lettre est donnée.
Tapez votre mot, puis Entrée. Les accents sont ignorés. En espagnol, le Ñ est une lettre à part entière. Vous pouvez taper le mot entier, ou le mot sans sa première lettre : le jeu l'ajoute.
Le mot doit avoir la bonne longueur, commencer par la bonne lettre et exister dans le dictionnaire. Sinon, il est refusé et l'essai n'est pas compté.
Après Entrée, le jeu joue un bip par lettre, dans l'ordre du mot.
Dans la liste, flèche droite et flèche gauche disent chaque lettre avec son bip. C'est le son qui vous indique si la lettre est bien placée, mal placée ou absente.
Une lettre n'est signalée qu'autant de fois qu'elle figure dans le mot. Si le mot ne contient qu'un E et que vous en proposez deux, le second est absent.
Points : longueur du mot multipliée par le nombre d'essais restant au moment où vous trouvez, plus un.

MISTERMIND

L'ordinateur cache une combinaison de couleurs, à retrouver en 10 essais au plus. Une même couleur peut revenir plusieurs fois.
Facile : 4 couleurs à trouver parmi 6. Expert : 5 couleurs parmi 8.
Les couleurs et leur touche : R rouge, V vert, B bleu, J jaune, O orange, N noir. En expert, en plus : M marron, G gris.
Dans la zone de saisie, tapez les lettres, par exemple R V B J, puis Entrée. Le jeu dit le nom de chaque couleur.
Flèches gauche et droite : parcourir les cases. Flèches haut et bas, ou Page précédente et Page suivante : changer la couleur de la case. Retour arrière : effacer la couleur précédente. Suppr : effacer la case. Espace : relire la combinaison. Échap : tout effacer.
Avec l'option « indice couleur par couleur » (F5, cochée au départ), le jeu fait comme l'adversaire dans certaines règles : il indique pour chaque couleur si elle est bien placée, mal placée ou absente, exactement comme les lettres de Misterword. Après Entrée, un bip par couleur, dans l'ordre. Dans la liste, flèche gauche et flèche droite disent chaque couleur avec son bip.
Sans cette option, c'est la règle classique : le jeu ne dit pas quelles couleurs sont justes, seulement combien. Un bip aigu par couleur bien placée, un bip moyen par couleur mal placée, un seul bip grave si aucune n'est juste. Dans la liste, ces bips sont rejoués en arrivant sur une ligne, et les flèches gauche et droite disent les couleurs sans bip.
Points : nombre d'essais restant au moment où vous trouvez, plus un ; le double en expert.

TAQUIN

Une grille contient des lettres mélangées et une case vide. Le but : remettre les lettres dans l'ordre alphabétique, ligne par ligne, la case vide en bas à droite.
Trois niveaux :
- Facile : 3 cases sur 3, lettres A à H.
- Normal : 4 cases sur 4, lettres A à O. Ligne 1 : A B C D. Ligne 2 : E F G H. Ligne 3 : I J K L. Ligne 4 : M N O, puis la case vide.
- Expert : 5 cases sur 5, lettres A à X.
Flèches : se déplacer d'une case. Chaque case est annoncée par sa lettre, ou « vide ».
Entrée ou Espace : faire glisser la lettre où vous êtes vers la case vide. La lettre doit être sur la même ligne ou la même colonne que la case vide.
Si d'autres lettres la séparent de la case vide, elles glissent toutes ensemble. Le jeu trouve seul le sens du déplacement. Un son de glissement accompagne chaque lettre déplacée, puis le jeu annonce par exemple : F, G vers la droite.
Après le glissement, vous restez sur la lettre que vous avez choisie.
L : lire la ligne. C : lire la colonne. T : lire tout le tableau. P : position de la case. V : aller à la case vide.
Début et Fin : première et dernière colonne. Page précédente et Page suivante : première et dernière ligne.
F3 : nombre de déplacements, temps écoulé et record.
Le chronomètre démarre au premier déplacement. Quand le tableau est rangé, le jeu vous propose d'enregistrer votre temps, et annonce s'il s'agit d'un nouveau record. Chaque niveau a son propre record.

MOTS CROISÉS

Le jeu fabrique une grille nouvelle à chaque partie, avec des mots qui se croisent. Quatre tailles : facile 5 sur 5 (environ 6 mots), moyen 7 sur 7 (10 mots), difficile 9 sur 9 (15 mots), expert 11 sur 11 (une vingtaine de mots).
Les mots et leurs définitions viennent du dictionnaire de la langue choisie dans le menu Langue, Langue des mots. Les accents ne comptent pas : tapez E pour É. En espagnol, Ñ est une lettre à part.
Les cases sont nommées par une lettre pour la ligne et un chiffre pour la colonne : A1 est en haut à gauche, B3 est à la deuxième ligne, troisième colonne.
Flèches : se déplacer d'une case. Le jeu annonce la case, sa lettre ou « vide », et le numéro du mot quand vous changez de mot. Une case noire est signalée.
Tapez une lettre pour remplir la case : le curseur avance tout seul dans le sens du mot. « Mot complet » est annoncé quand toutes ses cases sont remplies.
Entrée : lire la définition du mot où vous êtes, avec son numéro, son sens, sa longueur et les lettres déjà placées.
Espace : changer de sens (horizontal ou vertical) quand la case appartient à deux mots, et lire la définition.
Début et Fin : première et dernière lettre du mot. Retour arrière : effacer la lettre précédente. Suppr : effacer la case.
Ctrl+L : épeler les lettres du mot. Ctrl+E : vérifier le mot, un bip par lettre (aigu juste, grave fausse, moyen vide). Ctrl+G : vérifier toute la grille. Ctrl+I : indice, révèle une lettre.
Tab : passer à la liste des définitions, et revenir à la grille. Dans la liste, flèches haut et bas pour parcourir les définitions, flèches gauche et droite pour entendre les lettres déjà placées, Entrée pour aller au mot dans la grille.
F3 : mots trouvés, cases remplies et temps écoulé. Ctrl+R : abandonner, la solution est remplie dans la grille.
Quand la grille est juste, le jeu annonce votre temps et propose de l'enregistrer s'il n'y a eu aucun indice. Chaque taille a son record.

SUDOKU

Une grille de 9 sur 9, divisée en 9 blocs de 3 sur 3. Remplissez-la avec les chiffres de 1 à 9, sans répéter un chiffre dans une ligne, une colonne ou un bloc. Chaque grille a une solution unique.
Quatre niveaux : facile, moyen, difficile et expert. Plus le niveau monte, moins il y a de chiffres donnés au départ.
Flèches : se déplacer. Le jeu annonce le chiffre, « vide » ou « donné » pour les chiffres de départ, qui ne peuvent pas être changés, et le bloc quand vous en changez.
Chiffres de 1 à 9, sur la rangée du haut ou le pavé numérique : placer un chiffre. S'il est déjà dans la ligne, la colonne ou le bloc, un bip d'erreur vous le signale.
Retour arrière, Suppr ou 0 : effacer.
L : lire la ligne. C : lire la colonne. B : lire le bloc. P : chiffres encore possibles dans la case. Espace : relire la case.
Début et Fin : première et dernière colonne. Page précédente et Page suivante : première et dernière ligne.
Ctrl+E : vérifier la grille. Ctrl+I : indice, place le bon chiffre dans la case.
F3 : cases vides et temps écoulé. Ctrl+R : abandonner, la solution est remplie dans la grille.
Le temps peut être enregistré à la fin s'il n'y a eu aucun indice. Chaque niveau a son record.

PERSONNALISER

Les dictionnaires, un par langue, se téléchargent depuis le jeu : menu Langue, Télécharger un dictionnaire. Le jeu le propose aussi tout seul quand un dictionnaire manque. Il faut une connexion à Internet.
Vous pouvez aussi les télécharger sur la page du jeu et les décompresser dans le dossier du jeu : ils se rangent dans le dossier dictionnaires, sous-dossier fr, en ou es.
Chaque dictionnaire contient mots.txt, la liste des mots acceptés, un mot par ligne, en majuscules sans accent, mots_secrets.txt, les mots à deviner, et definitions.txt, les définitions des mots croisés, une par ligne sous la forme MOT|définition. Vous pouvez les modifier.
Le Mistermind, le Taquin et le Sudoku fonctionnent sans dictionnaire.
Vos statistiques et vos options sont dans le dossier AppData\Roaming\Misterword.";

        public const string TexteEn =
@"MISTERWORD, VERSION 4.1

Five keyboard games for NVDA and JAWS: Misterword, a word game in the style of Lingo, Mistermind, the colour game, the slide puzzle, where letters must be put back in order, the Crossword and Sudoku.
Everything the game says goes through your screen reader, with your usual voice. Without a screen reader, the Windows voice takes over.
Press Escape to close this help.

THE MENU

When it opens, the game shows an empty page. Alt or F10 opens the menu.
File menu:
- New game: Misterword, with the length of the word (5 to 10 letters, or random), Mistermind, easy (4 colours out of 6) or expert (5 colours out of 8), Slide puzzle, easy (3 by 3), normal (4 by 4) or expert (5 by 5), Crossword, easy (5 by 5), medium (7 by 7), hard (9 by 9) or expert (11 by 11), or Sudoku, easy, medium, hard or expert.
- Play the same game again: Ctrl+N.
- Save the game: Ctrl+S. Resume the saved game: Ctrl+O.
  If you quit during a game, you are offered to save it. Next time you start, you are offered to resume it. Only one game is kept at a time: the last one saved.
- Give up and see the solution: Ctrl+R.
- Summary of the game: F3.
- Statistics: F2.
- Announce results in words: F4. Without this option, only the beeps tell you. With it, the game also says right place, wrong place or not there.
- Mistermind, clue for each colour: F5. On by default.
- Quit: Ctrl+Q.
Language menu:
- Interface: the language of the menus, announcements and this help. French, English or Spanish.
- Misterword words: the language of the words to find. It can differ from the interface, for example to play with Spanish words and English menus.
Help menu: this help (F1), Listen to the beeps, Check for updates, Check for updates at startup, About.
At startup, once a day, the game checks online whether a new version exists and tells you, without installing anything. Check for updates offers to open the download page. Your statistics, records and saved games are kept when you install the new version.

THE TWO AREAS

During a game, the screen has two areas. Tab moves from one to the other.
1. The input area, where you type your word or code. Enter confirms.
2. The list of guesses. Up and down arrows move from line to line. Left and right arrows read the line letter by letter, or colour by colour.
From the Misterword input, the up arrow also takes you to the last guess. The down arrow on the last line takes you back to the input.

THE BEEPS

High beep: right place.
Middle beep: in the word, but in the wrong place.
Low beep: not there.

MISTERWORD

Find a word of 5 to 10 letters, in 6 attempts at most. The first letter is given.
Type your word, then Enter. Accents are ignored. In Spanish, Ñ is a letter in its own right. You can type the whole word, or the word without its first letter: the game adds it.
The word must have the right length, start with the right letter and be in the dictionary. Otherwise it is refused and the attempt does not count.
After Enter, the game plays one beep per letter, in the order of the word.
In the list, the right and left arrows say each letter with its beep. The sound tells you whether the letter is in the right place, in the wrong place or not there.
A letter is reported only as many times as it appears in the word. If the word has only one E and you try two, the second one is not there.
Points: length of the word multiplied by the number of attempts left when you find it, plus one.

MISTERMIND

The computer hides a code of colours, to find in 10 attempts at most. The same colour can appear several times.
Easy: 4 colours to find among 6. Expert: 5 colours among 8.
The colours and their keys: R red, G green, B blue, Y yellow, O orange, W white. In expert, also: P purple, C cyan.
In the input area, type the letters, for example R G B Y, then Enter. The game says the name of each colour.
Left and right arrows: move through the slots. Up and down arrows, or Page Up and Page Down: change the colour of the slot. Backspace: clear the previous colour. Delete: clear the slot. Space: read the code again. Escape: clear everything.
With the option clue for each colour (F5, on by default), the game says for each colour whether it is in the right place, in the wrong place or not there, exactly like the letters of Misterword. After Enter, one beep per colour, in order. In the list, the left and right arrows say each colour with its beep.
Without this option, it is the classic rule: the game does not say which colours are right, only how many. One high beep per colour in the right place, one middle beep per colour in the wrong place, a single low beep if none is right. In the list, these beeps are played again when you reach a line, and the left and right arrows say the colours without beeps.
Points: number of attempts left when you find it, plus one; double in expert.

SLIDE PUZZLE

A grid contains shuffled letters and one empty square. The goal: put the letters back in alphabetical order, row by row, with the empty square at the bottom right.
Three levels:
- Easy: 3 by 3, letters A to H.
- Normal: 4 by 4, letters A to O. Row 1: A B C D. Row 2: E F G H. Row 3: I J K L. Row 4: M N O, then the empty square.
- Expert: 5 by 5, letters A to X.
Arrows: move one square. Each square is announced by its letter, or empty.
Enter or Space: slide the letter you are on towards the empty square. The letter must be in the same row or column as the empty square.
If other letters are between it and the empty square, they all slide together. The game works out the direction itself. A sliding sound goes with each letter moved, then the game announces for example: F, G to the right.
After the slide, you stay on the letter you chose.
L: read the row. C: read the column. T: read the whole board. P: position of the square. V: go to the empty square.
Home and End: first and last column. Page Up and Page Down: first and last row.
F3: number of moves, time elapsed and record.
The timer starts with the first move. When the board is solved, the game offers to record your time and tells you if it is a new record. Each level has its own record.

CROSSWORD

The game builds a new grid for each game, with interlocking words. Four sizes: easy 5 by 5 (about 6 words), medium 7 by 7 (10 words), hard 9 by 9 (15 words), expert 11 by 11 (about twenty words).
The words and their clues come from the dictionary of the language chosen in the Language menu, Word language. Accents do not count: type E for É. In Spanish, Ñ is a letter of its own.
Squares are named by a letter for the row and a number for the column: A1 is top left, B3 is on the second row, third column.
Arrows: move one square. The game announces the square, its letter or empty, and the word number when you move to another word. A black square is signalled.
Type a letter to fill the square: the cursor moves on by itself along the word. Word complete is announced when all its squares are filled.
Enter: read the clue of the word you are on, with its number, direction, length and the letters already placed.
Space: switch direction (across or down) when the square belongs to two words, and read the clue.
Home and End: first and last letter of the word. Backspace: erase the previous letter. Delete: erase the square.
Ctrl+L: spell the letters of the word. Ctrl+E: check the word, one beep per letter (high correct, low wrong, middle empty). Ctrl+G: check the whole grid. Ctrl+I: hint, reveals a letter.
Tab: go to the list of clues, and back to the grid. In the list, Up and Down arrows browse the clues, Left and Right arrows say the letters already placed, Enter goes to the word in the grid.
F3: words solved, squares filled and time elapsed. Ctrl+R: give up, the solution is filled into the grid.
When the grid is correct, the game announces your time and offers to record it if no hint was used. Each size has its own record.

SUDOKU

A 9 by 9 grid, divided into 9 boxes of 3 by 3. Fill it with the digits 1 to 9, without repeating a digit in a row, a column or a box. Each grid has a single solution.
Four levels: easy, medium, hard and expert. The higher the level, the fewer digits are given at the start.
Arrows: move. The game announces the digit, empty, or given for the starting digits, which cannot be changed, and the box when you move to another one.
Digits 1 to 9, on the top row or the numeric keypad: place a digit. If it is already in the row, column or box, an error beep tells you.
Backspace, Delete or 0: erase.
L: read the row. C: read the column. B: read the box. P: digits still possible in the square. Space: read the square again.
Home and End: first and last column. Page Up and Page Down: first and last row.
Ctrl+E: check the grid. Ctrl+I: hint, places the right digit in the square.
F3: empty squares and time elapsed. Ctrl+R: give up, the solution is filled into the grid.
Your time can be recorded at the end if no hint was used. Each level has its own record.

CUSTOMISING

Dictionaries, one per language, can be downloaded from the game: Language menu, Download a dictionary. The game also offers it by itself when a dictionary is missing. An Internet connection is needed.
You can also download them from the game page and unzip them into the game folder: they go into the dictionnaires folder, in the fr, en or es subfolder.
Each dictionary contains mots.txt, the list of accepted words, one per line, in capitals without accents, mots_secrets.txt, the words to guess, and definitions.txt, the crossword clues, one per line in the form WORD|clue. You can edit them.
Mistermind, the slide puzzle and Sudoku work without a dictionary.
Your statistics and options are in the AppData\Roaming\Misterword folder.";

        public const string TexteEs =
@"MISTERWORD, VERSIÓN 4.1

Cinco juegos con el teclado para NVDA y JAWS: Misterword, un juego de palabras al estilo de Lingo, Mistermind, el juego de los colores, el puzle deslizante, en el que hay que poner las letras en orden, el Crucigrama y el Sudoku.
Todo lo que dice el juego pasa por su lector de pantalla, con su voz habitual. Sin lector de pantalla, toma el relevo la voz de Windows.
Pulse Escape para cerrar esta ayuda.

EL MENÚ

Al abrirse, el juego muestra una página vacía. Alt o F10 abre el menú.
Menú Archivo:
- Nueva partida: Misterword, con la longitud de la palabra (de 5 a 10 letras, o al azar), Mistermind, fácil (4 colores de 6) o experto (5 colores de 8), Puzle deslizante, fácil (3 por 3), normal (4 por 4) o experto (5 por 5), Crucigrama, fácil (5 por 5), medio (7 por 7), difícil (9 por 9) o experto (11 por 11), o Sudoku, fácil, medio, difícil o experto.
- Volver a jugar al mismo juego: Ctrl+N.
- Guardar la partida: Ctrl+S. Reanudar la partida guardada: Ctrl+O.
  Si sale durante una partida, el juego le propone guardarla. La próxima vez que lo abra, le propone reanudarla. Solo se conserva una partida: la última guardada.
- Abandonar y ver la solución: Ctrl+R.
- Resumen de la partida: F3.
- Estadísticas: F2.
- Anunciar los resultados con palabras: F4. Sin esta opción, solo le informan los pitidos. Con ella, el juego dice también bien colocada, mal colocada o ausente.
- Mistermind, pista color por color: F5. Activada al principio.
- Salir: Ctrl+Q.
Menú Idioma:
- Interfaz: el idioma de los menús, de los anuncios y de esta ayuda. Francés, inglés o español.
- Palabras de Misterword: el idioma de las palabras que hay que encontrar. Puede ser distinto del de la interfaz, por ejemplo para jugar con palabras en inglés y menús en español.
Menú Ayuda: esta ayuda (F1), Escuchar los pitidos, Buscar actualizaciones, Buscar actualizaciones al iniciar, Acerca de.
Al iniciarse, una vez al día, el juego comprueba en Internet si existe una nueva versión y le avisa, sin instalar nada. Buscar actualizaciones propone abrir la página de descarga. Sus estadísticas, récords y partidas guardadas se conservan al instalar la nueva versión.

LAS DOS ZONAS

Durante una partida, la pantalla tiene dos zonas. Tab pasa de una a otra.
1. La zona de escritura, donde escribe su palabra o su combinación. Intro confirma.
2. La lista de propuestas. Las flechas arriba y abajo pasan de una línea a otra. Las flechas izquierda y derecha leen la línea letra por letra, o color por color.
Desde la escritura de Misterword, la flecha arriba lleva también a la última propuesta. La flecha abajo en la última línea vuelve a la escritura.

LOS PITIDOS

Pitido agudo: bien colocada.
Pitido medio: presente, pero mal colocada.
Pitido grave: ausente.

MISTERWORD

Hay que encontrar una palabra de 5 a 10 letras, en 6 intentos como máximo. La primera letra se da desde el principio.
Escriba su palabra y pulse Intro. Las tildes no cuentan. La Ñ es una letra propia. Puede escribir la palabra entera, o la palabra sin su primera letra: el juego la añade.
La palabra debe tener la longitud correcta, empezar por la letra correcta y estar en el diccionario. Si no, se rechaza y el intento no cuenta.
Después de Intro, el juego toca un pitido por letra, en el orden de la palabra.
En la lista, las flechas derecha e izquierda dicen cada letra con su pitido. El sonido le indica si la letra está bien colocada, mal colocada o ausente.
Una letra solo se señala tantas veces como aparece en la palabra. Si la palabra solo tiene una E y usted propone dos, la segunda es ausente.
Puntos: longitud de la palabra multiplicada por el número de intentos que quedan cuando la encuentra, más uno.

MISTERMIND

El ordenador esconde una combinación de colores, que hay que encontrar en 10 intentos como máximo. Un mismo color puede aparecer varias veces.
Fácil: 4 colores que encontrar entre 6. Experto: 5 colores entre 8.
Los colores y su tecla: R rojo, V verde, A azul, N negro, B blanco, G gris. En experto, además: M marrón, L lila.
En la zona de escritura, escriba las letras, por ejemplo R V A N, y pulse Intro. El juego dice el nombre de cada color.
Flechas izquierda y derecha: recorrer las casillas. Flechas arriba y abajo, o Re Pág y Av Pág: cambiar el color de la casilla. Retroceso: borrar el color anterior. Supr: borrar la casilla. Espacio: volver a leer la combinación. Escape: borrarlo todo.
Con la opción pista color por color (F5, activada al principio), el juego indica para cada color si está bien colocado, mal colocado o ausente, igual que las letras de Misterword. Después de Intro, un pitido por color, en orden. En la lista, las flechas izquierda y derecha dicen cada color con su pitido.
Sin esta opción, es la regla clásica: el juego no dice qué colores son correctos, solo cuántos. Un pitido agudo por color bien colocado, un pitido medio por color mal colocado, un solo pitido grave si ninguno es correcto. En la lista, estos pitidos se repiten al llegar a una línea, y las flechas izquierda y derecha dicen los colores sin pitido.
Puntos: número de intentos que quedan cuando la encuentra, más uno; el doble en experto.

PUZLE DESLIZANTE

Una cuadrícula contiene letras mezcladas y una casilla vacía. El objetivo: poner las letras en orden alfabético, fila por fila, con la casilla vacía abajo a la derecha.
Tres niveles:
- Fácil: 3 por 3, letras de la A a la H.
- Normal: 4 por 4, letras de la A a la O. Fila 1: A B C D. Fila 2: E F G H. Fila 3: I J K L. Fila 4: M N O, y luego la casilla vacía.
- Experto: 5 por 5, letras de la A a la X.
Flechas: moverse una casilla. Cada casilla se anuncia con su letra, o vacía.
Intro o Espacio: deslizar la letra en la que está hacia la casilla vacía. La letra debe estar en la misma fila o columna que la casilla vacía.
Si hay otras letras entre ella y la casilla vacía, se deslizan todas juntas. El juego encuentra solo la dirección. Un sonido de deslizamiento acompaña cada letra movida, y luego el juego anuncia por ejemplo: F, G hacia la derecha.
Después del deslizamiento, sigue en la letra que eligió.
L: leer la fila. C: leer la columna. T: leer todo el tablero. P: posición de la casilla. V: ir a la casilla vacía.
Inicio y Fin: primera y última columna. Re Pág y Av Pág: primera y última fila.
F3: número de movimientos, tiempo transcurrido y récord.
El cronómetro empieza con el primer movimiento. Cuando el tablero está ordenado, el juego le propone registrar su tiempo y le dice si es un nuevo récord. Cada nivel tiene su propio récord.

CRUCIGRAMA

El juego crea una cuadrícula nueva en cada partida, con palabras que se cruzan. Cuatro tamaños: fácil 5 por 5 (unas 6 palabras), medio 7 por 7 (10 palabras), difícil 9 por 9 (15 palabras), experto 11 por 11 (una veintena de palabras).
Las palabras y sus definiciones vienen del diccionario del idioma elegido en el menú Idioma, Idioma de las palabras. Las tildes no cuentan: escriba E para É. La Ñ es una letra propia.
Las casillas se nombran con una letra para la fila y un número para la columna: A1 está arriba a la izquierda, B3 está en la segunda fila, tercera columna.
Flechas: moverse una casilla. El juego anuncia la casilla, su letra o vacía, y el número de la palabra cuando cambia de palabra. Se avisa de las casillas negras.
Escriba una letra para llenar la casilla: el cursor avanza solo en el sentido de la palabra. Se anuncia Palabra completa cuando todas sus casillas están llenas.
Intro: leer la definición de la palabra en la que está, con su número, sentido, longitud y las letras ya puestas.
Espacio: cambiar de sentido (horizontal o vertical) cuando la casilla pertenece a dos palabras, y leer la definición.
Inicio y Fin: primera y última letra de la palabra. Retroceso: borrar la letra anterior. Supr: borrar la casilla.
Ctrl+L: deletrear la palabra. Ctrl+E: comprobar la palabra, un pitido por letra (agudo correcta, grave incorrecta, medio vacía). Ctrl+G: comprobar toda la cuadrícula. Ctrl+I: pista, revela una letra.
Tab: pasar a la lista de definiciones, y volver a la cuadrícula. En la lista, flechas arriba y abajo para recorrer las definiciones, flechas izquierda y derecha para oír las letras ya puestas, Intro para ir a la palabra en la cuadrícula.
F3: palabras resueltas, casillas llenas y tiempo transcurrido. Ctrl+R: abandonar, la solución aparece en la cuadrícula.
Cuando la cuadrícula es correcta, el juego anuncia su tiempo y le propone registrarlo si no ha usado ninguna pista. Cada tamaño tiene su récord.

SUDOKU

Una cuadrícula de 9 por 9, dividida en 9 bloques de 3 por 3. Llénela con las cifras del 1 al 9, sin repetir una cifra en una fila, una columna o un bloque. Cada cuadrícula tiene una única solución.
Cuatro niveles: fácil, medio, difícil y experto. Cuanto más alto el nivel, menos cifras se dan al principio.
Flechas: moverse. El juego anuncia la cifra, vacía, o dada para las cifras iniciales, que no se pueden cambiar, y el bloque cuando cambia de bloque.
Cifras del 1 al 9, en la fila superior o el teclado numérico: poner una cifra. Si ya está en la fila, la columna o el bloque, un pitido de error le avisa.
Retroceso, Supr o 0: borrar.
L: leer la fila. C: leer la columna. B: leer el bloque. P: cifras aún posibles en la casilla. Espacio: volver a leer la casilla.
Inicio y Fin: primera y última columna. Re Pág y Av Pág: primera y última fila.
Ctrl+E: comprobar la cuadrícula. Ctrl+I: pista, pone la cifra correcta en la casilla.
F3: casillas vacías y tiempo transcurrido. Ctrl+R: abandonar, la solución aparece en la cuadrícula.
Su tiempo se puede registrar al final si no ha usado ninguna pista. Cada nivel tiene su récord.

PERSONALIZAR

Los diccionarios, uno por idioma, se descargan desde el juego: menú Idioma, Descargar un diccionario. El juego también lo propone solo cuando falta un diccionario. Se necesita conexión a Internet.
También puede descargarlos en la página del juego y descomprimirlos en la carpeta del juego: se colocan en la carpeta dictionnaires, subcarpeta fr, en o es.
Cada diccionario contiene mots.txt, la lista de palabras aceptadas, una por línea, en mayúsculas sin tildes, mots_secrets.txt, las palabras que hay que adivinar, y definitions.txt, las definiciones del crucigrama, una por línea con la forma PALABRA|definición. Puede modificarlos.
El Mistermind, el puzle deslizante y el Sudoku funcionan sin diccionario.
Sus estadísticas y opciones están en la carpeta AppData\Roaming\Misterword.";

        public static void Afficher(IWin32Window parent)
        {
            using (var f = new Form())
            {
                f.Text = L.Code == "en" ? "Misterword help" : L.Code == "es" ? "Ayuda de Misterword" : "Aide de Misterword";
                f.ClientSize = new Size(720, 560);
                f.StartPosition = FormStartPosition.CenterParent;
                f.KeyPreview = true;
                f.ShowInTaskbar = false;
                var t = new TextBox
                {
                    Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill,
                    Text = (L.Code == "en" ? TexteEn : L.Code == "es" ? TexteEs : Texte).Replace("\r\n", "\n").Replace("\n", "\r\n"), Font = new Font("Segoe UI", 11f),
                    AccessibleName = L.Code == "en" ? "Help" : L.Code == "es" ? "Ayuda" : "Aide", BackColor = SystemColors.Window
                };
                var fermer = new Button { Text = L.Code == "en" ? "Close" : L.Code == "es" ? "Cerrar" : "Fermer", Dock = DockStyle.Bottom, DialogResult = DialogResult.Cancel };
                f.Controls.Add(t); f.Controls.Add(fermer);
                f.CancelButton = fermer;
                f.Shown += (s, e) => { t.Focus(); t.SelectionStart = 0; t.SelectionLength = 0; };
                f.ShowDialog(parent);
            }
        }
    }
}
