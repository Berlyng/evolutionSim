using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class PhenotypeExpressionStatisticsCalculator
{
    private readonly GeneExpressionSystem
        _geneExpressionSystem;


    public PhenotypeExpressionStatisticsCalculator(
        GeneExpressionSystem? geneExpressionSystem = null)
    {
        _geneExpressionSystem =
            geneExpressionSystem
            ??
            new GeneExpressionSystem();
    }


    public PhenotypeExpressionStatistics Calculate(
        IEnumerable<Organism> population)
    {
        ArgumentNullException.ThrowIfNull(
            population
        );


        List<Organism>
            living =
                population
                    .Where(
                        organism =>
                            organism.IsAlive
                    )
                    .ToList();


        List<PhenotypeProfile>
            profiles =
                living
                    .Select(
                        organism =>
                            _geneExpressionSystem
                                .Express(
                                    organism.Genome
                                )
                    )
                    .ToList();


        List<PhenotypeChannelStatistics>
            channels =
                Enum
                    .GetValues<
                        PhenotypeChannel
                    >()
                    .Select(
                        channel =>
                            CalculateChannel(
                                channel,
                                profiles
                            )
                    )
                    .ToList();


        int expressedGenomes =
            profiles.Count(
                profile =>
                    channels.Any(
                        channel =>
                            Math.Abs(
                                profile.GetModifier(
                                    channel.Channel
                                )
                            )
                            >
                            1e-12
                    )
            );


        return new PhenotypeExpressionStatistics(
            LivingPopulation:
                living.Count,
            ExpressedGenomes:
                expressedGenomes,
            Channels:
                channels
        );
    }


    private static PhenotypeChannelStatistics CalculateChannel(
        PhenotypeChannel channel,
        IReadOnlyList<PhenotypeProfile> profiles)
    {
        if (
            profiles.Count
            ==
            0
        )
        {
            return new PhenotypeChannelStatistics(
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
            );
        }


        double[] values =
            profiles
                .Select(
                    profile =>
                        profile.GetModifier(
                            channel
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
                        value
                        -
                        average;


                    return difference
                        *
                        difference;
                }
            );


        return new PhenotypeChannelStatistics(
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
        );
    }
}


public sealed record PhenotypeExpressionStatistics(
    int LivingPopulation,
    int ExpressedGenomes,
    IReadOnlyList<PhenotypeChannelStatistics> Channels
);


public sealed record PhenotypeChannelStatistics(
    PhenotypeChannel Channel,
    int ExpressingOrganisms,
    double AverageModifier,
    double StandardDeviation,
    double MinimumModifier,
    double MaximumModifier
);
