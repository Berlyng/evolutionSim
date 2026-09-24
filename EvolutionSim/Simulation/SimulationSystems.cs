using EvolutionSim.Ecology;
using EvolutionSim.Evolution;
using EvolutionSim.Statistics;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public sealed class SimulationSystems
{
    public ClimateSystem ClimateSystem { get; }

    public GenomeMutator GenomeMutator { get; }
    public GenomeRecombiner GenomeRecombiner { get; }
    public GeneticDistanceCalculator GeneticDistanceCalculator { get; }
    public RegionalGeneticDivergenceCalculator RegionalGeneticDivergenceCalculator { get; }
    public MateCompatibilityCalculator MateCompatibilityCalculator { get; }
    public EcologicalMateSimilarityCalculator EcologicalMateSimilarityCalculator { get; }
    public RegionalReproductiveIsolationCalculator RegionalReproductiveIsolationCalculator { get; }
    public SuccessfulMatingStatisticsCalculator SuccessfulMatingStatisticsCalculator { get; }
    public ImmigrantReproductiveSuccessCalculator ImmigrantReproductiveSuccessCalculator { get; }

    public SexualReproductionSystem ReproductionSystem { get; }
    public SpeciesDetectionSystem SpeciesDetectionSystem { get; }

    public GrazingSystem GrazingSystem { get; }
    public CarcassSystem CarcassSystem { get; }
    public PredationSystem PredationSystem { get; }
    public FeedingSystem FeedingSystem { get; }
    public MigrationSystem MigrationSystem { get; }
    public PatchMovementSystem PatchMovementSystem { get; }

    public PlantResourceCycleTracker PlantResourceCycleTracker { get; }
    public RegionalTraitVariationCalculator RegionalTraitVariationCalculator { get; }
    public RegionalTraitVariationPrinter RegionalTraitVariationPrinter { get; }
    public GeneticDistanceBreakdownCalculator GeneticDistanceBreakdownCalculator { get; }
    public GeneticDistanceBreakdownPrinter GeneticDistanceBreakdownPrinter { get; }

    public SimulationSystems(
        ClimateSystem climateSystem,
        GenomeMutator genomeMutator,
        GenomeRecombiner genomeRecombiner,
        GeneticDistanceCalculator geneticDistanceCalculator,
        RegionalGeneticDivergenceCalculator regionalGeneticDivergenceCalculator,
        MateCompatibilityCalculator mateCompatibilityCalculator,
        EcologicalMateSimilarityCalculator ecologicalMateSimilarityCalculator,
        RegionalReproductiveIsolationCalculator regionalReproductiveIsolationCalculator,
        SuccessfulMatingStatisticsCalculator successfulMatingStatisticsCalculator,
        ImmigrantReproductiveSuccessCalculator immigrantReproductiveSuccessCalculator,
        SexualReproductionSystem reproductionSystem,
        SpeciesDetectionSystem speciesDetectionSystem,
        GrazingSystem grazingSystem,
        CarcassSystem carcassSystem,
        PredationSystem predationSystem,
        FeedingSystem feedingSystem,
        MigrationSystem migrationSystem,
        PatchMovementSystem patchMovementSystem,
        PlantResourceCycleTracker plantResourceCycleTracker,
        RegionalTraitVariationCalculator regionalTraitVariationCalculator,
        RegionalTraitVariationPrinter regionalTraitVariationPrinter,
        GeneticDistanceBreakdownCalculator geneticDistanceBreakdownCalculator,
        GeneticDistanceBreakdownPrinter geneticDistanceBreakdownPrinter)
    {
        ClimateSystem = climateSystem;

        GenomeMutator = genomeMutator;
        GenomeRecombiner = genomeRecombiner;
        GeneticDistanceCalculator = geneticDistanceCalculator;
        RegionalGeneticDivergenceCalculator = regionalGeneticDivergenceCalculator;
        MateCompatibilityCalculator = mateCompatibilityCalculator;
        EcologicalMateSimilarityCalculator = ecologicalMateSimilarityCalculator;
        RegionalReproductiveIsolationCalculator = regionalReproductiveIsolationCalculator;
        SuccessfulMatingStatisticsCalculator = successfulMatingStatisticsCalculator;
        ImmigrantReproductiveSuccessCalculator = immigrantReproductiveSuccessCalculator;

        ReproductionSystem = reproductionSystem;
        SpeciesDetectionSystem = speciesDetectionSystem;

        GrazingSystem = grazingSystem;
        CarcassSystem = carcassSystem;
        PredationSystem = predationSystem;
        FeedingSystem = feedingSystem;
        MigrationSystem = migrationSystem;
        PatchMovementSystem = patchMovementSystem;

        PlantResourceCycleTracker = plantResourceCycleTracker;
        RegionalTraitVariationCalculator = regionalTraitVariationCalculator;
        RegionalTraitVariationPrinter = regionalTraitVariationPrinter;
        GeneticDistanceBreakdownCalculator = geneticDistanceBreakdownCalculator;
        GeneticDistanceBreakdownPrinter = geneticDistanceBreakdownPrinter;
    }
}
