namespace PDFEditorApp.Core.Services;

public enum CompressionLevel { Low, Medium, High }

public interface ICompressService
{
    Task<bool> CompressAsync(string inputPath, string outputPath, CompressionLevel level, IProgress<double> progress, CancellationToken ct);
    Task<long> EstimateCompressedSizeAsync(string inputPath, CompressionLevel level);
}
