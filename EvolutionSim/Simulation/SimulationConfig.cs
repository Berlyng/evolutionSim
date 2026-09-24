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
    /// Fase 7.10:
    /// queda oficialmente activo con el valor validado en 7.6.2.
    ///
    /// Los experimentos pueden sobrescribir este valor sin modificar
    /// el perfil normal de producción.
    /// </summary>
    public double ThermalRegulationEcologicalCoupling
    {
        get;
        init;
    } =
        PhenotypePhase7Profile
            .ThermalRegulationCoupling;


    /// <summary>
    /// Coupling ecológico de BodySize.
    ///
    /// Fase 7.10:
    /// queda oficialmente PASIVO.
    ///
    /// El mecanismo experimental BodySize -> MaxEnergy se conserva,
    /// pero no participa en la simulación normal.
    /// </summary>
    public double BodySizeEcologicalCoupling
    {
        get;
        init;
    } =
        PhenotypePhase7Profile
            .BodySizeCoupling;


    /// <summary>
    /// Coupling ecológico de Locomotion.
    ///
    /// Fase 7.10:
    /// queda oficialmente PASIVO.
    ///
    /// El mecanismo experimental Locomotion -> ActivityCost se conserva,
    /// pero no participa en la simulación normal.
    /// </summary>
    public double LocomotionEcologicalCoupling
    {
        get;
        init;
    } =
        PhenotypePhase7Profile
            .LocomotionCoupling;


    /// <summary>
    /// Coupling ecológico de FeedingSpecialization.
    ///
    /// Fase 7.10:
    /// queda oficialmente PASIVO.
    ///
    /// El mecanismo experimental sobre energía vegetal utilizable se
    /// conserva, pero no participa en la simulación normal.
    /// </summary>
    public double FeedingSpecializationEcologicalCoupling
    {
        get;
        init;
    } =
        PhenotypePhase7Profile
            .FeedingSpecializationCoupling;


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

            // Fase 7.10:
            // MetabolicEfficiency queda oficialmente activo.
            // Modifier ±0.08 × coupling 0.125
            // => efecto ecológico máximo ±1 %.
            MetabolicEfficiencyEcologicalCoupling:
                PhenotypePhase7Profile
                    .MetabolicEfficiencyCoupling,

            // false = simulación normal con todos los diagnósticos.
            // true  = corridas experimentales con telemetría intermedia
            //         reducida para acelerar estudios multiseed.
            FastExperimentMode:
                false
        );
}
