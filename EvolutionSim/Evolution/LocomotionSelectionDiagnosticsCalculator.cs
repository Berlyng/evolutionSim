using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico pasivo del canal Locomotion de la Fase 7.8.
///
/// En esta fase Locomotion solo modifica ActivityCost.
///
/// Se comparan organismos vivos y padres efectivos para observar si existe
/// una señal reproductiva persistente hacia una mayor eficiencia locomotora.
///
/// No utiliza Random y no modifica la simulación.
/// </summary>
public sealed class LocomotionSelectionDiagnosticsCalculator
{
    private const double NeutralTolerance =
        1e-12;


    public LocomotionSelectionDiagnostics Calculate(
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


        List<LocomotionRegionSelectionDiagnostics> regions =
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


                        return new LocomotionRegionSelectionDiagnostics(
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


        return new LocomotionSelectionDiagnostics(
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


    private static LocomotionCohortDiagnostics BuildCohort(
        string name,
        IReadOnlyList<Organism> organisms)
    {
        if (
            organisms.Count
            ==
            0
        )
        {
            return LocomotionCohortDiagnostics.Empty(
                name
            );
        }


        double[] modifiers =
            organisms
                .Select(
                    organism =>
                        organism.LocomotionModifier
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


        return new LocomotionCohortDiagnostics(
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

            AverageActivityCostMultiplier:
                organisms.Average(
                    organism =>
                        organism.LocomotionActivityCostMultiplier
                ),

            AverageBaseActivityCost:
                organisms.Average(
                    organism =>
                        organism.BaseActivityCost
                ),

            AverageActivityCost:
                organisms.Average(
                    organism =>
                        organism.ActivityCost
                ),

            AverageBasalMetabolicCost:
                organisms.Average(
                    organism =>
                        organism.BasalMetabolicCost
                ),

            AverageEnergyCost:
                organisms.Average(
                    organism =>
                        organism.EnergyCost
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


public sealed record LocomotionSelectionDiagnostics(
    int Population,
    int EffectiveParents,
    LocomotionCohortDiagnostics Overall,
    LocomotionCohortDiagnostics Parents,
    IReadOnlyList<LocomotionRegionSelectionDiagnostics> Regions
);


public sealed record LocomotionRegionSelectionDiagnostics(
    int RegionId,
    string RegionName,
    LocomotionCohortDiagnostics Living,
    LocomotionCohortDiagnostics Parents
);


public sealed record LocomotionCohortDiagnostics(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageActivityCostMultiplier,
    double AverageBaseActivityCost,
    double AverageActivityCost,
    double AverageBasalMetabolicCost,
    double AverageEnergyCost,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier)
{
    public static LocomotionCohortDiagnostics Empty(
        string name)
    {
        return new LocomotionCohortDiagnostics(
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
            AverageActivityCostMultiplier:
                1,
            AverageBaseActivityCost:
                0,
            AverageActivityCost:
                0,
            AverageBasalMetabolicCost:
                0,
            AverageEnergyCost:
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
