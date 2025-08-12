using System.Text.Json;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Common.Tests;

[TestClass]
public sealed class GameRegistryTests : StoreTestsBase
{
    [TestMethod]
    public void WhenCreated_BeEmpty()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);

        Assert.AreEqual(0, gameRegistry.Games.Count);
        Assert.IsFalse(gameRegistry.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromEmptyFile_BeEmpty()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);

        await gameRegistry.LoadAsync();

        Assert.AreEqual(0, gameRegistry.Games.Count);
        Assert.IsFalse(gameRegistry.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromNonExistingFile_BeEmpty()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        File.Delete(StorePath);

        await gameRegistry.LoadAsync();

        Assert.AreEqual(0, gameRegistry.Games.Count);
        Assert.IsFalse(gameRegistry.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenLoadedFromMalformedFile_Throw()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        await File.WriteAllTextAsync(StorePath, "!@$%^&*()");

        await Assert.ThrowsExactlyAsync<JsonException>(() => gameRegistry.LoadAsync());
    }

    [TestMethod]
    public async Task WhenLoadedFromValidFile_FillWithGames()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");

        await gameRegistry.LoadAsync();

        Assert.AreEqual(3, gameRegistry.Games.Count);

        var game1 = gameRegistry.Games.ElementAt(0);
        Assert.AreEqual(new Guid("91eda532-02c2-441c-808d-07a474692ede"), game1.Id);
        Assert.AreEqual("Game 1", game1.DisplayName);
        Assert.AreEqual(@"C:\Games\Game1", game1.Path);
        Assert.IsNull(game1.ModsPath);
        Assert.IsNull(game1.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game1.ModDeploymentMode);
        Assert.IsFalse(game1.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.DebuggingAndModLoading, game1.DoorstopMode);
        Assert.IsFalse(game1.UseAlternateDoorstopDllName);
        Assert.AreEqual("Game1", game1.GameName);
        Assert.AreEqual("Game1.exe", game1.GameExecutableFileName);
        Assert.AreEqual("X64", game1.Architecture);
        Assert.AreEqual("2022.3.22f1", game1.UnityVersion);
        Assert.IsNull(game1.TargetFrameworkMoniker);
        Assert.AreEqual(".NET Standard 2.1", game1.MonoProfile);

        var game2 = gameRegistry.Games.ElementAt(1);
        Assert.AreEqual(new Guid("01144589-1c88-4c52-91a4-e6cc2998f35c"), game2.Id);
        Assert.AreEqual("Game 2 v1.0", game2.DisplayName);
        Assert.AreEqual(@"C:\Games\Game2_1", game2.Path);
        Assert.AreEqual("Mods", game2.ModsPath);
        Assert.AreEqual("1.0", game2.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game2.ModDeploymentMode);
        Assert.IsTrue(game2.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game2.DoorstopMode);
        Assert.IsTrue(game2.UseAlternateDoorstopDllName);
        Assert.AreEqual("Game2", game2.GameName);
        Assert.AreEqual("Game2.exe", game2.GameExecutableFileName);
        Assert.AreEqual("X64", game2.Architecture);
        Assert.AreEqual("2018.4.36f1", game2.UnityVersion);
        Assert.IsNull(game2.TargetFrameworkMoniker);
        Assert.AreEqual(".NET 4.6", game2.MonoProfile);

        var game3 = gameRegistry.Games.ElementAt(2);
        Assert.AreEqual(new Guid("84d1866f-c4db-4d27-ad31-3b1fc9f34fd4"), game3.Id);
        Assert.AreEqual("Game 2 v2.0", game3.DisplayName);
        Assert.AreEqual(@"C:\Games\Game2_2", game3.Path);
        Assert.AreEqual("Mods", game3.ModsPath);
        Assert.AreEqual("2.0", game3.Version);
        Assert.AreEqual(ModDeploymentMode.Copy, game3.ModDeploymentMode);
        Assert.IsTrue(game3.DeploySourceCode);
        Assert.AreEqual(DoorstopMode.Debugging, game3.DoorstopMode);
        Assert.IsTrue(game3.UseAlternateDoorstopDllName);
        Assert.AreEqual("Game2", game3.GameName);
        Assert.AreEqual("Game2.exe", game3.GameExecutableFileName);
        Assert.AreEqual("X64", game3.Architecture);
        Assert.AreEqual("2018.4.36f1", game3.UnityVersion);
        Assert.AreEqual("net46", game3.TargetFrameworkMoniker);
        Assert.AreEqual(".NET 4.6", game3.MonoProfile);

        Assert.IsFalse(gameRegistry.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenSavingGame_WriteJson()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        gameRegistry.AddGame(new Game
        {
            Id = new Guid("b875ba73-84e8-4a51-a305-20edfa5d58f6"),
            DisplayName = "Game 1",
            Path = @"C:\Games\Game1",
        });

        await gameRegistry.SaveAsync();

        VerifyStoreEquals("GameRegistry_SingleGameSaved.json");
    }

    [TestMethod]
    public void WhenEnsureAllGamePropertiesInvoked_ResolveAllProperties()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        var game = new Game
        {
            DisplayName = "Game 1",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            GameName = "Unity2018Test",
        };
        gameRegistry.AddGame(game);
        
        gameRegistry.EnsureAllGameProperties(game);

        Assert.AreEqual("Unity2018Test", game.GameName);
        Assert.AreEqual("Unity2018Test.exe", game.GameExecutableFileName);
        Assert.AreEqual("X64", game.Architecture);
        Assert.AreEqual("2018.4.36f1", game.UnityVersion);
        Assert.AreEqual("net472", game.TargetFrameworkMoniker);
        Assert.AreEqual(".NET 4.7.2", game.MonoProfile);
    }

    [TestMethod]
    public void WhenAddingGame_AddToGames()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);

        var game = new Game
        {
            Id = new Guid("b875ba73-84e8-4a51-a305-20edfa5d58f6"),
            DisplayName = "Game 1",
            Path = @"C:\Games\Game1",
        };
        gameRegistry.AddGame(game);

        Assert.AreEqual(1, gameRegistry.Games.Count);
        Assert.AreEqual(game, gameRegistry.Games.Single());
    }

    [TestMethod]
    public async Task WhenRemovingGame_RemoveFromGames()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();
        var game1 = gameRegistry.Games.ElementAt(0);
        var game2 = gameRegistry.Games.ElementAt(1);
        var game3 = gameRegistry.Games.ElementAt(2);

        gameRegistry.RemoveGame(game2);

        Assert.AreEqual(2, gameRegistry.Games.Count);
        Assert.AreEqual(game1, gameRegistry.Games.ElementAt(0));
        Assert.AreEqual(game3, gameRegistry.Games.ElementAt(1));
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameById_ReturnGame()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var game = gameRegistry.FindGameById(new Guid("91eda532-02c2-441c-808d-07a474692ede"));

        Assert.IsNotNull(game);
        Assert.AreEqual("Game 1", game.DisplayName);
    }

    [TestMethod]
    public async Task WhenSearchingNonExistingGameById_ReturnNull()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var game = gameRegistry.FindGameById(new Guid("7436e934-de45-4784-8c0f-40b8c7f4777f"));

        Assert.IsNull(game);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByDisplayName_ReturnGame()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var game = gameRegistry.FindGameByDisplayName("Game 1");

        Assert.IsNotNull(game);
        Assert.AreEqual(new Guid("91eda532-02c2-441c-808d-07a474692ede"), game.Id);
    }

    [TestMethod]
    public async Task WhenSearchingNonExistingGameByDisplayName_ReturnNull()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var game = gameRegistry.FindGameByDisplayName("NonExistentGame");

        Assert.IsNull(game);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByGameName_ReturnMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"GameName", "Game1"},
        }, true);

        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.Match match);
        Assert.AreEqual(new Guid("91eda532-02c2-441c-808d-07a474692ede"), match.Game.Id);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByGameNameAndVersion_ReturnMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"GameName", "Game2"},
            {"Version", "2.0"},
        }, true);

        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.Match match);
        Assert.AreEqual(new Guid("84d1866f-c4db-4d27-ad31-3b1fc9f34fd4"), match.Game.Id);
    }

    [TestMethod]
    public async Task WhenSearchingNonExistingGameByGameName_ReturnNoMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"GameName", "NonExistentGame"},
        }, true);

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.NoMatch _);
    }

    [TestMethod]
    public async Task WhenSearchingMultiVersionGameByGameName_ReturnAmbiguousMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"GameName", "Game2"},
        }, true);

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.AmbiguousMatch match);
        Assert.AreEqual("", match.Message);
        Assert.AreEqual(2, match.Games.Count);
        Assert.AreEqual(new Guid("01144589-1c88-4c52-91a4-e6cc2998f35c"), match.Games.ElementAt(0).Id);
        Assert.AreEqual(new Guid("84d1866f-c4db-4d27-ad31-3b1fc9f34fd4"), match.Games.ElementAt(1).Id);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByAllProperties_ReturnMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"Id", "91eda532-02c2-441c-808d-07a474692ede"},
            {"DisplayName", "Game 1"},
            {"GameName", "Game1"},
        }, true);

        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.Match match);
        Assert.AreEqual(new Guid("91eda532-02c2-441c-808d-07a474692ede"), match.Game.Id);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByWrongIdAndGameNameLoose_ReturnMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"Id", "4aaec050-2ee4-434d-a5b2-5c164862f7f0"},
            {"GameName", "Game1"},
        }, false);

        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.Match match);
        Assert.AreEqual(new Guid("91eda532-02c2-441c-808d-07a474692ede"), match.Game.Id);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByWrongIdAndGameNameStrict_ReturnNoMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"Id", "4aaec050-2ee4-434d-a5b2-5c164862f7f0"},
            {"GameName", "Game1"},
        }, true);

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.NoMatch _);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByWrongDisplayNameAndGameNameLoose_ReturnMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"DisplayName", "Game"},
            {"GameName", "Game1"},
        }, false);

        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.Match match);
        Assert.AreEqual(new Guid("91eda532-02c2-441c-808d-07a474692ede"), match.Game.Id);
    }

    [TestMethod]
    public async Task WhenSearchingExistingGameByWrongDisplayNameAndGameNameStrict_ReturnNoMatch()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        SetStoreFile("GameRegistry_Initial.json");
        await gameRegistry.LoadAsync();

        var result = gameRegistry.FindGameByProperties(new Dictionary<string, string>
        {
            {"DisplayName", "Game"},
            {"GameName", "Game1"},
        }, true);

        Assert.IsFalse(result.Success);
        Assert.IsInstanceOfType(result, out GameMatchResult.NoMatch _);
    }

    [TestMethod]
    public void WhenEnablingWatchWithMissingGameRegistry_Succeed()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        File.Delete(StorePath);

        gameRegistry.WatchForChanges = true;

        Assert.IsTrue(gameRegistry.WatchForChanges);
    }

    [TestMethod]
    public async Task WhenEnablingWatch_ReloadOnExternalChange()
    {
        IGameRegistry gameRegistry = new GameRegistry(StorePath);
        IGameRegistry gameRegistry2 = new GameRegistry(StorePath);

        gameRegistry.WatchForChanges = true;
        gameRegistry2.AddGame(new Game
        {
            Id = new Guid("b875ba73-84e8-4a51-a305-20edfa5d58f6"),
            DisplayName = "Game 1",
            Path = @"C:\Games\Game1",
        });
        await gameRegistry2.SaveAsync();
        await Task.Delay(200);

        Assert.AreEqual(1, gameRegistry.Games.Count);
        Assert.AreEqual("Game 1", gameRegistry.Games.Single().DisplayName);
    }
}