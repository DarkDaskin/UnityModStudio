using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options;

[InheritedExport]
public interface IGameManager
{
    IGameRegistry GameRegistry { get; }
        
    bool ShowEditDialog(Game game);
    IEnumerable<Game> ShowAddGamesDialog<TViewModel>() where TViewModel : AddGamesViewModelBase, new();
    bool ShowGameRegistryDialog();
}

[method: ImportingConstructor]
public class GameManager(ICompositionService compositionService, IGameRegistry gameRegistry) : IGameManager
{
    public IGameRegistry GameRegistry { get; } = gameRegistry;

    public bool ShowEditDialog(Game game)
    {
        var viewModel = new GamePropertiesViewModel(game);
        compositionService.SatisfyImportsOnce(viewModel);
        var window = new GamePropertiesWindow(viewModel);
        return window.ShowModal() ?? false;
    }

    public IEnumerable<Game> ShowAddGamesDialog<TViewModel>() where TViewModel : AddGamesViewModelBase, new()
    {
        var viewModel = new TViewModel();
        compositionService.SatisfyImportsOnce(viewModel);
        var window = new AddGamesWindow(viewModel);
        return window.ShowModal() ?? false ? viewModel.SelectedGames : Enumerable.Empty<Game>();
    }

    public bool ShowGameRegistryDialog()
    {
        var innerViewModel = new GameRegistryViewModel();
        compositionService.SatisfyImportsOnce(innerViewModel);
        var viewModel = new GameRegistryWindowViewModel(innerViewModel);
        var window = new GameRegistryWindow(viewModel);
        return window.ShowModal() ?? false;
    }
}