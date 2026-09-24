using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

/// <summary>
/// Historial pasivo de especies confirmadas.
///
/// No ejecuta decisiones ecológicas, no consume Random y no modifica
/// organismos. Solamente observa los resultados del detector de especies.
/// </summary>
public sealed class SpeciesHistoryTracker
{
    private readonly Dictionary<int, MutableSpeciesHistory>
        _histories =
            new();

    private readonly HashSet<int>
        _activeSpeciesIds =
            new();

    private readonly List<SpeciesHistoryEvent>
        _events =
            new();

    public void RecordDetection(
        int cycle,
        IReadOnlyList<SpeciesCluster> detectedSpecies,
        bool simulationExtinct = false)
    {
        ArgumentNullException.ThrowIfNull(
            detectedSpecies
        );

        HashSet<int> currentSpeciesIds =
            detectedSpecies
                .Select(
                    species =>
                        species.SpeciesId
                )
                .ToHashSet();

        // --------------------------------------------------------
        // ESPECIES QUE YA NO APARECEN EN ESTA DETECCIÓN
        // --------------------------------------------------------
        //
        // Importante:
        // "No longer detected" NO significa automáticamente
        // extinción biológica. Puede ser pérdida del linaje,
        // reconexión reproductiva o fusión con otra especie.
        //

        foreach (
            int previousSpeciesId
            in _activeSpeciesIds
                .Where(
                    speciesId =>
                        !currentSpeciesIds.Contains(
                            speciesId
                        )
                )
                .OrderBy(
                    speciesId =>
                        speciesId
                )
                .ToList()
        )
        {
            if (
                !_histories.TryGetValue(
                    previousSpeciesId,
                    out MutableSpeciesHistory? history
                )
            )
            {
                continue;
            }

            history.IsActive =
                false;

            history.EndCycle =
                cycle;

            SpeciesHistoryEventType eventType =
                simulationExtinct
                    ? SpeciesHistoryEventType.SimulationExtinction
                    : SpeciesHistoryEventType.NoLongerDetected;

            AddEvent(
                new SpeciesHistoryEvent(
                    Cycle:
                        cycle,
                    Type:
                        eventType,
                    SpeciesId:
                        previousSpeciesId,
                    RelatedSpeciesId:
                        null,
                    Description:
                        simulationExtinct
                            ? $"S{previousSpeciesId} deja de existir al extinguirse la población global."
                            : $"S{previousSpeciesId} deja de aparecer como especie confirmada."
                )
            );
        }

        // --------------------------------------------------------
        // ESPECIES PRESENTES EN ESTA DETECCIÓN
        // --------------------------------------------------------

        foreach (
            SpeciesCluster species
            in detectedSpecies
                .OrderBy(
                    species =>
                        species.SpeciesId
                )
        )
        {
            bool isNewSpecies =
                !_histories.TryGetValue(
                    species.SpeciesId,
                    out MutableSpeciesHistory? history
                );

            if (isNewSpecies)
            {
                history =
                    new MutableSpeciesHistory(
                        speciesId:
                            species.SpeciesId,
                        parentSpeciesId:
                            species.ParentSpeciesId,
                        firstObservedCycle:
                            species.FirstDetectedCycle,
                        confirmedCycle:
                            cycle
                    );

                _histories[
                    species.SpeciesId
                ] =
                    history;

                if (
                    species.ParentSpeciesId
                    is int parentSpeciesId
                )
                {
                    AddEvent(
                        new SpeciesHistoryEvent(
                            Cycle:
                                cycle,
                            Type:
                                SpeciesHistoryEventType.SpeciesConfirmed,
                            SpeciesId:
                                species.SpeciesId,
                            RelatedSpeciesId:
                                parentSpeciesId,
                            Description:
                                $"S{species.SpeciesId} queda confirmada como descendiente de S{parentSpeciesId}. " +
                                $"Primera observación: ciclo {species.FirstDetectedCycle}."
                        )
                    );

                    AddEvent(
                        new SpeciesHistoryEvent(
                            Cycle:
                                cycle,
                            Type:
                                SpeciesHistoryEventType.DescendantConfirmed,
                            SpeciesId:
                                parentSpeciesId,
                            RelatedSpeciesId:
                                species.SpeciesId,
                            Description:
                                $"S{parentSpeciesId} registra una nueva especie descendiente: S{species.SpeciesId}."
                        )
                    );
                }
                else
                {
                    AddEvent(
                        new SpeciesHistoryEvent(
                            Cycle:
                                cycle,
                            Type:
                                SpeciesHistoryEventType.AncestralSpeciesEstablished,
                            SpeciesId:
                                species.SpeciesId,
                            RelatedSpeciesId:
                                null,
                            Description:
                                $"S{species.SpeciesId} queda registrada como especie ancestral."
                        )
                    );
                }
            }
            else if (
                history is not null
                &&
                !history.IsActive
            )
            {
                history.IsActive =
                    true;

                history.EndCycle =
                    null;

                AddEvent(
                    new SpeciesHistoryEvent(
                        Cycle:
                            cycle,
                        Type:
                            SpeciesHistoryEventType.Reactivated,
                        SpeciesId:
                            species.SpeciesId,
                        RelatedSpeciesId:
                            null,
                        Description:
                            $"S{species.SpeciesId} vuelve a aparecer como especie confirmada."
                    )
                );
            }

            if (history is null)
            {
                continue;
            }

            SpeciesHistoryObservation observation =
                new(
                    Cycle:
                        cycle,
                    Population:
                        species.Population,
                    AverageDistanceToCentroid:
                        species.AverageDistanceToCentroid,
                    Speed:
                        species.Centroid.Speed,
                    Size:
                        species.Centroid.Size,
                    Metabolism:
                        species.Centroid.Metabolism,
                    OptimalTemperature:
                        species.Centroid.OptimalTemperature,
                    ThermalTolerance:
                        species.Centroid.ThermalTolerance,
                    MeatAdaptation:
                        species.Centroid.MeatAdaptation,
                    PredatoryDrive:
                        species.Centroid.PredatoryDrive,
                    ExplorationDrive:
                        species.Centroid.ExplorationDrive,
                    RiskTolerance:
                        species.Centroid.RiskTolerance,
                    ScavengingDrive:
                        species.Centroid.ScavengingDrive,
                    MateSelectivity:
                        species.Centroid.MateSelectivity,
                    RegionPopulations:
                        new Dictionary<int, int>(
                            species.RegionPopulations
                        )
                );

            history.AddObservation(
                observation
            );
        }

        _activeSpeciesIds.Clear();

        foreach (
            int currentSpeciesId
            in currentSpeciesIds
        )
        {
            _activeSpeciesIds.Add(
                currentSpeciesId
            );
        }
    }

