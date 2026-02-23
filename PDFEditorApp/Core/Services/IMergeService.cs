namespace PDFEditorApp.Core.Services;

public interface IMergeService
{
    Task<bool> MergeAsync(IEnumerable<string> inputPaths, string outputPath, IProgress<double> progress, CancellationToken ct);
}
