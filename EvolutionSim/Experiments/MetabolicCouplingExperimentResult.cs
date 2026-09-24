namespace EvolutionSim.Experiments;

public sealed record MetabolicCouplingExperimentResult(
    int Seed,
    double Coupling,
    double MaximumEnergyEffectPercent,
    int FinalCycle,
    int FinalPopulation,
    int MinimumPopulation,
    int MinimumPopulationCycle,
    int FinalBirths,
    int FinalDeaths,
    double FinalAverageGeneration,
    int FinalSpeciesCount,
    int VallePopulation,
    int BosquePopulation,
    int LlanuraPopulation,
    RegionalOccupancyExperimentStatistics ValleOccupancy,
    RegionalOccupancyExperimentStatistics BosqueOccupancy,
    RegionalOccupancyExperimentStatistics LlanuraOccupancy,
    double FinalLivingMetabolicModifier,
    double FinalParentMetabolicModifier,
    double? ValleBosqueMateAcceptance,
    double? ValleLlanuraMateAcceptance,
    double? BosqueLlanuraMateAcceptance
);
