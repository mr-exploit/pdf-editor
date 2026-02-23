using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFEditorApp.Core.Models;
using PDFEditorApp.Core.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PDFEditorApp.ViewModels;

public partial class ConvertViewModel : ObservableObject
{
    private readonly IConversionService _conversionService;

    [ObservableProperty] private string _title = "Convert PDF to Word";
    [ObservableProperty] private string _inputFilter = "PDF Files (*.pdf)|*.pdf";
    [ObservableProperty] private string _outputExtension = ".docx";
    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private double _overallProgress;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<FileItem> Files { get; } = new();

    private NavigationItem _conversionMode = NavigationItem.ConvertPdfToWord;
    private CancellationTokenSource? _cts;

    public ConvertViewModel(IConversionService conversionService)
    {
        _conversionService = conversionService;
        OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    public void SetConversionMode(NavigationItem mode)
    {
        _conversionMode = mode;
        (Title, InputFilter, OutputExtension) = mode switch
        {
            NavigationItem.ConvertPdfToWord => ("Convert PDF \u2192 Word (.docx)", "PDF Files (*.pdf)|*.pdf", ".docx"),
            NavigationItem.ConvertPdfToExcel => ("Convert PDF \u2192 Excel (.xlsx)", "PDF Files (*.pdf)|*.pdf", ".xlsx"),
            NavigationItem.ConvertPdfToPpt => ("Convert PDF \u2192 PowerPoint (.pptx)", "PDF Files (*.pdf)|*.pdf", ".pptx"),
            NavigationItem.ConvertWordToPdf => ("Convert Word \u2192 PDF", "Word Files (*.docx;*.doc)|*.docx;*.doc", ".pdf"),
            NavigationItem.ConvertExcelToPdf => ("Convert Excel \u2192 PDF", "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls", ".pdf"),
            NavigationItem.ConvertPptToPdf => ("Convert PowerPoint \u2192 PDF", "PowerPoint Files (*.pptx;*.ppt)|*.pptx;*.ppt", ".pdf"),
            _ => ("Convert", "All Files (*.*)|*.*", ".pdf")
        };
    }

    [RelayCommand]
    private void AddFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = InputFilter,
            Multiselect = true
        };
        if (dialog.ShowDialog() == true)
        {
            foreach (var f in dialog.FileNames)
                Files.Add(FileItem.FromPath(f));
        }
    }

    [RelayCommand]
    private void RemoveFile(FileItem? item)
    {
        if (item != null) Files.Remove(item);
    }

    [RelayCommand]
    private void ClearFiles()
    {
        Files.Clear();
        OverallProgress = 0;
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void BrowseOutputFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            SelectedPath = OutputFolder
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            OutputFolder = dialog.SelectedPath;
    }

    [RelayCommand]
    private async Task StartConversionAsync()
    {
        if (Files.Count == 0 || IsBusy) return;
        IsBusy = true;
        _cts = new CancellationTokenSource();
        int done = 0;

        try
        {
            StatusMessage = $"Converting {Files.Count} file(s)...";
            foreach (var file in Files)
            {
                if (_cts.Token.IsCancellationRequested) break;
                file.Status = FileStatus.Processing;
                file.Progress = 0;
                var outPath = Path.Combine(OutputFolder,
                    Path.GetFileNameWithoutExtension(file.FileName) + OutputExtension);
                var progress = new Progress<double>(p =>
                {
                    file.Progress = p * 100;
                    OverallProgress = (done + p) / Files.Count * 100;
                });

                bool success = _conversionMode switch
                {
                    NavigationItem.ConvertPdfToWord => await _conversionService.PdfToWordAsync(file.FilePath, outPath, progress, _cts.Token),
                    NavigationItem.ConvertPdfToExcel => await _conversionService.PdfToExcelAsync(file.FilePath, outPath, progress, _cts.Token),
                    NavigationItem.ConvertPdfToPpt => await _conversionService.PdfToPptAsync(file.FilePath, outPath, progress, _cts.Token),
                    NavigationItem.ConvertWordToPdf => await _conversionService.WordToPdfAsync(file.FilePath, outPath, progress, _cts.Token),
                    NavigationItem.ConvertExcelToPdf => await _conversionService.ExcelToPdfAsync(file.FilePath, outPath, progress, _cts.Token),
                    NavigationItem.ConvertPptToPdf => await _conversionService.PptToPdfAsync(file.FilePath, outPath, progress, _cts.Token),
                    _ => false
                };

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
    private void CancelConversion()
    {
        _cts?.Cancel();
        StatusMessage = "Cancelled.";
    }

    [RelayCommand]
    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
            Process.Start("explorer.exe", OutputFolder);
    }

    public void AddFilesFromPaths(IEnumerable<string> paths)
    {
        foreach (var p in paths)
            if (File.Exists(p))
                Files.Add(FileItem.FromPath(p));
    }
}
