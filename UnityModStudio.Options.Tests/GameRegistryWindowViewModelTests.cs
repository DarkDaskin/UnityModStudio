using Moq;

namespace UnityModStudio.Options.Tests;

[TestClass]
public class GameRegistryWindowViewModelTests : GameManagerTestBase
{
    [TestMethod]
    public void WhenCreated_InitialStateIsCorrect()
    {
        var vm = new GameRegistryWindowViewModel(new GameRegistryViewModel());

        Assert.IsTrue(vm.ConfirmCommand.CanExecute(null));
        Assert.IsTrue(vm.CancelCommand.CanExecute(null));
    }

    [TestMethod]
    public void WhenLoadGamesIsInvoked_LoadGameRegistry()
    {
        var innerVm = new GameRegistryViewModel
        {
            GameManager = SetupGameManager()
        };
        Mock.Get(innerVm.GameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.LoadAsync())
            .Returns(Task.CompletedTask);
        var vm = new GameRegistryWindowViewModel(innerVm);

        vm.LoadGames();

        Mock.Get(innerVm.GameManager.GameRegistry).VerifyAll();
    }

    [TestMethod]
    public void WhenConfirmIsInvoked_SaveGameRegistry()
    {
        var innerVm = new GameRegistryViewModel
        {
            GameManager = SetupGameManager()
        };
        var vm = new GameRegistryWindowViewModel(innerVm);
        Mock.Get(innerVm.GameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.SaveAsync())
            .Returns(Task.CompletedTask);

        vm.ConfirmCommand.Execute(null);

        Mock.Get(innerVm.GameManager.GameRegistry).VerifyAll();
    }

    [TestMethod]
    public void WhenCancelIsInvoked_ReloadGameRegistry()
    {
        var innerVm = new GameRegistryViewModel
        {
            GameManager = SetupGameManager()
        };
        Mock.Get(innerVm.GameManager.GameRegistry)
            .Setup(gameRegistry => gameRegistry.LoadAsync())
            .Returns(Task.CompletedTask);
        var vm = new GameRegistryWindowViewModel(innerVm);

        vm.CancelCommand.Execute(null);

        Mock.Get(innerVm.GameManager.GameRegistry).VerifyAll();
    }
}