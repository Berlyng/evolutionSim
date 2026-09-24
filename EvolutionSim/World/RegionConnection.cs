namespace EvolutionSim.World;

public sealed class RegionConnection
{
    public int RegionAId { get; }

    public int RegionBId { get; }

    //
    // Cuanto mayor sea, más difícil
    // es atravesar esta conexión.
    //
    public double MigrationDifficulty { get; }

    //
    // Energía base necesaria para cruzarla.
    //
    public double EnergyCost { get; }

    public RegionConnection(
        int regionAId,
        int regionBId,
        double migrationDifficulty,
        double energyCost)
    {
        if (regionAId == regionBId)
        {
            throw new ArgumentException(
                "A region cannot connect to itself."
            );
        }

        RegionAId =
            regionAId;

        RegionBId =
            regionBId;

        MigrationDifficulty =
            Math.Max(
                0.1,
                migrationDifficulty
            );

        EnergyCost =
            Math.Max(
                0,
                energyCost
            );
    }

    public bool ContainsRegion(
        int regionId)
    {
        return
            RegionAId == regionId
            ||
            RegionBId == regionId;
    }

    public int GetOtherRegionId(
        int regionId)
    {
        if (regionId == RegionAId)
        {
            return RegionBId;
        }

        if (regionId == RegionBId)
        {
            return RegionAId;
        }

        throw new InvalidOperationException(
            $"Region {regionId} does not belong to this connection."
        );
    }
}