namespace EvolutionSim.Statistics;

public sealed record GenealogyRecord(
    Guid Id,
    Guid? ParentAId,
    Guid? ParentBId,
    int Generation,
    int BirthCycle,
    double Speed,
    double Size,
    double Metabolism,
    double OptimalTemperature,
    double ThermalTolerance
);