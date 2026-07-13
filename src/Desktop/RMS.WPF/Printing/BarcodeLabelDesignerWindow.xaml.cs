using System.Windows;
using System.Windows.Media.Imaging;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Models;
using RMS.WPF.Services;

namespace RMS.WPF.Printing;

public partial class BarcodeLabelDesignerWindow : Window
{
    private readonly IBarcodeGenerator _barcodes;
    private readonly IPrintingService _printing;
    private readonly IDialogService _dialogService;

    public BarcodeLabelDesignerWindow(IBarcodeGenerator barcodes, IPrintingService printing, IDialogService dialogService)
    {
        InitializeComponent();
        _barcodes = barcodes;
        _printing = printing;
        _dialogService = dialogService;

        SymbologyBox.ItemsSource = new[] { "EAN13", "Code128", "Code39", "QRCode" };
        SymbologyBox.SelectedItem = "EAN13";
        GeneratePreview();
    }

    private BarcodeSymbology SelectedSymbology => (BarcodeSymbology)Enum.Parse(typeof(BarcodeSymbology), (string)SymbologyBox.SelectedItem!);

    private void OnChanged(object sender, RoutedEventArgs e) => GeneratePreview();

    private void GeneratePreview()
    {
        try
        {
            var data = string.IsNullOrWhiteSpace(DataBox.Text) ? "0" : DataBox.Text;
            var bytes = SelectedSymbology == BarcodeSymbology.QRCode
                ? _barcodes.GenerateQr(data, 200)
                : _barcodes.Generate(data, SelectedSymbology, 260, SelectedSymbology == BarcodeSymbology.QRCode ? 200 : 90);
            PreviewImage.Source = LoadImage(bytes);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Could not render barcode: {ex.Message}");
        }
    }

    private static BitmapImage LoadImage(byte[] bytes)
    {
        var image = new BitmapImage();
        using var ms = new System.IO.MemoryStream(bytes);
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = ms;
        image.EndInit();
        return image;
    }

    private async void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var label = new LabelItem(
            NameBox.Text,
            DataBox.Text,
            SelectedSymbology,
            Sku: null,
            Price: string.IsNullOrWhiteSpace(PriceBox.Text) ? null : PriceBox.Text);

        var result = await _printing.PrintLabelsAsync(new[] { label }, DocumentType.ProductLabel);
        if (result.IsSuccess)
            _dialogService.ShowInfo("Label sent to the printer.");
        else
            _dialogService.ShowError(result.Error ?? "Print failed.");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
