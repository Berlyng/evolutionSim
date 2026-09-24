namespace EvolutionSim.Evolution;

/// <summary>
/// Construye una representación filogenética pasiva a partir del historial
/// confirmado de especies.
///
/// No utiliza Random, no modifica especies y no participa en detección.
/// </summary>
public sealed class PhylogenyBuilder
{
    public PhylogenySnapshot Build(
        int currentCycle,
        IReadOnlyList<SpeciesHistoryRecord> histories)
    {
        ArgumentNullException.ThrowIfNull(
            histories
        );

        Dictionary<int, SpeciesHistoryRecord>
            historyById =
                histories.ToDictionary(
                    history =>
                        history.SpeciesId
                );

        Dictionary<int, int>
            depthBySpeciesId =
                new();

        int CalculateDepth(
            int speciesId,
            HashSet<int>? visiting = null)
        {
            if (
                depthBySpeciesId.TryGetValue(
                    speciesId,
                    out int cachedDepth
                )
            )
            {
                return cachedDepth;
            }

            if (
                !historyById.TryGetValue(
                    speciesId,
                    out SpeciesHistoryRecord? history
                )
            )
            {
                return 0;
            }

            visiting ??=
                new HashSet<int>();

            if (
                !visiting.Add(
                    speciesId
                )
            )
            {
                // Protección contra una relación parental corrupta/cíclica.
                return 0;
            }

            int depth =
                history.ParentSpeciesId
                is int parentSpeciesId
                &&
                historyById.ContainsKey(
                    parentSpeciesId
                )
                    ? CalculateDepth(
                        parentSpeciesId,
                        visiting
                    )
                    + 1
                    : 0;

            visiting.Remove(
                speciesId
            );

            depthBySpeciesId[
                speciesId
            ] =
                depth;

            return depth;
        }

        List<PhylogenyNode> nodes =
            histories
                .OrderBy(
                    history =>
                        history.ConfirmedCycle
                )
                .ThenBy(
                    history =>
                        history.SpeciesId
                )
                .Select(
                    history =>
                    {
                        int directDescendantCount =
                            histories.Count(
                                candidate =>
                                    candidate.ParentSpeciesId
                                    ==
                                    history.SpeciesId
                            );

                        return new PhylogenyNode(
                            SpeciesId:
                                history.SpeciesId,
                            ParentSpeciesId:
                                history.ParentSpeciesId,
                            Depth:
                                CalculateDepth(
                                    history.SpeciesId
                                ),
                            FirstObservedCycle:
                                history.FirstObservedCycle,
                            ConfirmedCycle:
                                history.ConfirmedCycle,
                            LastConfirmedCycle:
                                history.LastConfirmedCycle,
                            EndCycle:
                                history.EndCycle,
                            IsActive:
                                history.IsActive,
                            LatestPopulation:
                                history.LatestPopulation,
                            PeakPopulation:
                                history.PeakPopulation,
                            DirectDescendantCount:
                                directDescendantCount
                        );
                    }
                )
                .ToList();

        List<PhylogenyEdge> edges =
            nodes
                .Where(
                    node =>
                        node.ParentSpeciesId
                        is not null
                )
                .Select(
                    node =>
                        new PhylogenyEdge(
                            ParentSpeciesId:
                                node.ParentSpeciesId!.Value,
                            ChildSpeciesId:
                                node.SpeciesId,
                            BranchCycle:
                                node.ConfirmedCycle
                        )
                )
                .OrderBy(
                    edge =>
                        edge.BranchCycle
                )
                .ThenBy(
                    edge =>
                        edge.ParentSpeciesId
                )
                .ThenBy(
                    edge =>
                        edge.ChildSpeciesId
                )
                .ToList();

        IReadOnlyList<int> rootSpeciesIds =
            nodes
                .Where(
                    node =>
                        node.ParentSpeciesId
                        is null
                        ||
                        !historyById.ContainsKey(
                            node.ParentSpeciesId.Value
                        )
                )
                .Select(
                    node =>
                        node.SpeciesId
                )
                .OrderBy(
                    speciesId =>
                        speciesId
                )
                .ToList();

        return new PhylogenySnapshot(
            CurrentCycle:
                currentCycle,
            MaximumDepth:
                nodes.Count == 0
                    ? 0
                    : nodes.Max(
                        node =>
                            node.Depth
                    ),
            RootSpeciesIds:
                rootSpeciesIds,
            Nodes:
                nodes,
            Edges:
                edges
        );
    }
}

public sealed record PhylogenySnapshot(
    int CurrentCycle,
    int MaximumDepth,
    IReadOnlyList<int> RootSpeciesIds,
    IReadOnlyList<PhylogenyNode> Nodes,
    IReadOnlyList<PhylogenyEdge> Edges
);

public sealed record PhylogenyNode(
    int SpeciesId,
    int? ParentSpeciesId,
    int Depth,
    int FirstObservedCycle,
    int ConfirmedCycle,
    int LastConfirmedCycle,
    int? EndCycle,
    bool IsActive,
    int LatestPopulation,
    int PeakPopulation,
    int DirectDescendantCount
);

public sealed record PhylogenyEdge(
    int ParentSpeciesId,
    int ChildSpeciesId,
    int BranchCycle
);
