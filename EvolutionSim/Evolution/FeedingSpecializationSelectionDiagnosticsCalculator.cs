using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico pasivo del canal FeedingSpecialization de la Fase 7.9.
///
/// En esta fase FeedingSpecialization solo modifica la energía utilizable
/// obtenida de biomasa vegetal ya consumida.
///
/// Se comparan organismos vivos y padres efectivos para detectar si existe
/// una señal reproductiva persistente hacia una mejor asimilación vegetal.
///
/// No utiliza Random y no modifica la simulación.
/// </summary>
public sealed class FeedingSpecializationSelectionDiagnosticsCalculator
{
    private const double NeutralTolerance =
        1e-12;


    public FeedingSpecializationSelectionDiagnostics Calculate(
        Worldd world,
        IEnumerable<Organism> population,
        IReadOnlyList<Organism>? effectiveParents = null)
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


        List<Organism> parents =
            effectiveParents?
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .DistinctBy(
                    organism =>
                        organism.Id
                )
                .ToList()
            ??
            new List<Organism>();


        List<FeedingSpecializationRegionSelectionDiagnostics> regions =
            world.Regions
                .Select(
                    region =>
                    {
                        List<Organism> regionLiving =
                            living
                                .Where(
                                    organism =>
                                        organism.RegionId
                                        ==
                                        region.Id
                                )
                                .ToList();


                        List<Organism> regionParents =
                            parents
                                .Where(
                                    organism =>
                                        organism.RegionId
                                        ==
                                        region.Id
                                )
                                .ToList();


                        return new FeedingSpecializationRegionSelectionDiagnostics(
                            RegionId:
                                region.Id,

                            RegionName:
                                region.Name,

                            Living:
                                BuildCohort(
                                    "Living",
                                    regionLiving
                                ),

                            Parents:
                                BuildCohort(
                                    "Parents",
                                    regionParents
                                )
                        );
                    }
                )
                .ToList();


        return new FeedingSpecializationSelectionDiagnostics(
            Population:
                living.Count,

            EffectiveParents:
                parents.Count,

            Overall:
                BuildCohort(
                    "Global",
                    living
                ),

            Parents:
                BuildCohort(
                    "Parents",
                    parents
                ),

            Regions:
                regions
        );
    }


    private static FeedingSpecializationCohortDiagnostics BuildCohort(
        string name,
        IReadOnlyList<Organism> organisms)
    {
        if (
            organisms.Count
            ==
            0
        )
        {
            return FeedingSpecializationCohortDiagnostics.Empty(
                name
            );
        }


        double[] modifiers =
            organisms
                .Select(
                    organism =>
                        organism.FeedingSpecializationModifier
                )
                .OrderBy(
                    value =>
                        value
                )
                .ToArray();


        int beneficial =
            modifiers.Count(
                value =>
                    value
                    >
                    NeutralTolerance
            );


        int detrimental =
            modifiers.Count(
                value =>
                    value
                    <
                    -NeutralTolerance
            );


        int neutral =
            modifiers.Length
            -
            beneficial
            -
            detrimental;


        return new FeedingSpecializationCohortDiagnostics(
            Name:
                name,

            Count:
                organisms.Count,

            Beneficial:
                beneficial,

            Detrimental:
                detrimental,

            Neutral:
                neutral,

            AverageModifier:
                modifiers.Average(),

            AveragePlantEnergyMultiplier:
                organisms.Average(
                    organism =>
                        organism.FeedingSpecializationPlantEnergyMultiplier
                ),

            AveragePlantDigestionEfficiency:
                organisms.Average(
                    organism =>
                        organism.PlantDigestionEfficiency
                ),

            AverageEffectivePlantEnergyAssimilation:
                organisms.Average(
                    organism =>
                        organism.EffectivePlantEnergyAssimilation
                ),

            AverageFeedingCapacity:
                organisms.Average(
                    organism =>
                        organism.FeedingCapacity
                ),

            AverageCurrentEnergy:
                organisms.Average(
                    organism =>
                        organism.Energy
                ),

            AverageEnergyFillFraction:
                organisms.Average(
                    organism =>
                        organism.MaxEnergy > 0
                            ?
                            organism.Energy
                            /
                            organism.MaxEnergy
                            :
                            0
                ),

            P10Modifier:
                Percentile(
                    modifiers,
                    0.10
                ),

            P50Modifier:
                Percentile(
                    modifiers,
                    0.50
                ),

            P90Modifier:
                Percentile(
                    modifiers,
                    0.90
                )
        );
    }


    private static double Percentile(
        IReadOnlyList<double> sortedValues,
        double percentile)
    {
        if (
            sortedValues.Count
            ==
            0
        )
        {
            return 0;
        }


        if (
            sortedValues.Count
            ==
            1
        )
        {
            return sortedValues[
                0
            ];
        }


        double position =
            percentile
            *
            (
                sortedValues.Count
                -
                1
            );


        int lower =
            (int)Math.Floor(
                position
            );


        int upper =
            (int)Math.Ceiling(
                position
            );


        if (
            lower
            ==
            upper
        )
        {
            return sortedValues[
                lower
            ];
        }


        double fraction =
            position
            -
            lower;


        return
            sortedValues[
                lower
            ]
            +
            (
                sortedValues[
                    upper
                ]
                -
                sortedValues[
                    lower
                ]
            )
            *
            fraction;
    }
}


public sealed record FeedingSpecializationSelectionDiagnostics(
    int Population,
    int EffectiveParents,
    FeedingSpecializationCohortDiagnostics Overall,
    FeedingSpecializationCohortDiagnostics Parents,
    IReadOnlyList<FeedingSpecializationRegionSelectionDiagnostics> Regions
);


public sealed record FeedingSpecializationRegionSelectionDiagnostics(
    int RegionId,
    string RegionName,
    FeedingSpecializationCohortDiagnostics Living,
    FeedingSpecializationCohortDiagnostics Parents
);


public sealed record FeedingSpecializationCohortDiagnostics(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AveragePlantEnergyMultiplier,
    double AveragePlantDigestionEfficiency,
    double AverageEffectivePlantEnergyAssimilation,
    double AverageFeedingCapacity,
    double AverageCurrentEnergy,
    double AverageEnergyFillFraction,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier)
{
    public static FeedingSpecializationCohortDiagnostics Empty(
        string name)
    {
        return new FeedingSpecializationCohortDiagnostics(
            Name:
                name,
            Count:
                0,
            Beneficial:
                0,
            Detrimental:
                0,
            Neutral:
                0,
            AverageModifier:
                0,
            AveragePlantEnergyMultiplier:
                1,
            AveragePlantDigestionEfficiency:
                0,
            AverageEffectivePlantEnergyAssimilation:
                0,
            AverageFeedingCapacity:
                0,
            AverageCurrentEnergy:
                0,
            AverageEnergyFillFraction:
                0,
            P10Modifier:
                0,
            P50Modifier:
                0,
            P90Modifier:
                0
        );
    }
}
