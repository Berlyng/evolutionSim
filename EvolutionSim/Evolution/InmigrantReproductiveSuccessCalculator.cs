using EvolutionSim.Ecology;
using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;


public sealed record ImmigrantParentOriginStatistics(
    string OriginRegionName,
    int UniqueParentCount,
    int BirthsInvolvingOrigin);


public sealed record ImmigrantReproductiveRegionStatistics(
    int RegionId,
    string RegionName,
    int TrackedImmigrantsAlive,
    int TrackedImmigrantCandidates,
    int UniqueImmigrantParents,
    int BirthsWithImmigrantParent,
    int ResidentResidentBirths,
    int ResidentImmigrantBirths,
    int ImmigrantImmigrantBirths,
    int SameCycleArrivals,
    int SameCycleImmigrantCandidates,
    int SameCycleImmigrantParents,
    double AverageImmigrantParentOptimalTemperature,
    double AverageResidentParentOptimalTemperature,
    int UnresolvedBirths,
    IReadOnlyList<ImmigrantParentOriginStatistics> ParentOrigins);


public sealed class ImmigrantReproductiveSuccessCalculator
{
    private sealed record TrackedImmigrantState(
        Guid OrganismId,
        int OriginRegionId,
        string OriginRegionName,
        int DestinationRegionId,
        string DestinationRegionName,
        int ArrivalCycle);


    private readonly Dictionary<Guid, TrackedImmigrantState>
        _trackedImmigrants =
            new();


    // =========================================================
    // REGISTRAR MIGRACIONES DEL CICLO
    // =========================================================
    //
    // Si un organismo vuelve a migrar más adelante, su estado
    // se reemplaza por la migración más reciente. De este modo
    // "inmigrante" siempre significa: organismo cuya última
    // migración entre regiones terminó en la región actual.
    //
    // Este diagnóstico no consume Random y no altera el estado
    // evolutivo del organismo.
    //

    public void RegisterMigrations(
        int cycle,
        IReadOnlyList<MigrationEventStatistics> migrationEvents)
    {
        foreach (
            MigrationEventStatistics migrationEvent
            in migrationEvents
        )
        {
            _trackedImmigrants[
                migrationEvent.OrganismId
            ] =
                new TrackedImmigrantState(
                    OrganismId:
                        migrationEvent.OrganismId,

                    OriginRegionId:
                        migrationEvent.OriginRegionId,

                    OriginRegionName:
                        migrationEvent.OriginRegionName,

                    DestinationRegionId:
                        migrationEvent.DestinationRegionId,

                    DestinationRegionName:
                        migrationEvent.DestinationRegionName,

                    ArrivalCycle:
                        cycle
                );
        }
    }


    // =========================================================
    // ÉXITO REPRODUCTIVO DE INMIGRANTES
    // =========================================================

