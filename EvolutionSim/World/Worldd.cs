namespace EvolutionSim.World;

public class Worldd
{
    private readonly Dictionary<int, Region>
        _regions = new();


    private readonly Dictionary<int, Patch>
        _patches = new();


    private readonly List<RegionConnection>
        _connections = new();


    private readonly List<PatchConnection>
        _patchConnections = new();


    public IEnumerable<Region> Regions =>
        _regions.Values;


    public IEnumerable<Patch> Patches =>
        _patches.Values;


    public IReadOnlyCollection<RegionConnection>
        Connections =>
            _connections;


    public IReadOnlyCollection<PatchConnection>
        PatchConnections =>
            _patchConnections;


    public int Cycle { get; private set; }


    public Worldd(
        IEnumerable<Region> regions)
    {
        foreach (
            Region region
            in regions
        )
        {
            if (
                !_regions.TryAdd(
                    region.Id,
                    region
                )
            )
            {
                throw new InvalidOperationException(
                    $"Duplicate region id: {region.Id}"
                );
            }


            foreach (
                Patch patch
                in region.Patches
            )
            {
                if (
                    !_patches.TryAdd(
                        patch.Id,
                        patch
                    )
                )
                {
                    throw new InvalidOperationException(
                        $"Duplicate global patch id: {patch.Id}"
                    );
                }
            }
        }
    }


    public Region GetRegion(
        int regionId)
    {
        if (
            !_regions.TryGetValue(
                regionId,
                out Region? region
            )
        )
        {
            throw new InvalidOperationException(
                $"Region {regionId} does not exist."
            );
        }


        return region;
    }


    public Patch GetPatch(
        int patchId)
    {
        if (
            !_patches.TryGetValue(
                patchId,
                out Patch? patch
            )
        )
        {
            throw new InvalidOperationException(
                $"Patch {patchId} does not exist."
            );
        }


        return patch;
    }


    public void ConnectRegions(
        int regionAId,
        int regionBId,
        double migrationDifficulty = 1,
        double energyCost = 3)
    {
        GetRegion(
            regionAId
        );

        GetRegion(
            regionBId
        );


        bool alreadyConnected =
            _connections.Any(
                connection =>
                    connection.ContainsRegion(
                        regionAId
                    )
                    &&
                    connection.ContainsRegion(
                        regionBId
                    )
            );


        if (
            alreadyConnected
        )
        {
            return;
        }


        RegionConnection connection =
            new(
                regionAId,
                regionBId,
                migrationDifficulty,
                energyCost
            );


        _connections.Add(
            connection
        );
    }


    public void ConnectPatches(
        int patchAId,
        int patchBId,
        double movementDifficulty = 1,
        double energyCost = 1)
    {
        Patch patchA =
            GetPatch(
                patchAId
            );


        Patch patchB =
            GetPatch(
                patchBId
            );


        if (
            patchA.RegionId
            !=
            patchB.RegionId
        )
        {
            throw new InvalidOperationException(
                "Local patch connections must belong " +
                "to the same region."
            );
        }


        bool alreadyConnected =
            _patchConnections.Any(
                connection =>
                    connection.ContainsPatch(
                        patchAId
                    )
                    &&
                    connection.ContainsPatch(
                        patchBId
                    )
            );


        if (
            alreadyConnected
        )
        {
            return;
        }


        PatchConnection connection =
            new(
                patchAId,
                patchBId,
                movementDifficulty,
                energyCost
            );


        _patchConnections.Add(
            connection
        );
    }


    public List<RegionConnection> GetConnections(
        int regionId)
    {
        return
            _connections
                .Where(
                    connection =>
                        connection.ContainsRegion(
                            regionId
                        )
                )
                .ToList();
    }


    public List<PatchConnection> GetPatchConnections(
        int patchId)
    {
        return
            _patchConnections
                .Where(
                    connection =>
                        connection.ContainsPatch(
                            patchId
                        )
                )
                .ToList();
    }


    public void AdvanceCycle()
    {
        Cycle++;


        foreach (
            Patch patch
            in _patches.Values
        )
        {
            patch.Plants.Grow();
        }
    }
}