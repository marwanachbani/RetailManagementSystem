using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace RMS.WPF.Printing;

public partial class PrintPreviewWindow : Window
{
    private readonly string _pdfPath;

    public PrintPreviewWindow(string pdfPath)
    {
        InitializeComponent();
        _pdfPath = pdfPath;
        Browser.Navigate(new Uri(pdfPath));

        ZoomSlider.ValueChanged += (_, _) =>
        {
            var scale = ZoomSlider.Value / 100d;
            PageHost.LayoutTransform = new ScaleTransform(scale, scale);
            ZoomLabel.Text = $"{ZoomSlider.Value:0}%";
        };
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var psi = new ProcessStartInfo(_pdfPath) { Verb = "print", CreateNoWindow = true, UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not print the document:\n{ex.Message}", "Print", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e) => SaveAs(false);

    private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveAs(true);

    private void SaveAs(bool keepName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PDF documents (*.pdf)|*.pdf",
            FileName = keepName ? System.IO.Path.GetFileName(_pdfPath) : "document.pdf"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                System.IO.File.Copy(_pdfPath, dialog.FileName, true);
                MessageBox.Show($"Saved to {dialog.FileName}", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the file:\n{ex.Message}", "Save", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
