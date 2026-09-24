using EvolutionSim.Output;

namespace EvolutionSim.Statistics;

public sealed class GeneticDistanceBreakdownPrinter
{
    private readonly ISimulationOutput _output;

    public GeneticDistanceBreakdownPrinter(
        ISimulationOutput output)
    {
        _output =
            output;
    }

    public void PrintRegional(
        int cycle,
        IReadOnlyList<RegionalGeneticDistanceBreakdown> reports)
    {
        _output.WriteLine();

        _output.WriteLine(
            $"   Desglose genetico normalizado por region [Ciclo {cycle}]:"
        );

        foreach (RegionalGeneticDistanceBreakdown report in reports)
        {
            _output.WriteLine();

            _output.WriteLine(
                $"      {report.RegionAName} <-> {report.RegionBName} | " +
                $"ModelDist: {report.ModelDistance:F3} | " +
                $"StdEffectDist: {report.StandardizedEffectDistance:F3}"
            );

            PrintContributions(
                report.Contributions
            );
        }
    }

    public void PrintSpecies(
        int cycle,
        IReadOnlyList<SpeciesGeneticDistanceBreakdown> reports)
    {
        if (reports.Count == 0)
        {
            return;
        }

        _output.WriteLine();

        _output.WriteLine(
            $"   Desglose genetico normalizado entre especies [Ciclo {cycle}]:"
        );

        foreach (SpeciesGeneticDistanceBreakdown report in reports)
        {
            _output.WriteLine();

            _output.WriteLine(
                $"      S{report.SpeciesAId} (Pop {report.SpeciesAPopulation}) " +
                $"<-> S{report.SpeciesBId} (Pop {report.SpeciesBPopulation}) | " +
                $"ModelDist: {report.ModelDistance:F3} | " +
                $"StdEffectDist: {report.StandardizedEffectDistance:F3}"
            );

            PrintContributions(
                report.Contributions
            );
        }
    }

    private void PrintContributions(
        IReadOnlyList<TraitDistanceContribution> contributions)
    {
        foreach (TraitDistanceContribution contribution in contributions)
        {
            _output.WriteLine(
                $"         " +
                $"{contribution.TraitName,-20}" +
                $" | A: {contribution.ValueA,7:F3}" +
                $" | B: {contribution.ValueB,7:F3}" +
                $" | Delta: {contribution.RawDifference,7:F3}" +
                $" | PopStd: {contribution.PopulationStandardDeviation,7:F3}" +
                $" | ZDelta: {contribution.StandardizedDifference,7:F3}" +
                $" | Contrib: {contribution.ContributionFraction,7:P2}"
            );
        }
    }
}
