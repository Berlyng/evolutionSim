using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;


public sealed record SuccessfulMatingRegionStatistics(
    int RegionId,
    string RegionName,
    int BirthCount,
    int ResolvedPairCount,
    int UnresolvedBirthCount,
    double AverageGeneticDistance,
    double MaximumGeneticDistance,
    double AverageEcologicalSimilarity,
    double MinimumEcologicalSimilarity,
    double AverageMateAcceptance,
    double MinimumMateAcceptance,
    double AverageOptimalTemperatureDifference,
    double MaximumOptimalTemperatureDifference,
    int LowEcologicalSimilarityPairs,
    int LowMateAcceptancePairs);


public sealed class SuccessfulMatingStatisticsCalculator
{
    private const double EcologicalSelectionStrengthMultiplier =
        3.0;

    private const double LowEcologicalSimilarityThreshold =
        0.70;

    private const double LowMateAcceptanceThreshold =
        0.50;


    private readonly GeneticDistanceCalculator
        _distanceCalculator;

    private readonly MateCompatibilityCalculator
        _compatibilityCalculator;

    private readonly EcologicalMateSimilarityCalculator
        _ecologicalSimilarityCalculator;


    public SuccessfulMatingStatisticsCalculator(
        GeneticDistanceCalculator distanceCalculator,
        MateCompatibilityCalculator compatibilityCalculator,
        EcologicalMateSimilarityCalculator ecologicalSimilarityCalculator)
    {
        _distanceCalculator =
            distanceCalculator;

        _compatibilityCalculator =
            compatibilityCalculator;

        _ecologicalSimilarityCalculator =
            ecologicalSimilarityCalculator;
    }


