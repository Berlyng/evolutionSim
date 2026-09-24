namespace EvolutionSim.World;

public sealed class Region
{
    private readonly Dictionary<int, Patch>
        _patches = new();


    public int Id { get; }

    public string Name { get; }

    public double BaseTemperature { get; }

    public double Temperature { get; private set; }


    public IEnumerable<Patch> Patches =>
        _patches.Values;


    public double TotalPlantBiomass =>
        _patches.Values.Sum(
            patch =>
                patch.Plants.Biomass
        );


    public double TotalPlantCarryingCapacity =>
        _patches.Values.Sum(
            patch =>
                patch.Plants.CarryingCapacity
        );


    public double PlantAvailability
    {
        get
        {
            if (
                TotalPlantCarryingCapacity
                <=
                0
            )
            {
                return 0;
            }


            return Math.Clamp(
                TotalPlantBiomass
                /
                TotalPlantCarryingCapacity,
                0,
                1
            );
        }
    }


    public Region(
        int id,
        string name,
        double temperature,
        IEnumerable<Patch> patches)
    {
        Id = id;

        Name = name;

        BaseTemperature = temperature;

        Temperature = temperature;


        foreach (
            Patch patch
            in patches
        )
        {
            if (
                patch.RegionId
                !=
                id
            )
            {
                throw new InvalidOperationException(
                    $"Patch {patch.Id} belongs to region " +
                    $"{patch.RegionId}, not region {id}."
                );
            }


            if (
                !_patches.TryAdd(
                    patch.Id,
                    patch
                )
            )
            {
                throw new InvalidOperationException(
                    $"Duplicate patch id {patch.Id} " +
                    $"inside region {id}."
                );
            }
        }


        if (
            _patches.Count == 0
        )
        {
            throw new InvalidOperationException(
                $"Region {id} must contain at least one patch."
            );
        }
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
                $"Patch {patchId} does not exist " +
                $"inside region {Id}."
            );
        }


        return patch;
    }


    public void SetTemperature(
        double temperature)
    {
        Temperature =
            Math.Clamp(
                temperature,
                -40,
                60
            );
    }
}