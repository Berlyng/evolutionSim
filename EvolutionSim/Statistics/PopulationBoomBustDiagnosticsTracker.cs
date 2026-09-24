using EvolutionSim.Ecology;

namespace EvolutionSim.Statistics;

/// <summary>
/// Passive, stateful diagnostics for detecting boom/bust dynamics.
///
/// IMPORTANT:
/// - Does not consume Random.
/// - Does not mutate organisms, resources, climate, movement or reproduction.
/// - Thresholds are diagnostic labels only; they do not affect biology.
/// </summary>
public sealed class PopulationBoomBustDiagnosticsTracker
{
    private const double SevereScarcityThreshold = 0.10;
    private const double RecoveryThreshold = 0.25;
    private const double OvershootConsumptionRatio = 1.00;

    private int _globalNoBirthStreak;

    private readonly Dictionary<int, RegionDiagnosticState>
        _regionStates = new();


    public PopulationBoomBustDiagnostics Calculate(
        int cycle,
        int finalPopulation,
        int births,
        int deaths,
        IReadOnlyList<PlantResourceRegionStatistics> plantStatistics,
        IReadOnlyDictionary<int, int> birthsByRegion,
        IReadOnlyDictionary<int, int> deathsByRegion)
    {
        _globalNoBirthStreak =
            births == 0
                ? _globalNoBirthStreak + 1
                : 0;


        double totalEndBiomass =
            plantStatistics.Sum(
                statistics =>
                    statistics.EndBiomass
            );


        double totalCarryingCapacity =
            plantStatistics.Sum(
                statistics =>
                    statistics.CarryingCapacity
            );


        double totalConsumed =
            plantStatistics.Sum(
                statistics =>
                    statistics.ConsumedBiomass
            );


        double totalRegrowth =
            plantStatistics.Sum(
                statistics =>
                    statistics.Regrowth
            );


        double totalPlantCapacityFraction =
            totalCarryingCapacity > 0
                ? totalEndBiomass / totalCarryingCapacity
                : 0;


        double totalPlantBiomassPerCapita =
            finalPopulation > 0
                ? totalEndBiomass / finalPopulation
                : 0;


        double totalConsumptionToRegrowthRatio =
            totalRegrowth > 0
                ? totalConsumed / totalRegrowth
                : totalConsumed > 0
                    ? double.PositiveInfinity
                    : 0;


        List<PopulationBoomBustRegionDiagnostics>
            regionDiagnostics =
                new();


        foreach (
            PlantResourceRegionStatistics statistics
            in plantStatistics
        )
        {
            if (
                !_regionStates.TryGetValue(
                    statistics.RegionId,
                    out RegionDiagnosticState? state
                )
            )
            {
                state =
                    new RegionDiagnosticState();

                _regionStates[
                    statistics.RegionId
                ] =
                    state;
            }


            int regionalBirths =
                birthsByRegion.GetValueOrDefault(
                    statistics.RegionId
                );


            int regionalDeaths =
                deathsByRegion.GetValueOrDefault(
                    statistics.RegionId
                );


            state.NoBirthStreak =
                regionalBirths == 0
                    ? state.NoBirthStreak + 1
                    : 0;


            bool isSeverelyScarce =
                statistics.EndCapacityFraction
                <
                SevereScarcityThreshold;


            if (
                !state.IsScarcityEpisodeActive
                &&
                isSeverelyScarce
            )
            {
                state.IsScarcityEpisodeActive =
                    true;

                state.CurrentScarcityRecoveryDuration =
                    1;

                state.ScarcityEpisodeCount++;
            }
            else if (
                state.IsScarcityEpisodeActive
            )
            {
                if (
                    statistics.EndCapacityFraction
                    >=
                    RecoveryThreshold
                )
                {
                    state.LastScarcityRecoveryDuration =
                        state.CurrentScarcityRecoveryDuration;

                    state.LongestScarcityRecoveryDuration =
                        Math.Max(
                            state.LongestScarcityRecoveryDuration,
                            state.CurrentScarcityRecoveryDuration
                        );

                    state.CurrentScarcityRecoveryDuration =
                        0;

                    state.IsScarcityEpisodeActive =
                        false;
                }
                else
                {
                    state.CurrentScarcityRecoveryDuration++;
                }
            }


            if (
                state.IsScarcityEpisodeActive
            )
            {
                state.LongestScarcityRecoveryDuration =
                    Math.Max(
                        state.LongestScarcityRecoveryDuration,
                        state.CurrentScarcityRecoveryDuration
                    );
            }


            bool hasOvershootSignal =
                statistics.ConsumptionToRegrowthRatio
                    >
                    OvershootConsumptionRatio
                &&
                statistics.EndCapacityFraction
                    <
                    RecoveryThreshold;


            regionDiagnostics.Add(
                new PopulationBoomBustRegionDiagnostics(
                    RegionId:
                        statistics.RegionId,

                    RegionName:
                        statistics.RegionName,

                    Population:
                        statistics.LivingPopulation,

                    Births:
                        regionalBirths,

                    Deaths:
                        regionalDeaths,

                    PlantCapacityFraction:
                        statistics.EndCapacityFraction,

                    PlantBiomassPerCapita:
                        statistics.EndBiomassPerCapita,

                    ConsumptionToRegrowthRatio:
                        statistics.ConsumptionToRegrowthRatio,

                    NoBirthStreak:
                        state.NoBirthStreak,

                    IsSeverelyResourceScarce:
                        isSeverelyScarce,

                    HasOvershootSignal:
                        hasOvershootSignal,

                    ScarcityEpisodeCount:
                        state.ScarcityEpisodeCount,

                    IsScarcityRecoveryActive:
                        state.IsScarcityEpisodeActive,

                    CurrentScarcityRecoveryDuration:
                        state.CurrentScarcityRecoveryDuration,

                    LastScarcityRecoveryDuration:
                        state.LastScarcityRecoveryDuration,

                    LongestScarcityRecoveryDuration:
                        state.LongestScarcityRecoveryDuration
                )
            );
        }


        return new PopulationBoomBustDiagnostics(
            Cycle:
                cycle,

            Population:
                finalPopulation,

            Births:
                births,

            Deaths:
                deaths,

            NoBirthStreak:
                _globalNoBirthStreak,

            TotalPlantBiomass:
                totalEndBiomass,

            TotalPlantCapacity:
                totalCarryingCapacity,

            TotalPlantCapacityFraction:
                totalPlantCapacityFraction,

            TotalPlantBiomassPerCapita:
                totalPlantBiomassPerCapita,

            ConsumptionToRegrowthRatio:
                totalConsumptionToRegrowthRatio,

            RegionsWithSevereScarcity:
                regionDiagnostics.Count(
                    region =>
                        region.IsSeverelyResourceScarce
                ),

            RegionsWithOvershootSignal:
                regionDiagnostics.Count(
                    region =>
                        region.HasOvershootSignal
                ),

            Regions:
                regionDiagnostics
        );
    }


