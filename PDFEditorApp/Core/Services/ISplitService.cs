namespace PDFEditorApp.Core.Services;

public record PageRange(int Start, int End);

public interface ISplitService
{
    Task<bool> SplitByPageAsync(string inputPath, string outputFolder, string filePrefix, IProgress<double> progress, CancellationToken ct);
    Task<bool> SplitByRangeAsync(string inputPath, string outputFolder, string filePrefix, IEnumerable<PageRange> ranges, IProgress<double> progress, CancellationToken ct);
    Task<bool> SplitEveryNPagesAsync(string inputPath, string outputFolder, string filePrefix, int n, IProgress<double> progress, CancellationToken ct);
    IList<PageRange> ParseRanges(string rangeInput, int totalPages);
    int GetPageCount(string inputPath);
}
