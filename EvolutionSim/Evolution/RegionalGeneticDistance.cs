using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

public sealed record RegionalGeneticDistance(
    int RegionAId,
    string RegionAName,
    int RegionBId,
    string RegionBName,
    double Distance);


public sealed class RegionalGeneticDivergenceCalculator
{
    private readonly GeneticDistanceCalculator
        _distanceCalculator;


    public RegionalGeneticDivergenceCalculator(
        GeneticDistanceCalculator distanceCalculator)
    {
        _distanceCalculator =
            distanceCalculator;
    }


    public List<RegionalGeneticDistance> Calculate(
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

                            Centroid:
                                CalculateCentroid(
                                    item.Members
                                )
                        )
                )
                .ToList();


        List<RegionalGeneticDistance> distances =
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


                double distance =
                    _distanceCalculator.Calculate(
                        profileA.Centroid,
                        profileB.Centroid
                    );


                distances.Add(
                    new RegionalGeneticDistance(
                        RegionAId:
                            profileA.RegionId,

                        RegionAName:
                            profileA.RegionName,

                        RegionBId:
                            profileB.RegionId,

                        RegionBName:
                            profileB.RegionName,

                        Distance:
                            distance
                    )
                );
            }
        }


        return distances;
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
        Genome Centroid);
}
