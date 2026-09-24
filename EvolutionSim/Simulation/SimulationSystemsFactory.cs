using EvolutionSim.Ecology;
using EvolutionSim.Evolution;
using EvolutionSim.Output;
using EvolutionSim.Statistics;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public static class SimulationSystemsFactory
{
    public static SimulationSystems Create(
        Random random,
        Random climateRandom,
        Random structuralGenomeRandom,
        ISimulationOutput output)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(climateRandom);
        ArgumentNullException.ThrowIfNull(structuralGenomeRandom);
        ArgumentNullException.ThrowIfNull(output);

        ClimateSystem climateSystem =
            new(
                climateRandom,
                new[]
                {
                    new ClimateProfile(
                        RegionId: 0,
                        SeasonalAmplitude: 2.0,
                        WeatherVariation: 0.50
                    ),

                    new ClimateProfile(
                        RegionId: 1,
                        SeasonalAmplitude: 3.0,
                        WeatherVariation: 0.60
                    ),

                    new ClimateProfile(
                        RegionId: 2,
                        SeasonalAmplitude: 3.5,
                        WeatherVariation: 0.70
                    )
                }
            );

        GenomeMutator genomeMutator =
            new(
                random,
                structuralGenomeRandom
            );

        GenomeRecombiner genomeRecombiner =
            new(
                random,
                structuralGenomeRandom
            );

        GeneticDistanceCalculator geneticDistanceCalculator =
            new();

        RegionalGeneticDivergenceCalculator regionalGeneticDivergenceCalculator =
            new(
                geneticDistanceCalculator
            );

        MateCompatibilityCalculator mateCompatibilityCalculator =
            new(
                geneticDistanceCalculator
            );

        EcologicalMateSimilarityCalculator ecologicalMateSimilarityCalculator =
            new();

        RegionalReproductiveIsolationCalculator regionalReproductiveIsolationCalculator =
            new(
                geneticDistanceCalculator,
                mateCompatibilityCalculator,
                ecologicalMateSimilarityCalculator
            );

        SuccessfulMatingStatisticsCalculator successfulMatingStatisticsCalculator =
            new(
                geneticDistanceCalculator,
                mateCompatibilityCalculator,
                ecologicalMateSimilarityCalculator
            );

        ImmigrantReproductiveSuccessCalculator immigrantReproductiveSuccessCalculator =
            new();

        SexualReproductionSystem reproductionSystem =
            new(
                random,
                genomeRecombiner,
                genomeMutator,
                mateCompatibilityCalculator,
                ecologicalMateSimilarityCalculator
            );

        SpeciesDetectionSystem speciesDetectionSystem =
            new(
                geneticDistanceCalculator,
                mateCompatibilityCalculator,
                ecologicalMateSimilarityCalculator
            );

        GrazingSystem grazingSystem =
            new(
                halfSaturationBiomass: 5000
            );

        CarcassSystem carcassSystem =
            new();

        PredationSystem predationSystem =
            new(
                random
            );

        FeedingSystem feedingSystem =
            new(
                random,
                grazingSystem,
                predationSystem,
                carcassSystem
            );

        MigrationSystem migrationSystem =
            new(
                random,
                carcassSystem
            );

        PatchMovementSystem patchMovementSystem =
            new(
                random,
                carcassSystem
            );

        PlantResourceCycleTracker plantResourceCycleTracker =
            new();

        RegionalTraitVariationCalculator regionalTraitVariationCalculator =
            new();

        RegionalTraitVariationPrinter regionalTraitVariationPrinter =
            new(
                output
            );

        GeneticDistanceBreakdownCalculator geneticDistanceBreakdownCalculator =
            new(
                geneticDistanceCalculator
            );

        GeneticDistanceBreakdownPrinter geneticDistanceBreakdownPrinter =
            new(
                output
            );

        return new SimulationSystems(
            climateSystem,
            genomeMutator,
            genomeRecombiner,
            geneticDistanceCalculator,
            regionalGeneticDivergenceCalculator,
            mateCompatibilityCalculator,
            ecologicalMateSimilarityCalculator,
            regionalReproductiveIsolationCalculator,
            successfulMatingStatisticsCalculator,
            immigrantReproductiveSuccessCalculator,
            reproductionSystem,
            speciesDetectionSystem,
            grazingSystem,
            carcassSystem,
            predationSystem,
            feedingSystem,
            migrationSystem,
            patchMovementSystem,
            plantResourceCycleTracker,
            regionalTraitVariationCalculator,
            regionalTraitVariationPrinter,
            geneticDistanceBreakdownCalculator,
            geneticDistanceBreakdownPrinter
        );
    }
}
