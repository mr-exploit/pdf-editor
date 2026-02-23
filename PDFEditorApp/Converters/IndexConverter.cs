using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

using Binding = System.Windows.Data.Binding;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;

namespace PDFEditorApp.Converters;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ListViewItem item)
        {
            var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            return listView?.ItemContainerGenerator.IndexFromContainer(item) + 1 ?? 0;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
