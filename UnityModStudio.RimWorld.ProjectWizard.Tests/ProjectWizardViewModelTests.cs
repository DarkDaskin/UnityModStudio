using Moq;
using UnityModStudio.Common.Options;
using UnityModStudio.Options;
using UnityModStudio.Options.Tests;
using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.ProjectWizard.Tests;

[TestClass]
public sealed class ProjectWizardViewModelTests : GameManagerTestBase
{
    [TestMethod]
    public void WhenCreated_InitialStateIsCorrect()
    {
        var vm = new ProjectWizardViewModel();

        Assert.IsNull(vm.GameManager);
        Assert.IsNull(vm.RimWorldSettingsManager);
        Assert.IsNull(vm.ProjectName);
        Assert.IsEmpty(vm.GameVersions);
        Assert.IsTrue(vm.AreGameVersionsAbsent);
        Assert.IsNull(vm.ModPackageId);
        Assert.IsNull(vm.ModAuthor);
        Assert.IsNull(vm.ModName);
        Assert.IsNull(vm.ModDescription);
        Assert.IsFalse(vm.UseHarmony);
        Assert.AreEqual(ProjectLayout.AssetsAtTopLevel, vm.ProjectLayout);
        Assert.IsTrue(vm.HasErrors);
        Assert.AreEqual("Mod package ID is required.", vm.GetErrors(nameof(ProjectWizardViewModel.ModPackageId)).Single());
        Assert.AreEqual("Mod author is required.", vm.GetErrors(nameof(ProjectWizardViewModel.ModAuthor)).Single());
        Assert.AreEqual("Mod name is required.", vm.GetErrors(nameof(ProjectWizardViewModel.ModName)).Single());
        Assert.AreEqual("Mod description is required.", vm.GetErrors(nameof(ProjectWizardViewModel.ModDescription)).Single());
        Assert.IsTrue(vm.OpenGameRegistryCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
    }

    [TestMethod]
    public void WhenInitialized_InitialStateIsCorrectAndStoresAreLoaded()
    {
        var game1 = new Game { DisplayName = "Game 1", GameName = "Game1" };
        var game2 = new Game { DisplayName = "RimWorld", GameName = "RimWorld by Ludeon Studios" };
        var game3 = new Game { DisplayName = "RimWorld 1.5", GameName = "RimWorld by Ludeon Studios", Version = "1.5" };
        var game4 = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(game1, game2, game3, game4), 
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "TestMod"
        };

        Assert.HasCount(2, vm.GameVersions);
        Assert.AreEqual(game3, vm.GameVersions[0].Game);
        Assert.AreEqual("1.5", vm.GameVersions[0].Version);
        Assert.IsFalse(vm.GameVersions[0].IsSelected);
        Assert.AreEqual(game4, vm.GameVersions[1].Game);
        Assert.AreEqual("1.6", vm.GameVersions[1].Version);
        Assert.IsFalse(vm.GameVersions[1].IsSelected);
        Assert.IsFalse(vm.AreGameVersionsAbsent);
        Assert.AreEqual("TestMod", vm.ModPackageId);
        Assert.AreEqual(Environment.UserName, vm.ModAuthor);
        Assert.AreEqual("TestMod", vm.ModName);
        Assert.AreEqual("TestMod", vm.ModDescription);
        Assert.IsTrue(vm.UseHarmony);
        Assert.AreEqual(ProjectLayout.AssetsAtTopLevel, vm.ProjectLayout);
        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.OpenGameRegistryCommand.CanExecute(null));
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));

        Mock.Get(vm.GameManager).VerifyAll();
        Mock.Get(vm.GameManager).VerifyNoOtherCalls();
        Mock.Get(vm.RimWorldSettingsManager).VerifyAll();
        Mock.Get(vm.RimWorldSettingsManager).VerifyNoOtherCalls();
    }

    [TestMethod]
    public void WhenProjectNameIsSetAndDefaultPackageIdPrefixIsNotSet_SetPackageIdToProjectName()
    {
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(), 
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
        };

        vm.ProjectName = "MyMod";

        Assert.IsFalse(vm.HasErrors);
        Assert.AreEqual("MyMod", vm.ModPackageId);
    }

    [TestMethod]
    public void WhenProjectNameIsSetAndDefaultPackageIdPrefixIsSet_SetPackageIdToPrefixAndProjectName()
    {
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(new RimWorldSettings
            {
                ProjectDefaults = { ModPackageIdPrefix = "MyCompany" }
            }),
        };

        vm.ProjectName = "MyMod";

        Assert.IsFalse(vm.HasErrors);
        Assert.AreEqual("MyCompany.MyMod", vm.ModPackageId);
    }

    [TestMethod]
    public void WhenProjectNameIsSetWithPrefixAndDefaultPackageIdPrefixIsSet_SetPackageIdProjectName()
    {
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(new RimWorldSettings
            {
                ProjectDefaults = { ModPackageIdPrefix = "MyCompany" }
            }),
        };

        vm.ProjectName = "MyCompany.MyMod";

        Assert.IsFalse(vm.HasErrors);
        Assert.AreEqual("MyCompany.MyMod", vm.ModPackageId);
    }

    [TestMethod]
    public void WhenModPackageIdIsRemoved_ProduceError()
    {
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "MyMod",
        };

        vm.ModPackageId = "";

        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(ProjectWizardViewModel.ModPackageId)).SequenceEqual(["Mod package ID is required."]));
    }

    [TestMethod]
    public void WhenPropertiesAreSetToCorrectValues_ProduceNotificationsAndNoErrors()
    {
        var changedPropertyNames = new HashSet<string>();
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "MyMod",
        };
        vm.PropertyChanged += (_, args) => changedPropertyNames.Add(args.PropertyName!);

        vm.ModPackageId = "Author.MyMod";
        vm.ModAuthor = "Author";
        vm.ModName = "My Mod";
        vm.ModDescription = "This is a test mod.";
        vm.UseHarmony = false;
        vm.ProjectLayout = ProjectLayout.ProjectAtTopLevel;

        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(changedPropertyNames.SetEquals([
            nameof(ProjectWizardViewModel.ModPackageId),
            nameof(ProjectWizardViewModel.ModAuthor),
            nameof(ProjectWizardViewModel.ModName),
            nameof(ProjectWizardViewModel.ModDescription),
            nameof(ProjectWizardViewModel.UseHarmony),
            nameof(ProjectWizardViewModel.ProjectLayout),
        ]));
    }

    [TestMethod]
    public void WhenNoGamesAreSelected_DontAllowToConfirm()
    {
        var closedInvocations = new List<bool>();
        var game = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(game),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "MyMod",
        };
        vm.Closed += success => closedInvocations.Add(success);

        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));

        vm.ConfirmCommand.Execute(null);

        Assert.IsEmpty(closedInvocations);
    }

    [TestMethod]
    public void WhenPropertiesAreInvalid_DontAllowToConfirm()
    {
        var closedInvocations = new List<bool>();
        var game = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(game),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "MyMod",
        };
        vm.Closed += success => closedInvocations.Add(success);

        vm.GameVersions[0].IsSelected = true;
        vm.ModPackageId = "";

        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));

        vm.ConfirmCommand.Execute(null);

        Assert.IsEmpty(closedInvocations);
    }

    [TestMethod]
    public void WhenConfirmIsInvoked_CloseWindow()
    {
        var closedInvocations = new List<bool>();
        var game = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(game),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "MyMod",
        };
        vm.Closed += success => closedInvocations.Add(success);

        vm.GameVersions[0].IsSelected = true;
        vm.ModName = "My Mod";
        vm.ModAuthor = "Author";
        vm.ModDescription = "This is test mod.";

        Assert.IsTrue(vm.ConfirmCommand.CanExecute(null));

        vm.ConfirmCommand.Execute(null);

        Assert.IsTrue(closedInvocations.SequenceEqual([true]));
    }

    [TestMethod]
    public void WhenCancelIsInvoked_CloseWindow()
    {
        var closedInvocations = new List<bool>();
        var game = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(game),
            RimWorldSettingsManager = SetupRimWorldSettingsManagerWithLoad(),
            ProjectName = "MyMod",
        };
        vm.Closed += success => closedInvocations.Add(success);

        Assert.IsTrue(vm.CancelCommand.CanExecute(null));

        vm.CancelCommand.Execute(null);

        Assert.IsTrue(closedInvocations.SequenceEqual([false]));
    }
    
    [TestMethod]
    public void WhenNoGameIsSelected_ReturnEmptyArray()
    {
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManager(),
        };

        var selectedGames = vm.GetSelectedGames();

        Assert.IsEmpty(selectedGames);
    }

    [TestMethod]
    public void WhenGamesAreSelected_ReturnGames()
    {
        var game1 = new Game { DisplayName = "RimWorld 1.4", GameName = "RimWorld by Ludeon Studios", Version = "1.4" };
        var game2 = new Game { DisplayName = "RimWorld 1.5", GameName = "RimWorld by Ludeon Studios", Version = "1.5" };
        var game3 = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManager(game1, game2, game3),
        };

        vm.GameVersions[1].IsSelected = true;
        vm.GameVersions[2].IsSelected = true;
        var selectedGames = vm.GetSelectedGames();

        Assert.HasCount(2, selectedGames);
        Assert.AreEqual(game2, selectedGames[0]);
        Assert.AreEqual(game3, selectedGames[1]);
    }

    [TestMethod]
    public void WhenOpenGameRegistryIsInvokedAndDialogIsCofirmed_UpdateGameVersions()
    {
        var changedPropertyNames = new HashSet<string>();
        var game = new Game { DisplayName = "RimWorld 1.6", GameName = "RimWorld by Ludeon Studios", Version = "1.6" };
        var vm = new ProjectWizardViewModel
        {
            GameManager = SetupGameManagerWithLoad(game),
        };
        vm.PropertyChanged += (_, args) => changedPropertyNames.Add(args.PropertyName!);
        Mock.Get(vm.GameManager).Setup(gameManager => gameManager.ShowGameRegistryDialog()).Returns(true);

        vm.OpenGameRegistryCommand.Execute(null);

        Assert.IsTrue(changedPropertyNames.SetEquals([
            nameof(ProjectWizardViewModel.GameVersions), 
            nameof(ProjectWizardViewModel.AreGameVersionsAbsent)]));
        Mock.Get(vm.GameManager).VerifyAll();
    }

    private static IGameManager SetupGameManagerWithLoad(params Game[] games)
    {
        var gameManager = SetupGameManager(games);
        Mock.Get(gameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.LoadAsync())
            .Returns(Task.CompletedTask);
        return gameManager;
    }

    private static IRimWorldSettingsManager SetupRimWorldSettingsManager(RimWorldSettings? settings = null) =>
        Mock.Of<IRimWorldSettingsManager>(settingsManager => settingsManager.Settings == (settings ?? new RimWorldSettings()));

    private static IRimWorldSettingsManager SetupRimWorldSettingsManagerWithLoad(RimWorldSettings? settings = null)
    {
        var settingsManager = SetupRimWorldSettingsManager(settings);
        Mock.Get(settingsManager)
            .Setup(gsm => gsm.LoadAsync())
            .Returns(Task.CompletedTask);
        return settingsManager;
    }
}
