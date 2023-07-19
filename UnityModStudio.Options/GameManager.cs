using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    [InheritedExport]
    public interface IGameManager
    {
        IGameRegistry GameRegistry { get; }
        
        bool ShowEditDialog(Game game);
        IEnumerable<Game> ShowAddGamesDialog<TViewModel>() where TViewModel : AddGamesViewModelBase, new();
    }

    public class GameManager : IGameManager
    {
        private readonly ICompositionService _compositionService;

        [ImportingConstructor]
        public GameManager(ICompositionService compositionService, IGameRegistry gameRegistry)
        {
            _compositionService = compositionService;
            GameRegistry = gameRegistry;
        }

        public IGameRegistry GameRegistry { get; }

        public bool ShowEditDialog(Game game)
        {
            var viewModel = new GamePropertiesViewModel(game);
            _compositionService.SatisfyImportsOnce(viewModel);
            var window = new GamePropertiesWindow(viewModel);
            return window.ShowModal() ?? false;
        }

        public IEnumerable<Game> ShowAddGamesDialog<TViewModel>() where TViewModel : AddGamesViewModelBase, new()
        {
            var viewModel = new TViewModel();
            _compositionService.SatisfyImportsOnce(viewModel);
            var window = new AddGamesWindow(viewModel);
            return window.ShowModal() ?? false ? viewModel.SelectedGames : Enumerable.Empty<Game>();
        }
    }
}