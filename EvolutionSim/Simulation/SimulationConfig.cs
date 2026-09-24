namespace EvolutionSim.Simulation;

public sealed record SimulationConfig(
    int RandomSeed,
    int InitialPopulation,
    int Cycles,
    double InitialOrganismEnergy,
    double EnergyPerFoodUnit,
    double ReproductionThreshold,
    double ReproductionCostPerParent,
    double ChildInitialEnergy,
    int SpeciesDetectionInterval,
    double MetabolicEfficiencyEcologicalCoupling,
    bool FastExperimentMode)
{
    /// <summary>
    /// Coupling ecológico de ThermalRegulation.
    ///
    /// Se mantiene fuera del constructor posicional para no romper
    /// llamadas existentes a SimulationConfig.
    ///
    /// Fase 7.6.2:
    /// 0.125 queda congelado como valor normal validado.
    ///
    /// 0.000 = control/pasivo.
    /// 0.125 = modifier ±0.08 produce un efecto térmico máximo de ±1 %.
    /// 0.250 = modifier ±0.08 produce un efecto térmico máximo de ±2 %.
    /// </summary>
    public double ThermalRegulationEcologicalCoupling
    {
        get;
        init;
    } = 0.125;


    /// <summary>
    /// Coupling ecológico experimental de BodySize.
    ///
    /// Fase 7.7:
    /// BodySize permanece en 0.0 en la simulación normal hasta completar
    /// el estudio emparejado.
    ///
    /// 0.000 = control/pasivo.
    /// 0.125 = modifier ±0.08 produce un efecto máximo de ±1 % sobre MaxEnergy.
    /// 0.250 = modifier ±0.08 produce un efecto máximo de ±2 % sobre MaxEnergy.
    /// </summary>
    public double BodySizeEcologicalCoupling
    {
        get;
        init;
    } = 0.0;


    /// <summary>
    /// Coupling ecológico experimental de Locomotion.
    ///
    /// Fase 7.8:
    /// Locomotion permanece en 0.0 en la simulación normal hasta completar
    /// el estudio emparejado.
    ///
    /// 0.000 = control/pasivo.
    /// 0.125 = modifier ±0.08 produce un efecto máximo de ±1 % sobre ActivityCost.
    /// 0.250 = modifier ±0.08 produce un efecto máximo de ±2 % sobre ActivityCost.
    /// </summary>
    public double LocomotionEcologicalCoupling
    {
        get;
        init;
    } = 0.0;


    public static SimulationConfig Default { get; } =
        new(
            RandomSeed:
                98765,

            InitialPopulation:
                1000,

            Cycles:
                1600,

            InitialOrganismEnergy:
                50,

            EnergyPerFoodUnit:
                20,

            ReproductionThreshold:
                90,

            ReproductionCostPerParent:
                25,

            ChildInitialEnergy:
                30,

            SpeciesDetectionInterval:
                25,

            // Phase 7.5:
            // MetabolicEfficiency queda activo de forma conservadora.
            // Phenotype modifier ±0.08 × 0.125
            // => efecto ecológico máximo ±1 %.
            MetabolicEfficiencyEcologicalCoupling:
                0.125,

            // false = simulación normal con todos los diagnósticos.
            // true  = las corridas experimentales omiten telemetría
            //         pesada en ciclos intermedios.
            FastExperimentMode:
                false
        );
}
