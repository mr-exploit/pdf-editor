using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PDFEditorApp.Core.Models;
using PDFEditorApp.Core.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace PDFEditorApp.ViewModels;

public partial class ReorganizeViewModel : ObservableObject
{
    private readonly IReorganizeService _reorganizeService;

    [ObservableProperty] private string _currentFilePath = string.Empty;
    [ObservableProperty] private string _currentFileName = string.Empty;
    [ObservableProperty] private double _saveProgress;
    [ObservableProperty] private string _statusMessage = "Open a PDF file to begin.";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasFile;

    public ObservableCollection<PdfPageModel> Pages { get; } = new();

    private CancellationTokenSource? _cts;

    public ReorganizeViewModel(IReorganizeService reorganizeService)
    {
        _reorganizeService = reorganizeService;
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var dialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf" };
        if (dialog.ShowDialog() != true) return;

        IsBusy = true;
        StatusMessage = "Loading pages...";
        Pages.Clear();
        CurrentFilePath = dialog.FileName;
        CurrentFileName = Path.GetFileName(dialog.FileName);

        try
        {
            var pages = await _reorganizeService.LoadPagesAsync(dialog.FileName);
            foreach (var p in pages)
                Pages.Add(p);
            HasFile = true;
            StatusMessage = $"Loaded {Pages.Count} pages from {CurrentFileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void TogglePageSelection(PdfPageModel? page)
    {
        if (page != null) page.IsSelected = !page.IsSelected;
    }

    [RelayCommand]
    private void DeleteSelectedPages()
    {
        var selected = Pages.Where(p => p.IsSelected).ToList();
        if (selected.Count == 0) return;

        var result = MessageBox.Show(
            $"Delete {selected.Count} selected page(s)?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            foreach (var p in selected)
                Pages.Remove(p);
            UpdatePageIndices();
            StatusMessage = $"Deleted {selected.Count} page(s). {Pages.Count} pages remaining.";
        }
    }

    [RelayCommand]
    private void SortAscending()
    {
        var sorted = Pages.OrderBy(p => p.OriginalPageIndex).ToList();
        Pages.Clear();
        foreach (var p in sorted) Pages.Add(p);
        UpdatePageIndices();
        StatusMessage = "Pages sorted ascending.";
    }

    [RelayCommand]
    private void SortDescending()
    {
        var sorted = Pages.OrderByDescending(p => p.OriginalPageIndex).ToList();
        Pages.Clear();
        foreach (var p in sorted) Pages.Add(p);
        UpdatePageIndices();
        StatusMessage = "Pages sorted descending.";
    }

    [RelayCommand]
    private async Task AddFromAnotherPdfAsync()
    {
        var dialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf" };
        if (dialog.ShowDialog() != true) return;

        IsBusy = true;
        StatusMessage = "Loading additional PDF...";
        try
        {
            var newPages = await _reorganizeService.LoadPagesAsync(dialog.FileName);
            foreach (var p in newPages)
            {
                p.CurrentDisplayIndex = Pages.Count + 1;
                Pages.Add(p);
            }
            UpdatePageIndices();
            StatusMessage = $"Added {newPages.Count} pages from {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var dialog = new SaveFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", FileName = "reorganized.pdf" };
        if (dialog.ShowDialog() != true) return;
        await SaveToPathAsync(dialog.FileName);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(CurrentFilePath)) return;
        var tmpPath = CurrentFilePath + ".tmp";
        if (await SaveToPathAsync(tmpPath))
        {
            File.Replace(tmpPath, CurrentFilePath, null);
            StatusMessage = "Saved successfully.";
        }
    }

    private async Task<bool> SaveToPathAsync(string outputPath)
    {
        IsBusy = true;
        _cts = new CancellationTokenSource();
        try
        {
            StatusMessage = "Saving...";
            var progress = new Progress<double>(p => SaveProgress = p * 100);
            bool success = await _reorganizeService.SaveReorganizedAsync(Pages, outputPath, progress, _cts.Token);
            StatusMessage = success ? "Saved successfully!" : "Save failed.";
            return success;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void UpdatePageIndices()
    {
        for (int i = 0; i < Pages.Count; i++)
            Pages[i].CurrentDisplayIndex = i + 1;
    }

    public void MovePage(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || toIndex < 0 || fromIndex >= Pages.Count || toIndex >= Pages.Count) return;
        Pages.Move(fromIndex, toIndex);
        UpdatePageIndices();
    }
}
