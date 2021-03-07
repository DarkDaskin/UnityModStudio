namespace UnityModStudio.Options
{
    public partial class GamePropertiesWindow
    {
        public GamePropertiesViewModel ViewModel { get; }
        

        public GamePropertiesWindow(GamePropertiesViewModel viewModel)
        {
            InitializeComponent();

            DataContext = ViewModel = viewModel;

            ViewModel.Closed += success =>
            {
                DialogResult = success;
                Close();
            };
        }
    }
}