    public void RecordSimulationExtinction(
        int cycle)
    {
        if (
            _activeSpeciesIds.Count == 0
        )
        {
            return;
        }

        foreach (
            int speciesId
            in _activeSpeciesIds
                .OrderBy(
                    value =>
                        value
                )
                .ToList()
        )
        {
            if (
                !_histories.TryGetValue(
                    speciesId,
                    out MutableSpeciesHistory? history
                )
            )
            {
                continue;
            }

            history.IsActive =
                false;

            history.EndCycle =
                cycle;

            AddEvent(
                new SpeciesHistoryEvent(
                    Cycle:
                        cycle,
                    Type:
                        SpeciesHistoryEventType.SimulationExtinction,
                    SpeciesId:
                        speciesId,
                    RelatedSpeciesId:
                        null,
                    Description:
                        $"S{speciesId} deja de existir al extinguirse la población global."
                )
            );
        }

        _activeSpeciesIds.Clear();
    }

    public IReadOnlyList<SpeciesHistoryRecord>
        GetHistories()
    {
        return _histories
            .Values
            .OrderBy(
                history =>
                    history.SpeciesId
            )
            .Select(
                history =>
                    history.ToRecord()
            )
            .ToList();
    }

    public IReadOnlyList<SpeciesHistoryEvent>
        GetEvents()
    {
        return _events
            .OrderBy(
                historyEvent =>
                    historyEvent.Cycle
            )
            .ThenBy(
                historyEvent =>
                    historyEvent.SpeciesId
            )
            .ThenBy(
                historyEvent =>
                    historyEvent.Type
            )
            .ToList();
    }

    private void AddEvent(
        SpeciesHistoryEvent historyEvent)
    {
        _events.Add(
            historyEvent
        );

        if (
            _histories.TryGetValue(
                historyEvent.SpeciesId,
                out MutableSpeciesHistory? history
            )
        )
        {
            history.Events.Add(
                historyEvent
            );
        }
    }

    private sealed class MutableSpeciesHistory
    {
        public int SpeciesId
        {
            get;
        }

        public int? ParentSpeciesId
        {
            get;
        }

        public int FirstObservedCycle
        {
            get;
        }

        public int ConfirmedCycle
        {
            get;
        }

        public bool IsActive
        {
            get;
            set;
        } = true;

        public int? EndCycle
        {
            get;
            set;
        }

        public int PeakPopulation
        {
            get;
            private set;
        }

        public int PeakPopulationCycle
        {
            get;
            private set;
        }

        public List<SpeciesHistoryObservation>
            Observations
        {
            get;
        } =
            new();

        public List<SpeciesHistoryEvent>
            Events
        {
            get;
        } =
            new();

