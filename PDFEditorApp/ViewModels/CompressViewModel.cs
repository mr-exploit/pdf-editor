using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFEditorApp.Core.Models;
using PDFEditorApp.Core.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PDFEditorApp.ViewModels;

public partial class CompressViewModel : ObservableObject
{
    private readonly ICompressService _compressService;

    [ObservableProperty] private CompressionLevel _selectedLevel = CompressionLevel.Medium;
    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<FileItem> Files { get; } = new();
    public CompressionLevel[] CompressionLevels { get; } = [CompressionLevel.Low, CompressionLevel.Medium, CompressionLevel.High];

    private CancellationTokenSource? _cts;

    public CompressViewModel(ICompressService compressService)
    {
        _compressService = compressService;
        OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    [RelayCommand]
    private void AddFiles()
    {
        var dialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", Multiselect = true };
        if (dialog.ShowDialog() == true)
            foreach (var f in dialog.FileNames)
                Files.Add(FileItem.FromPath(f));
    }

    [RelayCommand]
    private void RemoveFile(FileItem? item)
    {
        if (item != null) Files.Remove(item);
    }

    [RelayCommand]
    private void ClearFiles() { Files.Clear(); OverallProgress = 0; StatusMessage = "Ready"; }

    [RelayCommand]
    private void BrowseOutputFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = OutputFolder };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            OutputFolder = dialog.SelectedPath;
    }

    [RelayCommand]
    private async Task StartCompressionAsync()
    {
        if (Files.Count == 0 || IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        int done = 0;

        try
        {
            StatusMessage = $"Compressing {Files.Count} file(s)...";
            foreach (var file in Files)
            {
                if (_cts.Token.IsCancellationRequested) break;
                file.Status = FileStatus.Processing;
                var outPath = Path.Combine(OutputFolder,
                    Path.GetFileNameWithoutExtension(file.FileName) + "_compressed.pdf");
                var progress = new Progress<double>(p =>
                {
                    file.Progress = p * 100;
                    OverallProgress = (done + p) / Files.Count * 100;
                });

                bool success = await _compressService.CompressAsync(file.FilePath, outPath, SelectedLevel, progress, _cts.Token);
                file.Status = success ? FileStatus.Done : FileStatus.Error;
                file.Progress = 100;
                done++;
                OverallProgress = (double)done / Files.Count * 100;
            }
            StatusMessage = $"Completed: {Files.Count(f => f.Status == FileStatus.Done)} succeeded, {Files.Count(f => f.Status == FileStatus.Error)} failed.";
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void CancelCompression() { _cts?.Cancel(); StatusMessage = "Cancelled."; }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
            Process.Start("explorer.exe", OutputFolder);
    }

    public void AddFilesFromPaths(IEnumerable<string> paths)
    {
        foreach (var p in paths)
            if (File.Exists(p) && p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                Files.Add(FileItem.FromPath(p));
    }
}
