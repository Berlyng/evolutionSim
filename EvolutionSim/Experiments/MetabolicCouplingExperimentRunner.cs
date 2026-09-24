using System.Collections.Concurrent;
using System.Globalization;
using EvolutionSim.Evolution;
using EvolutionSim.Simulation;
using EvolutionSim.World;

namespace EvolutionSim.Experiments;

/// <summary>
/// Phase 7.5 experimental runner.
///
/// This remains an opt-in research utility. It is not part of the normal
/// simulation flow.
///
/// Biology and RNG remain unchanged.
/// Each run owns its SimulationConfig and SimulationEngine.
///
/// Parallelism is bounded to avoid saturating the machine.
/// Results are checkpointed safely after each completed run.
/// </summary>
public sealed class MetabolicCouplingExperimentRunner
{
    private static readonly int[] Seeds =
        Enumerable.Range(
            start:
                98765,
            count:
                20
        )
        .ToArray();


    private static readonly double[] Couplings =
    [
        0.00,
        0.125,
        0.25
    ];


    private const string DetailCsvFileName =
        "metabolic_coupling_parallel_study.csv";


    private const string SummaryCsvFileName =
        "metabolic_coupling_parallel_summary.csv";


    private static readonly object
        FileWriteLock =
            new();


    public IReadOnlyList<MetabolicCouplingExperimentResult> RunDefault()
    {
        ConcurrentBag<MetabolicCouplingExperimentResult> results =
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
            "EvolutionSim — Phase 7.5"
        );

        Console.WriteLine(
            "Parallel Metabolic Coupling Research Runner"
        );

        Console.WriteLine(
            $"Runs: {totalRuns} " +
            $"({Seeds.Length} seeds × {Couplings.Length} couplings)"
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
                MetabolicCouplingExperimentResult result =
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
                    $"Coupling {result.Coupling:F3} | " +
                    $"FinalPop {result.FinalPopulation} | " +
                    $"MinPop {result.MinimumPopulation} @ C{result.MinimumPopulationCycle} | " +
                    $"Species {result.FinalSpeciesCount} | " +
                    $"Llanura {result.LlanuraPopulation} | " +
                    $"Llanura local-ext {result.LlanuraOccupancy.LocalExtinctionEpisodeCount} | " +
                    $"recol {result.LlanuraOccupancy.RecolonizationCount}"
                );


