namespace PDFEditorApp.Core.Models;

public enum ConversionType
{
    PdfToWord, PdfToExcel, PdfToPpt,
    WordToPdf, ExcelToPdf, PptToPdf
}

public class ConversionTask
{
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public ConversionType Type { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