        public MutableSpeciesHistory(
            int speciesId,
            int? parentSpeciesId,
            int firstObservedCycle,
            int confirmedCycle)
        {
            SpeciesId =
                speciesId;

            ParentSpeciesId =
                parentSpeciesId;

            FirstObservedCycle =
                firstObservedCycle;

            ConfirmedCycle =
                confirmedCycle;
        }

        public void AddObservation(
            SpeciesHistoryObservation observation)
        {
            Observations.Add(
                observation
            );

            if (
                observation.Population
                >
                PeakPopulation
            )
            {
                PeakPopulation =
                    observation.Population;

                PeakPopulationCycle =
                    observation.Cycle;
            }
        }

        public SpeciesHistoryRecord ToRecord()
        {
            SpeciesHistoryObservation?
                latestObservation =
                    Observations
                        .LastOrDefault();

            return new SpeciesHistoryRecord(
                SpeciesId:
                    SpeciesId,
                ParentSpeciesId:
                    ParentSpeciesId,
                FirstObservedCycle:
                    FirstObservedCycle,
                ConfirmedCycle:
                    ConfirmedCycle,
                LastConfirmedCycle:
                    latestObservation?.Cycle
                    ??
                    ConfirmedCycle,
                EndCycle:
                    EndCycle,
                IsActive:
                    IsActive,
                DetectionCount:
                    Observations.Count,
                LatestPopulation:
                    latestObservation?.Population
                    ??
                    0,
                PeakPopulation:
                    PeakPopulation,
                PeakPopulationCycle:
                    PeakPopulationCycle,
                LatestAverageDistanceToCentroid:
                    latestObservation
                        ?.AverageDistanceToCentroid
                    ??
                    0,
                LatestSpeed:
                    latestObservation?.Speed
                    ??
                    0,
                LatestSize:
                    latestObservation?.Size
                    ??
                    0,
                LatestMetabolism:
                    latestObservation?.Metabolism
                    ??
                    0,
                LatestOptimalTemperature:
                    latestObservation
                        ?.OptimalTemperature
                    ??
                    0,
                LatestThermalTolerance:
                    latestObservation
                        ?.ThermalTolerance
                    ??
                    0,
                LatestMeatAdaptation:
                    latestObservation
                        ?.MeatAdaptation
                    ??
                    0,
                LatestPredatoryDrive:
                    latestObservation
                        ?.PredatoryDrive
                    ??
                    0,
                LatestExplorationDrive:
                    latestObservation
                        ?.ExplorationDrive
                    ??
                    0,
                LatestRiskTolerance:
                    latestObservation
                        ?.RiskTolerance
                    ??
                    0,
                LatestScavengingDrive:
                    latestObservation
                        ?.ScavengingDrive
                    ??
                    0,
                LatestMateSelectivity:
                    latestObservation
                        ?.MateSelectivity
                    ??
                    0,
                Observations:
                    Observations.ToList(),
                Events:
                    Events.ToList()
            );
        }
    }
}

public enum SpeciesHistoryEventType
{
    AncestralSpeciesEstablished,
    SpeciesConfirmed,
    DescendantConfirmed,
    NoLongerDetected,
    Reactivated,
    SimulationExtinction
}

public sealed record SpeciesHistoryEvent(
    int Cycle,
    SpeciesHistoryEventType Type,
    int SpeciesId,
    int? RelatedSpeciesId,
    string Description
);

public sealed record SpeciesHistoryObservation(
    int Cycle,
    int Population,
    double AverageDistanceToCentroid,
    double Speed,
    double Size,
    double Metabolism,
    double OptimalTemperature,
    double ThermalTolerance,
    double MeatAdaptation,
    double PredatoryDrive,
    double ExplorationDrive,
    double RiskTolerance,
    double ScavengingDrive,
    double MateSelectivity,
    IReadOnlyDictionary<int, int> RegionPopulations
);

public sealed record SpeciesHistoryRecord(
    int SpeciesId,
    int? ParentSpeciesId,
    int FirstObservedCycle,
    int ConfirmedCycle,
    int LastConfirmedCycle,
    int? EndCycle,
    bool IsActive,
    int DetectionCount,
    int LatestPopulation,
    int PeakPopulation,
    int PeakPopulationCycle,
    double LatestAverageDistanceToCentroid,
    double LatestSpeed,
    double LatestSize,
    double LatestMetabolism,
    double LatestOptimalTemperature,
    double LatestThermalTolerance,
    double LatestMeatAdaptation,
    double LatestPredatoryDrive,
    double LatestExplorationDrive,
    double LatestRiskTolerance,
    double LatestScavengingDrive,
    double LatestMateSelectivity,
    IReadOnlyList<SpeciesHistoryObservation> Observations,
    IReadOnlyList<SpeciesHistoryEvent> Events
);
