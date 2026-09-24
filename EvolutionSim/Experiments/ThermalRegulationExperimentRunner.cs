using System.Collections.Concurrent;
using System.Globalization;
using EvolutionSim.Evolution;
using EvolutionSim.Models;
using EvolutionSim.Simulation;
using EvolutionSim.World;

namespace EvolutionSim.Experiments;

/// <summary>
/// Estudio paralelo emparejado de la Fase 7.6.
///
/// MetabolicEfficiency permanece fijo en su valor validado:
/// coupling 0.125 => efecto energético máximo de ±1 %.
///
/// SOLO varía el coupling de ThermalRegulation.
///
/// 10 seeds × 3 couplings = 30 corridas.
///
/// Fase 7.6.1:
/// el runner mantiene explícitamente el contexto térmico activo mientras
/// calcula diagnósticos posteriores a cada Step(). Esto corrige únicamente
/// métricas diagnósticas; no cambia la biología de la simulación.
/// </summary>
public sealed class ThermalRegulationExperimentRunner
{
    private static readonly int[] Seeds =
        Enumerable.Range(
            start:
                98765,
            count:
                10
        )
        .ToArray();


    private static readonly double[] Couplings =
    [
        0.00,
        0.125,
        0.25
    ];


    private const string DetailCsvFileName =
        "thermal_regulation_parallel_study.csv";


    private const string SummaryCsvFileName =
        "thermal_regulation_parallel_summary.csv";


    private static readonly object
        FileWriteLock =
            new();


    public IReadOnlyList<ThermalRegulationExperimentResult> RunDefault()
    {
        ConcurrentBag<ThermalRegulationExperimentResult> results =
            new();


        List<ExperimentDefinition> definitions =
            (
                from coupling in Couplings
                from seed in Seeds
                select new ExperimentDefinition(
                    seed,
                    coupling
                )
            )
            .ToList();


        int totalRuns =
            definitions.Count;


        int completedRuns =
            0;


        int maxDegreeOfParallelism =
            CalculateParallelism();


        string detailCsvPath =
            Path.GetFullPath(
                DetailCsvFileName
            );


        string summaryCsvPath =
            Path.GetFullPath(
                SummaryCsvFileName
            );


        Console.WriteLine(
            "EvolutionSim — Phase 7.6"
        );

        Console.WriteLine(
            "Parallel ThermalRegulation Study"
        );

        Console.WriteLine(
            $"Runs: {totalRuns} " +
            $"({Seeds.Length} seeds × {Couplings.Length} couplings)"
        );

        Console.WriteLine(
            "MetabolicEfficiency remains fixed at coupling 0.125 (±1%)."
        );

        Console.WriteLine(
            $"Parallel workers: {maxDegreeOfParallelism}"
        );

        Console.WriteLine();


        ParallelOptions options =
            new()
            {
                MaxDegreeOfParallelism =
                    maxDegreeOfParallelism
            };


        Parallel.ForEach(
            definitions,
            options,
            definition =>
            {
                ThermalRegulationExperimentResult result =
                    RunSingle(
                        definition.Seed,
                        definition.Coupling
                    );


                results.Add(
                    result
                );


                int finished =
                    Interlocked.Increment(
                        ref completedRuns
                    );


                Console.WriteLine(
                    $"[{finished}/{totalRuns}] " +
                    $"Seed {result.Seed} | " +
                    $"Thermal {result.Coupling:F3} | " +
                    $"Final {result.FinalPopulation} | " +
                    $"Min {result.MinimumPopulation}@C{result.MinimumPopulationCycle} | " +
                    $"Max {result.MaximumPopulation}@C{result.MaximumPopulationCycle} | " +
                    $"NoBirthMax {result.LongestNoBirthStreak} | " +
                    $"Llanura {result.LlanuraPopulation}"
                );


                Checkpoint(
                    detailCsvPath,
                    summaryCsvPath,
                    results
                );
            }
        );


        List<ThermalRegulationExperimentResult> orderedResults =
            results
                .OrderBy(
                    result =>
                        result.Coupling
                )
                .ThenBy(
                    result =>
                        result.Seed
                )
                .ToList();


        WriteDetailCsv(
            detailCsvPath,
            orderedResults
        );


        WriteSummaryCsv(
            summaryCsvPath,
            orderedResults
        );


        Console.WriteLine();
        Console.WriteLine(
            $"Detail CSV: {detailCsvPath}"
        );

        Console.WriteLine(
            $"Summary CSV: {summaryCsvPath}"
        );

        Console.WriteLine();


        PrintCompactSummary(
            orderedResults
        );


        return orderedResults;
    }


