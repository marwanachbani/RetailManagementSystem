using System.Windows;
using RMS.WPF.ViewModels;

namespace RMS.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestLogout += (_, _) =>
        {
            Close();
            ((App)Application.Current).ShowLoginWindow();
        };
    }
}
