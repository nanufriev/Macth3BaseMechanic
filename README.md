# Match3 Base Mechanic in Unity

## Overview
This Unity-based repository showcases the foundational mechanics for a Match3 game, focusing on modular architecture, advanced animation handling, and flexible grid management. It is ideal for developers interested in exploring Match3 mechanics or developing their own puzzle games based on this foundation.

## Features

### Advanced Animation System
Leverages DOTween to enable smooth transitions and animations that can be toggled on or off through `AnimationConfig`. This system is designed for high flexibility and expandability, enabling easy customization and addition of new animation types.

### Configurable Gameplay and Animation Settings
Utilizes `GameConfig` and `AnimationConfig` to adjust gameplay mechanics like grid size and animation behaviors directly from the Unity Editor.

### Grid and Animation Separation
Manages game logic and animations separately, facilitating non-graphical simulations for testing and AI development.

### Dynamic Grid Management
Controls tile spawning, swapping, and matching logic, which can be customized via `GameConfig`, supporting easy customization of gameplay dynamics.

### Responsive Design
Features adaptive camera and UI adjustments to accommodate different screen sizes and resolutions.

## Configuration

### GameConfig
`GameConfig` is a `ScriptableObject` that provides crucial settings for the game's mechanics:

- **Grid Width and Height**: Sets the dimensions of the game grid.
- **Tile Colors**: Defines the list of colors available for the game tiles, affecting visual diversity and gameplay complexity.
- **Maximum Reshuffle Amount**: Specifies the upper limit on how many times the grid can be reshuffled automatically when no moves are available.

### AnimationConfig
`AnimationConfig` manages all settings related to the animations within the game:

- **Animations Enabled**: A toggle to enable or disable all animations, useful for testing game logic without visual distractions.
- **Swap Animation Duration**: Determines the duration of the animation when two tiles are swapped.
- **Tiles Fall Speed**: Sets the speed at which tiles fall during grid refills.
- **Delay After Refill**: Configures a delay time after tiles are refilled, which can be used to pause game interactions briefly.

## Core Components

### AnimationManager
Manages the queueing and execution of animation sequences, interfacing directly with DOTween to streamline all animations throughout the game.

### GridManager
Handles the logic related to tile management, including the spawning, swapping, matching, and overall grid updates.

### GameController
Acts as the central controller for the game.

## License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments
Special thanks to the Unity community and the developers of DOTween and UniTask for their excellent tools that enhance the animation capabilities and asynchronous programming of Unity projects. Their contributions make complex game mechanics more manageable and efficient.
