using System.Windows.Media.Imaging;
using System.Windows;

using FlowDirection = System.Windows.FlowDirection;
using Point = System.Windows.Point;

namespace PDFEditorApp.Core.Helpers;

public static class ThumbnailHelper
{
    public static BitmapSource CreatePlaceholder(int pageNumber, int width = 120, int height = 160)
    {
        var drawingVisual = new System.Windows.Media.DrawingVisual();
        using (var ctx = drawingVisual.RenderOpen())
        {
            ctx.DrawRectangle(
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)),
                new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180)), 1),
                new Rect(0, 0, width, height));

            var ft = new System.Windows.Media.FormattedText(
                $"Page {pageNumber}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Segoe UI"),
                12,
                System.Windows.Media.Brushes.Gray,
                96);
            ctx.DrawText(ft, new Point((width - ft.Width) / 2, (height - ft.Height) / 2));
        }

        var bmp = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bmp.Render(drawingVisual);
        bmp.Freeze();
        return bmp;
    }
}
