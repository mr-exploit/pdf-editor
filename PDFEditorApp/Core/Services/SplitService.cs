using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.RegularExpressions;

namespace PDFEditorApp.Core.Services;

public class SplitService : ISplitService
{
    private readonly ILogger<SplitService> _logger;

    public SplitService(ILogger<SplitService> logger)
    {
        _logger = logger;
    }

    public int GetPageCount(string inputPath)
    {
        using var reader = new PdfReader(inputPath);
        using var pdf = new PdfDocument(reader);
        return pdf.GetNumberOfPages();
    }

    public IList<PageRange> ParseRanges(string rangeInput, int totalPages)
    {
        var ranges = new List<PageRange>();
        var parts = rangeInput.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            var match = Regex.Match(trimmed, @"^(\d+)(?:-(\d+))?$");
            if (match.Success)
            {
                int start = int.Parse(match.Groups[1].Value);
                int end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : start;
                if (start >= 1 && end <= totalPages && start <= end)
                    ranges.Add(new PageRange(start, end));
            }
        }
        return ranges;
    }

    public async Task<bool> SplitByPageAsync(string inputPath, string outputFolder, string filePrefix, IProgress<double> progress, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);
                int total = srcDoc.GetNumberOfPages();
                _logger.LogInformation("Splitting {Input} into {Total} pages", inputPath, total);

                for (int i = 1; i <= total; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var outPath = Path.Combine(outputFolder, $"{filePrefix}_page_{i:D4}.pdf");
                    using var writer = new PdfWriter(outPath);
                    using var destDoc = new PdfDocument(writer);
                    srcDoc.CopyPagesTo(i, i, destDoc);
                    progress.Report((double)i / total);
                }
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting by page {Input}", inputPath);
                return false;
            }
        }, ct);
    }

    public async Task<bool> SplitByRangeAsync(string inputPath, string outputFolder, string filePrefix, IEnumerable<PageRange> ranges, IProgress<double> progress, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            var rangeList = ranges.ToList();
            try
            {
                Directory.CreateDirectory(outputFolder);
                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);
                _logger.LogInformation("Splitting {Input} by {Count} ranges", inputPath, rangeList.Count);

                for (int i = 0; i < rangeList.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var r = rangeList[i];
                    var outPath = Path.Combine(outputFolder, $"{filePrefix}_part_{i + 1:D3}.pdf");
                    using var writer = new PdfWriter(outPath);
                    using var destDoc = new PdfDocument(writer);
                    srcDoc.CopyPagesTo(r.Start, r.End, destDoc);
                    progress.Report((double)(i + 1) / rangeList.Count);
                }
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting by range {Input}", inputPath);
                return false;
            }
        }, ct);
    }

    public async Task<bool> SplitEveryNPagesAsync(string inputPath, string outputFolder, string filePrefix, int n, IProgress<double> progress, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
                using var reader = new PdfReader(inputPath);
                using var srcDoc = new PdfDocument(reader);
                int total = srcDoc.GetNumberOfPages();
                int chunks = (int)Math.Ceiling((double)total / n);
                _logger.LogInformation("Splitting {Input} every {N} pages into {Chunks} files", inputPath, n, chunks);

                for (int i = 0; i < chunks; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    int start = i * n + 1;
                    int end = Math.Min(start + n - 1, total);
                    var outPath = Path.Combine(outputFolder, $"{filePrefix}_chunk_{i + 1:D3}.pdf");
                    using var writer = new PdfWriter(outPath);
                    using var destDoc = new PdfDocument(writer);
                    srcDoc.CopyPagesTo(start, end, destDoc);
                    progress.Report((double)(i + 1) / chunks);
                }
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error splitting every N pages {Input}", inputPath);
                return false;
            }
        }, ct);
    }
}
