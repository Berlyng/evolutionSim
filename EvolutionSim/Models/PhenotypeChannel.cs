namespace EvolutionSim.Models;

/// <summary>
/// Canales fenotípicos agregados derivados de loci estructurales.
///
/// Phase 7.3 solamente calcula estos modificadores.
/// Todavía NO alteran la ecología del organismo.
/// </summary>
public enum PhenotypeChannel
{
    ThermalRegulation = 0,
    BodySize = 1,
    MetabolicEfficiency = 2,
    Locomotion = 3,
    FeedingSpecialization = 4
}
