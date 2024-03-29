# Unity Mod Studio build tools
MSBuild tasks and targets for Unity Mod Studio projects.

Designed to be used with the [Unity Mod Studio](https://marketplace.visualstudio.com/items?itemName=darkdaskin.UnityModStudio2022) extension for Visual Studio 2022.
Currently not very usable stand-alone because it requires the game registry which is managed by said extension.


## Implemented features
- Resolving the game path from the game registry.
- Referencing game assemblies.
- Deploying the mod into the game.
  - Deploying and configuring the [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) mod loader.

## Game registry
The list of installed games is stored in `%LOCALAPPDATA%\UnityModStudio\GameRegistry.json`. You may copy this file to the same location on the target machine if you need to build your project on a machine which does not have the extension installed.