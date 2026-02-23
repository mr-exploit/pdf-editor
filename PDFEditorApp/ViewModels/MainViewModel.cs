using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PDFEditorApp.ViewModels;

public enum NavigationItem
{
    ConvertPdfToWord,
    ConvertPdfToExcel,
    ConvertPdfToPpt,
    ConvertWordToPdf,
    ConvertExcelToPdf,
    ConvertPptToPdf,
    Compress,
    Merge,
    Split,
    Reorganize
}

public partial class MainViewModel : ObservableObject
{
    private readonly ConvertViewModel _convertViewModel;
    private readonly CompressViewModel _compressViewModel;
    private readonly MergeViewModel _mergeViewModel;
    private readonly SplitViewModel _splitViewModel;
    private readonly ReorganizeViewModel _reorganizeViewModel;

    [ObservableProperty] private ObservableObject? _currentViewModel;
    [ObservableProperty] private NavigationItem _selectedNavItem = NavigationItem.ConvertPdfToWord;
    [ObservableProperty] private string _statusMessage = "Ready";

    public MainViewModel(
        ConvertViewModel convertViewModel,
        CompressViewModel compressViewModel,
        MergeViewModel mergeViewModel,
        SplitViewModel splitViewModel,
        ReorganizeViewModel reorganizeViewModel)
    {
        _convertViewModel = convertViewModel;
        _compressViewModel = compressViewModel;
        _mergeViewModel = mergeViewModel;
        _splitViewModel = splitViewModel;
        _reorganizeViewModel = reorganizeViewModel;

        NavigateTo(NavigationItem.ConvertPdfToWord);
    }

    [RelayCommand]
    private void NavigateTo(NavigationItem item)
    {
        SelectedNavItem = item;
        CurrentViewModel = item switch
        {
            NavigationItem.ConvertPdfToWord => _convertViewModel,
            NavigationItem.ConvertPdfToExcel => _convertViewModel,
            NavigationItem.ConvertPdfToPpt => _convertViewModel,
            NavigationItem.ConvertWordToPdf => _convertViewModel,
            NavigationItem.ConvertExcelToPdf => _convertViewModel,
            NavigationItem.ConvertPptToPdf => _convertViewModel,
            NavigationItem.Compress => _compressViewModel,
            NavigationItem.Merge => _mergeViewModel,
            NavigationItem.Split => _splitViewModel,
            NavigationItem.Reorganize => _reorganizeViewModel,
            _ => _convertViewModel
        };

        if (CurrentViewModel == _convertViewModel)
        {
            _convertViewModel.SetConversionMode(item);
        }
    }
}