    private static int CalculateParallelism()
    {
        int processorCount =
            Environment.ProcessorCount;


        if (
            processorCount
            <=
            2
        )
        {
            return 1;
        }


        return Math.Clamp(
            processorCount
            -
            1,
            2,
            4
        );
    }


    private static void Checkpoint(
        string detailCsvPath,
        string summaryCsvPath,
        ConcurrentBag<ThermalRegulationExperimentResult> results)
    {
        lock (
            FileWriteLock
        )
        {
            List<ThermalRegulationExperimentResult> snapshot =
                results
                    .OrderBy(
                        result =>
                            result.Coupling
                    )
                    .ThenBy(
                        result =>
                            result.Seed
                    )
                    .ToList();


            WriteDetailCsv(
                detailCsvPath,
                snapshot
            );


            WriteSummaryCsv(
                summaryCsvPath,
                snapshot
            );
        }
    }


    private static ThermalRegulationExperimentResult RunSingle(
        int seed,
        double coupling)
    {
        SimulationConfig config =
            SimulationConfig.Default
                with
            {
                RandomSeed =
                        seed,

                ThermalRegulationEcologicalCoupling =
                        coupling,

                FastExperimentMode =
                        true
            };


        SimulationEngine engine =
            SimulationFactory.Create(
                config
            );


        int minimumPopulation =
            engine.State.Population.Count;

        int minimumPopulationCycle =
            0;


        int maximumPopulation =
            engine.State.Population.Count;

        int maximumPopulationCycle =
            0;


        int currentNoBirthStreak =
            0;

        int longestNoBirthStreak =
            0;


        Dictionary<int, RegionalOccupancyExperimentTracker>
            occupancyTrackers =
                engine.State.World.Regions
                    .ToDictionary(
                        region =>
                            region.Id,

                        region =>
                            new RegionalOccupancyExperimentTracker(
                                initiallyOccupied:
                                    engine.State.Population.Any(
                                        organism =>
                                            organism.RegionId
                                            ==
                                            region.Id
                                    )
                            )
                    );


        double parentModifierSum =
            0;

        double parentStressMultiplierSum =
            0;

        double parentThermalFitnessSum =
            0;

        int parentObservationCount =
            0;


        SimulationStepResult? lastResult =
            null;


        while (
            engine.CanStep
        )
        {
            SimulationStepResult? result =
                engine.Step();


            if (
                result is null
            )
            {
                break;
            }


            lastResult =
                result;


            // El SimulationEngine aplica el coupling durante Step(), pero su
            // scope termina al devolver el resultado. Los diagnósticos que
            // calculamos aquí también deben usar exactamente el mismo coupling.
            //
            // Este scope NO modifica la simulación ya ejecutada ni consume RNG.
            using IDisposable thermalDiagnosticsScope =
                PhenotypeEcologyContext
                    .PushThermalRegulationCoupling(
                        coupling
                    );


            if (
                result.Population
                <
                minimumPopulation
            )
            {
                minimumPopulation =
                    result.Population;

                minimumPopulationCycle =
                    result.Cycle;
            }


            if (
                result.Population
                >
                maximumPopulation
            )
            {
                maximumPopulation =
                    result.Population;

                maximumPopulationCycle =
                    result.Cycle;
            }


            currentNoBirthStreak =
                result.Births == 0
                    ? currentNoBirthStreak + 1
                    : 0;


            longestNoBirthStreak =
                Math.Max(
                    longestNoBirthStreak,
                    currentNoBirthStreak
                );


            foreach (
                Region region
                in engine.State.World.Regions
            )
            {
                bool isOccupied =
                    engine.State.Population.Any(
                        organism =>
                            organism.RegionId
                            ==
                            region.Id
                    );


                occupancyTrackers[
                    region.Id
                ]
                .Observe(
                    isOccupied
                );
            }


            if (
                result.Births
                >
                0
            )
            {
                HashSet<Guid> parentIds =
                    engine.State.Population
                        .Where(
                            organism =>
                                organism.Generation
                                >
                                0
                                &&
                                organism.Age
                                ==
                                0
                        )
                        .SelectMany(
                            organism =>
                                new[]
                                {
                                    organism.ParentAId,
                                    organism.ParentBId
                                }
                        )
                        .Where(
                            parentId =>
                                parentId.HasValue
                        )
                        .Select(
                            parentId =>
                                parentId!.Value
                        )
                        .ToHashSet();


                foreach (
                    Organism parent
                    in engine.State.Population
                        .Where(
                            organism =>
                                parentIds.Contains(
                                    organism.Id
                                )
                        )
                )
                {
                    parentModifierSum +=
                        parent.ThermalRegulationModifier;


                    parentStressMultiplierSum +=
                        parent.ThermalRegulationStressMultiplier;


                    parentThermalFitnessSum +=
                        GetLocalThermalFitness(
                            engine.State.World,
                            parent
                        );


                    parentObservationCount++;
                }
            }
        }


        if (
            lastResult is null
        )
        {
            throw new InvalidOperationException(
                "El experimento de regulación térmica no produjo ningún SimulationStepResult."
            );
        }


        Dictionary<int, int> regionalPopulation =
            engine.State.World.Regions
                .ToDictionary(
                    region =>
                        region.Id,

                    region =>
                        engine.State.Population.Count(
                            organism =>
                                organism.RegionId
                                ==
                                region.Id
                        )
                );


        RegionalOccupancyExperimentStatistics OccupancyFor(
            int regionId)
        {
            return occupancyTrackers[
                regionId
            ]
            .Snapshot(
                isEmptyAtEnd:
                    regionalPopulation.GetValueOrDefault(
                        regionId
                    )
                    ==
                    0
            );
        }


        // Step() ya terminó, por lo que reactivamos el mismo coupling solo
        // para calcular correctamente los diagnósticos térmicos finales.
        //
        // Sin este scope, PhenotypeEcologyContext volvería al control 0.0 y
        // StressMultiplier/ThermalFitness serían medidos como si el experimento
        // no tuviera efecto térmico.
        using IDisposable finalThermalDiagnosticsScope =
            PhenotypeEcologyContext
                .PushThermalRegulationCoupling(
                    coupling
                );


        List<Organism> finalLiving =
            engine.State.Population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .ToList();


        List<Organism> finalLlanura =
            finalLiving
                .Where(
                    organism =>
                        organism.RegionId
                        ==
                        2
                )
                .ToList();


        double finalLivingModifier =
            finalLiving.Count > 0
                ?
                finalLiving.Average(
                    organism =>
                        organism.ThermalRegulationModifier
                )
                :
                0;


        double finalLivingStressMultiplier =
            finalLiving.Count > 0
                ?
                finalLiving.Average(
                    organism =>
                        organism.ThermalRegulationStressMultiplier
                )
                :
                1;


        double finalLivingThermalFitness =
            finalLiving.Count > 0
                ?
                finalLiving.Average(
                    organism =>
                        GetLocalThermalFitness(
                            engine.State.World,
                            organism
                        )
                )
                :
                0;


        double finalLlanuraModifier =
            finalLlanura.Count > 0
                ?
                finalLlanura.Average(
                    organism =>
                        organism.ThermalRegulationModifier
                )
                :
                0;


        double finalLlanuraThermalFitness =
            finalLlanura.Count > 0
                ?
                finalLlanura.Average(
                    organism =>
                        GetLocalThermalFitness(
                            engine.State.World,
                            organism
                        )
                )
                :
                0;


        double meanParentModifier =
            parentObservationCount > 0
                ?
                parentModifierSum
                /
                parentObservationCount
                :
                0;


        double meanParentStressMultiplier =
            parentObservationCount > 0
                ?
                parentStressMultiplierSum
                /
                parentObservationCount
                :
                1;


        double meanParentThermalFitness =
            parentObservationCount > 0
                ?
                parentThermalFitnessSum
                /
                parentObservationCount
                :
                0;


        GeneticDistanceCalculator geneticDistanceCalculator =
            new();


        MateCompatibilityCalculator mateCompatibilityCalculator =
            new(
                geneticDistanceCalculator
            );


        EcologicalMateSimilarityCalculator ecologicalMateSimilarityCalculator =
            new();


        RegionalReproductiveIsolationCalculator isolationCalculator =
            new(
                geneticDistanceCalculator,
                mateCompatibilityCalculator,
                ecologicalMateSimilarityCalculator
            );


        List<RegionalReproductiveIsolation> isolations =
            isolationCalculator.Calculate(
                engine.State.Population,
                engine.State.World
            );


        double? FindMateAcceptance(
            int regionAId,
            int regionBId)
        {
            RegionalReproductiveIsolation? match =
                isolations.FirstOrDefault(
                    item =>
                        (
                            item.RegionAId
                            ==
                            regionAId
                            &&
                            item.RegionBId
                            ==
                            regionBId
                        )
                        ||
                        (
                            item.RegionAId
                            ==
                            regionBId
                            &&
                            item.RegionBId
                            ==
                            regionAId
                        )
                );


            return match?
                .MateAcceptance;
        }


        return new ThermalRegulationExperimentResult(
            Seed:
                seed,

            Coupling:
                coupling,

            MaximumThermalChallengeEffectPercent:
                0.08
                *
                coupling
                *
                100,

            FinalCycle:
                lastResult.Cycle,

            FinalPopulation:
                lastResult.Population,

            MinimumPopulation:
                minimumPopulation,

            MinimumPopulationCycle:
                minimumPopulationCycle,

            MaximumPopulation:
                maximumPopulation,

            MaximumPopulationCycle:
                maximumPopulationCycle,

            LongestNoBirthStreak:
                longestNoBirthStreak,

            FinalBirths:
                lastResult.Births,

            FinalDeaths:
                lastResult.Deaths,

            FinalAverageGeneration:
                lastResult.AverageGeneration,

            FinalSpeciesCount:
                lastResult.Species.Count,

            VallePopulation:
                regionalPopulation.GetValueOrDefault(
                    0
                ),

            BosquePopulation:
                regionalPopulation.GetValueOrDefault(
                    1
                ),

            LlanuraPopulation:
                regionalPopulation.GetValueOrDefault(
                    2
                ),

            ValleOccupancy:
                OccupancyFor(
                    0
                ),

            BosqueOccupancy:
                OccupancyFor(
                    1
                ),

            LlanuraOccupancy:
                OccupancyFor(
                    2
                ),

            FinalLivingThermalModifier:
                finalLivingModifier,

            MeanEffectiveParentThermalModifier:
                meanParentModifier,

            FinalLivingStressMultiplier:
                finalLivingStressMultiplier,

            MeanEffectiveParentStressMultiplier:
                meanParentStressMultiplier,

            FinalLivingThermalFitness:
                finalLivingThermalFitness,

            MeanEffectiveParentThermalFitness:
                meanParentThermalFitness,

            FinalLlanuraThermalModifier:
                finalLlanuraModifier,

            FinalLlanuraThermalFitness:
                finalLlanuraThermalFitness,

            ValleBosqueMateAcceptance:
                FindMateAcceptance(
                    0,
                    1
                ),

            ValleLlanuraMateAcceptance:
                FindMateAcceptance(
                    0,
                    2
                ),

            BosqueLlanuraMateAcceptance:
                FindMateAcceptance(
                    1,
                    2
                )
        );
    }


