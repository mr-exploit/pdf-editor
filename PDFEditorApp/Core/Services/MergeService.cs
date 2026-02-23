using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.Extensions.Logging;

namespace PDFEditorApp.Core.Services;

public class MergeService : IMergeService
{
    private readonly ILogger<MergeService> _logger;

    public MergeService(ILogger<MergeService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> MergeAsync(IEnumerable<string> inputPaths, string outputPath, IProgress<double> progress, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            var paths = inputPaths.ToList();
            try
            {
                progress.Report(0.05);
                _logger.LogInformation("Merging {Count} files into {Output}", paths.Count, outputPath);

                using var writer = new PdfWriter(outputPath);
                using var mergedDoc = new PdfDocument(writer);
                var merger = new PdfMerger(mergedDoc);

                for (int i = 0; i < paths.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    using var reader = new PdfReader(paths[i]);
                    using var srcDoc = new PdfDocument(reader);
                    merger.Merge(srcDoc, 1, srcDoc.GetNumberOfPages());
                    progress.Report(0.05 + 0.9 * (i + 1) / paths.Count);
                }

                progress.Report(1.0);
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging files");
                return false;
            }
        }, ct);
    }
}
