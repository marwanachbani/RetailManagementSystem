using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            PasswordBox.PasswordChanged += (_, __) => vm.Password = PasswordBox.Password;
        }

        UserNameTextBox.Focus();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.ErrorMessage) && DataContext is LoginViewModel vm)
        {
            ErrorTextBlock.Visibility = string.IsNullOrEmpty(vm.ErrorMessage)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        if (e.PropertyName == nameof(LoginViewModel.ShowPassword) && DataContext is LoginViewModel vm2)
        {
            if (!vm2.ShowPassword)
            {
                PasswordBox.Password = vm2.Password;
            }
        }
    }

    private void OnUsernameOrPasswordKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter || DataContext is not LoginViewModel vm)
            return;

        if (vm.LoginCommand.CanExecute(null))
            vm.LoginCommand.Execute(null);
    }
}
