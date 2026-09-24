namespace EvolutionSim.Models;

/// <summary>
/// Expresión fenotípica potencial derivada del genoma.
///
/// Todos los modificadores son adimensionales y están fuertemente acotados.
/// En Phase 7.3 son diagnósticos pasivos.
/// </summary>
public sealed record PhenotypeProfile(
    double ThermalRegulation,
    double BodySize,
    double MetabolicEfficiency,
    double Locomotion,
    double FeedingSpecialization)
{
    public static PhenotypeProfile Neutral { get; } =
        new(
            ThermalRegulation:
                0,
            BodySize:
                0,
            MetabolicEfficiency:
                0,
            Locomotion:
                0,
            FeedingSpecialization:
                0
        );


    public double GetModifier(
        PhenotypeChannel channel)
    {
        return channel switch
        {
            PhenotypeChannel.ThermalRegulation =>
                ThermalRegulation,

            PhenotypeChannel.BodySize =>
                BodySize,

            PhenotypeChannel.MetabolicEfficiency =>
                MetabolicEfficiency,

            PhenotypeChannel.Locomotion =>
                Locomotion,

            PhenotypeChannel.FeedingSpecialization =>
                FeedingSpecialization,

            _ =>
                0
        };
    }
}
