using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFEditorApp.Core.Services;
using System.Diagnostics;
using System.IO;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PDFEditorApp.ViewModels;

public enum SplitMode { PerPage, ByRange, EveryNPages }

public partial class SplitViewModel : ObservableObject
{
    private readonly ISplitService _splitService;

    [ObservableProperty] private string _inputFilePath = string.Empty;
    [ObservableProperty] private string _inputFileName = string.Empty;
    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private string _filePrefix = "output";
    [ObservableProperty] private SplitMode _selectedMode = SplitMode.PerPage;
    [ObservableProperty] private string _rangeInput = "1-3, 4-7";
    [ObservableProperty] private int _pagesPerChunk = 5;
    [ObservableProperty] private double _splitProgress;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _totalPages;
    [ObservableProperty] private string _previewText = string.Empty;

    public SplitMode[] SplitModes { get; } = [SplitMode.PerPage, SplitMode.ByRange, SplitMode.EveryNPages];

    private CancellationTokenSource? _cts;

    public SplitViewModel(ISplitService splitService)
    {
        _splitService = splitService;
        OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    [RelayCommand]
    private void BrowseInputFile()
    {
        var dialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf" };
        if (dialog.ShowDialog() == true)
        {
            InputFilePath = dialog.FileName;
            InputFileName = Path.GetFileName(dialog.FileName);
            TotalPages = _splitService.GetPageCount(InputFilePath);
            UpdatePreview();
            StatusMessage = $"Loaded: {InputFileName} ({TotalPages} pages)";
        }
    }

    [RelayCommand]
    private void BrowseOutputFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = OutputFolder };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            OutputFolder = dialog.SelectedPath;
    }

    partial void OnSelectedModeChanged(SplitMode value) => UpdatePreview();
    partial void OnRangeInputChanged(string value) => UpdatePreview();
    partial void OnPagesPerChunkChanged(int value) => UpdatePreview();

    private void UpdatePreview()
    {
        if (string.IsNullOrEmpty(InputFilePath) || TotalPages == 0) return;

        PreviewText = SelectedMode switch
        {
            SplitMode.PerPage => $"Will produce {TotalPages} file(s), one per page.",
            SplitMode.ByRange => BuildRangePreview(),
            SplitMode.EveryNPages => $"Will produce {(int)Math.Ceiling((double)TotalPages / PagesPerChunk)} file(s), {PagesPerChunk} pages each.",
            _ => string.Empty
        };
    }

    private string BuildRangePreview()
    {
        var ranges = _splitService.ParseRanges(RangeInput, TotalPages);
        return ranges.Count > 0
            ? $"Will produce {ranges.Count} file(s) from specified ranges."
            : "Invalid or empty ranges.";
    }

    [RelayCommand]
    private async Task StartSplitAsync()
    {
        if (string.IsNullOrEmpty(InputFilePath) || IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();

        try
        {
            StatusMessage = "Splitting...";
            var progress = new Progress<double>(p => SplitProgress = p * 100);
            bool success = SelectedMode switch
            {
                SplitMode.PerPage => await _splitService.SplitByPageAsync(InputFilePath, OutputFolder, FilePrefix, progress, _cts.Token),
                SplitMode.ByRange => await _splitService.SplitByRangeAsync(InputFilePath, OutputFolder, FilePrefix,
                    _splitService.ParseRanges(RangeInput, TotalPages), progress, _cts.Token),
                SplitMode.EveryNPages => await _splitService.SplitEveryNPagesAsync(InputFilePath, OutputFolder, FilePrefix, PagesPerChunk, progress, _cts.Token),
                _ => false
            };
            StatusMessage = success ? "Split completed successfully!" : "Split failed.";
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void CancelSplit() { _cts?.Cancel(); StatusMessage = "Cancelled."; }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
            Process.Start("explorer.exe", OutputFolder);
    }
}
