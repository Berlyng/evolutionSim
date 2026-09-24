using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico pasivo del canal ThermalRegulation de la Fase 7.6.
///
/// Mide el modifier expresado, el multiplicador efectivo del desafío térmico,
/// el estrés térmico realizado y el fitness térmico en organismos vivos
/// y padres efectivos.
///
/// No utiliza Random y no modifica la ecología.
/// </summary>
public sealed class ThermalRegulationSelectionDiagnosticsCalculator
{
    private const double NeutralTolerance =
        1e-12;


    public ThermalRegulationSelectionDiagnostics Calculate(
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


        List<ThermalRegulationRegionSelectionDiagnostics> regions =
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


                        return new ThermalRegulationRegionSelectionDiagnostics(
                            RegionId:
                                region.Id,

                            RegionName:
                                region.Name,

                            Living:
                                BuildCohort(
                                    world,
                                    "Living",
                                    regionLiving
                                ),

                            Parents:
                                BuildCohort(
                                    world,
                                    "Parents",
                                    regionParents
                                )
                        );
                    }
                )
                .ToList();


        return new ThermalRegulationSelectionDiagnostics(
            Population:
                living.Count,

            EffectiveParents:
                parents.Count,

            Overall:
                BuildCohort(
                    world,
                    "Global",
                    living
                ),

            Parents:
                BuildCohort(
                    world,
                    "Parents",
                    parents
                ),

            Regions:
                regions
        );
    }


    private static ThermalRegulationCohortDiagnostics BuildCohort(
        Worldd world,
        string name,
        IReadOnlyList<Organism> organisms)
    {
        if (
            organisms.Count
            ==
            0
        )
        {
            return ThermalRegulationCohortDiagnostics.Empty(
                name
            );
        }


        double[] modifiers =
            organisms
                .Select(
                    organism =>
                        organism.ThermalRegulationModifier
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


        double thermalFitnessSum =
            0;

        double thermalStressSum =
            0;


        foreach (
            Organism organism
            in organisms
        )
        {
            Region region =
                world.GetRegion(
                    organism.RegionId
                );


            Patch patch =
                world.GetPatch(
                    organism.PatchId
                );


            double localTemperature =
                patch.GetLocalTemperature(
                    region.Temperature
                );


            thermalFitnessSum +=
                organism.GetThermalFitness(
                    localTemperature
                );


            thermalStressSum +=
                organism.GetThermalStress(
                    localTemperature
                );
        }


        return new ThermalRegulationCohortDiagnostics(
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

            AverageStressMultiplier:
                organisms.Average(
                    organism =>
                        organism
                            .ThermalRegulationStressMultiplier
                ),

            AverageThermalFitness:
                thermalFitnessSum
                /
                organisms.Count,

            AverageThermalStress:
                thermalStressSum
                /
                organisms.Count,

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


public sealed record ThermalRegulationSelectionDiagnostics(
    int Population,
    int EffectiveParents,
    ThermalRegulationCohortDiagnostics Overall,
    ThermalRegulationCohortDiagnostics Parents,
    IReadOnlyList<ThermalRegulationRegionSelectionDiagnostics> Regions
);


public sealed record ThermalRegulationRegionSelectionDiagnostics(
    int RegionId,
    string RegionName,
    ThermalRegulationCohortDiagnostics Living,
    ThermalRegulationCohortDiagnostics Parents
);


public sealed record ThermalRegulationCohortDiagnostics(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageStressMultiplier,
    double AverageThermalFitness,
    double AverageThermalStress,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier)
{
    public static ThermalRegulationCohortDiagnostics Empty(
        string name)
    {
        return new ThermalRegulationCohortDiagnostics(
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
            AverageStressMultiplier:
                1,
            AverageThermalFitness:
                0,
            AverageThermalStress:
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
