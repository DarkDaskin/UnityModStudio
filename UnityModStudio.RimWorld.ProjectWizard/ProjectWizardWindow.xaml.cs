namespace UnityModStudio.RimWorld.ProjectWizard;

public partial class ProjectWizardWindow
{
    public ProjectWizardViewModel ViewModel { get; }

    public ProjectWizardWindow()
    {
        InitializeComponent();

        ViewModel = new ProjectWizardViewModel();
        DataContext = ViewModel;

        ViewModel.Closed += success =>
        {
            DialogResult = success;
            Close();
        };
    }
}