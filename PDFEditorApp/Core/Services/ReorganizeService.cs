using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;
using PDFEditorApp.Core.Models;

namespace PDFEditorApp.Core.Services;

public class ReorganizeService : IReorganizeService
{
    private readonly ILogger<ReorganizeService> _logger;

    public ReorganizeService(ILogger<ReorganizeService> logger)
    {
        _logger = logger;
    }

    public int GetPageCount(string filePath)
    {
        using var reader = new PdfReader(filePath);
        using var pdf = new PdfDocument(reader);
        return pdf.GetNumberOfPages();
    }

    public async Task<IList<PdfPageModel>> LoadPagesAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var pages = new List<PdfPageModel>();
            using var reader = new PdfReader(filePath);
            using var pdf = new PdfDocument(reader);
            int total = pdf.GetNumberOfPages();

            for (int i = 1; i <= total; i++)
            {
                var model = new PdfPageModel
                {
                    SourceFilePath = filePath,
                    OriginalPageIndex = i,
                    CurrentDisplayIndex = i
                };
                pages.Add(model);
            }
            return (IList<PdfPageModel>)pages;
        });
    }

    public async Task<bool> SaveReorganizedAsync(IEnumerable<PdfPageModel> pages, string outputPath, IProgress<double> progress, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            var pageList = pages.ToList();
            var sourceReaders = new Dictionary<string, (PdfReader reader, PdfDocument doc)>();

            try
            {
                progress.Report(0.05);
                _logger.LogInformation("Saving reorganized PDF with {Count} pages to {Output}", pageList.Count, outputPath);

                var sources = pageList.Select(p => p.SourceFilePath).Distinct().ToList();

                foreach (var src in sources)
                {
                    var r = new PdfReader(src);
                    var d = new PdfDocument(r);
                    sourceReaders[src] = (r, d);
                }

                using var writer = new PdfWriter(outputPath);
                using var destDoc = new PdfDocument(writer);

                for (int i = 0; i < pageList.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var page = pageList[i];
                    var (_, srcDoc) = sourceReaders[page.SourceFilePath];
                    srcDoc.CopyPagesTo(page.OriginalPageIndex, page.OriginalPageIndex, destDoc);
                    progress.Report(0.05 + 0.9 * (i + 1) / pageList.Count);
                }

                progress.Report(1.0);
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving reorganized PDF");
                return false;
            }
            finally
            {
                foreach (var (_, (reader, doc)) in sourceReaders)
                {
                    try { doc.Close(); } catch { }
                    try { reader.Close(); } catch { }
                }
            }
        }, ct);
    }
}
