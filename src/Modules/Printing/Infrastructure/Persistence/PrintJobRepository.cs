using Dapper;
using Microsoft.Data.Sqlite;
using RMS.BuildingBlocks.Contracts;
using RMS.Modules.Printing.Application.Contracts;
using RMS.Modules.Printing.Domain;
using RMS.Modules.Printing.Domain.Entities;

namespace RMS.Modules.Printing.Infrastructure.Persistence;

public sealed class PrintJobRepository : IPrintJobRepository
{
    private readonly IDbConnectionFactory _factory;

    public PrintJobRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task AddAsync(PrintJob job, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO PrintJobs (Id, DocumentType, DocumentNumber, PrinterName, Status, CreatedAt, CompletedAt, OutputPath, ErrorMessage, Copies)
            VALUES (@Id, @DocumentType, @DocumentNumber, @PrinterName, @Status, @CreatedAt, @CompletedAt, @OutputPath, @ErrorMessage, @Copies);
            """;
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, job, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(PrintJob job, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE PrintJobs
            SET Status = @Status, CompletedAt = @CompletedAt, OutputPath = @OutputPath, ErrorMessage = @ErrorMessage
            WHERE Id = @Id;
            """;
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            job.Id,
            Status = (int)job.Status,
            job.CompletedAt,
            job.OutputPath,
            job.ErrorMessage
        }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<PrintJob>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM PrintJobs ORDER BY CreatedAt DESC LIMIT @Limit;";
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<PrintJobRow>(new CommandDefinition(sql, new { Limit = limit }, cancellationToken: cancellationToken));
        return rows.Select(Map).ToList();
    }

    private static PrintJob Map(PrintJobRow r) => new()
    {
        Id = r.Id,
        DocumentType = r.DocumentType,
        DocumentNumber = r.DocumentNumber,
        PrinterName = r.PrinterName,
        Status = (PrintJobStatus)r.Status,
        CreatedAt = r.CreatedAt,
        CompletedAt = r.CompletedAt,
        OutputPath = r.OutputPath,
        ErrorMessage = r.ErrorMessage,
        Copies = r.Copies
    };

    private sealed class PrintJobRow
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? OutputPath { get; set; }
        public string? ErrorMessage { get; set; }
        public int Copies { get; set; }
    }
}