    public List<ImmigrantReproductiveRegionStatistics> Calculate(
        int cycle,
        List<Organism> newborns,
        List<Organism> parentPopulation,
        Worldd world,
        IReadOnlyDictionary<int, HashSet<Guid>>
            reproductiveCandidateIdsByRegion)
    {
        Dictionary<Guid, Organism> parentById =
            parentPopulation
                .ToDictionary(
                    organism => organism.Id
                );


        List<ImmigrantReproductiveRegionStatistics> results =
            new();


        foreach (
            Region region
            in world.Regions
        )
        {
            List<Organism> regionalNewborns =
                newborns
                    .Where(
                        newborn =>
                            newborn.RegionId
                            ==
                            region.Id
                    )
                    .ToList();


            List<Organism> trackedImmigrantsAlive =
                parentPopulation
                    .Where(
                        organism =>
                            organism.IsAlive
                            &&
                            organism.RegionId
                            ==
                            region.Id
                            &&
                            IsCurrentImmigrant(
                                organism.Id,
                                region.Id
                            )
                    )
                    .ToList();


            HashSet<Guid> regionalCandidateIds =
                reproductiveCandidateIdsByRegion
                    .TryGetValue(
                        region.Id,
                        out HashSet<Guid>? candidateIds
                    )

                    ?

                    candidateIds

                    :

                    new HashSet<Guid>();


            HashSet<Guid> trackedImmigrantCandidateIds =
                regionalCandidateIds
                    .Where(
                        organismId =>
                            IsCurrentImmigrant(
                                organismId,
                                region.Id
                            )
                    )
                    .ToHashSet();


            HashSet<Guid> immigrantParentIds =
                new();


            HashSet<Guid> residentParentIds =
                new();


            Dictionary<string, HashSet<Guid>>
                immigrantParentIdsByOrigin =
                    new();


            Dictionary<string, int>
                birthsByImmigrantOrigin =
                    new();


            int birthsWithImmigrantParent =
                0;

            int residentResidentBirths =
                0;

            int residentImmigrantBirths =
                0;

            int immigrantImmigrantBirths =
                0;

            int unresolvedBirths =
                0;


            foreach (
                Organism newborn
                in regionalNewborns
            )
            {
                if (
                    !newborn.ParentAId.HasValue
                    ||
                    !newborn.ParentBId.HasValue
                )
                {
                    unresolvedBirths++;
                    continue;
                }


                Guid parentAId =
                    newborn.ParentAId.Value;

                Guid parentBId =
                    newborn.ParentBId.Value;


                if (
                    !parentById.TryGetValue(
                        parentAId,
                        out Organism? parentA
                    )
                    ||
                    !parentById.TryGetValue(
                        parentBId,
                        out Organism? parentB
                    )
                )
                {
                    unresolvedBirths++;
                    continue;
                }


                bool parentAIsImmigrant =
                    IsCurrentImmigrant(
                        parentAId,
                        region.Id
                    );

                bool parentBIsImmigrant =
                    IsCurrentImmigrant(
                        parentBId,
                        region.Id
                    );


                if (
                    parentAIsImmigrant
                    ||
                    parentBIsImmigrant
                )
                {
                    birthsWithImmigrantParent++;
                }


                if (
                    parentAIsImmigrant
                    &&
                    parentBIsImmigrant
                )
                {
                    immigrantImmigrantBirths++;
                }
                else if (
                    parentAIsImmigrant
                    ||
                    parentBIsImmigrant
                )
                {
                    residentImmigrantBirths++;
                }
                else
                {
                    residentResidentBirths++;
                }


                RegisterParentClassification(
                    parentA,
                    parentAIsImmigrant,
                    immigrantParentIds,
                    residentParentIds,
                    immigrantParentIdsByOrigin
                );


                RegisterParentClassification(
                    parentB,
                    parentBIsImmigrant,
                    immigrantParentIds,
                    residentParentIds,
                    immigrantParentIdsByOrigin
                );


                HashSet<string> immigrantOriginsInBirth =
                    new();


                if (
                    parentAIsImmigrant
                    &&
                    _trackedImmigrants.TryGetValue(
                        parentAId,
                        out TrackedImmigrantState? parentAState
                    )
                )
                {
                    immigrantOriginsInBirth.Add(
                        parentAState.OriginRegionName
                    );
                }


                if (
                    parentBIsImmigrant
                    &&
                    _trackedImmigrants.TryGetValue(
                        parentBId,
                        out TrackedImmigrantState? parentBState
                    )
                )
                {
                    immigrantOriginsInBirth.Add(
                        parentBState.OriginRegionName
                    );
                }


                foreach (
                    string originName
                    in immigrantOriginsInBirth
                )
                {
                    birthsByImmigrantOrigin.TryGetValue(
                        originName,
                        out int currentBirthCount
                    );


                    birthsByImmigrantOrigin[
                        originName
                    ] =
                        currentBirthCount + 1;
                }
            }


            int sameCycleArrivals =
                _trackedImmigrants.Values
                    .Count(
                        state =>
                            state.DestinationRegionId
                            ==
                            region.Id
                            &&
                            state.ArrivalCycle
                            ==
                            cycle
                    );


            int sameCycleImmigrantCandidates =
                trackedImmigrantCandidateIds
                    .Count(
                        organismId =>
                            _trackedImmigrants[
                                organismId
                            ].ArrivalCycle
                            ==
                            cycle
                    );


            int sameCycleImmigrantParents =
                immigrantParentIds
                    .Count(
                        organismId =>
                            _trackedImmigrants[
                                organismId
                            ].ArrivalCycle
                            ==
                            cycle
                    );


            double averageImmigrantParentOptimalTemperature =
                immigrantParentIds.Count > 0

                    ?

                    immigrantParentIds.Average(
                        organismId =>
                            parentById[
                                organismId
                            ].OptimalTemperature
                    )

                    :

                    0;


            double averageResidentParentOptimalTemperature =
                residentParentIds.Count > 0

                    ?

                    residentParentIds.Average(
                        organismId =>
                            parentById[
                                organismId
                            ].OptimalTemperature
                    )

                    :

                    0;


            List<ImmigrantParentOriginStatistics> parentOrigins =
                immigrantParentIdsByOrigin
                    .Select(
                        entry =>
                            new ImmigrantParentOriginStatistics(
                                OriginRegionName:
                                    entry.Key,

                                UniqueParentCount:
                                    entry.Value.Count,

                                BirthsInvolvingOrigin:
                                    birthsByImmigrantOrigin
                                        .TryGetValue(
                                            entry.Key,
                                            out int birthCount
                                        )

                                        ?

                                        birthCount

                                        :

                                        0
                            )
                    )
                    .OrderByDescending(
                        stats =>
                            stats.UniqueParentCount
                    )
                    .ThenBy(
                        stats =>
                            stats.OriginRegionName
                    )
                    .ToList();


            results.Add(
                new ImmigrantReproductiveRegionStatistics(
                    RegionId:
                        region.Id,

                    RegionName:
                        region.Name,

                    TrackedImmigrantsAlive:
                        trackedImmigrantsAlive.Count,

                    TrackedImmigrantCandidates:
                        trackedImmigrantCandidateIds.Count,

                    UniqueImmigrantParents:
                        immigrantParentIds.Count,

                    BirthsWithImmigrantParent:
                        birthsWithImmigrantParent,

                    ResidentResidentBirths:
                        residentResidentBirths,

                    ResidentImmigrantBirths:
                        residentImmigrantBirths,

                    ImmigrantImmigrantBirths:
                        immigrantImmigrantBirths,

                    SameCycleArrivals:
                        sameCycleArrivals,

                    SameCycleImmigrantCandidates:
                        sameCycleImmigrantCandidates,

                    SameCycleImmigrantParents:
                        sameCycleImmigrantParents,

                    AverageImmigrantParentOptimalTemperature:
                        averageImmigrantParentOptimalTemperature,

                    AverageResidentParentOptimalTemperature:
                        averageResidentParentOptimalTemperature,

                    UnresolvedBirths:
                        unresolvedBirths,

                    ParentOrigins:
                        parentOrigins
                )
            );
        }


        // =====================================================
        // LIMPIAR MIGRANTES QUE YA MURIERON
        // =====================================================
        //
        // Se hace DESPUÉS del cálculo para que un organismo que
        // haya producido descendencia y haya quedado sin energía
        // por el costo reproductivo todavía pueda clasificarse
        // correctamente en este mismo ciclo.
        //

        HashSet<Guid> livingIds =
            parentPopulation
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .Select(
                    organism =>
                        organism.Id
                )
                .ToHashSet();


        foreach (
            Guid trackedId
            in _trackedImmigrants.Keys.ToList()
        )
        {
            if (
                !livingIds.Contains(
                    trackedId
                )
            )
            {
                _trackedImmigrants.Remove(
                    trackedId
                );
            }
        }


        return results;
    }


