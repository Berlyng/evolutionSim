using EvolutionSim.Evolution;
using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Statistics;

public sealed class GeneticDistanceBreakdownCalculator
{
    private const double MinimumScale = 1e-9;

    private readonly GeneticDistanceCalculator
        _geneticDistanceCalculator;

    private static readonly TraitDefinition[] TraitDefinitions =
    [
        new("Speed", genome => genome.Speed),
        new("Size", genome => genome.Size),
        new("Metabolism", genome => genome.Metabolism),
        new("OptimalTemperature", genome => genome.OptimalTemperature),
        new("ThermalTolerance", genome => genome.ThermalTolerance),
        new("MeatAdaptation", genome => genome.MeatAdaptation),
        new("PredatoryDrive", genome => genome.PredatoryDrive),
        new("ExplorationDrive", genome => genome.ExplorationDrive),
        new("RiskTolerance", genome => genome.RiskTolerance),
        new("ScavengingDrive", genome => genome.ScavengingDrive),
        new("MateSelectivity", genome => genome.MateSelectivity)
    ];

    public GeneticDistanceBreakdownCalculator(
        GeneticDistanceCalculator geneticDistanceCalculator)
    {
        _geneticDistanceCalculator =
            geneticDistanceCalculator;
    }

    public IReadOnlyList<RegionalGeneticDistanceBreakdown> Calculate(
        IEnumerable<Organism> organisms,
        IEnumerable<Region> regions)
    {
        List<Organism> livingPopulation =
            organisms
                .Where(organism => organism.IsAlive)
                .ToList();

        List<Region> regionList =
            regions.ToList();

        Dictionary<string, double> traitScales =
            CalculateTraitScales(
                livingPopulation
            );

        Dictionary<int, Genome> centroidsByRegion =
            new();

        foreach (Region region in regionList)
        {
            List<Organism> regionalPopulation =
                livingPopulation
                    .Where(
                        organism =>
                            organism.RegionId == region.Id
                    )
                    .ToList();

            if (regionalPopulation.Count == 0)
            {
                continue;
            }

            centroidsByRegion[region.Id] =
                CalculateCentroid(
                    regionalPopulation
                );
        }

        List<RegionalGeneticDistanceBreakdown> reports =
            new();

        for (int i = 0; i < regionList.Count; i++)
        {
            Region regionA =
                regionList[i];

            if (
                !centroidsByRegion.TryGetValue(
                    regionA.Id,
                    out Genome? genomeA
                )
            )
            {
                continue;
            }

            for (int j = i + 1; j < regionList.Count; j++)
            {
                Region regionB =
                    regionList[j];

                if (
                    !centroidsByRegion.TryGetValue(
                        regionB.Id,
                        out Genome? genomeB
                    )
                )
                {
                    continue;
                }

                GeneticDistanceBreakdownCore core =
                    CalculatePair(
                        genomeA,
                        genomeB,
                        traitScales
                    );

                reports.Add(
                    new RegionalGeneticDistanceBreakdown(
                        RegionAId:
                            regionA.Id,

                        RegionAName:
                            regionA.Name,

                        RegionBId:
                            regionB.Id,

                        RegionBName:
                            regionB.Name,

                        ModelDistance:
                            core.ModelDistance,

                        StandardizedEffectDistance:
                            core.StandardizedEffectDistance,

                        Contributions:
                            core.Contributions
                    )
                );
            }
        }

        return reports;
    }

    public IReadOnlyList<SpeciesGeneticDistanceBreakdown> CalculateSpecies(
        IEnumerable<Organism> organisms,
        IEnumerable<SpeciesCluster> species)
    {
        List<Organism> livingPopulation =
            organisms
                .Where(organism => organism.IsAlive)
                .ToList();

        List<SpeciesCluster> speciesList =
            species
                .OrderBy(item => item.SpeciesId)
                .ToList();

        Dictionary<string, double> traitScales =
            CalculateTraitScales(
                livingPopulation
            );

        List<SpeciesGeneticDistanceBreakdown> reports =
            new();

        for (int i = 0; i < speciesList.Count; i++)
        {
            for (int j = i + 1; j < speciesList.Count; j++)
            {
                SpeciesCluster speciesA =
                    speciesList[i];

                SpeciesCluster speciesB =
                    speciesList[j];

                GeneticDistanceBreakdownCore core =
                    CalculatePair(
                        speciesA.Centroid,
                        speciesB.Centroid,
                        traitScales
                    );

                reports.Add(
                    new SpeciesGeneticDistanceBreakdown(
                        SpeciesAId:
                            speciesA.SpeciesId,

                        SpeciesAPopulation:
                            speciesA.Population,

                        SpeciesBId:
                            speciesB.SpeciesId,

                        SpeciesBPopulation:
                            speciesB.Population,

                        ModelDistance:
                            core.ModelDistance,

                        StandardizedEffectDistance:
                            core.StandardizedEffectDistance,

                        Contributions:
                            core.Contributions
                    )
                );
            }
        }

        return reports;
    }

