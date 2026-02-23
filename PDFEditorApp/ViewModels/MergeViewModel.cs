using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFEditorApp.Core.Models;
using PDFEditorApp.Core.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace PDFEditorApp.ViewModels;

public partial class MergeViewModel : ObservableObject
{
    private readonly IMergeService _mergeService;

    [ObservableProperty] private string _outputPath = string.Empty;
    [ObservableProperty] private double _mergeProgress;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _totalPages;

    public ObservableCollection<FileItem> Files { get; } = new();

    private CancellationTokenSource? _cts;

    public MergeViewModel(IMergeService mergeService)
    {
        _mergeService = mergeService;
        OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "merged.pdf");
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
    private void ClearFiles() { Files.Clear(); MergeProgress = 0; StatusMessage = "Ready"; }

    [RelayCommand]
    private void MoveUp(FileItem? item)
    {
        if (item == null) return;
        int idx = Files.IndexOf(item);
        if (idx > 0) Files.Move(idx, idx - 1);
    }

    [RelayCommand]
    private void MoveDown(FileItem? item)
    {
        if (item == null) return;
        int idx = Files.IndexOf(item);
        if (idx < Files.Count - 1) Files.Move(idx, idx + 1);
    }

    [RelayCommand]
    private void BrowseOutputPath()
    {
        var dialog = new SaveFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", FileName = "merged.pdf" };
        if (dialog.ShowDialog() == true)
            OutputPath = dialog.FileName;
    }

    [RelayCommand]
    private async Task StartMergeAsync()
    {
        if (Files.Count < 2 || IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();

        try
        {
            StatusMessage = $"Merging {Files.Count} files...";
            foreach (var f in Files) f.Status = FileStatus.Processing;

            var progress = new Progress<double>(p => MergeProgress = p * 100);
            bool success = await _mergeService.MergeAsync(Files.Select(f => f.FilePath), OutputPath, progress, _cts.Token);

            foreach (var f in Files) f.Status = success ? FileStatus.Done : FileStatus.Error;
            StatusMessage = success ? "Merge completed successfully!" : "Merge failed.";
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void CancelMerge() { _cts?.Cancel(); StatusMessage = "Cancelled."; }

    [RelayCommand]
    private void OpenOutputFile()
    {
        if (File.Exists(OutputPath))
            Process.Start(new ProcessStartInfo(OutputPath) { UseShellExecute = true });
    }

    public void AddFilesFromPaths(IEnumerable<string> paths)
    {
        foreach (var p in paths)
            if (File.Exists(p) && p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                Files.Add(FileItem.FromPath(p));
    }
}
