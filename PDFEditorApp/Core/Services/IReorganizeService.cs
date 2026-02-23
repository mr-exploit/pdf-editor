using PDFEditorApp.Core.Models;

namespace PDFEditorApp.Core.Services;

public interface IReorganizeService
{
    Task<IList<PdfPageModel>> LoadPagesAsync(string filePath);
    Task<bool> SaveReorganizedAsync(IEnumerable<PdfPageModel> pages, string outputPath, IProgress<double> progress, CancellationToken ct);
    int GetPageCount(string filePath);
}
