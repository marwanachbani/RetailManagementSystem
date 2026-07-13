using System.Windows;

namespace RMS.WPF.Printing;

public partial class PrinterSelectionDialog : Window
{
    public PrinterSelectionDialog()
    {
        InitializeComponent();
    }

    public void LoadPrinters(System.Collections.Generic.IEnumerable<string> printers, string? selected = null)
    {
        PrinterCombo.Items.Clear();
        PrinterCombo.Items.Add(string.Empty);
        foreach (var p in printers)
            PrinterCombo.Items.Add(p);
        PrinterCombo.Text = selected ?? string.Empty;
    }

    public string SelectedPrinter => PrinterCombo.Text;

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
