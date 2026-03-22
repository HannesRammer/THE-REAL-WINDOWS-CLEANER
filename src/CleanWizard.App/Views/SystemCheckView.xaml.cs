using System.Windows.Controls;

namespace CleanWizard.App.Views;

public partial class SystemCheckView : UserControl
{
    public SystemCheckView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is ViewModels.SystemCheckViewModel vm)
                await vm.EnsureLoadedAsync();
        };
    }
}
