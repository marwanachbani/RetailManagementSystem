using System;
using System.Windows;
using RMS.WPF.Services;
using RMS.WPF.ViewModels;

namespace RMS.WPF.Views;

public partial class CreateSaleWindow : Window
{
    private readonly ICurrentSessionService _session;

    /// <summary>When set before ShowDialog, the window resumes this existing pending sale
    /// instead of starting a brand new one.</summary>
    public Guid? ResumeSaleId { get; set; }

    public CreateSaleWindow(CreateSaleViewModel viewModel, ICurrentSessionService session)
    {
        InitializeComponent();
        DataContext = viewModel;
        _session = session;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (DataContext is CreateSaleViewModel vm)
        {
            vm.RequestClose += (_, _) =>
            {
                DialogResult = vm.DialogResult;
                Close();
            };

            if (ResumeSaleId is { } saleId)
            {
                var cashierId = _session.IsAuthenticated ? _session.UserId : Guid.Empty;
                _ = vm.ResumeSaleAsync(saleId, cashierId);
            }
            else
            {
                // The sale must be attributed to the operator who is actually signed in,
                // not a throwaway random id.
                var cashierId = _session.IsAuthenticated ? _session.UserId : Guid.Empty;
                _ = vm.InitializeSaleAsync(cashierId);
            }

            _ = vm.LoadProductsAsync();
        }
    }
}
