using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class SpeciesCluster
{
    public int SpeciesId { get; }

    public int? ParentSpeciesId { get; }

    public int FirstDetectedCycle { get; }

    public int Population { get; }

    public Genome Centroid { get; }

    public double AverageDistanceToCentroid
    {
        get;
    }

    public IReadOnlyDictionary<int, int>
        RegionPopulations
    {
        get;
    }


    public double AverageSpeed =>
        Centroid.Speed;


    public double AverageSize =>
        Centroid.Size;


    public double AverageMetabolism =>
        Centroid.Metabolism;


    public double AverageOptimalTemperature =>
        Centroid.OptimalTemperature;


    public double AverageThermalTolerance =>
        Centroid.ThermalTolerance;


    public double AverageMeatAdaptation =>
        Centroid.MeatAdaptation;


    public double AveragePredatoryDrive =>
        Centroid.PredatoryDrive;


    public SpeciesCluster(
        int speciesId,
        int? parentSpeciesId,
        int firstDetectedCycle,
        int population,
        Genome centroid,
        double averageDistanceToCentroid,
        Dictionary<int, int> regionPopulations)
    {
        SpeciesId =
            speciesId;

        ParentSpeciesId =
            parentSpeciesId;

        FirstDetectedCycle =
            firstDetectedCycle;

        Population =
            population;

        Centroid =
            centroid;

        AverageDistanceToCentroid =
            averageDistanceToCentroid;

        RegionPopulations =
            new Dictionary<int, int>(
                regionPopulations
            );
    }
}