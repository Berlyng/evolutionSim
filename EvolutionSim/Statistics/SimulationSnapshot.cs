namespace EvolutionSim.Statistics;

public sealed record SimulationSnapshot(
    int Cycle,
    int Population,
    int Births,
    int Deaths,

    double PlantBiomass,
    double SeedBank,
    double RootBiomass,
    double PlantsConsumed,

    double AverageSpeed,
    double AverageSize,
    double AverageMetabolism,

    double SpeedStdDev,
    double SizeStdDev,
    double MetabolismStdDev,

    double MinSpeed,
    double MaxSpeed,

    double MinSize,
    double MaxSize,

    double MinMetabolism,
    double MaxMetabolism,

    double AverageEnergy,
    double AverageMaxEnergy,

    double AverageFoodEfficiency,
    double AveragePhysicalPerformance,
    double AverageAbsorptionCapacity,

    double AverageBasalMetabolicCost,
    double AverageActivityCost,
    double AverageEnergyCost,

    double AverageAge,

    double AverageGeneration,
    int MaxGeneration
);