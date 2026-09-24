namespace EvolutionSim.Simulation;

/// <summary>
/// Perfil fenotípico oficial al cierre de la Fase 7.
///
/// Esta clase concentra los couplings ecológicos normales validados
/// para evitar que el estado final del sistema quede repartido como
/// números mágicos en distintas partes del proyecto.
///
/// IMPORTANTE:
/// estos valores describen la simulación normal.
///
/// Los runners experimentales pueden sobrescribirlos mediante
/// SimulationConfig para reproducir estudios anteriores sin modificar
/// este perfil oficial.
/// </summary>
public static class PhenotypePhase7Profile
{
    /// <summary>
    /// MetabolicEfficiency quedó activo después del estudio multiseed.
    ///
    /// Modifier máximo:
    /// ±0.08
    ///
    /// Coupling:
    /// 0.125
    ///
    /// Efecto ecológico máximo:
    /// ±1 % sobre el costo energético total.
    /// </summary>
    public const double MetabolicEfficiencyCoupling =
        0.125;


    /// <summary>
    /// ThermalRegulation quedó activo después del estudio multiseed
    /// y de la corrección del scope diagnóstico.
    ///
    /// Efecto ecológico máximo:
    /// ±1 % sobre la diferencia térmica efectiva.
    /// </summary>
    public const double ThermalRegulationCoupling =
        0.125;


    /// <summary>
    /// BodySize permanece pasivo.
    ///
    /// Se estudió inicialmente sobre MaxEnergy con ±1 % y ±2 %,
    /// y posteriormente con ±0.25 % y ±0.50 %.
    ///
    /// Los estudios mostraron alta sensibilidad demográfica sin una
    /// señal reproductiva suficientemente consistente para activarlo.
    /// </summary>
    public const double BodySizeCoupling =
        0.0;


    /// <summary>
    /// Locomotion permanece pasivo.
    ///
    /// Se estudió sobre ActivityCost con ±1 % y ±2 %.
    ///
    /// Alteró las trayectorias demográficas, pero no apareció una
    /// señal reproductiva consistente hacia una mayor eficiencia.
    /// </summary>
    public const double LocomotionCoupling =
        0.0;


    /// <summary>
    /// FeedingSpecialization permanece pasivo.
    ///
    /// Se estudió sobre la energía vegetal utilizable con ±1 % y ±2 %.
    ///
    /// La respuesta demográfica fue sensible y la señal padres-vivos
    /// no apoyó una selección consistente hacia mayor asimilación.
    /// </summary>
    public const double FeedingSpecializationCoupling =
        0.0;


    /// <summary>
    /// Límite de expresión utilizado por los canales fenotípicos
    /// estructurales de la Fase 7.
    /// </summary>
    public const double MaximumStructuralModifierMagnitude =
        0.08;


    /// <summary>
    /// Devuelve true únicamente cuando la configuración representa
    /// exactamente el perfil fenotípico oficial de cierre de la Fase 7.
    ///
    /// Es un helper diagnóstico. No modifica la simulación.
    /// </summary>
    public static bool MatchesFinalProfile(
        SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(
            config
        );


        return
            NearlyEqual(
                config.MetabolicEfficiencyEcologicalCoupling,
                MetabolicEfficiencyCoupling
            )
            &&
            NearlyEqual(
                config.ThermalRegulationEcologicalCoupling,
                ThermalRegulationCoupling
            )
            &&
            NearlyEqual(
                config.BodySizeEcologicalCoupling,
                BodySizeCoupling
            )
            &&
            NearlyEqual(
                config.LocomotionEcologicalCoupling,
                LocomotionCoupling
            )
            &&
            NearlyEqual(
                config.FeedingSpecializationEcologicalCoupling,
                FeedingSpecializationCoupling
            );
    }


    private static bool NearlyEqual(
        double left,
        double right)
    {
        return Math.Abs(
            left
            -
            right
        )
        <
        1e-12;
    }
}
