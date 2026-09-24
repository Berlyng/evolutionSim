namespace EvolutionSim.Models;

/// <summary>
/// Configuración de mutación estructural neutral.
///
/// Estas probabilidades NO usan el RNG ecológico principal.
/// </summary>
public sealed record GenomeStructuralMutationSettings(
    double InnovationChance,
    double DuplicationChance,
    double LossChancePerLocus,
    double ActivationToggleChancePerLocus,
    double ValueMutationChancePerLocus,
    double ValueMutationStrength,
    int MaximumStructuralLoci)
{
    public static GenomeStructuralMutationSettings Default { get; } =
        new(
            InnovationChance:
                0.015,

            DuplicationChance:
                0.010,

            LossChancePerLocus:
                0.0025,

            ActivationToggleChancePerLocus:
                0.010,

            ValueMutationChancePerLocus:
                0.030,

            ValueMutationStrength:
                0.10,

            MaximumStructuralLoci:
                24
        );
}