    private bool IsCurrentImmigrant(
        Guid organismId,
        int regionId)
    {
        return
            _trackedImmigrants.TryGetValue(
                organismId,
                out TrackedImmigrantState? state
            )
            &&
            state.DestinationRegionId
            ==
            regionId;
    }


    private void RegisterParentClassification(
        Organism parent,
        bool isImmigrant,
        HashSet<Guid> immigrantParentIds,
        HashSet<Guid> residentParentIds,
        Dictionary<string, HashSet<Guid>>
            immigrantParentIdsByOrigin)
    {
        if (
            !isImmigrant
        )
        {
            residentParentIds.Add(
                parent.Id
            );

            return;
        }


        immigrantParentIds.Add(
            parent.Id
        );


        if (
            !_trackedImmigrants.TryGetValue(
                parent.Id,
                out TrackedImmigrantState? state
            )
        )
        {
            return;
        }


        if (
            !immigrantParentIdsByOrigin.TryGetValue(
                state.OriginRegionName,
                out HashSet<Guid>? originParentIds
            )
        )
        {
            originParentIds =
                new HashSet<Guid>();


            immigrantParentIdsByOrigin[
                state.OriginRegionName
            ] =
                originParentIds;
        }


        originParentIds.Add(
            parent.Id
        );
    }
}
