using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico pasivo de morfología de la Fase 8.1.
///
/// Calcula la expresión morfológica global y regional.
///
/// No utiliza Random.
/// No modifica organismos.
/// No altera la ecología.
/// </summary>
public sealed class MorphologyStatisticsCalculator
{
    private static readonly MorphologyChannel[]
        ChannelOrder =
        [
            MorphologyChannel.LimbLength,
            MorphologyChannel.LimbRobustness,
            MorphologyChannel.Insulation,
            MorphologyChannel.JawStrength,
            MorphologyChannel.DigestiveStructure
        ];


    public MorphologyStatistics Calculate(
        Worldd world,
        IEnumerable<Organism> population)
    {
        ArgumentNullException.ThrowIfNull(
            world
        );

        ArgumentNullException.ThrowIfNull(
            population
        );


        List<Organism> living =
            population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .ToList();


        IReadOnlyList<MorphologyChannelStatistics>
            globalChannels =
                CalculateChannels(
                    living
                );


        List<RegionalMorphologyStatistics> regions =
            world.Regions
                .Select(
                    region =>
                    {
                        List<Organism> regionPopulation =
                            living
                                .Where(
                                    organism =>
                                        organism.RegionId
                                        ==
                                        region.Id
                                )
                                .ToList();


                        return new RegionalMorphologyStatistics(
                            RegionId:
                                region.Id,

                            RegionName:
                                region.Name,

                            Population:
                                regionPopulation.Count,

                            AverageBodyScale:
                                AverageOrZero(
                                    regionPopulation,
                                    organism =>
                                        organism.Morphology.BodyScale
                                ),

                            AverageStructuralExpressionMagnitude:
                                AverageOrZero(
                                    regionPopulation,
                                    organism =>
                                        organism
                                            .Morphology
                                            .StructuralExpressionMagnitude
                                ),

                            Channels:
                                CalculateChannels(
                                    regionPopulation
                                )
                        );
                    }
                )
                .ToList();


        int expressedOrganisms =
            living.Count(
                organism =>
                    organism
                        .Morphology
                        .StructuralExpressionMagnitude
                    >
                    1e-12
            );


        return new MorphologyStatistics(
            LivingPopulation:
                living.Count,

            ExpressedOrganisms:
                expressedOrganisms,

            AverageBodyScale:
                AverageOrZero(
                    living,
                    organism =>
                        organism.Morphology.BodyScale
                ),

            AverageStructuralExpressionMagnitude:
                AverageOrZero(
                    living,
                    organism =>
                        organism
                            .Morphology
                            .StructuralExpressionMagnitude
                ),

            Channels:
                globalChannels,

            Regions:
                regions
        );
    }


    private static IReadOnlyList<MorphologyChannelStatistics>
        CalculateChannels(
            IReadOnlyList<Organism> organisms)
    {
        List<MorphologyChannelStatistics> result =
            new();


        foreach (
            MorphologyChannel channel
            in ChannelOrder
        )
        {
            double[] values =
                organisms
                    .Select(
                        organism =>
                            organism
                                .Morphology
                                .GetModifier(
                                    channel
                                )
                    )
                    .ToArray();


            if (
                values.Length
                ==
                0
            )
            {
                result.Add(
                    new MorphologyChannelStatistics(
                        Channel:
                            channel,
                        ExpressingOrganisms:
                            0,
                        AverageModifier:
                            0,
                        StandardDeviation:
                            0,
                        MinimumModifier:
                            0,
                        MaximumModifier:
                            0
                    )
                );


                continue;
            }


            double average =
                values.Average();


            double variance =
                values.Average(
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
                );


            result.Add(
                new MorphologyChannelStatistics(
                    Channel:
                        channel,

                    ExpressingOrganisms:
                        values.Count(
                            value =>
                                Math.Abs(
                                    value
                                )
                                >
                                1e-12
                        ),

                    AverageModifier:
                        average,

                    StandardDeviation:
                        Math.Sqrt(
                            variance
                        ),

                    MinimumModifier:
                        values.Min(),

                    MaximumModifier:
                        values.Max()
                )
            );
        }


        return result;
    }


    private static double AverageOrZero(
        IReadOnlyList<Organism> organisms,
        Func<Organism, double> selector)
    {
        return organisms.Count > 0
            ?
            organisms.Average(
                selector
            )
            :
            0;
    }
}


public sealed record MorphologyStatistics(
    int LivingPopulation,
    int ExpressedOrganisms,
    double AverageBodyScale,
    double AverageStructuralExpressionMagnitude,
    IReadOnlyList<MorphologyChannelStatistics> Channels,
    IReadOnlyList<RegionalMorphologyStatistics> Regions
);


public sealed record RegionalMorphologyStatistics(
    int RegionId,
    string RegionName,
    int Population,
    double AverageBodyScale,
    double AverageStructuralExpressionMagnitude,
    IReadOnlyList<MorphologyChannelStatistics> Channels
);


public sealed record MorphologyChannelStatistics(
    MorphologyChannel Channel,
    int ExpressingOrganisms,
    double AverageModifier,
    double StandardDeviation,
    double MinimumModifier,
    double MaximumModifier
);
