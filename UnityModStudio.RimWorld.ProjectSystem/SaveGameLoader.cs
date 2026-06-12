using Microsoft.IO;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnityModStudio.ProjectSystem;

namespace UnityModStudio.RimWorld.ProjectSystem;

// This class assumes that RimWorld has been launched at least once before, so that the save game directory and config files have been created.
[Export(typeof(IProcessLifecycleParticipant))]
[AppliesTo(ProjectCapability.UnityModStudioForRimWorld)]
public class SaveGameLoader : IProcessLifecycleParticipant
{
    private const string AutoStartSaveFileName = "autostart.rws";
    private const string BackupExtension = ".bak";

    private static readonly XmlReaderSettings XmlReaderSettings = new() { Async = true };

    private readonly string _saveGameDirectoryPath;
    private readonly string _modsConfigFilePath;
    private readonly string _prefsFilePath;
    private FileInfo? _autoStartFile;
    private FileInfo? _autoStartBackupFile;
    private FileInfo? _modsConfigFile;
    private FileInfo? _modsConfigBackupFile;
    private bool _isSetUp;
    private bool _isDevModeSetUp;

    public SaveGameLoader()
    {
        var dataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "AppData", "LocalLow", "Ludeon Studios", "RimWorld by Ludeon Studios");
        _saveGameDirectoryPath = Path.Combine(dataDirectoryPath, "Saves");
        var configDirectoryPath = Path.Combine(dataDirectoryPath, "Config");
        _modsConfigFilePath = Path.Combine(configDirectoryPath, "ModsConfig.xml");
        _prefsFilePath = Path.Combine(configDirectoryPath, "Prefs.xml");
    }

    public async Task OnBeforeLaunchAsync(ILaunchProfile profile, ProcessStartInfo processStartInfo)
    {
        if (profile.OtherSettings == null || !profile.OtherSettings.TryGetValue("SaveFile", out var saveFileNameObj) || saveFileNameObj is not string saveFileName)
            return;

        var saveFile = new FileInfo(Path.Combine(_saveGameDirectoryPath, saveFileName));

        if (!saveFile.Exists)
            return;

        SetUpAutostartFile(saveFile);

        await SetUpModListAsync(saveFile);

        _isDevModeSetUp = await SetDevModeAsync(true);

        _isSetUp = true;
    }

    private void SetUpAutostartFile(FileInfo saveFile)
    {
        _autoStartFile = new FileInfo(Path.Combine(_saveGameDirectoryPath, AutoStartSaveFileName));
        _autoStartBackupFile = new FileInfo(_autoStartFile.FullName + BackupExtension);

        if (_autoStartFile.Exists)
            _autoStartFile.MoveTo(_autoStartBackupFile.FullName, true);

        saveFile.CopyTo(_autoStartFile.FullName, true);
    }

    private async Task SetUpModListAsync(FileInfo saveFile)
    {
        var modList = await GetModListFromSaveAsync(saveFile.FullName);

        _modsConfigFile = new FileInfo(_modsConfigFilePath);
        _modsConfigBackupFile = new FileInfo(_modsConfigFilePath + BackupExtension);

        if (!_modsConfigFile.Exists)
            return;

        _modsConfigFile.CopyTo(_modsConfigBackupFile.FullName, true);

        var modsConfigText = await File.ReadAllTextAsync(_modsConfigFilePath);
        var modsConfigDocument = XDocument.Parse(modsConfigText);

        var activeModsElement = modsConfigDocument.Root?.Element("activeMods") ?? new XElement("activeMods");
        if (activeModsElement.Document == null)
            modsConfigDocument.Root?.Add(activeModsElement);

        activeModsElement.RemoveAll();
        foreach (var modId in modList)
            activeModsElement.Add(new XElement("li", modId));

        modsConfigText = modsConfigDocument.ToString();
        await File.WriteAllTextAsync(_modsConfigFilePath, modsConfigText);
    }

    private static async Task<IReadOnlyList<string>> GetModListFromSaveAsync(string saveFilePath)
    {
        // Use XmlReader instead of XDocument to avoid loading the entire save file into memory, which can amount to hundreds of megabytes.
        using var reader = XmlReader.Create(saveFilePath, XmlReaderSettings);
        var modList = new List<string>();
        var nodePath = new List<string>();
        
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Text)
                if (nodePath is ["savegame", "meta", "modIds", "li"])
                    modList.Add(await reader.ReadContentAsStringAsync());

            if (reader.NodeType == XmlNodeType.Element)
                nodePath.Add(reader.Name);

            if (reader.NodeType == XmlNodeType.EndElement)
            {
                // We can stop reading once we exit the modIds node, as we only care about the mod list, and it should be near the beginning of the file.
                if (nodePath is ["savegame", "meta", "modIds"])
                    break;

                nodePath.RemoveAt(nodePath.Count - 1);
            }
        }

        return modList;
    }
    
    private async Task<bool> SetDevModeAsync(bool value)
    {
        if (!File.Exists(_prefsFilePath))
            return false;

        var prefsText = await File.ReadAllTextAsync(_prefsFilePath);
        var prefsDocument = XDocument.Parse(prefsText);

        var devModeElement = prefsDocument.Root?.Element("devMode") ?? new XElement("devMode");
        if (devModeElement.Document == null)
            prefsDocument.Root?.Add(devModeElement);

        if (devModeElement.Value == value.ToString())
            return false;

        devModeElement.Value = value.ToString();

        prefsText = prefsDocument.ToString();
        await File.WriteAllTextAsync(_prefsFilePath, prefsText);

        return true;
    }

    public Task OnAfterLaunchAsync(ILaunchProfile profile, Process process) => Task.CompletedTask;

    public async Task OnAfterExitAsync(ILaunchProfile profile, Process process)
    {
        if (!_isSetUp)
            return;

        TearDownAutostartFile();

        TearDownModsConfig();

        if (_isDevModeSetUp)
        {
            await SetDevModeAsync(false);
            _isDevModeSetUp = false;
        }

        _isSetUp = false;
    }

    private void TearDownAutostartFile()
    {
        _autoStartFile!.Refresh();
        _autoStartBackupFile!.Refresh();

        if (_autoStartBackupFile!.Exists)
            _autoStartBackupFile.MoveTo(_autoStartFile!.FullName, true);
        else if (_autoStartFile!.Exists)
            _autoStartFile.Delete();
    }

    private void TearDownModsConfig()
    {
        _modsConfigBackupFile!.Refresh();

        if (_modsConfigBackupFile!.Exists)
            _modsConfigBackupFile.MoveTo(_modsConfigFile!.FullName, true);
    }
}