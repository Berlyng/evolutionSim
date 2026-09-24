using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace EvolutionSim.Desktop;

public sealed class TrendChartControl : FrameworkElement
{
    private IReadOnlyList<TrendChartSeries> _series =
        Array.Empty<TrendChartSeries>();

    public string ChartTitle
    {
        get;
        set;
    } = string.Empty;

    public string ValueSuffix
    {
        get;
        set;
    } = string.Empty;

    public void SetSeries(
        IReadOnlyList<TrendChartSeries> series)
    {
        _series = series;
        InvalidateVisual();
    }

    protected override void OnRender(
        DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        double width = ActualWidth;
        double height = ActualHeight;

        if (width <= 80 || height <= 80)
        {
            return;
        }

        Brush foreground =
            new SolidColorBrush(
                Color.FromRgb(
                    244,
                    247,
                    251
                )
            );

        Brush secondaryForeground =
            new SolidColorBrush(
                Color.FromRgb(
                    183,
                    192,
                    206
                )
            );

        Brush gridBrush =
            new SolidColorBrush(
                Color.FromArgb(
                    58,
                    152,
                    160,
                    175
                )
            );

        Pen axisPen =
            new(
                gridBrush,
                1
            );

        double dpi =
            VisualTreeHelper
                .GetDpi(this)
                .PixelsPerDip;

        const double left = 62;
        const double right = 16;
        const double top = 42;
        const double bottom = 42;

        Rect plot =
            new(
                left,
                top,
                Math.Max(1, width - left - right),
                Math.Max(1, height - top - bottom)
            );

        DrawText(
            drawingContext,
            ChartTitle,
            new Point(8, 8),
            15,
            foreground,
            dpi,
            FontWeights.SemiBold
        );

        List<TrendChartPoint> allPoints =
            _series
                .SelectMany(series => series.Points)
                .ToList();

        if (allPoints.Count == 0)
        {
            DrawText(
                drawingContext,
                "Sin datos todavía",
                new Point(
                    plot.Left + 12,
                    plot.Top + 12
                ),
                12,
                secondaryForeground,
                dpi
            );

            return;
        }

        double minX = allPoints.Min(point => point.X);
        double maxX = allPoints.Max(point => point.X);
        double minY = allPoints.Min(point => point.Y);
        double maxY = allPoints.Max(point => point.Y);

        if (Math.Abs(maxX - minX) < 1e-9)
        {
            maxX = minX + 1;
        }

        if (Math.Abs(maxY - minY) < 1e-9)
        {
            double padding =
                Math.Abs(maxY) > 1e-9
                    ? Math.Abs(maxY) * 0.10
                    : 1;

            minY -= padding;
            maxY += padding;
        }
        else
        {
            double padding =
                (maxY - minY) * 0.08;

            minY -= padding;
            maxY += padding;
        }

        const int horizontalGridLines = 4;

        for (
            int index = 0;
            index <= horizontalGridLines;
            index++
        )
        {
            double fraction =
                index / (double)horizontalGridLines;

            double y =
                plot.Bottom
                -
                fraction * plot.Height;

            drawingContext.DrawLine(
                axisPen,
                new Point(plot.Left, y),
                new Point(plot.Right, y)
            );

            double value =
                minY
                +
                fraction * (maxY - minY);

            DrawText(
                drawingContext,
                FormatValue(value),
                new Point(4, y - 8),
                10,
                secondaryForeground,
                dpi
            );
        }

        drawingContext.DrawLine(
            axisPen,
            new Point(plot.Left, plot.Bottom),
            new Point(plot.Right, plot.Bottom)
        );

        DrawText(
            drawingContext,
            $"C{minX:F0}",
            new Point(plot.Left, plot.Bottom + 8),
            10,
            secondaryForeground,
            dpi
        );

        string maxCycleText =
            $"C{maxX:F0}";

        FormattedText maxCycleFormatted =
            CreateFormattedText(
                maxCycleText,
                10,
                secondaryForeground,
                dpi,
                FontWeights.Normal
            );

        drawingContext.DrawText(
            maxCycleFormatted,
            new Point(
                plot.Right - maxCycleFormatted.Width,
                plot.Bottom + 8
            )
        );

        foreach (
            TrendChartSeries series
            in _series
        )
        {
            if (series.Points.Count == 0)
            {
                continue;
            }

            Pen seriesPen =
                new(
                    series.Brush,
                    2
                );

            StreamGeometry geometry =
                new();

            using (
                StreamGeometryContext context =
                    geometry.Open()
            )
            {
                TrendChartPoint first =
                    series.Points[0];

                context.BeginFigure(
                    MapPoint(
                        first,
                        plot,
                        minX,
                        maxX,
                        minY,
                        maxY
                    ),
                    false,
                    false
                );

                for (
                    int index = 1;
                    index < series.Points.Count;
                    index++
                )
                {
                    context.LineTo(
                        MapPoint(
                            series.Points[index],
                            plot,
                            minX,
                            maxX,
                            minY,
                            maxY
                        ),
                        true,
                        false
                    );
                }
            }

            geometry.Freeze();

            drawingContext.DrawGeometry(
                null,
                seriesPen,
                geometry
            );
        }

        double legendX = plot.Left;
        double legendY = 22;

        foreach (
            TrendChartSeries series
            in _series
        )
        {
            drawingContext.DrawRectangle(
                series.Brush,
                null,
                new Rect(
                    legendX,
                    legendY,
                    12,
                    3
                )
            );

            FormattedText label =
                CreateFormattedText(
                    series.Name,
                    10,
                    secondaryForeground,
                    dpi,
                    FontWeights.Normal
                );

            drawingContext.DrawText(
                label,
                new Point(
                    legendX + 17,
                    legendY - 6
                )
            );

            legendX +=
                25 + label.Width;
        }
    }

    private string FormatValue(
        double value)
    {
        string formatted =
            Math.Abs(value) >= 1000
                ? value.ToString("N0")
                : value.ToString("F1");

        return
            string.IsNullOrWhiteSpace(ValueSuffix)
                ? formatted
                : $"{formatted}{ValueSuffix}";
    }

    private static Point MapPoint(
        TrendChartPoint point,
        Rect plot,
        double minX,
        double maxX,
        double minY,
        double maxY)
    {
        double x =
            plot.Left
            +
            (
                (point.X - minX)
                /
                (maxX - minX)
            )
            *
            plot.Width;

        double y =
            plot.Bottom
            -
            (
                (point.Y - minY)
                /
                (maxY - minY)
            )
            *
            plot.Height;

        return new Point(x, y);
    }

    private static void DrawText(
        DrawingContext drawingContext,
        string text,
        Point origin,
        double fontSize,
        Brush brush,
        double dpi,
        FontWeight? weight = null)
    {
        drawingContext.DrawText(
            CreateFormattedText(
                text,
                fontSize,
                brush,
                dpi,
                weight ?? FontWeights.Normal
            ),
            origin
        );
    }

    private static FormattedText CreateFormattedText(
        string text,
        double fontSize,
        Brush brush,
        double dpi,
        FontWeight weight)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(
                new FontFamily("Segoe UI"),
                FontStyles.Normal,
                weight,
                FontStretches.Normal
            ),
            fontSize,
            brush,
            dpi
        );
    }
}

public sealed record TrendChartSeries(
    string Name,
    Brush Brush,
    IReadOnlyList<TrendChartPoint> Points
);

public readonly record struct TrendChartPoint(
    double X,
    double Y
);