    public List<SuccessfulMatingRegionStatistics> Calculate(
        List<Organism> newborns,
        List<Organism> parentPopulation,
        Worldd world)
    {
        Dictionary<Guid, Organism> parentById =
            parentPopulation
                .ToDictionary(
                    organism => organism.Id
                );


        List<SuccessfulMatingRegionStatistics> results =
            new();


        foreach (
            Region region
            in world.Regions
        )
        {
            List<Organism> regionalNewborns =
                newborns
                    .Where(
                        newborn =>
                            newborn.RegionId
                            ==
                            region.Id
                    )
                    .ToList();


            List<SuccessfulMatingPairMetrics> resolvedPairs =
                new();


            int unresolvedBirthCount =
                0;


            foreach (
                Organism newborn
                in regionalNewborns
            )
            {
                if (
                    !newborn.ParentAId.HasValue
                    ||
                    !newborn.ParentBId.HasValue
                )
                {
                    unresolvedBirthCount++;
                    continue;
                }


                if (
                    !parentById.TryGetValue(
                        newborn.ParentAId.Value,
                        out Organism? parentA
                    )
                    ||
                    !parentById.TryGetValue(
                        newborn.ParentBId.Value,
                        out Organism? parentB
                    )
                )
                {
                    unresolvedBirthCount++;
                    continue;
                }


                double geneticDistance =
                    _distanceCalculator.Calculate(
                        parentA.Genome,
                        parentB.Genome
                    );


                double geneticCompatibility =
                    _compatibilityCalculator.CalculateCompatibility(
                        parentA.Genome,
                        parentB.Genome
                    );


                double ecologicalSimilarity =
                    _ecologicalSimilarityCalculator.Calculate(
                        parentA.Genome,
                        parentB.Genome
                    );


                double pairMateSelectivity =
                    (
                        parentA.MateSelectivity
                        +
                        parentB.MateSelectivity
                    )
                    /
                    2.0;


                pairMateSelectivity =
                    Math.Clamp(
                        pairMateSelectivity,
                        0,
                        1
                    );


                double ecologicalSelectionStrength =
                    EcologicalSelectionStrengthMultiplier
                    *
                    pairMateSelectivity;


                double ecologicalPreference =
                    Math.Pow(
                        ecologicalSimilarity,
                        ecologicalSelectionStrength
                    );


                ecologicalPreference =
                    Math.Clamp(
                        ecologicalPreference,
                        0.05,
                        1.0
                    );


                double mateAcceptance =
                    geneticCompatibility
                    *
                    ecologicalPreference;


                mateAcceptance =
                    Math.Clamp(
                        mateAcceptance,
                        0,
                        1
                    );


                double optimalTemperatureDifference =
                    Math.Abs(
                        parentA.OptimalTemperature
                        -
                        parentB.OptimalTemperature
                    );


                resolvedPairs.Add(
                    new SuccessfulMatingPairMetrics(
                        GeneticDistance:
                            geneticDistance,

                        EcologicalSimilarity:
                            ecologicalSimilarity,

                        MateAcceptance:
                            mateAcceptance,

                        OptimalTemperatureDifference:
                            optimalTemperatureDifference
                    )
                );
            }


            if (
                resolvedPairs.Count == 0
            )
            {
                results.Add(
                    new SuccessfulMatingRegionStatistics(
                        RegionId:
                            region.Id,

                        RegionName:
                            region.Name,

                        BirthCount:
                            regionalNewborns.Count,

                        ResolvedPairCount:
                            0,

                        UnresolvedBirthCount:
                            unresolvedBirthCount,

                        AverageGeneticDistance:
                            0,

                        MaximumGeneticDistance:
                            0,

                        AverageEcologicalSimilarity:
                            0,

                        MinimumEcologicalSimilarity:
                            0,

                        AverageMateAcceptance:
                            0,

                        MinimumMateAcceptance:
                            0,

                        AverageOptimalTemperatureDifference:
                            0,

                        MaximumOptimalTemperatureDifference:
                            0,

                        LowEcologicalSimilarityPairs:
                            0,

                        LowMateAcceptancePairs:
                            0
                    )
                );

                continue;
            }


            results.Add(
                new SuccessfulMatingRegionStatistics(
                    RegionId:
                        region.Id,

                    RegionName:
                        region.Name,

                    BirthCount:
                        regionalNewborns.Count,

                    ResolvedPairCount:
                        resolvedPairs.Count,

                    UnresolvedBirthCount:
                        unresolvedBirthCount,

                    AverageGeneticDistance:
                        resolvedPairs.Average(
                            pair =>
                                pair.GeneticDistance
                        ),

                    MaximumGeneticDistance:
                        resolvedPairs.Max(
                            pair =>
                                pair.GeneticDistance
                        ),

                    AverageEcologicalSimilarity:
                        resolvedPairs.Average(
                            pair =>
                                pair.EcologicalSimilarity
                        ),

                    MinimumEcologicalSimilarity:
                        resolvedPairs.Min(
                            pair =>
                                pair.EcologicalSimilarity
                        ),

                    AverageMateAcceptance:
                        resolvedPairs.Average(
                            pair =>
                                pair.MateAcceptance
                        ),

                    MinimumMateAcceptance:
                        resolvedPairs.Min(
                            pair =>
                                pair.MateAcceptance
                        ),

                    AverageOptimalTemperatureDifference:
                        resolvedPairs.Average(
                            pair =>
                                pair.OptimalTemperatureDifference
                        ),

                    MaximumOptimalTemperatureDifference:
                        resolvedPairs.Max(
                            pair =>
                                pair.OptimalTemperatureDifference
                        ),

                    LowEcologicalSimilarityPairs:
                        resolvedPairs.Count(
                            pair =>
                                pair.EcologicalSimilarity
                                <
                                LowEcologicalSimilarityThreshold
                        ),

                    LowMateAcceptancePairs:
                        resolvedPairs.Count(
                            pair =>
                                pair.MateAcceptance
                                <
                                LowMateAcceptanceThreshold
                        )
                )
            );
        }


        return results;
    }


    private sealed record SuccessfulMatingPairMetrics(
        double GeneticDistance,
        double EcologicalSimilarity,
        double MateAcceptance,
        double OptimalTemperatureDifference);
}