    private static double GetLocalThermalFitness(
        Worldd world,
        Organism organism)
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


        return organism.GetThermalFitness(
            localTemperature
        );
    }


    private static void WriteDetailCsv(
        string path,
        IReadOnlyList<ThermalRegulationExperimentResult> results)
    {
        CultureInfo culture =
            CultureInfo.InvariantCulture;


        List<string> lines =
        [
            "Seed,Coupling,MaxThermalChallengeEffectPercent," +
            "FinalCycle,FinalPopulation,MinimumPopulation,MinimumPopulationCycle," +
            "MaximumPopulation,MaximumPopulationCycle,LongestNoBirthStreak," +
            "FinalBirths,FinalDeaths,FinalAverageGeneration,FinalSpeciesCount," +
            "VallePopulation,BosquePopulation,LlanuraPopulation," +
            "ValleLocalExtinctionEpisodes,ValleRecolonizations,ValleLongestEmptyPeriod,ValleEmptyAtEnd," +
            "BosqueLocalExtinctionEpisodes,BosqueRecolonizations,BosqueLongestEmptyPeriod,BosqueEmptyAtEnd," +
            "LlanuraLocalExtinctionEpisodes,LlanuraRecolonizations,LlanuraLongestEmptyPeriod,LlanuraEmptyAtEnd," +
            "FinalLivingThermalModifier,MeanEffectiveParentThermalModifier," +
            "FinalLivingStressMultiplier,MeanEffectiveParentStressMultiplier," +
            "FinalLivingThermalFitness,MeanEffectiveParentThermalFitness," +
            "FinalLlanuraThermalModifier,FinalLlanuraThermalFitness," +
            "ValleBosqueMateAcceptance,ValleLlanuraMateAcceptance,BosqueLlanuraMateAcceptance"
        ];


        foreach (
            ThermalRegulationExperimentResult result
            in results
        )
        {
            lines.Add(
                string.Join(
                    ",",
                    result.Seed.ToString(culture),
                    result.Coupling.ToString("F4", culture),
                    result.MaximumThermalChallengeEffectPercent.ToString("F3", culture),
                    result.FinalCycle.ToString(culture),
                    result.FinalPopulation.ToString(culture),
                    result.MinimumPopulation.ToString(culture),
                    result.MinimumPopulationCycle.ToString(culture),
                    result.MaximumPopulation.ToString(culture),
                    result.MaximumPopulationCycle.ToString(culture),
                    result.LongestNoBirthStreak.ToString(culture),
                    result.FinalBirths.ToString(culture),
                    result.FinalDeaths.ToString(culture),
                    result.FinalAverageGeneration.ToString("F3", culture),
                    result.FinalSpeciesCount.ToString(culture),
                    result.VallePopulation.ToString(culture),
                    result.BosquePopulation.ToString(culture),
                    result.LlanuraPopulation.ToString(culture),
                    result.ValleOccupancy.LocalExtinctionEpisodeCount.ToString(culture),
                    result.ValleOccupancy.RecolonizationCount.ToString(culture),
                    result.ValleOccupancy.LongestEmptyPeriod.ToString(culture),
                    result.ValleOccupancy.IsEmptyAtEnd,
                    result.BosqueOccupancy.LocalExtinctionEpisodeCount.ToString(culture),
                    result.BosqueOccupancy.RecolonizationCount.ToString(culture),
                    result.BosqueOccupancy.LongestEmptyPeriod.ToString(culture),
                    result.BosqueOccupancy.IsEmptyAtEnd,
                    result.LlanuraOccupancy.LocalExtinctionEpisodeCount.ToString(culture),
                    result.LlanuraOccupancy.RecolonizationCount.ToString(culture),
                    result.LlanuraOccupancy.LongestEmptyPeriod.ToString(culture),
                    result.LlanuraOccupancy.IsEmptyAtEnd,
                    result.FinalLivingThermalModifier.ToString("F6", culture),
                    result.MeanEffectiveParentThermalModifier.ToString("F6", culture),
                    result.FinalLivingStressMultiplier.ToString("F6", culture),
                    result.MeanEffectiveParentStressMultiplier.ToString("F6", culture),
                    result.FinalLivingThermalFitness.ToString("F6", culture),
                    result.MeanEffectiveParentThermalFitness.ToString("F6", culture),
                    result.FinalLlanuraThermalModifier.ToString("F6", culture),
                    result.FinalLlanuraThermalFitness.ToString("F6", culture),
                    FormatNullable(result.ValleBosqueMateAcceptance, culture),
                    FormatNullable(result.ValleLlanuraMateAcceptance, culture),
                    FormatNullable(result.BosqueLlanuraMateAcceptance, culture)
                )
            );
        }


        File.WriteAllLines(
            path,
            lines
        );
    }


    private static void WriteSummaryCsv(
        string path,
        IReadOnlyList<ThermalRegulationExperimentResult> results)
    {
        CultureInfo culture =
            CultureInfo.InvariantCulture;


        List<string> lines =
        [
            "Coupling,MaxThermalChallengeEffectPercent,Runs," +
            "TotalExtinctionCount,TotalExtinctionRate," +
            "LlanuraEmptyAtEndCount,LlanuraEmptyAtEndRate," +
            "FinalPopulationMedian,MinimumPopulationMedian,MaximumPopulationMedian," +
            "LongestNoBirthStreakMedian,FinalSpeciesMedian,LlanuraPopulationMedian," +
            "FinalLivingThermalModifierMean,MeanEffectiveParentThermalModifierMean," +
            "FinalLivingStressMultiplierMean,MeanEffectiveParentStressMultiplierMean," +
            "FinalLivingThermalFitnessMean,MeanEffectiveParentThermalFitnessMean," +
            "FinalLlanuraThermalModifierMean,FinalLlanuraThermalFitnessMean," +
            "ValleBosqueMateAcceptanceMean,ValleLlanuraMateAcceptanceMean,BosqueLlanuraMateAcceptanceMean"
        ];


        foreach (
            IGrouping<double, ThermalRegulationExperimentResult> group
            in results
                .GroupBy(
                    result =>
                        result.Coupling
                )
                .OrderBy(
                    group =>
                        group.Key
                )
        )
        {
            List<ThermalRegulationExperimentResult> values =
                group.ToList();


            int runCount =
                values.Count;


            int extinctionCount =
                values.Count(
                    result =>
                        result.FinalPopulation
                        ==
                        0
                );


            int llanuraEmptyCount =
                values.Count(
                    result =>
                        result.LlanuraOccupancy.IsEmptyAtEnd
                );


            lines.Add(
                string.Join(
                    ",",
                    group.Key.ToString("F4", culture),
                    (0.08 * group.Key * 100).ToString("F3", culture),
                    runCount.ToString(culture),
                    extinctionCount.ToString(culture),
                    Rate(extinctionCount, runCount).ToString("F6", culture),
                    llanuraEmptyCount.ToString(culture),
                    Rate(llanuraEmptyCount, runCount).ToString("F6", culture),
                    Median(values.Select(x => (double)x.FinalPopulation)).ToString("F3", culture),
                    Median(values.Select(x => (double)x.MinimumPopulation)).ToString("F3", culture),
                    Median(values.Select(x => (double)x.MaximumPopulation)).ToString("F3", culture),
                    Median(values.Select(x => (double)x.LongestNoBirthStreak)).ToString("F3", culture),
                    Median(values.Select(x => (double)x.FinalSpeciesCount)).ToString("F3", culture),
                    Median(values.Select(x => (double)x.LlanuraPopulation)).ToString("F3", culture),
                    values.Average(x => x.FinalLivingThermalModifier).ToString("F6", culture),
                    values.Average(x => x.MeanEffectiveParentThermalModifier).ToString("F6", culture),
                    values.Average(x => x.FinalLivingStressMultiplier).ToString("F6", culture),
                    values.Average(x => x.MeanEffectiveParentStressMultiplier).ToString("F6", culture),
                    values.Average(x => x.FinalLivingThermalFitness).ToString("F6", culture),
                    values.Average(x => x.MeanEffectiveParentThermalFitness).ToString("F6", culture),
                    values.Average(x => x.FinalLlanuraThermalModifier).ToString("F6", culture),
                    values.Average(x => x.FinalLlanuraThermalFitness).ToString("F6", culture),
                    MeanNullable(values.Select(x => x.ValleBosqueMateAcceptance)).ToString("F6", culture),
                    MeanNullable(values.Select(x => x.ValleLlanuraMateAcceptance)).ToString("F6", culture),
                    MeanNullable(values.Select(x => x.BosqueLlanuraMateAcceptance)).ToString("F6", culture)
                )
            );
        }


        File.WriteAllLines(
            path,
            lines
        );
    }


    private static void PrintCompactSummary(
        IReadOnlyList<ThermalRegulationExperimentResult> results)
    {
        Console.WriteLine(
            "ThermalRegulation summary:"
        );


        foreach (
            IGrouping<double, ThermalRegulationExperimentResult> group
            in results
                .GroupBy(
                    result =>
                        result.Coupling
                )
                .OrderBy(
                    group =>
                        group.Key
                )
        )
        {
            List<ThermalRegulationExperimentResult> values =
                group.ToList();


            int extinctionCount =
                values.Count(
                    result =>
                        result.FinalPopulation
                        ==
                        0
                );


            int llanuraEmptyCount =
                values.Count(
                    result =>
                        result.LlanuraOccupancy.IsEmptyAtEnd
                );


            Console.WriteLine(
                $"  Thermal {group.Key:F3} " +
                $"(±{0.08 * group.Key * 100:F2}%) | " +
                $"Ext {extinctionCount}/{values.Count} | " +
                $"Llanura0 {llanuraEmptyCount}/{values.Count} | " +
                $"Final med {Median(values.Select(x => (double)x.FinalPopulation)):F1} | " +
                $"Min med {Median(values.Select(x => (double)x.MinimumPopulation)):F1} | " +
                $"Max med {Median(values.Select(x => (double)x.MaximumPopulation)):F1} | " +
                $"NoBirth med {Median(values.Select(x => (double)x.LongestNoBirthStreak)):F1}"
            );
        }
    }


    private static double Median(
        IEnumerable<double> values)
    {
        double[] ordered =
            values
                .OrderBy(
                    value =>
                        value
                )
                .ToArray();


        if (
            ordered.Length
            ==
            0
        )
        {
            return 0;
        }


        int middle =
            ordered.Length
            /
            2;


        if (
            ordered.Length
            %
            2
            ==
            1
        )
        {
            return ordered[
                middle
            ];
        }


        return
            (
                ordered[
                    middle
                    -
                    1
                ]
                +
                ordered[
                    middle
                ]
            )
            /
            2.0;
    }


    private static double MeanNullable(
        IEnumerable<double?> values)
    {
        double[] available =
            values
                .Where(
                    value =>
                        value.HasValue
                )
                .Select(
                    value =>
                        value!.Value
                )
                .ToArray();


        return available.Length > 0
            ?
            available.Average()
            :
            0;
    }


    private static double Rate(
        int count,
        int total)
    {
        return total > 0
            ?
            count
            /
            (double)total
            :
            0;
    }


    private static string FormatNullable(
        double? value,
        CultureInfo culture)
    {
        return value.HasValue
            ?
            value.Value.ToString(
                "F6",
                culture
            )
            :
            string.Empty;
    }


    private sealed record ExperimentDefinition(
        int Seed,
        double Coupling
    );
}
