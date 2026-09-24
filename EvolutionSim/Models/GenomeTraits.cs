namespace EvolutionSim.Models;

/// <summary>
/// IDs canónicos de los 11 traits actualmente validados.
///
/// Mantener estos IDs estables permite que el resto del simulador siga
/// trabajando mientras la representación interna del genoma evoluciona.
/// </summary>
public static class GenomeTraits
{
    public static readonly GenomeTraitId Speed =
        new("Speed");

    public static readonly GenomeTraitId Size =
        new("Size");

    public static readonly GenomeTraitId Metabolism =
        new("Metabolism");

    public static readonly GenomeTraitId OptimalTemperature =
        new("OptimalTemperature");

    public static readonly GenomeTraitId ThermalTolerance =
        new("ThermalTolerance");

    public static readonly GenomeTraitId MeatAdaptation =
        new("MeatAdaptation");

    public static readonly GenomeTraitId PredatoryDrive =
        new("PredatoryDrive");

    public static readonly GenomeTraitId ExplorationDrive =
        new("ExplorationDrive");

    public static readonly GenomeTraitId RiskTolerance =
        new("RiskTolerance");

    public static readonly GenomeTraitId ScavengingDrive =
        new("ScavengingDrive");

    public static readonly GenomeTraitId MateSelectivity =
        new("MateSelectivity");
}
