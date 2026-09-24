namespace EvolutionSim.World;

public sealed class Patch
{
    public int Id { get; }

    public int RegionId { get; }

    public string Name { get; }

    public PlantPopulation Plants { get; }


    // =========================================================
    // MICROCLIMA
    // =========================================================
    //
    // Diferencia permanente respecto a la
    // temperatura regional.
    //
    // Ejemplo:
    //
    // Temperatura regional = 25 C
    // TemperatureOffset    = -1 C
    //
    // Temperatura local    = 24 C
    //

    public double TemperatureOffset { get; }


    public Patch(
        int id,
        int regionId,
        string name,
        PlantPopulation plants,
        double temperatureOffset = 0)
    {
        Id =
            id;

        RegionId =
            regionId;

        Name =
            name;

        Plants =
            plants;

        TemperatureOffset =
            temperatureOffset;
    }


    // =========================================================
    // TEMPERATURA LOCAL
    // =========================================================

    public double GetLocalTemperature(
        double regionalTemperature)
    {
        return
            regionalTemperature
            +
            TemperatureOffset;
    }
}