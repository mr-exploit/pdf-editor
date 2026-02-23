using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;
using System.IO;

namespace PDFEditorApp.Core.Services;

public class CompressService : ICompressService
{
    private readonly ILogger<CompressService> _logger;

    public CompressService(ILogger<CompressService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> CompressAsync(string inputPath, string outputPath, CompressionLevel level, IProgress<double> progress, CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            try
            {
                progress.Report(0.1);
                ct.ThrowIfCancellationRequested();
                _logger.LogInformation("Compressing {Input} with level {Level}", inputPath, level);

                int compressionLevel = level switch
                {
                    CompressionLevel.Low => iText.Kernel.Pdf.CompressionConstants.BEST_SPEED,
                    CompressionLevel.Medium => iText.Kernel.Pdf.CompressionConstants.DEFAULT_COMPRESSION,
                    CompressionLevel.High => iText.Kernel.Pdf.CompressionConstants.BEST_COMPRESSION,
                    _ => iText.Kernel.Pdf.CompressionConstants.DEFAULT_COMPRESSION
                };

                var writerProps = new WriterProperties()
                    .SetCompressionLevel(compressionLevel)
                    .UseSmartMode();

                using var reader = new PdfReader(inputPath);
                using var writer = new PdfWriter(outputPath, writerProps);
                using var pdfDoc = new PdfDocument(reader, writer);

                int totalPages = pdfDoc.GetNumberOfPages();
                for (int i = 1; i <= totalPages; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(0.1 + 0.8 * i / totalPages);
                }

                progress.Report(1.0);
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing {Input}", inputPath);
                return false;
            }
        }, ct);
    }

    public async Task<long> EstimateCompressedSizeAsync(string inputPath, CompressionLevel level)
    {
        return await Task.Run(() =>
        {
            var originalSize = new FileInfo(inputPath).Length;
            double ratio = level switch
            {
                CompressionLevel.Low => 0.85,
                CompressionLevel.Medium => 0.65,
                CompressionLevel.High => 0.45,
                _ => 0.65
            };
            return (long)(originalSize * ratio);
        });
    }
}
