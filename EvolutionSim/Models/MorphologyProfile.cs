namespace EvolutionSim.Models;

/// <summary>
/// Descripción morfológica determinista de un organismo.
///
/// BodyScale conserva directamente el Size histórico del genoma.
///
/// Los otros valores son modificadores estructurales acotados:
/// -0.08 .. +0.08
///
/// Fase 8.1:
/// estos valores son únicamente descriptivos.
/// No afectan energía, movimiento, temperatura, alimentación,
/// reproducción, mating ni supervivencia.
/// </summary>
public sealed record MorphologyProfile(
    double BodyScale,
    double LimbLengthModifier,
    double LimbRobustnessModifier,
    double InsulationModifier,
    double JawStrengthModifier,
    double DigestiveStructureModifier)
{
    public static MorphologyProfile FromBaseSize(
        double bodyScale)
    {
        return new MorphologyProfile(
            BodyScale:
                bodyScale,
            LimbLengthModifier:
                0,
            LimbRobustnessModifier:
                0,
            InsulationModifier:
                0,
            JawStrengthModifier:
                0,
            DigestiveStructureModifier:
                0
        );
    }


    public double LimbLengthFactor =>
        1
        +
        LimbLengthModifier;


    public double LimbRobustnessFactor =>
        1
        +
        LimbRobustnessModifier;


    public double InsulationFactor =>
        1
        +
        InsulationModifier;


    public double JawStrengthFactor =>
        1
        +
        JawStrengthModifier;


    public double DigestiveStructureFactor =>
        1
        +
        DigestiveStructureModifier;


    public double StructuralExpressionMagnitude =>
        (
            Math.Abs(
                LimbLengthModifier
            )
            +
            Math.Abs(
                LimbRobustnessModifier
            )
            +
            Math.Abs(
                InsulationModifier
            )
            +
            Math.Abs(
                JawStrengthModifier
            )
            +
            Math.Abs(
                DigestiveStructureModifier
            )
        )
        /
        5.0;


    public double GetModifier(
        MorphologyChannel channel)
    {
        return channel switch
        {
            MorphologyChannel.LimbLength =>
                LimbLengthModifier,

            MorphologyChannel.LimbRobustness =>
                LimbRobustnessModifier,

            MorphologyChannel.Insulation =>
                InsulationModifier,

            MorphologyChannel.JawStrength =>
                JawStrengthModifier,

            MorphologyChannel.DigestiveStructure =>
                DigestiveStructureModifier,

            _ =>
                0
        };
    }
}
