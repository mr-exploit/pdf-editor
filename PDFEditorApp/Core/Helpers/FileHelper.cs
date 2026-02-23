using System.IO;

namespace PDFEditorApp.Core.Helpers;

public static class FileHelper
{
    public static bool IsPdf(string path) =>
        string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase);

    public static bool IsWordDoc(string path) =>
        new[] { ".doc", ".docx" }.Contains(Path.GetExtension(path).ToLower());

    public static bool IsExcelDoc(string path) =>
        new[] { ".xls", ".xlsx" }.Contains(Path.GetExtension(path).ToLower());

    public static bool IsPptDoc(string path) =>
        new[] { ".ppt", ".pptx" }.Contains(Path.GetExtension(path).ToLower());

    public static string GetLogDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PDFEditor", "logs");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string EnsureOutputPath(string inputPath, string suffix, string extension)
    {
        var dir = Path.GetDirectoryName(inputPath) ?? ".";
        var name = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(dir, $"{name}{suffix}{extension}");
    }

    public static bool HasEnoughDiskSpace(string targetPath, long requiredBytes)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(targetPath)) ?? ".";
        var drive = new DriveInfo(Path.GetPathRoot(dir) ?? dir);
        return drive.AvailableFreeSpace >= requiredBytes;
    }
}
