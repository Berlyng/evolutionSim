namespace EvolutionSim.Experiments;

/// <summary>
/// Passive experimental statistics describing regional occupancy dynamics.
///
/// An empty episode begins only after the region has previously been occupied.
/// A recolonization occurs when an established region transitions from empty
/// back to occupied.
/// </summary>
public sealed record RegionalOccupancyExperimentStatistics(
    bool EverEstablished,
    int LocalExtinctionEpisodeCount,
    int RecolonizationCount,
    int LongestEmptyPeriod,
    bool IsEmptyAtEnd
);
