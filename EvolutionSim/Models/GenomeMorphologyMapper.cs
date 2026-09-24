namespace EvolutionSim.Models;

/// <summary>
/// Mapeo determinista Genome -> MorphologyProfile.
///
/// Fase 8.1:
/// la morfología estructural es heredable y observable, pero pasiva.
///
/// IMPORTANTE:
/// este mapper NO reutiliza GenomePhenotypeMapper.ResolveChannel().
/// Usa una sal distinta antes del hash para mantener independiente el
/// espacio morfológico sin cambiar la asignación fenotípica validada
/// durante la Fase 7.
///
/// No utiliza Random.
/// </summary>
public static class GenomeMorphologyMapper
{
    private const double ContributionScale =
        0.020;


    private const double MaximumAbsoluteModifier =
        0.080;


    // Sal fija para independizar el mapeo morfológico del fenotípico.
    //
    // No representa un parámetro biológico.
    // Solo separa ambos espacios deterministas.
    private const ulong MorphologyHashSalt =
        0x9E3779B97F4A7C15UL;


    public static MorphologyProfile Express(
        Genome genome)
    {
        ArgumentNullException.ThrowIfNull(
            genome
        );


        double limbLength =
            0;

        double limbRobustness =
            0;

        double insulation =
            0;

        double jawStrength =
            0;

        double digestiveStructure =
            0;


        foreach (
            GenomeLocus locus
            in genome.StructuralLoci
        )
        {
            if (
                !locus.IsActive
            )
            {
                continue;
            }


            MorphologyChannel channel =
                ResolveChannel(
                    locus.FamilyId
                );


            double centeredValue =
                (
                    locus.Value
                    -
                    0.5
                )
                *
                2.0;


            double contribution =
                centeredValue
                *
                ContributionScale;


            switch (channel)
            {
                case MorphologyChannel.LimbLength:
                    limbLength +=
                        contribution;
                    break;

                case MorphologyChannel.LimbRobustness:
                    limbRobustness +=
                        contribution;
                    break;

                case MorphologyChannel.Insulation:
                    insulation +=
                        contribution;
                    break;

                case MorphologyChannel.JawStrength:
                    jawStrength +=
                        contribution;
                    break;

                case MorphologyChannel.DigestiveStructure:
                    digestiveStructure +=
                        contribution;
                    break;
            }
        }


        return new MorphologyProfile(
            BodyScale:
                genome.Size,

            LimbLengthModifier:
                Clamp(
                    limbLength
                ),

            LimbRobustnessModifier:
                Clamp(
                    limbRobustness
                ),

            InsulationModifier:
                Clamp(
                    insulation
                ),

            JawStrengthModifier:
                Clamp(
                    jawStrength
                ),

            DigestiveStructureModifier:
                Clamp(
                    digestiveStructure
                )
        );
    }


    public static MorphologyChannel ResolveChannel(
        long familyId)
    {
        ulong value =
            unchecked(
                (ulong)familyId
            );


        value ^=
            MorphologyHashSalt;


        value ^=
            value
            >>
            33;

        value *=
            0xff51afd7ed558ccdUL;

        value ^=
            value
            >>
            33;

        value *=
            0xc4ceb9fe1a85ec53UL;

        value ^=
            value
            >>
            33;


        int channelIndex =
            (
                int
            )
            (
                value
                %
                5UL
            );


        return
            (
                MorphologyChannel
            )
            channelIndex;
    }


    private static double Clamp(
        double value)
    {
        return Math.Clamp(
            value,
            -MaximumAbsoluteModifier,
            MaximumAbsoluteModifier
        );
    }
}
