namespace EvolutionSim.Models;

/// <summary>
/// Canales morfológicos estructurales introducidos en la Fase 8.1.
///
/// En esta fase son completamente PASIVOS:
/// describen la forma corporal, pero todavía no modifican la ecología.
///
/// El mapeo desde familias estructurales utiliza un hash independiente
/// del mapeo de PhenotypeChannel, por lo que no altera ningún resultado
/// validado durante la Fase 7.
/// </summary>
public enum MorphologyChannel
{
    LimbLength = 0,
    LimbRobustness = 1,
    Insulation = 2,
    JawStrength = 3,
    DigestiveStructure = 4
}
