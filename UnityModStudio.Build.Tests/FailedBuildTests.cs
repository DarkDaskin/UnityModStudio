using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tests;

[TestClass]
public class FailedBuildTests : BuildTestsBase
{
    [TestMethod]
    public void WhenNoGamePropertiesAreDefined_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\NoGameProperties\NoGameProperties.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game properties are defined.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenUnknownGameNameIsSpecified_ProduceError()
    {
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenGameWithUnknownVersionIsSpecified_ProduceError()
    {
        SetupGameRegistry(new Game
        {
            DisplayName = "Unity2018Test",
            Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
            GameName = "Unity2018Test",
        });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersion\SingleVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenMultiVersionGameWithUnknownVersionIsSpecified_ProduceError()
    {
        SetupGameRegistry(new Game
        {
            DisplayName = "Unity2018Test",
            Path = MakeGameCopy("2018-net4-v1.0"),
            GameName = "Unity2018Test",
            Version = "1.0",
        });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\MultiVersion\MultiVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("No game registry entries match speified game properties.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenMultipleGamesWithSameNameAreDefined_ProduceError()
    {
        SetupGameRegistry(new Game
            {
                DisplayName = "Unity2018Test",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
            },
            new Game
            {
                DisplayName = "Unity2018Test (2)",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
            });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("Multiple game registry entries match speified game properties:\n  'Unity2018Test'\n  'Unity2018Test (2)'", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenMultipleGamesWithSameVersionAreDefined_ProduceError()
    {
        SetupGameRegistry(new Game
            {
                DisplayName = "Unity2018Test",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
                Version = "1.0",
            },
            new Game
            {
                DisplayName = "Unity2018Test (2)",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
                Version = "1.0",
            });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\SingleVersion\SingleVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("Multiple game registry entries match speified game properties:\n  'Unity2018Test'\n  'Unity2018Test (2)'", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenMultipleGamesWithVersionAreDefinedAndNoVersionIsSpecified_ProduceError()
    {
        SetupGameRegistry(new Game
            {
                DisplayName = "Unity2018Test [1.0]",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
                Version = "1.0",
            },
            new Game
            {
                DisplayName = "Unity2018Test [1.1]",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
                GameName = "Unity2018Test",
                Version = "1.1",
            });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("Multiple game registry entries match speified game properties:\n  'Unity2018Test [1.0]'\n  'Unity2018Test [1.1]'", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenGamePathPointsToNonExistentDir_ProduceError()
    {
        SetupGameRegistry(new Game
        {
            DisplayName = "Unity2018Test",
            Path = @"C:\NonExistentDir",
            GameName = "Unity2018Test",
        });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("Game directory does not exist.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenGamePathPointsToNonGameDir_ProduceError()
    {
        SetupGameRegistry(new Game
        {
            DisplayName = "Unity2018Test",
            Path = @"C:\Program Files\Windows Mail",
            GameName = "Unity2018Test",
        });
        var (project, logger) = GetProjectWithRestore(@"Projects\Correct\NonVersioned\NonVersioned.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("Unable to determine game data directory.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenGameVersionIsNotInGameVersions_ProduceError()
    {
        SetupGameRegistry(new Game
            {
                DisplayName = "Unity2018Test [1.0]",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
                Version = "1.0",
            },
            new Game
            {
                DisplayName = "Unity2018Test [1.1]",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
                GameName = "Unity2018Test",
                Version = "1.1",
            });
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\WrongGameVersion\WrongGameVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("UMS0001", logger.BuildErrors[0].Code);
        Assert.AreEqual("Specified GameVersion must be one of specified GameVersions.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }

    [TestMethod]
    public void WhenDefaultGameVersionIsNotInGameVersions_ProduceError()
    {
        SetupGameRegistry(new Game
            {
                DisplayName = "Unity2018Test [1.0]",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.0"),
                GameName = "Unity2018Test",
                Version = "1.0",
            },
            new Game
            {
                DisplayName = "Unity2018Test [1.1]",
                Path = Path.Combine(SampleGameInfo.DownloadPath, "2018-net4-v1.1"),
                GameName = "Unity2018Test",
                Version = "1.1",
            });
        var (project, logger) = GetProjectWithRestore(@"Projects\Incorrect\WrongDefaultGameVersion\WrongDefaultGameVersion.csproj");

        var success = project.Build([logger, AssemblyFixture.BinaryLogger]);

        Assert.IsFalse(success);
        Assert.AreEqual(1, logger.BuildErrors.Count);
        Assert.AreEqual("UMS0001", logger.BuildErrors[0].Code);
        Assert.AreEqual("Specified DefaultGameVersion must be one of specified GameVersions.", logger.BuildErrors[0].Message);
        Assert.AreEqual(0, logger.BuildWarnings.Count);
    }
}