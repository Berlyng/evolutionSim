using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;


public sealed record RegionalReproductiveIsolation(
    int RegionAId,
    string RegionAName,
    int RegionBId,
    string RegionBName,
    double GeneticDistance,
    double GeneticCompatibility,
    double EcologicalSimilarity,
    double PairMateSelectivity,
    double EcologicalPreference,
    double MateAcceptance);


public sealed class RegionalReproductiveIsolationCalculator
{
    private readonly GeneticDistanceCalculator
        _distanceCalculator;

    private readonly MateCompatibilityCalculator
        _compatibilityCalculator;

    private readonly EcologicalMateSimilarityCalculator
        _ecologicalSimilarityCalculator;


    public RegionalReproductiveIsolationCalculator(
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


    public List<RegionalReproductiveIsolation> Calculate(
        List<Organism> population,
        Worldd world)
    {
        List<RegionalGenomeProfile> profiles =
            world.Regions
                .Select(
                    region =>
                    {
                        List<Organism> members =
                            population
                                .Where(
                                    organism =>
                                        organism.IsAlive
                                        &&
                                        organism.RegionId
                                        ==
                                        region.Id
                                )
                                .ToList();


                        return new
                        {
                            Region = region,
                            Members = members
                        };
                    }
                )
                .Where(
                    item =>
                        item.Members.Count > 0
                )
                .Select(
                    item =>
                        new RegionalGenomeProfile(
                            RegionId:
                                item.Region.Id,

                            RegionName:
                                item.Region.Name,

                            Population:
                                item.Members.Count,

                            Centroid:
                                CalculateCentroid(
                                    item.Members
                                )
                        )
                )
                .ToList();


        List<RegionalReproductiveIsolation> results =
            new();


        for (
            int i = 0;
            i < profiles.Count;
            i++
        )
        {
            for (
                int j = i + 1;
                j < profiles.Count;
                j++
            )
            {
                RegionalGenomeProfile profileA =
                    profiles[i];

                RegionalGenomeProfile profileB =
                    profiles[j];


                double geneticDistance =
                    _distanceCalculator.Calculate(
                        profileA.Centroid,
                        profileB.Centroid
                    );


                double geneticCompatibility =
                    _compatibilityCalculator.CalculateCompatibility(
                        profileA.Centroid,
                        profileB.Centroid
                    );


                double ecologicalSimilarity =
                    _ecologicalSimilarityCalculator.Calculate(
                        profileA.Centroid,
                        profileB.Centroid
                    );


                double pairMateSelectivity =
                    (
                        profileA.Centroid.MateSelectivity
                        +
                        profileB.Centroid.MateSelectivity
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
                    3.0
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


                results.Add(
                    new RegionalReproductiveIsolation(
                        RegionAId:
                            profileA.RegionId,

                        RegionAName:
                            profileA.RegionName,

                        RegionBId:
                            profileB.RegionId,

                        RegionBName:
                            profileB.RegionName,

                        GeneticDistance:
                            geneticDistance,

                        GeneticCompatibility:
                            geneticCompatibility,

                        EcologicalSimilarity:
                            ecologicalSimilarity,

                        PairMateSelectivity:
                            pairMateSelectivity,

                        EcologicalPreference:
                            ecologicalPreference,

                        MateAcceptance:
                            mateAcceptance
                    )
                );
            }
        }


        return results;
    }


    private static Genome CalculateCentroid(
        List<Organism> members)
    {
        return new Genome(
            speed:
                members.Average(
                    organism =>
                        organism.Speed
                ),

            size:
                members.Average(
                    organism =>
                        organism.Size
                ),

            metabolism:
                members.Average(
                    organism =>
                        organism.Metabolism
                ),

            optimalTemperature:
                members.Average(
                    organism =>
                        organism.OptimalTemperature
                ),

            thermalTolerance:
                members.Average(
                    organism =>
                        organism.ThermalTolerance
                ),

            meatAdaptation:
                members.Average(
                    organism =>
                        organism.MeatAdaptation
                ),

            predatoryDrive:
                members.Average(
                    organism =>
                        organism.PredatoryDrive
                ),

            explorationDrive:
                members.Average(
                    organism =>
                        organism.ExplorationDrive
                ),

            riskTolerance:
                members.Average(
                    organism =>
                        organism.RiskTolerance
                ),

            scavengingDrive:
                members.Average(
                    organism =>
                        organism.ScavengingDrive
                ),

            mateSelectivity:
                members.Average(
                    organism =>
                        organism.MateSelectivity
                )
        );
    }


    private sealed record RegionalGenomeProfile(
        int RegionId,
        string RegionName,
        int Population,
        Genome Centroid);
}
