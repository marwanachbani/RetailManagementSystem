using System.IO;
using System.Text;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using RMS.Modules.Reporting.Application.Contracts;

namespace RMS.WPF.Reports.Exports;

public interface IReportExporter
{
    Task<byte[]> ExportAsync<T>(ReportFilter filter, IReadOnlyList<T> items, string title, CancellationToken cancellationToken = default);
}

public sealed class CsvReportExporter : IReportExporter
{
    public Task<byte[]> ExportAsync<T>(ReportFilter filter, IReadOnlyList<T> items, string title, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{title}");
        if (filter.DateRange is { FromDate: not null })
            sb.AppendLine($"From: {filter.DateRange.FromDate:yyyy-MM-dd}");
        if (filter.DateRange is { ToDate: not null })
            sb.AppendLine($"To: {filter.DateRange.ToDate:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            sb.AppendLine($"Search: {filter.SearchTerm}");
        sb.AppendLine();

        var type = typeof(T);
        var properties = type.GetProperties();
        sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var str = value?.ToString() ?? string.Empty;
                if (str.Contains(',') || str.Contains('"') || str.Contains('\n'))
                    str = $"\"{str.Replace("\"", "\"\"")}\"";
                return str;
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }
}

public sealed class PdfReportExporter : IReportExporter
{
    public Task<byte[]> ExportAsync<T>(ReportFilter filter, IReadOnlyList<T> items, string title, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.Header().Element(header => header.AlignCenter().Text(title).FontSize(18).Bold());
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        var type = typeof(T);
                        var properties = type.GetProperties();
                        foreach (var _ in properties)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        var type = typeof(T);
                        foreach (var prop in type.GetProperties())
                        {
                            header.Cell().Border(1).Padding(4).Text(prop.Name).FontSize(10);
                        }
                    });

                    foreach (var item in items)
                    {
                        var type = typeof(T);
                        foreach (var prop in type.GetProperties())
                        {
                            table.Cell().Border(1).Padding(4).Text(prop.GetValue(item)?.ToString() ?? string.Empty).FontSize(10);
                        }
                    }
                });
            });
        });

        var tempPath = Path.Combine(Path.GetTempPath(), $"report_{Guid.NewGuid():N}.pdf");
        document.GeneratePdf(tempPath);
        var bytes = File.ReadAllBytes(tempPath);
        File.Delete(tempPath);
        return Task.FromResult(bytes);
    }
}

public sealed class PrintReportExporter : IReportExporter
{
    public Task<byte[]> ExportAsync<T>(ReportFilter filter, IReadOnlyList<T> items, string title, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
