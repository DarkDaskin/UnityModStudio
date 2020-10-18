# Unity Mod Studio
Allows creating mods for Unity games using Visual Studio 2019.

## Implemented features
- Creating a new project.
- Setting the base game path using a project wizard.
- Referencing game assemblies.

## Planned features
- Resolving the game path from Steam or Windows registry.
- Determining proper target framework.
- Deploying the mod into the game (patching game assemblies if needed).
- Debugging the mod.

# Contents
- A Visual Studio 2019 extension containing the project template and wizard.
- The `UnityModStudio.Build` NuGet package which resolves game assembly references and helps building the mod.