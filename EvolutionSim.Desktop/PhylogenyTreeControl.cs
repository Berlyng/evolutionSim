using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EvolutionSim.Simulation;

namespace EvolutionSim.Desktop;

/// <summary>
/// Árbol filogenético temporal.
/// Eje X = ciclo.
/// Eje Y = ramas de especies.
/// </summary>
public sealed class PhylogenyTreeControl : FrameworkElement
{
    private const double LeftMargin = 78;
    private const double RightMargin = 28;
    private const double TopMargin = 34;
    private const double BottomMargin = 44;
    private const double RowHeight = 42;

    private SimulationPhylogenyStepResult? _phylogeny;

    public PhylogenyTreeControl()
    {
        SnapsToDevicePixels =
            true;

        UseLayoutRounding =
            true;
    }

    public void SetPhylogeny(
        SimulationPhylogenyStepResult phylogeny)
    {
        _phylogeny =
            phylogeny;

        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override Size MeasureOverride(
        Size availableSize)
    {
        int nodeCount =
            _phylogeny?.Nodes.Count
            ??
            1;

        double desiredHeight =
            TopMargin
            +
            BottomMargin
            +
            Math.Max(
                1,
                nodeCount
            )
            *
            RowHeight;

        return new Size(
            double.IsInfinity(
                availableSize.Width
            )
                ? 900
                : availableSize.Width,
            Math.Max(
                260,
                desiredHeight
            )
        );
    }

    protected override void OnRender(
        DrawingContext drawingContext)
    {
        base.OnRender(
            drawingContext
        );

        drawingContext.DrawRectangle(
            BrushFromHex(
                "#11151B"
            ),
            null,
            new Rect(
                0,
                0,
                ActualWidth,
                ActualHeight
            )
        );

        if (
            _phylogeny is null
            ||
            _phylogeny.Nodes.Count == 0
        )
        {
            DrawText(
                drawingContext,
                "SIN DATOS FILOGENÉTICOS",
                24,
                24,
                13,
                BrushFromHex(
                    "#98A0AF"
                ),
                FontWeights.SemiBold
            );

            return;
        }

        List<SimulationPhylogenyNodeStepResult> nodes =
            OrderNodes(
                _phylogeny.Nodes
            );

        Dictionary<int, double> yBySpecies =
            new();

        for (
            int i = 0;
            i < nodes.Count;
            i++
        )
        {
            yBySpecies[
                nodes[i].SpeciesId
            ] =
                TopMargin
                +
                i
                *
                RowHeight
                +
                RowHeight
                /
                2;
        }

        int maximumCycle =
            Math.Max(
                1,
                _phylogeny.CurrentCycle
            );

        double timelineWidth =
            Math.Max(
                80,
                ActualWidth
                -
                LeftMargin
                -
                RightMargin
            );

        DrawTimelineGrid(
            drawingContext,
            maximumCycle,
            timelineWidth
        );

        // Primero conexiones para que queden debajo de las ramas.
        foreach (
            SimulationPhylogenyEdgeStepResult edge
            in _phylogeny.Edges
        )
        {
            if (
                !yBySpecies.TryGetValue(
                    edge.ParentSpeciesId,
                    out double parentY
                )
                ||
                !yBySpecies.TryGetValue(
                    edge.ChildSpeciesId,
                    out double childY
                )
            )
            {
                continue;
            }

            double branchX =
                CycleToX(
                    edge.BranchCycle,
                    maximumCycle,
                    timelineWidth
                );

            Pen connectorPen =
                new(
                    BrushFromHex(
                        "#657080"
                    ),
                    1.5
                );

            drawingContext.DrawLine(
                connectorPen,
                new Point(
                    branchX,
                    parentY
                ),
                new Point(
                    branchX,
                    childY
                )
            );

            drawingContext.DrawEllipse(
                BrushFromHex(
                    "#C792EA"
                ),
                null,
                new Point(
                    branchX,
                    childY
                ),
                4,
                4
            );
        }

        foreach (
            SimulationPhylogenyNodeStepResult node
            in nodes
        )
        {
            double y =
                yBySpecies[
                    node.SpeciesId
                ];

            double startX =
                CycleToX(
                    node.ConfirmedCycle,
                    maximumCycle,
                    timelineWidth
                );

            int visualEndCycle =
                node.EndCycle
                ??
                (
                    node.IsActive
                        ? maximumCycle
                        : node.LastConfirmedCycle
                );

            double endX =
                CycleToX(
                    Math.Max(
                        node.ConfirmedCycle,
                        visualEndCycle
                    ),
                    maximumCycle,
                    timelineWidth
                );

            Brush branchBrush =
                node.IsActive
                    ? BrushFromHex(
                        "#5DD6A8"
                    )
                    : BrushFromHex(
                        "#E8A33D"
                    );

            Pen branchPen =
                new(
                    branchBrush,
                    node.IsActive
                        ? 3
                        : 2
                );

            drawingContext.DrawLine(
                branchPen,
                new Point(
                    startX,
                    y
                ),
                new Point(
                    endX,
                    y
                )
            );

            drawingContext.DrawEllipse(
                branchBrush,
                null,
                new Point(
                    startX,
                    y
                ),
                4,
                4
            );

            if (!node.IsActive)
            {
                drawingContext.DrawLine(
                    new Pen(
                        BrushFromHex(
                            "#E2596B"
                        ),
                        2
                    ),
                    new Point(
                        endX - 4,
                        y - 4
                    ),
                    new Point(
                        endX + 4,
                        y + 4
                    )
                );

                drawingContext.DrawLine(
                    new Pen(
                        BrushFromHex(
                            "#E2596B"
                        ),
                        2
                    ),
                    new Point(
                        endX - 4,
                        y + 4
                    ),
                    new Point(
                        endX + 4,
                        y - 4
                    )
                );
            }

            DrawText(
                drawingContext,
                $"S{node.SpeciesId}",
                16,
                y - 8,
                12,
                branchBrush,
                FontWeights.Bold
            );

            string parentText =
                node.ParentSpeciesId
                is int parentSpeciesId
                    ? $"← S{parentSpeciesId}"
                    : "RAÍZ";

            DrawText(
                drawingContext,
                parentText,
                42,
                y - 7,
                9,
                BrushFromHex(
                    "#98A0AF"
                ),
                FontWeights.SemiBold
            );

            DrawText(
                drawingContext,
                $"pop {node.LatestPopulation:N0}",
                Math.Min(
                    endX + 8,
                    ActualWidth - 82
                ),
                y - 7,
                9,
                BrushFromHex(
                    "#B7C0CE"
                ),
                FontWeights.Normal
            );
        }
    }

    private void DrawTimelineGrid(
        DrawingContext drawingContext,
        int maximumCycle,
        double timelineWidth)
    {
        const int tickCount =
            5;

        Pen gridPen =
            new(
                BrushFromHex(
                    "#2A2F3A"
                ),
                1
            );

        for (
            int i = 0;
            i <= tickCount;
            i++
        )
        {
            double ratio =
                i
                /
                (double)tickCount;

            int cycle =
                (int)Math.Round(
                    maximumCycle
                    *
                    ratio
                );

            double x =
                LeftMargin
                +
                timelineWidth
                *
                ratio;

            drawingContext.DrawLine(
                gridPen,
                new Point(
                    x,
                    TopMargin - 12
                ),
                new Point(
                    x,
                    Math.Max(
                        TopMargin,
                        ActualHeight - BottomMargin
                    )
                )
            );

            DrawText(
                drawingContext,
                cycle.ToString(
                    "N0"
                ),
                x - 12,
                Math.Max(
                    8,
                    ActualHeight
                    -
                    BottomMargin
                    +
                    12
                ),
                9,
                BrushFromHex(
                    "#98A0AF"
                ),
                FontWeights.Normal
            );
        }
    }

    private static List<SimulationPhylogenyNodeStepResult>
        OrderNodes(
            IReadOnlyList<SimulationPhylogenyNodeStepResult> nodes)
    {
        Dictionary<int, List<SimulationPhylogenyNodeStepResult>>
            childrenByParent =
                nodes
                    .Where(
                        node =>
                            node.ParentSpeciesId
                            is not null
                    )
                    .GroupBy(
                        node =>
                            node.ParentSpeciesId!.Value
                    )
                    .ToDictionary(
                        group =>
                            group.Key,
                        group =>
                            group
                                .OrderBy(
                                    child =>
                                        child.ConfirmedCycle
                                )
                                .ThenBy(
                                    child =>
                                        child.SpeciesId
                                )
                                .ToList()
                    );

        Dictionary<int, SimulationPhylogenyNodeStepResult>
            byId =
                nodes.ToDictionary(
                    node =>
                        node.SpeciesId
                );

        List<SimulationPhylogenyNodeStepResult>
            ordered =
                new();

        HashSet<int>
            visited =
                new();

        void Visit(
            SimulationPhylogenyNodeStepResult node)
        {
            if (
                !visited.Add(
                    node.SpeciesId
                )
            )
            {
                return;
            }

            ordered.Add(
                node
            );

            if (
                childrenByParent.TryGetValue(
                    node.SpeciesId,
                    out List<SimulationPhylogenyNodeStepResult>?
                        children
                )
            )
            {
                foreach (
                    SimulationPhylogenyNodeStepResult child
                    in children
                )
                {
                    Visit(
                        child
                    );
                }
            }
        }

        foreach (
            SimulationPhylogenyNodeStepResult root
            in nodes
                .Where(
                    node =>
                        node.ParentSpeciesId
                        is null
                        ||
                        !byId.ContainsKey(
                            node.ParentSpeciesId.Value
                        )
                )
                .OrderBy(
                    node =>
                        node.ConfirmedCycle
                )
                .ThenBy(
                    node =>
                        node.SpeciesId
                )
        )
        {
            Visit(
                root
            );
        }

        foreach (
            SimulationPhylogenyNodeStepResult remaining
            in nodes
                .OrderBy(
                    node =>
                        node.ConfirmedCycle
                )
                .ThenBy(
                    node =>
                        node.SpeciesId
                )
        )
        {
            Visit(
                remaining
            );
        }

        return ordered;
    }

    private static double CycleToX(
        int cycle,
        int maximumCycle,
        double timelineWidth)
    {
        double normalized =
            Math.Clamp(
                cycle
                /
                (double)Math.Max(
                    1,
                    maximumCycle
                ),
                0,
                1
            );

        return LeftMargin
            +
            timelineWidth
            *
            normalized;
    }

    private static void DrawText(
        DrawingContext drawingContext,
        string text,
        double x,
        double y,
        double fontSize,
        Brush brush,
        FontWeight fontWeight)
    {
        FormattedText formattedText =
            new(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(
                        "Segoe UI"
                    ),
                    FontStyles.Normal,
                    fontWeight,
                    FontStretches.Normal
                ),
                fontSize,
                brush,
                1.0
            );

        drawingContext.DrawText(
            formattedText,
            new Point(
                x,
                y
            )
        );
    }

    private static Brush BrushFromHex(
        string hex)
    {
        Brush brush =
            new BrushConverter()
                .ConvertFromString(
                    hex
                )
            as Brush
            ??
            Brushes.White;

        if (
            brush.CanFreeze
        )
        {
            brush.Freeze();
        }

        return brush;
    }
}
