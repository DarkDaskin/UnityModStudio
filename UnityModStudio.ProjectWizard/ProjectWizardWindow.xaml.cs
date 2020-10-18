namespace UnityModStudio.ProjectWizard
{
    public partial class ProjectWizardWindow
    {
        public ProjectWizardViewModel ViewModel { get; }

        public ProjectWizardWindow()
        {
            InitializeComponent();

            ViewModel = new ProjectWizardViewModel(new WindowsFormsWindowAdapter(this));
            DataContext = ViewModel;

            ViewModel.Closed += success =>
            {
                DialogResult = success;
                Close();
            };
        }
    }
}
