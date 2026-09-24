using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico pasivo del canal BodySize de la Fase 7.7.
///
/// En esta fase BodySize solo modifica MaxEnergy.
///
/// Se comparan organismos vivos y padres efectivos para observar si existe
/// una señal reproductiva persistente sobre el modifier estructural.
///
/// No utiliza Random y no modifica la simulación.
/// </summary>
public sealed class BodySizeSelectionDiagnosticsCalculator
{
    private const double NeutralTolerance =
        1e-12;


    public BodySizeSelectionDiagnostics Calculate(
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


        List<BodySizeRegionSelectionDiagnostics> regions =
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


                        return new BodySizeRegionSelectionDiagnostics(
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


        return new BodySizeSelectionDiagnostics(
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


    private static BodySizeCohortDiagnostics BuildCohort(
        string name,
        IReadOnlyList<Organism> organisms)
    {
        if (
            organisms.Count
            ==
            0
        )
        {
            return BodySizeCohortDiagnostics.Empty(
                name
            );
        }


        double[] modifiers =
            organisms
                .Select(
                    organism =>
                        organism.BodySizeModifier
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


        return new BodySizeCohortDiagnostics(
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

            AverageEnergyCapacityMultiplier:
                organisms.Average(
                    organism =>
                        organism.BodySizeEnergyCapacityMultiplier
                ),

            AverageGeneticSize:
                organisms.Average(
                    organism =>
                        organism.Size
                ),

            AverageBaseMaxEnergy:
                organisms.Average(
                    organism =>
                        organism.BaseMaxEnergy
                ),

            AverageMaxEnergy:
                organisms.Average(
                    organism =>
                        organism.MaxEnergy
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


public sealed record BodySizeSelectionDiagnostics(
    int Population,
    int EffectiveParents,
    BodySizeCohortDiagnostics Overall,
    BodySizeCohortDiagnostics Parents,
    IReadOnlyList<BodySizeRegionSelectionDiagnostics> Regions
);


public sealed record BodySizeRegionSelectionDiagnostics(
    int RegionId,
    string RegionName,
    BodySizeCohortDiagnostics Living,
    BodySizeCohortDiagnostics Parents
);


public sealed record BodySizeCohortDiagnostics(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageEnergyCapacityMultiplier,
    double AverageGeneticSize,
    double AverageBaseMaxEnergy,
    double AverageMaxEnergy,
    double AverageCurrentEnergy,
    double AverageEnergyFillFraction,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier)
{
    public static BodySizeCohortDiagnostics Empty(
        string name)
    {
        return new BodySizeCohortDiagnostics(
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
            AverageEnergyCapacityMultiplier:
                1,
            AverageGeneticSize:
                0,
            AverageBaseMaxEnergy:
                0,
            AverageMaxEnergy:
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
