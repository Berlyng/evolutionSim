namespace EvolutionSim.Models;

/// <summary>
/// Mapeo determinista Genome -> PhenotypeProfile.
///
/// Vive en Models para que Organism pueda usar exactamente la misma
/// expresión fenotípica que los diagnósticos sin depender de la capa
/// Evolution.
///
/// No usa Random.
/// </summary>
public static class GenomePhenotypeMapper
{
    private const double ContributionScale =
        0.020;

    private const double MaximumAbsoluteModifier =
        0.080;


    public static PhenotypeProfile Express(
        Genome genome)
    {
        ArgumentNullException.ThrowIfNull(
            genome
        );


        if (
            genome.StructuralLoci.Count
            ==
            0
        )
        {
            return PhenotypeProfile.Neutral;
        }


        double thermalRegulation =
            0;

        double bodySize =
            0;

        double metabolicEfficiency =
            0;

        double locomotion =
            0;

        double feedingSpecialization =
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


            PhenotypeChannel channel =
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
                case PhenotypeChannel.ThermalRegulation:
                    thermalRegulation +=
                        contribution;
                    break;

                case PhenotypeChannel.BodySize:
                    bodySize +=
                        contribution;
                    break;

                case PhenotypeChannel.MetabolicEfficiency:
                    metabolicEfficiency +=
                        contribution;
                    break;

                case PhenotypeChannel.Locomotion:
                    locomotion +=
                        contribution;
                    break;

                case PhenotypeChannel.FeedingSpecialization:
                    feedingSpecialization +=
                        contribution;
                    break;
            }
        }


        return new PhenotypeProfile(
            ThermalRegulation:
                Clamp(
                    thermalRegulation
                ),
            BodySize:
                Clamp(
                    bodySize
                ),
            MetabolicEfficiency:
                Clamp(
                    metabolicEfficiency
                ),
            Locomotion:
                Clamp(
                    locomotion
                ),
            FeedingSpecialization:
                Clamp(
                    feedingSpecialization
                )
        );
    }


    public static PhenotypeChannel ResolveChannel(
        long familyId)
    {
        ulong value =
            unchecked(
                (ulong)familyId
            );


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
                PhenotypeChannel
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
