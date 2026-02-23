using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace PDFEditorApp.Core.Models;

public partial class PdfPageModel : ObservableObject
{
    [ObservableProperty] private string _sourceFilePath = string.Empty;
    [ObservableProperty] private int _originalPageIndex;
    [ObservableProperty] private int _currentDisplayIndex;
    [ObservableProperty] private BitmapSource? _thumbnail;
    [ObservableProperty] private bool _isSelected;
}
