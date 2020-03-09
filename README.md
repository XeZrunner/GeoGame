# Geographic guessing game for GYMKCH 2019
This game was created for the for the geographic-historic contest held at Gymnázium Kráľovský Chlmec on 23rd of October, 2019.

Written in C# WPF (XAML), Visual Studio 2019.

# Try it out!
[Pre-compiled builds are available at the Releases tab. Click here!](https://github.com/XeZrunner/GeoGame/releases)

# How it works

## Gameplay

* The game was created with a second screen in mind:
  * The game tries to detect a second screen, and if it succeeds, it automatically starts on that second screen.
    * The screen on which the game starts can be specified.
  * The operator controls the gameplay with the control panel seen on their main display. This includes choosing the desired behavior of the game, and selecting the location the player will have to guess.
  * The player only sees the second display, and interacts only with that screen. Their task is determined by the operator controlling the game.
  
## Features

* The main premise of the game is the following:
  * One of the players from the 3-member groups picks a piece of paper with a town written on it.
  * As they come to the board, they let the operator know what they have picked.
  * The operator changes the target location to the town that the player has gotten.
  * The player either picks the correct town, wrong town, or clicks onto an empty space on the map.
* As a backup plan: if two groups were to tie:
  * The game has an autopilot mode - The 12 medzibodrožie locations + 6 extra will be given as target locations, and the two players have to compete with each other. Whichever player gets more correct, wins the first place.

## Behind the scenes

* Upon starting the game, you're greeted with a control panel. This panel lets you:
  * start the game on a specified screen (preferably the second screen)
  * disable the mouse cursor on the game screen (for a seamless transition from game to control panel)
  * change the target location the player has to guess
  * change the behavior of the game (whether the game should show the correct result after a wrong answer, whether the game should have pretty animations etc...)
  * enable or disable autopilot mode (codename autophase)
  
  If debugging is enabled, the control panel also lets you test some of the features of the game:
  * debug status, showing the game state, whether the game is ready for interaction or the count of remaining items.
  * user interface tests enable testing of the design and animations
  * checkboxes that help test and debug location hitboxes
  * buttons to show or hide all location targets, including their hitboxes and different states (such as error state)
  
## Customizability & planned features
  
The game was thrown together from scratch, in C# WPF, in a total of 2 days, after which it was used at the contest.

For this reason, the game has no end-user customizability, thus no way to make custom maps or location targets, yet...  
All target locations are hard-coded into the game.

As a personal project, I might end up working on it more whenever my free time will let me.

Plans out the top of my head:

* Define target locations and other map properties using JSON. This would enable the next point:
* A map editing interface to create or import a map background image, and place target locations on them.
  * Target locations have customization options for hitbox width and height as well as 2 language variations for their names.
* More user interface and behavior customizations
* More game modes, limits, exceptions or miscellaneous cases.
