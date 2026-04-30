![build status](https://github.com/DarkDaskin/UnityModStudio/actions/workflows/dotnet-build.yml/badge.svg)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/darkdaskin.UnityModStudio2022?logo=visualstudio)
![NuGet Downloads](https://img.shields.io/nuget/dt/UnityModStudio.Build?logo=nuget)

# Unity Mod Studio
Allows creating mods for Unity games using Visual Studio 2022+.

## Implemented features
### Core
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

### BepInEx
- BepInEx 5/6 project templates.

### RimWorld
- RimWorld project template.
- Resolving assembly references from other mods.

## Planned features
### Core
- Installing multiple versions of games in parallel from Steam.
### RimWorld
- XML files validation and IntelliSense.

## Visual Studio support
Release 1.1 is the last release which supports Visual Studio 2019. New development will be focused on Visual Studio 2022+.

Visual Studio 17.12 or newer is required.

Required workloads:
- .NET desktop development
- Game development with Unity

Additionally you may need to install targeting packs for .NET versions your games use.

# Contents
- The [Unity Mod Studio Visual Studio 2022+ extension](https://marketplace.visualstudio.com/items?itemName=darkdaskin.UnityModStudio2022) containing the generic project template and its wizard, options page for managing the game registry, and the debugger.
- The [Unity Mod Studio for BepInEx Visual Studio 2022+ extension](https://marketplace.visualstudio.com/items?itemName=darkdaskin.UnityModStudioForBepInEx) containing the BepInEx project templates and their wizard. 
- The [Unity Mod Studio for RimWorld Visual Studio 2022+ extension](https://marketplace.visualstudio.com/items?itemName=darkdaskin.UnityModStudioForRimWorld) containing the RimWorld project template and its wizard, and an options page. 
- The [`UnityModStudio.Build`](https://www.nuget.org/packages/UnityModStudio.Build/) NuGet package which resolves game assembly references and helps building and debugging the mod.
- The [`UnityModStudio.BepInEx.Build`](https://www.nuget.org/packages/UnityModStudio.BepInEx.Build/) NuGet package which helps building BepInEx mods.
- The [`UnityModStudio.RimWorld.Build`](https://www.nuget.org/packages/UnityModStudio.RimWorld.Build/) NuGet package which helps building RimWorld mods.
