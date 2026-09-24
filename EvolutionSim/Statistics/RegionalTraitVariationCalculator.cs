using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Statistics;

public sealed class RegionalTraitVariationCalculator
{
    private static readonly TraitDefinition[] TraitDefinitions =
    [
        new(
            "Speed",
            organism => organism.Genome.Speed
        ),

        new(
            "Size",
            organism => organism.Genome.Size
        ),

        new(
            "Metabolism",
            organism => organism.Genome.Metabolism
        ),

        new(
            "OptimalTemperature",
            organism => organism.Genome.OptimalTemperature
        ),

        new(
            "ThermalTolerance",
            organism => organism.Genome.ThermalTolerance
        ),

        new(
            "MeatAdaptation",
            organism => organism.Genome.MeatAdaptation
        ),

        new(
            "PredatoryDrive",
            organism => organism.Genome.PredatoryDrive
        ),

        new(
            "ExplorationDrive",
            organism => organism.Genome.ExplorationDrive
        ),

        new(
            "RiskTolerance",
            organism => organism.Genome.RiskTolerance
        ),

        new(
            "ScavengingDrive",
            organism => organism.Genome.ScavengingDrive
        ),

        new(
            "MateSelectivity",
            organism => organism.Genome.MateSelectivity
        )
    ];

    public IReadOnlyList<RegionalTraitVariationReport> Calculate(
        IEnumerable<Organism> organisms,
        IEnumerable<Region> regions)
    {
        List<Organism> organismList =
            organisms.ToList();

        List<RegionalTraitVariationReport> reports =
            new();

        foreach (
            Region region
            in regions
        )
        {
            List<Organism> regionalPopulation =
                organismList
                    .Where(
                        organism =>
                            organism.IsAlive
                            &&
                            organism.RegionId
                            ==
                            region.Id
                    )
                    .ToList();

            List<TraitVariationStatistics> traitStatistics =
                new();

            foreach (
                TraitDefinition trait
                in TraitDefinitions
            )
            {
                TraitVariationStatistics statistics =
                    CalculateTraitStatistics(
                        regionalPopulation,
                        trait
                    );

                traitStatistics.Add(
                    statistics
                );
            }

            reports.Add(
                new RegionalTraitVariationReport(
                    region.Id,
                    region.Name,
                    regionalPopulation.Count,
                    traitStatistics
                )
            );
        }

        return reports;
    }

    private static TraitVariationStatistics
        CalculateTraitStatistics(
            IReadOnlyCollection<Organism> population,
            TraitDefinition trait)
    {
        if (
            population.Count == 0
        )
        {
            return new TraitVariationStatistics(
                TraitName:
                    trait.Name,

                Average:
                    0,

                StandardDeviation:
                    0,

                Minimum:
                    0,

                Maximum:
                    0,

                Range:
                    0
            );
        }

        double[] values =
            population
                .Select(
                    trait.ValueSelector
                )
                .ToArray();

        double average =
            values.Average();

        double variance =
            values
                .Select(
                    value =>
                    {
                        double difference =
                            value
                            -
                            average;

                        return
                            difference
                            *
                            difference;
                    }
                )
                .Average();

        double standardDeviation =
            Math.Sqrt(
                variance
            );

        double minimum =
            values.Min();

        double maximum =
            values.Max();

        double range =
            maximum
            -
            minimum;

        return new TraitVariationStatistics(
            TraitName:
                trait.Name,

            Average:
                average,

            StandardDeviation:
                standardDeviation,

            Minimum:
                minimum,

            Maximum:
                maximum,

            Range:
                range
        );
    }

    private sealed record TraitDefinition(
        string Name,
        Func<Organism, double> ValueSelector
    );
}

public sealed record RegionalTraitVariationReport(
    int RegionId,
    string RegionName,
    int Population,
    IReadOnlyList<TraitVariationStatistics> Traits
);

public sealed record TraitVariationStatistics(
    string TraitName,
    double Average,
    double StandardDeviation,
    double Minimum,
    double Maximum,
    double Range
);