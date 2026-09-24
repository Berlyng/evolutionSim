using System.Collections.Concurrent;
using System.Globalization;
using EvolutionSim.Evolution;
using EvolutionSim.Models;
using EvolutionSim.Simulation;
using EvolutionSim.World;

namespace EvolutionSim.Experiments;

/// <summary>
/// Estudio paralelo emparejado de FeedingSpecialization para la Fase 7.9.
///
/// MetabolicEfficiency permanece activo en 0.125.
/// ThermalRegulation permanece activo en 0.125.
/// BodySize permanece pasivo en 0.0.
/// Locomotion permanece pasivo en 0.0.
///
/// SOLO varía FeedingSpecialization.
///
/// 10 seeds × 3 couplings = 30 corridas.
/// </summary>
public sealed class FeedingSpecializationExperimentRunner
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
        "feeding_specialization_parallel_study.csv";


    private const string SummaryCsvFileName =
        "feeding_specialization_parallel_summary.csv";


    private static readonly object
        FileWriteLock =
            new();


    public IReadOnlyList<FeedingSpecializationExperimentResult> RunDefault()
    {
        ConcurrentBag<FeedingSpecializationExperimentResult> results =
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
            "EvolutionSim — Fase 7.9"
        );

        Console.WriteLine(
            "Estudio paralelo de FeedingSpecialization"
        );

        Console.WriteLine(
            $"Corridas: {totalRuns} " +
            $"({Seeds.Length} seeds × {Couplings.Length} couplings)"
        );

        Console.WriteLine(
            "MetabolicEfficiency = 0.125 (±1 %) fijo."
        );

        Console.WriteLine(
            "ThermalRegulation = 0.125 (±1 %) fijo."
        );

        Console.WriteLine(
            "BodySize = 0.0 (pasivo)."
        );

        Console.WriteLine(
            "Locomotion = 0.0 (pasivo)."
        );

        Console.WriteLine(
            $"Workers paralelos: {maxDegreeOfParallelism}"
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
                FeedingSpecializationExperimentResult result =
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
                    $"Feeding {result.Coupling:F3} | " +
                    $"Final {result.FinalPopulation} | " +
                    $"Min {result.MinimumPopulation}@C{result.MinimumPopulationCycle} | " +
                    $"Max {result.MaximumPopulation}@C{result.MaximumPopulationCycle} | " +
                    $"SinNacMax {result.LongestNoBirthStreak} | " +
                    $"Llanura {result.LlanuraPopulation}"
                );


                Checkpoint(
                    detailCsvPath,
                    summaryCsvPath,
                    results
                );
            }
        );


        List<FeedingSpecializationExperimentResult> orderedResults =
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
            $"CSV detallado: {detailCsvPath}"
        );

        Console.WriteLine(
            $"CSV resumen: {summaryCsvPath}"
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
        ConcurrentBag<FeedingSpecializationExperimentResult> results)
    {
        lock (
            FileWriteLock
        )
        {
            List<FeedingSpecializationExperimentResult> snapshot =
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


    private static FeedingSpecializationExperimentResult RunSingle(
        int seed,
        double coupling)
    {
        SimulationConfig config =
            SimulationConfig.Default
                with
            {
                RandomSeed =
                        seed,

                BodySizeEcologicalCoupling =
                        0.0,

                LocomotionEcologicalCoupling =
                        0.0,

                FeedingSpecializationEcologicalCoupling =
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

        double parentPlantEnergyMultiplierSum =
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


            // Step() ya cerró su scope de FeedingSpecialization.
            // Reabrimos el coupling solo para medir las propiedades de padres.
            // No se vuelve a ejecutar biología ni se consume RNG.
            using IDisposable feedingDiagnosticsScope =
                PhenotypeEcologyContext
                    .PushFeedingSpecializationCoupling(
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
                        parent.FeedingSpecializationModifier;


                    parentPlantEnergyMultiplierSum +=
                        parent.FeedingSpecializationPlantEnergyMultiplier;


                    parentObservationCount++;
                }
            }
        }


        if (
            lastResult is null
        )
        {
            throw new InvalidOperationException(
                "El experimento de FeedingSpecialization no produjo ningún SimulationStepResult."
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


        // El último Step() ya cerró su scope.
        // Este scope solo permite medir el estado final con el coupling correcto.
        using IDisposable finalFeedingDiagnosticsScope =
            PhenotypeEcologyContext
                .PushFeedingSpecializationCoupling(
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
            AverageOrZero(
                finalLiving,
                organism =>
                    organism.FeedingSpecializationModifier
            );


        double finalLivingPlantEnergyMultiplier =
            finalLiving.Count > 0
                ?
                finalLiving.Average(
                    organism =>
                        organism.FeedingSpecializationPlantEnergyMultiplier
                )
                :
                1;


        double finalLivingPlantDigestionEfficiency =
            AverageOrZero(
                finalLiving,
                organism =>
                    organism.PlantDigestionEfficiency
            );


        double finalLivingEffectivePlantEnergyAssimilation =
            AverageOrZero(
                finalLiving,
                organism =>
                    organism.EffectivePlantEnergyAssimilation
            );


        double finalLlanuraModifier =
            AverageOrZero(
                finalLlanura,
                organism =>
                    organism.FeedingSpecializationModifier
            );


        double finalLlanuraPlantEnergyMultiplier =
            finalLlanura.Count > 0
                ?
                finalLlanura.Average(
                    organism =>
                        organism.FeedingSpecializationPlantEnergyMultiplier
                )
                :
                1;


        double finalLlanuraPlantDigestionEfficiency =
            AverageOrZero(
                finalLlanura,
                organism =>
                    organism.PlantDigestionEfficiency
            );


        double finalLlanuraEffectivePlantEnergyAssimilation =
            AverageOrZero(
                finalLlanura,
                organism =>
                    organism.EffectivePlantEnergyAssimilation
            );


        double meanParentModifier =
            parentObservationCount > 0
                ?
                parentModifierSum
                /
                parentObservationCount
                :
                0;


        double meanParentPlantEnergyMultiplier =
            parentObservationCount > 0
                ?
                parentPlantEnergyMultiplierSum
                /
                parentObservationCount
                :
                1;


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


        return new FeedingSpecializationExperimentResult(
            Seed:
                seed,

            Coupling:
                coupling,

            MaximumPlantEnergyEffectPercent:
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

            FinalLivingFeedingModifier:
                finalLivingModifier,

            MeanEffectiveParentFeedingModifier:
                meanParentModifier,

            FinalLivingPlantEnergyMultiplier:
                finalLivingPlantEnergyMultiplier,

            MeanEffectiveParentPlantEnergyMultiplier:
                meanParentPlantEnergyMultiplier,

            FinalLivingPlantDigestionEfficiency:
                finalLivingPlantDigestionEfficiency,

            FinalLivingEffectivePlantEnergyAssimilation:
                finalLivingEffectivePlantEnergyAssimilation,

            FinalLlanuraFeedingModifier:
                finalLlanuraModifier,

            FinalLlanuraPlantEnergyMultiplier:
                finalLlanuraPlantEnergyMultiplier,

            FinalLlanuraPlantDigestionEfficiency:
                finalLlanuraPlantDigestionEfficiency,

            FinalLlanuraEffectivePlantEnergyAssimilation:
                finalLlanuraEffectivePlantEnergyAssimilation,

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


    private static double AverageOrZero(
        IReadOnlyList<Organism> organisms,
        Func<Organism, double> selector)
    {
        return organisms.Count > 0
            ?
            organisms.Average(
                selector
            )
            :
            0;
    }


    private static void WriteDetailCsv(
        string path,
        IReadOnlyList<FeedingSpecializationExperimentResult> results)
    {
        CultureInfo culture =
            CultureInfo.InvariantCulture;


        List<string> lines =
        [
            "Seed,Coupling,MaxPlantEnergyEffectPercent," +
            "FinalCycle,FinalPopulation,MinimumPopulation,MinimumPopulationCycle," +
            "MaximumPopulation,MaximumPopulationCycle,LongestNoBirthStreak," +
            "FinalBirths,FinalDeaths,FinalAverageGeneration,FinalSpeciesCount," +
            "VallePopulation,BosquePopulation,LlanuraPopulation," +
            "ValleLocalExtinctionEpisodes,ValleRecolonizations,ValleLongestEmptyPeriod,ValleEmptyAtEnd," +
            "BosqueLocalExtinctionEpisodes,BosqueRecolonizations,BosqueLongestEmptyPeriod,BosqueEmptyAtEnd," +
            "LlanuraLocalExtinctionEpisodes,LlanuraRecolonizations,LlanuraLongestEmptyPeriod,LlanuraEmptyAtEnd," +
            "FinalLivingFeedingModifier,MeanEffectiveParentFeedingModifier," +
            "FinalLivingPlantEnergyMultiplier,MeanEffectiveParentPlantEnergyMultiplier," +
            "FinalLivingPlantDigestionEfficiency,FinalLivingEffectivePlantEnergyAssimilation," +
            "FinalLlanuraFeedingModifier,FinalLlanuraPlantEnergyMultiplier," +
            "FinalLlanuraPlantDigestionEfficiency,FinalLlanuraEffectivePlantEnergyAssimilation," +
            "ValleBosqueMateAcceptance,ValleLlanuraMateAcceptance,BosqueLlanuraMateAcceptance"
        ];


        foreach (
            FeedingSpecializationExperimentResult result
            in results
        )
        {
            lines.Add(
                string.Join(
                    ",",
                    result.Seed.ToString(culture),
                    result.Coupling.ToString("F4", culture),
                    result.MaximumPlantEnergyEffectPercent.ToString("F3", culture),
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
                    result.FinalLivingFeedingModifier.ToString("F6", culture),
                    result.MeanEffectiveParentFeedingModifier.ToString("F6", culture),
                    result.FinalLivingPlantEnergyMultiplier.ToString("F6", culture),
                    result.MeanEffectiveParentPlantEnergyMultiplier.ToString("F6", culture),
                    result.FinalLivingPlantDigestionEfficiency.ToString("F6", culture),
                    result.FinalLivingEffectivePlantEnergyAssimilation.ToString("F6", culture),
                    result.FinalLlanuraFeedingModifier.ToString("F6", culture),
                    result.FinalLlanuraPlantEnergyMultiplier.ToString("F6", culture),
                    result.FinalLlanuraPlantDigestionEfficiency.ToString("F6", culture),
                    result.FinalLlanuraEffectivePlantEnergyAssimilation.ToString("F6", culture),
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
        IReadOnlyList<FeedingSpecializationExperimentResult> results)
    {
        CultureInfo culture =
            CultureInfo.InvariantCulture;


        List<string> lines =
        [
            "Coupling,MaxPlantEnergyEffectPercent,Runs," +
            "TotalExtinctionCount,TotalExtinctionRate," +
            "LlanuraEmptyAtEndCount,LlanuraEmptyAtEndRate," +
            "FinalPopulationMedian,MinimumPopulationMedian,MaximumPopulationMedian," +
            "LongestNoBirthStreakMedian,FinalSpeciesMedian,LlanuraPopulationMedian," +
            "FinalLivingFeedingModifierMean,MeanEffectiveParentFeedingModifierMean," +
            "FinalLivingPlantEnergyMultiplierMean,MeanEffectiveParentPlantEnergyMultiplierMean," +
            "FinalLivingPlantDigestionEfficiencyMean,FinalLivingEffectivePlantEnergyAssimilationMean," +
            "FinalLlanuraFeedingModifierMean,FinalLlanuraPlantEnergyMultiplierMean," +
            "FinalLlanuraPlantDigestionEfficiencyMean,FinalLlanuraEffectivePlantEnergyAssimilationMean," +
            "ValleBosqueMateAcceptanceMean,ValleLlanuraMateAcceptanceMean,BosqueLlanuraMateAcceptanceMean"
        ];


        foreach (
            IGrouping<double, FeedingSpecializationExperimentResult> group
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
            List<FeedingSpecializationExperimentResult> values =
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
                    values.Average(x => x.FinalLivingFeedingModifier).ToString("F6", culture),
                    values.Average(x => x.MeanEffectiveParentFeedingModifier).ToString("F6", culture),
                    values.Average(x => x.FinalLivingPlantEnergyMultiplier).ToString("F6", culture),
                    values.Average(x => x.MeanEffectiveParentPlantEnergyMultiplier).ToString("F6", culture),
                    values.Average(x => x.FinalLivingPlantDigestionEfficiency).ToString("F6", culture),
                    values.Average(x => x.FinalLivingEffectivePlantEnergyAssimilation).ToString("F6", culture),
                    values.Average(x => x.FinalLlanuraFeedingModifier).ToString("F6", culture),
                    values.Average(x => x.FinalLlanuraPlantEnergyMultiplier).ToString("F6", culture),
                    values.Average(x => x.FinalLlanuraPlantDigestionEfficiency).ToString("F6", culture),
                    values.Average(x => x.FinalLlanuraEffectivePlantEnergyAssimilation).ToString("F6", culture),
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
        IReadOnlyList<FeedingSpecializationExperimentResult> results)
    {
        Console.WriteLine(
            "Resumen de FeedingSpecialization:"
        );


        foreach (
            IGrouping<double, FeedingSpecializationExperimentResult> group
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
            List<FeedingSpecializationExperimentResult> values =
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
                $"  Feeding {group.Key:F3} " +
                $"(±{0.08 * group.Key * 100:F2}%) | " +
                $"Ext {extinctionCount}/{values.Count} | " +
                $"Llanura0 {llanuraEmptyCount}/{values.Count} | " +
                $"Final med {Median(values.Select(x => (double)x.FinalPopulation)):F1} | " +
                $"Min med {Median(values.Select(x => (double)x.MinimumPopulation)):F1} | " +
                $"Max med {Median(values.Select(x => (double)x.MaximumPopulation)):F1} | " +
                $"SinNac med {Median(values.Select(x => (double)x.LongestNoBirthStreak)):F1}"
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
