using PDFEditorApp.ViewModels;
using System.Windows.Controls;

using UserControl = System.Windows.Controls.UserControl;
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;

namespace PDFEditorApp.Views;

public partial class ConvertView : UserControl
{
    public ConvertView()
    {
        InitializeComponent();
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && DataContext is ConvertViewModel vm)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            vm.AddFilesFromPaths(files);
        }
    }
}
