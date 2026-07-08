using System.Windows;
using RMS.WPF.Services;
using RMS.WPF.ViewModels;

namespace RMS.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel, ICurrentSessionService session)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestLogout += (_, _) =>
        {
            session.SignOut();
            Close();
            ((App)Application.Current).ShowLoginWindow();
        };
    }
}
