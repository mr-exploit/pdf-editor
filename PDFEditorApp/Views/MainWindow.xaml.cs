using PDFEditorApp.ViewModels;
using System.Windows;

namespace PDFEditorApp.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
