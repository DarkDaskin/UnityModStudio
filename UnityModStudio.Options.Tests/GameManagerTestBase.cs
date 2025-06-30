using Moq;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options.Tests;

public abstract class GameManagerTestBase
{
    [TestInitialize]
    public void TestInitialize()
    {
        AssemblyFixture.MockServiceProvider.Reset();
    }

    protected static IGameManager SetupGameManager(params Game[] games) =>
        Mock.Of<IGameManager>(gameManager =>
            gameManager.GameRegistry == Mock.Of<IGameRegistry>(gameRegistry => gameRegistry.Games == games));
}