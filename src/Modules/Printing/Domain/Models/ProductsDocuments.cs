using RMS.Modules.Printing.Domain.Models;

namespace RMS.Modules.Printing.Domain.Models;

/// <summary>A single label to be printed on a sheet (barcode, product or shelf).</summary>
public sealed record LabelItem(
    string Name,
    string BarcodeValue,
    BarcodeSymbology Symbology,
    string? Sku = null,
    string? Price = null,
    string? ExtraLine = null);

/// <summary>Layout configuration for a page of labels.</summary>
public sealed record LabelLayout(
    int Columns,
    int Rows,
    decimal LabelWidthMm,
    decimal LabelHeightMm,
    bool ShowName = true,
    bool ShowPrice = false,
    bool ShowBarcode = true,
    bool ShowSku = false)
{
    public int LabelsPerPage => Columns * Rows;
}

public sealed record BarcodeLabelDocument(
    IReadOnlyList<LabelItem> Items,
    LabelLayout Layout)
{
    public BarcodeLabelDocument(IReadOnlyList<LabelItem> items)
        : this(items, new LabelLayout(3, 8, 60, 35, ShowName: true, ShowPrice: false)) { }
}

public sealed record ProductLabelDocument(
    IReadOnlyList<LabelItem> Items,
    LabelLayout Layout)
{
    public ProductLabelDocument(IReadOnlyList<LabelItem> items)
        : this(items, new LabelLayout(3, 6, 60, 45, ShowName: true, ShowPrice: true, ShowBarcode: true, ShowSku: true)) { }
}

public sealed record ShelfLabelDocument(
    IReadOnlyList<LabelItem> Items,
    LabelLayout Layout)
{
    public ShelfLabelDocument(IReadOnlyList<LabelItem> items)
        : this(items, new LabelLayout(4, 5, 45, 50, ShowName: true, ShowPrice: true, ShowBarcode: true, ShowSku: false)) { }
}
