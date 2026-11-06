namespace DzentoPOS2.App.Services.Systems;

public interface IDocumentNumberService
{
    Task<string> NextInvoiceNumberAsync(
        CancellationToken cancellationToken = default);

    Task<string> NextDocumentNumberAsync(
        string prefix,
        CancellationToken cancellationToken = default);
}
