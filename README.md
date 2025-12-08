![build status](https://github.com/DarkDaskin/UnityModStudio/actions/workflows/dotnet-build.yml/badge.svg)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/darkdaskin.UnityModStudio2022?logo=visualstudio)
![NuGet Downloads](https://img.shields.io/nuget/dt/UnityModStudio.Build?logo=nuget)

# Unity Mod Studio
Allows creating mods for Unity games using Visual Studio 2022.

## Implemented features
- Creating a new project.
- Selecting a game to be modded using a project wizard.
- Determining proper target framework. 
- Referencing game assemblies.
- Deploying the mod into the game.
  - Deploying and configuring the [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) mod loader.
  - Deploying mod content files along compiled binaries.
- Resolving the game path from built-in game registry.
- Importing games from the list of installed programs or Steam.
- Debugging the mod.
- Building mods supporting multiple game versions.
- Game-specific templates and features (in separate extensions).
  - BepInEx 5 project template.

## Planned features
- Game-specific templates and features (in separate extensions).
  - BepInEx 6 project template.
  - RimWorld project template.

## Visual Studio support
Release 1.1 is the last release which supports Visual Studio 2019. New development will be focused on Visual Studio 2022.

Visual Studio 17.10 or newer is required.

Required workloads:
- .NET desktop development
- Game development with Unity

Additionally you may need to install targeting packs for .NET versions your games use.

# Contents
- A [Visual Studio 2022 extension](https://marketplace.visualstudio.com/items?itemName=darkdaskin.UnityModStudio2022) containing the project template and its wizard, options page for managing the game registry, and the debugger.
- The [`UnityModStudio.Build`](https://www.nuget.org/packages/UnityModStudio.Build/) NuGet package which resolves game assembly references and helps building and debugging the mod.
