using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico pasivo para separar selección metabólica real
/// de divergencia causada por sensibilidad demográfica.
///
/// No usa Random y no modifica ningún organismo.
/// </summary>
public sealed class MetabolicSelectionDiagnosticsCalculator
{
    private const double NeutralTolerance =
        1e-12;


    public MetabolicSelectionDiagnostics Calculate(
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


        List<MetabolicRegionSelectionDiagnostics> regions =
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


                        return BuildRegion(
                            region.Id,
                            region.Name,
                            regionLiving,
                            regionParents
                        );
                    }
                )
                .ToList();


        return new MetabolicSelectionDiagnostics(
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


    private static MetabolicRegionSelectionDiagnostics BuildRegion(
        int regionId,
        string regionName,
        IReadOnlyList<Organism> living,
        IReadOnlyList<Organism> parents)
    {
        return new MetabolicRegionSelectionDiagnostics(
            RegionId:
                regionId,
            RegionName:
                regionName,
            Living:
                BuildCohort(
                    "Living",
                    living
                ),
            Parents:
                BuildCohort(
                    "Parents",
                    parents
                )
        );
    }


    private static MetabolicCohortDiagnostics BuildCohort(
        string name,
        IReadOnlyList<Organism> organisms)
    {
        if (
            organisms.Count
            ==
            0
        )
        {
            return new MetabolicCohortDiagnostics(
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
                AverageCostMultiplier:
                    1,
                P10Modifier:
                    0,
                P50Modifier:
                    0,
                P90Modifier:
                    0
            );
        }


        double[] modifiers =
            organisms
                .Select(
                    organism =>
                        organism.MetabolicEfficiencyModifier
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


        return new MetabolicCohortDiagnostics(
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
            AverageCostMultiplier:
                organisms.Average(
                    organism =>
                        organism
                            .MetabolicEfficiencyCostMultiplier
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
            return sortedValues[0];
        }


        double position =
            percentile
            *
            (
                sortedValues.Count
                -
                1
            );


        int lowerIndex =
            (int)Math.Floor(
                position
            );


        int upperIndex =
            (int)Math.Ceiling(
                position
            );


        if (
            lowerIndex
            ==
            upperIndex
        )
        {
            return sortedValues[
                lowerIndex
            ];
        }


        double weight =
            position
            -
            lowerIndex;


        return
            sortedValues[
                lowerIndex
            ]
            *
            (
                1
                -
                weight
            )
            +
            sortedValues[
                upperIndex
            ]
            *
            weight;
    }
}


public sealed record MetabolicSelectionDiagnostics(
    int Population,
    int EffectiveParents,
    MetabolicCohortDiagnostics Overall,
    MetabolicCohortDiagnostics Parents,
    IReadOnlyList<MetabolicRegionSelectionDiagnostics> Regions
);


public sealed record MetabolicRegionSelectionDiagnostics(
    int RegionId,
    string RegionName,
    MetabolicCohortDiagnostics Living,
    MetabolicCohortDiagnostics Parents
);


public sealed record MetabolicCohortDiagnostics(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageCostMultiplier,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier
);