    private sealed class RegionDiagnosticState
    {
        public int NoBirthStreak { get; set; }

        public bool IsScarcityEpisodeActive { get; set; }

        public int ScarcityEpisodeCount { get; set; }

        public int CurrentScarcityRecoveryDuration { get; set; }

        public int LastScarcityRecoveryDuration { get; set; }

        public int LongestScarcityRecoveryDuration { get; set; }
    }
}


public sealed record PopulationBoomBustDiagnostics(
    int Cycle,
    int Population,
    int Births,
    int Deaths,
    int NoBirthStreak,
    double TotalPlantBiomass,
    double TotalPlantCapacity,
    double TotalPlantCapacityFraction,
    double TotalPlantBiomassPerCapita,
    double ConsumptionToRegrowthRatio,
    int RegionsWithSevereScarcity,
    int RegionsWithOvershootSignal,
    IReadOnlyList<PopulationBoomBustRegionDiagnostics> Regions
);


public sealed record PopulationBoomBustRegionDiagnostics(
    int RegionId,
    string RegionName,
    int Population,
    int Births,
    int Deaths,
    double PlantCapacityFraction,
    double PlantBiomassPerCapita,
    double ConsumptionToRegrowthRatio,
    int NoBirthStreak,
    bool IsSeverelyResourceScarce,
    bool HasOvershootSignal,
    int ScarcityEpisodeCount,
    bool IsScarcityRecoveryActive,
    int CurrentScarcityRecoveryDuration,
    int LastScarcityRecoveryDuration,
    int LongestScarcityRecoveryDuration
);