    private GeneticDistanceBreakdownCore CalculatePair(
        Genome genomeA,
        Genome genomeB,
        IReadOnlyDictionary<string, double> traitScales)
    {
        List<TraitDistanceContribution> contributions =
            new();

        double totalSquaredStandardizedDifference =
            0;

        foreach (TraitDefinition trait in TraitDefinitions)
        {
            double valueA =
                trait.ValueSelector(
                    genomeA
                );

            double valueB =
                trait.ValueSelector(
                    genomeB
                );

            double rawDifference =
                Math.Abs(
                    valueA - valueB
                );

            double scale =
                traitScales[trait.Name];

            double standardizedDifference =
                scale > MinimumScale
                    ?
                    rawDifference / scale
                    :
                    0;

            double squaredStandardizedDifference =
                standardizedDifference
                *
                standardizedDifference;

            totalSquaredStandardizedDifference +=
                squaredStandardizedDifference;

            contributions.Add(
                new TraitDistanceContribution(
                    TraitName:
                        trait.Name,

                    ValueA:
                        valueA,

                    ValueB:
                        valueB,

                    RawDifference:
                        rawDifference,

                    PopulationStandardDeviation:
                        scale,

                    StandardizedDifference:
                        standardizedDifference,

                    SquaredStandardizedContribution:
                        squaredStandardizedDifference,

                    ContributionFraction:
                        0
                )
            );
        }

        List<TraitDistanceContribution> normalizedContributions =
            contributions
                .Select(
                    contribution =>
                        contribution with
                        {
                            ContributionFraction =
                                totalSquaredStandardizedDifference > 0
                                    ?
                                    contribution.SquaredStandardizedContribution
                                    /
                                    totalSquaredStandardizedDifference
                                    :
                                    0
                        }
                )
                .OrderByDescending(
                    contribution =>
                        contribution.ContributionFraction
                )
                .ToList();

        double standardizedEffectDistance =
            TraitDefinitions.Length > 0
                ?
                Math.Sqrt(
                    totalSquaredStandardizedDifference
                    /
                    TraitDefinitions.Length
                )
                :
                0;

        double modelDistance =
            _geneticDistanceCalculator.Calculate(
                genomeA,
                genomeB
            );

        return new GeneticDistanceBreakdownCore(
            ModelDistance:
                modelDistance,

            StandardizedEffectDistance:
                standardizedEffectDistance,

            Contributions:
                normalizedContributions
        );
    }

    private static Dictionary<string, double> CalculateTraitScales(
        IReadOnlyCollection<Organism> population)
    {
        Dictionary<string, double> scales =
            new();

        foreach (TraitDefinition trait in TraitDefinitions)
        {
            if (population.Count == 0)
            {
                scales[trait.Name] =
                    1;

                continue;
            }

            double[] values =
                population
                    .Select(
                        organism =>
                            trait.ValueSelector(
                                organism.Genome
                            )
                    )
                    .ToArray();

            double average =
                values.Average();

            double variance =
                values.Average(
                    value =>
                    {
                        double difference =
                            value - average;

                        return difference * difference;
                    }
                );

            double standardDeviation =
                Math.Sqrt(
                    variance
                );

            scales[trait.Name] =
                Math.Max(
                    standardDeviation,
                    MinimumScale
                );
        }

        return scales;
    }

    private static Genome CalculateCentroid(
        IReadOnlyCollection<Organism> population)
    {
        return new Genome(
            speed:
                population.Average(
                    organism => organism.Genome.Speed
                ),

            size:
                population.Average(
                    organism => organism.Genome.Size
                ),

            metabolism:
                population.Average(
                    organism => organism.Genome.Metabolism
                ),

            optimalTemperature:
                population.Average(
                    organism => organism.Genome.OptimalTemperature
                ),

            thermalTolerance:
                population.Average(
                    organism => organism.Genome.ThermalTolerance
                ),

            meatAdaptation:
                population.Average(
                    organism => organism.Genome.MeatAdaptation
                ),

            predatoryDrive:
                population.Average(
                    organism => organism.Genome.PredatoryDrive
                ),

            explorationDrive:
                population.Average(
                    organism => organism.Genome.ExplorationDrive
                ),

            riskTolerance:
                population.Average(
                    organism => organism.Genome.RiskTolerance
                ),

            scavengingDrive:
                population.Average(
                    organism => organism.Genome.ScavengingDrive
                ),

            mateSelectivity:
                population.Average(
                    organism => organism.Genome.MateSelectivity
                )
        );
    }

    private sealed record TraitDefinition(
        string Name,
        Func<Genome, double> ValueSelector
    );

    private sealed record GeneticDistanceBreakdownCore(
        double ModelDistance,
        double StandardizedEffectDistance,
        IReadOnlyList<TraitDistanceContribution> Contributions
    );
}

public sealed record RegionalGeneticDistanceBreakdown(
    int RegionAId,
    string RegionAName,
    int RegionBId,
    string RegionBName,
    double ModelDistance,
    double StandardizedEffectDistance,
    IReadOnlyList<TraitDistanceContribution> Contributions
);

public sealed record SpeciesGeneticDistanceBreakdown(
    int SpeciesAId,
    int SpeciesAPopulation,
    int SpeciesBId,
    int SpeciesBPopulation,
    double ModelDistance,
    double StandardizedEffectDistance,
    IReadOnlyList<TraitDistanceContribution> Contributions
);

public sealed record TraitDistanceContribution(
    string TraitName,
    double ValueA,
    double ValueB,
    double RawDifference,
    double PopulationStandardDeviation,
    double StandardizedDifference,
    double SquaredStandardizedContribution,
    double ContributionFraction
);