                Checkpoint(
                    detailCsvPath,
                    summaryCsvPath,
                    results
                );
            }
        );


        List<MetabolicCouplingExperimentResult> orderedResults =
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
        ConcurrentBag<MetabolicCouplingExperimentResult> results)
    {
        lock (
            FileWriteLock
        )
        {
            List<MetabolicCouplingExperimentResult> snapshot =
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


    private static MetabolicCouplingExperimentResult RunSingle(
        int seed,
        double coupling)
    {
        SimulationConfig config =
            SimulationConfig.Default
                with
            {
                RandomSeed =
                        seed,

                MetabolicEfficiencyEcologicalCoupling =
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
        }


        if (
            lastResult is null
        )
        {
            throw new InvalidOperationException(
                "La corrida experimental no produjo SimulationStepResult."
            );
        }


        Dictionary<int, int>
            regionalPopulation =
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


        RegionalOccupancyExperimentStatistics
            OccupancyFor(
                int regionId)
        {
            int population =
                regionalPopulation.GetValueOrDefault(
                    regionId
                );


            return occupancyTrackers[
                regionId
            ]
            .Snapshot(
                isEmptyAtEnd:
                    population
                    ==
                    0
            );
        }


        GeneticDistanceCalculator
            geneticDistanceCalculator =
                new();


        MateCompatibilityCalculator
            mateCompatibilityCalculator =
                new(
                    geneticDistanceCalculator
                );


        EcologicalMateSimilarityCalculator
            ecologicalMateSimilarityCalculator =
                new();


        RegionalReproductiveIsolationCalculator
            isolationCalculator =
                new(
                    geneticDistanceCalculator,
                    mateCompatibilityCalculator,
                    ecologicalMateSimilarityCalculator
                );


        List<RegionalReproductiveIsolation>
            isolations =
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


        return new MetabolicCouplingExperimentResult(
            Seed:
                seed,

            Coupling:
                coupling,

            MaximumEnergyEffectPercent:
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

            FinalLivingMetabolicModifier:
                lastResult
                    .MetabolicSelectionDiagnostics
                    .Overall
                    .AverageModifier,

            FinalParentMetabolicModifier:
                lastResult
                    .MetabolicSelectionDiagnostics
                    .Parents
                    .AverageModifier,

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


    private static void WriteDetailCsv(
        string path,
        IReadOnlyList<MetabolicCouplingExperimentResult> results)
    {
        CultureInfo culture =
            CultureInfo.InvariantCulture;


        List<string> lines =
        [
            "Seed,Coupling,MaxEnergyEffectPercent," +
            "FinalCycle,FinalPopulation,MinimumPopulation,MinimumPopulationCycle," +
            "FinalBirths,FinalDeaths,FinalAverageGeneration,FinalSpeciesCount," +
            "VallePopulation,BosquePopulation,LlanuraPopulation," +
            "ValleLocalExtinctionEpisodes,ValleRecolonizations,ValleLongestEmptyPeriod,ValleEmptyAtEnd," +
            "BosqueLocalExtinctionEpisodes,BosqueRecolonizations,BosqueLongestEmptyPeriod,BosqueEmptyAtEnd," +
            "LlanuraLocalExtinctionEpisodes,LlanuraRecolonizations,LlanuraLongestEmptyPeriod,LlanuraEmptyAtEnd," +
            "FinalLivingMetabolicModifier,FinalParentMetabolicModifier," +
            "ValleBosqueMateAcceptance,ValleLlanuraMateAcceptance,BosqueLlanuraMateAcceptance"
        ];


        foreach (
            MetabolicCouplingExperimentResult result
            in results
        )
        {
            lines.Add(
                string.Join(
                    ",",
                    result.Seed.ToString(
                        culture
                    ),
                    result.Coupling.ToString(
                        "F4",
                        culture
                    ),
                    result.MaximumEnergyEffectPercent.ToString(
                        "F3",
                        culture
                    ),
                    result.FinalCycle.ToString(
                        culture
                    ),
                    result.FinalPopulation.ToString(
                        culture
                    ),
                    result.MinimumPopulation.ToString(
                        culture
                    ),
                    result.MinimumPopulationCycle.ToString(
                        culture
                    ),
                    result.FinalBirths.ToString(
                        culture
                    ),
                    result.FinalDeaths.ToString(
                        culture
                    ),
                    result.FinalAverageGeneration.ToString(
                        "F3",
                        culture
                    ),
                    result.FinalSpeciesCount.ToString(
                        culture
                    ),
                    result.VallePopulation.ToString(
                        culture
                    ),
                    result.BosquePopulation.ToString(
                        culture
                    ),
                    result.LlanuraPopulation.ToString(
                        culture
                    ),
                    result.ValleOccupancy.LocalExtinctionEpisodeCount.ToString(
                        culture
                    ),
                    result.ValleOccupancy.RecolonizationCount.ToString(
                        culture
                    ),
                    result.ValleOccupancy.LongestEmptyPeriod.ToString(
                        culture
                    ),
                    result.ValleOccupancy.IsEmptyAtEnd,
                    result.BosqueOccupancy.LocalExtinctionEpisodeCount.ToString(
                        culture
                    ),
                    result.BosqueOccupancy.RecolonizationCount.ToString(
                        culture
                    ),
                    result.BosqueOccupancy.LongestEmptyPeriod.ToString(
                        culture
                    ),
                    result.BosqueOccupancy.IsEmptyAtEnd,
                    result.LlanuraOccupancy.LocalExtinctionEpisodeCount.ToString(
                        culture
                    ),
                    result.LlanuraOccupancy.RecolonizationCount.ToString(
                        culture
                    ),
                    result.LlanuraOccupancy.LongestEmptyPeriod.ToString(
                        culture
                    ),
                    result.LlanuraOccupancy.IsEmptyAtEnd,
                    result.FinalLivingMetabolicModifier.ToString(
                        "F6",
                        culture
                    ),
                    result.FinalParentMetabolicModifier.ToString(
                        "F6",
                        culture
                    ),
                    FormatNullable(
                        result.ValleBosqueMateAcceptance,
                        culture
                    ),
                    FormatNullable(
                        result.ValleLlanuraMateAcceptance,
                        culture
                    ),
                    FormatNullable(
                        result.BosqueLlanuraMateAcceptance,
                        culture
                    )
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
        IReadOnlyList<MetabolicCouplingExperimentResult> results)
    {
        CultureInfo culture =
            CultureInfo.InvariantCulture;


        List<string> lines =
        [
            "Coupling,MaxEnergyEffectPercent,Runs," +
            "TotalExtinctionCount,TotalExtinctionRate," +
            "LlanuraEmptyAtEndCount,LlanuraEmptyAtEndRate," +
            "LlanuraLocalExtinctionEpisodesMean,LlanuraRecolonizationsMean,LlanuraLongestEmptyPeriodMedian," +
            "FinalPopulationMean,FinalPopulationMedian," +
            "MinimumPopulationMean,MinimumPopulationMedian," +
            "FinalSpeciesMean,FinalSpeciesMedian," +
            "LlanuraPopulationMean,LlanuraPopulationMedian," +
            "ParentMinusLivingModifierMean," +
            "ValleBosqueMateAcceptanceMean," +
            "ValleLlanuraMateAcceptanceMean," +
            "BosqueLlanuraMateAcceptanceMean"
        ];


        foreach (
            IGrouping<double, MetabolicCouplingExperimentResult> group
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
            List<MetabolicCouplingExperimentResult> values =
                group.ToList();


            int runCount =
                values.Count;


            int totalExtinctionCount =
                values.Count(
                    result =>
                        result.FinalPopulation
                        ==
                        0
                );


            int llanuraEmptyAtEndCount =
                values.Count(
                    result =>
                        result.LlanuraOccupancy.IsEmptyAtEnd
                );


            lines.Add(
                string.Join(
                    ",",
                    group.Key.ToString(
                        "F4",
                        culture
                    ),
                    (
                        0.08
                        *
                        group.Key
                        *
                        100
                    ).ToString(
                        "F3",
                        culture
                    ),
                    runCount.ToString(
                        culture
                    ),
                    totalExtinctionCount.ToString(
                        culture
                    ),
                    Rate(
                        totalExtinctionCount,
                        runCount
                    ).ToString(
                        "F6",
                        culture
                    ),
                    llanuraEmptyAtEndCount.ToString(
                        culture
                    ),
                    Rate(
                        llanuraEmptyAtEndCount,
                        runCount
                    ).ToString(
                        "F6",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.LlanuraOccupancy.LocalExtinctionEpisodeCount
                    ).ToString(
                        "F3",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.LlanuraOccupancy.RecolonizationCount
                    ).ToString(
                        "F3",
                        culture
                    ),
                    Median(
                        values.Select(
                            result =>
                                (double)result.LlanuraOccupancy.LongestEmptyPeriod
                        )
                    ).ToString(
                        "F3",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.FinalPopulation
                    ).ToString(
                        "F3",
                        culture
                    ),
                    Median(
                        values.Select(
                            result =>
                                (double)result.FinalPopulation
                        )
                    ).ToString(
                        "F3",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.MinimumPopulation
                    ).ToString(
                        "F3",
                        culture
                    ),
                    Median(
                        values.Select(
                            result =>
                                (double)result.MinimumPopulation
                        )
                    ).ToString(
                        "F3",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.FinalSpeciesCount
                    ).ToString(
                        "F3",
                        culture
                    ),
                    Median(
                        values.Select(
                            result =>
                                (double)result.FinalSpeciesCount
                        )
                    ).ToString(
                        "F3",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.LlanuraPopulation
                    ).ToString(
                        "F3",
                        culture
                    ),
                    Median(
                        values.Select(
                            result =>
                                (double)result.LlanuraPopulation
                        )
                    ).ToString(
                        "F3",
                        culture
                    ),
                    values.Average(
                        result =>
                            result.FinalParentMetabolicModifier
                            -
                            result.FinalLivingMetabolicModifier
                    ).ToString(
                        "F6",
                        culture
                    ),
                    MeanNullable(
                        values.Select(
                            result =>
                                result.ValleBosqueMateAcceptance
                        )
                    ).ToString(
                        "F6",
                        culture
                    ),
                    MeanNullable(
                        values.Select(
                            result =>
                                result.ValleLlanuraMateAcceptance
                        )
                    ).ToString(
                        "F6",
                        culture
                    ),
                    MeanNullable(
                        values.Select(
                            result =>
                                result.BosqueLlanuraMateAcceptance
                        )
                    ).ToString(
                        "F6",
                        culture
                    )
                )
            );
        }


        File.WriteAllLines(
            path,
            lines
        );
    }


    private static void PrintCompactSummary(
        IReadOnlyList<MetabolicCouplingExperimentResult> results)
    {
        Console.WriteLine(
            "Final coupling summary:"
        );


        foreach (
            IGrouping<double, MetabolicCouplingExperimentResult> group
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
            PrintCouplingSummary(
                group.ToList()
            );
        }
    }


    private static void PrintCouplingSummary(
        IReadOnlyList<MetabolicCouplingExperimentResult> values)
    {
        if (
            values.Count
            ==
            0
        )
        {
            return;
        }


        double coupling =
            values[0].Coupling;


        int extinctionCount =
            values.Count(
                result =>
                    result.FinalPopulation
                    ==
                    0
            );


        int llanuraEmptyAtEndCount =
            values.Count(
                result =>
                    result.LlanuraOccupancy.IsEmptyAtEnd
            );


        Console.WriteLine(
            $"  Coupling {coupling:F3} " +
            $"(±{0.08 * coupling * 100:F2}%) | " +
            $"Runs {values.Count} | " +
            $"Ext {extinctionCount}/{values.Count} " +
            $"({Rate(extinctionCount, values.Count):P1}) | " +
            $"Llanura empty-end {llanuraEmptyAtEndCount}/{values.Count} " +
            $"({Rate(llanuraEmptyAtEndCount, values.Count):P1}) | " +
            $"FinalPop med {Median(values.Select(x => (double)x.FinalPopulation)):F1} | " +
            $"MinPop med {Median(values.Select(x => (double)x.MinimumPopulation)):F1} | " +
            $"Llanura local-ext avg " +
            $"{values.Average(x => x.LlanuraOccupancy.LocalExtinctionEpisodeCount):F2} | " +
            $"recol avg " +
            $"{values.Average(x => x.LlanuraOccupancy.RecolonizationCount):F2} | " +
            $"Parent-Living Δ " +
            $"{values.Average(x => x.FinalParentMetabolicModifier - x.FinalLivingMetabolicModifier):+0.000000;-0.000000;0.000000}"
        );
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
