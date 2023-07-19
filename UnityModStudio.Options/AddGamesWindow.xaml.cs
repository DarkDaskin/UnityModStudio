using System.Collections.Specialized;
using System.Windows.Controls;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    public partial class AddGamesWindow
    {
        private bool _isUpdatingSelection;

        public AddGamesViewModelBase ViewModel { get; }

        public AddGamesWindow(AddGamesViewModelBase viewModel)
        {
            InitializeComponent();

            DataContext = ViewModel = viewModel;

            ViewModel.Closed += success =>
            {
                DialogResult = success;
                Close();
            };

            ViewModel.SelectedGames.CollectionChanged += SelectedGames_CollectionChanged;
        }

        private void SelectedGames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingSelection)
                return;

            _isUpdatingSelection = true;
            GameList.SelectedItems.Clear();
            foreach (var game in ViewModel.SelectedGames)
                GameList.SelectedItems.Add(game);
            _isUpdatingSelection = false;
        }

        private void GameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection)
                return;

            _isUpdatingSelection = true;
            using (ViewModel.SelectedGames.SuspendChangeNotification())
            {
                ViewModel.SelectedGames.Clear();
                foreach (Game game in GameList.SelectedItems)
                    ViewModel.SelectedGames.Add(game);
            }
            _isUpdatingSelection = false;
        }
    }
}
