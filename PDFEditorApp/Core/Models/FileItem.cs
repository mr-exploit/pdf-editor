using CommunityToolkit.Mvvm.ComponentModel;

namespace PDFEditorApp.Core.Models;

public enum FileStatus { Pending, Processing, Done, Error }

public partial class FileItem : ObservableObject
{
    [ObservableProperty] private string _filePath = string.Empty;
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private long _fileSizeBytes;
    [ObservableProperty] private string _fileSizeDisplay = string.Empty;
    [ObservableProperty] private FileStatus _status = FileStatus.Pending;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public static FileItem FromPath(string path)
    {
        var info = new System.IO.FileInfo(path);
        return new FileItem
        {
            FilePath = path,
            FileName = info.Name,
            FileSizeBytes = info.Exists ? info.Length : 0,
            FileSizeDisplay = FormatSize(info.Exists ? info.Length : 0)
        };
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024):F1} MB";
    }
}
