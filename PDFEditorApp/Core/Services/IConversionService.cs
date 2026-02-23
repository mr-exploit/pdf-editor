namespace PDFEditorApp.Core.Services;

public interface IConversionService
{
    Task<bool> PdfToWordAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> PdfToExcelAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> PdfToPptAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> WordToPdfAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> ExcelToPdfAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
    Task<bool> PptToPdfAsync(string inputPath, string outputPath, IProgress<double> progress, CancellationToken ct);
}
