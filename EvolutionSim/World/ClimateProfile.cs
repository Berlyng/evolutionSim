namespace EvolutionSim.World;

public sealed record ClimateProfile(
    int RegionId,
    double SeasonalAmplitude,
    double WeatherVariation,
    double PhaseOffset = 0
);