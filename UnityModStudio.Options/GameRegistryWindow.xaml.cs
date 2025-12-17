namespace UnityModStudio.Options;

public partial class GameRegistryWindow
{
    public GameRegistryWindowViewModel ViewModel { get; }

    public GameRegistryWindow(GameRegistryWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = ViewModel = viewModel;

        ViewModel.Closed += success =>
        {
            DialogResult = success;
            Close();
        };

        GameRegistryView.Content = new GameRegistryView(viewModel.InnerViewModel);
    }
}