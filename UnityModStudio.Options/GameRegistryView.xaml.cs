namespace UnityModStudio.Options
{
    public partial class GameRegistryView
    {
        public GameRegistryViewModel ViewModel { get; }

        public GameRegistryView(GameRegistryViewModel viewModel)
        {
            InitializeComponent();

            DataContext = ViewModel = viewModel;
        }
    }
}
