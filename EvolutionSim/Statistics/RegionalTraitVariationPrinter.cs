using EvolutionSim.Output;

namespace EvolutionSim.Statistics;

public sealed class RegionalTraitVariationPrinter
{
    private readonly ISimulationOutput _output;

    public RegionalTraitVariationPrinter(
        ISimulationOutput output)
    {
        _output =
            output;
    }

    public void Print(
        int cycle,
        IReadOnlyList<RegionalTraitVariationReport> reports)
    {
        _output.WriteLine();
        _output.WriteLine(
            $"   Variacion genetica regional [Ciclo {cycle}]:"
        );

        foreach (RegionalTraitVariationReport report in reports)
        {
            _output.WriteLine(
                $"      [{report.RegionName}] "
                +
                $"Pop: {report.Population}"
            );

            foreach (TraitVariationStatistics trait in report.Traits)
            {
                _output.WriteLine(
                    $"         "
                    +
                    $"{trait.TraitName,-20}"
                    +
                    $" | Avg: {trait.Average,7:F3}"
                    +
                    $" | Std: {trait.StandardDeviation,7:F3}"
                    +
                    $" | Min: {trait.Minimum,7:F3}"
                    +
                    $" | Max: {trait.Maximum,7:F3}"
                    +
                    $" | Range: {trait.Range,7:F3}"
                );
            }
        }
    }
}
