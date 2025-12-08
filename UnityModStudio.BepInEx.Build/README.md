# Unity Mod Studio for BepInEx build tools
MSBuild targets for Unity Mod Studio for BepInEx projects.

An add-on package for [UnityModStudio.Build](https://www.nuget.org/packages/UnityModStudio.Build).

Designed to be used with the [Unity Mod Studio](https://marketplace.visualstudio.com/items?itemName=darkdaskin.UnityModStudio2022) extension for Visual Studio 2022.
Currently not very usable stand-alone because it requires the game registry which is managed by said extension.


## Implemented features
- Deploying and configuring the [BepInEx](https://github.com/BepInEx/BepInEx) mod loader.

## Game registry
The list of installed games is stored in `%LOCALAPPDATA%\UnityModStudio\GameRegistry.json`. You may copy this file to the same location on the target machine if you need to build your project on a machine which does not have the extension installed.