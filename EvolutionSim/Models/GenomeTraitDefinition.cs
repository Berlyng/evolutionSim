namespace EvolutionSim.Models;

/// <summary>
/// Metadatos de un trait conocido por el modelo actual.
///
/// GenomeMinimum/Maximum reproducen exactamente el clamp histórico de
/// Genome.cs.
///
/// MutationMinimum/Maximum reproducen exactamente los límites históricos
/// usados por GenomeMutator.
/// </summary>
public sealed record GenomeTraitDefinition(
    GenomeTraitId Id,
    double GenomeMinimum,
    double GenomeMaximum,
    double MutationStrength,
    double MutationMinimum,
    double MutationMaximum,
    double GeneticDistanceScale
)
{
    public double ClampGenomeValue(
        double value)
    {
        return Math.Clamp(
            value,
            GenomeMinimum,
            GenomeMaximum
        );
    }

    public double ClampMutationValue(
        double value)
    {
        return Math.Clamp(
            value,
            MutationMinimum,
            MutationMaximum
        );
    }
}
