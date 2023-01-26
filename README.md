# Unity Mod Studio
Allows creating mods for Unity games using Visual Studio 2022.

## Implemented features
- Creating a new project.
- Setting the base game path using a project wizard.
- Determining proper target framework. 
- Referencing game assemblies.
- Deploying the mod into the game.
  - Deploying and configuring the [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) mod loader.
- Resolving the game path from built-in game registry.
- Debugging the mod.

## Planned features
- Resolving the game path from Steam or Windows registry.

## Visual Studio support
Release 1.1 is the last release which supports Visual Studio 2019. New development will be focused on Visual Studio 2022.

# Contents
- A Visual Studio 2022 extension containing the project template and wizard.
- The `UnityModStudio.Build` NuGet package which resolves game assembly references and helps building and debugging the mod.
