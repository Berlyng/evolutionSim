namespace EvolutionSim.Models;

public class Organism
{
    public Guid Id { get; }

    public Guid? ParentAId { get; }

    public Guid? ParentBId { get; }

    public int Generation { get; }


    public int RegionId
    {
        get;
        private set;
    }
    public int PatchId { get; private set; }


    public Genome Genome { get; }


    private readonly PhenotypeProfile
        _phenotype;


    public PhenotypeProfile Phenotype =>
        _phenotype;


    /// <summary>
    /// Fase 8.1:
    /// morfología determinista derivada del genoma.
    ///
    /// Es completamente pasiva: consultar esta propiedad no modifica
    /// la ecología ni consume Random.
    /// </summary>
    public MorphologyProfile Morphology =>
        GenomeMorphologyMapper.Express(
            Genome
        );


    /// <summary>
    /// Fase 7.9:
    /// expresión del genoma estructural asignada al canal
    /// FeedingSpecialization.
    ///
    /// Rango:
    /// -0.08 .. +0.08
    ///
    /// En esta primera activación solo modifica la energía utilizable
    /// obtenida de la biomasa vegetal ya consumida.
    /// </summary>
    public double FeedingSpecializationModifier =>
        _phenotype.FeedingSpecialization;


    /// <summary>
    /// Multiplicador aplicado únicamente a la energía vegetal utilizable.
    ///
    /// Coupling 0:
    ///     siempre 1.0, control ecológico exacto.
    ///
    /// Coupling 0.125:
    ///     modifier ±0.08 => efecto máximo ±1 %.
    ///
    /// Coupling 0.25:
    ///     modifier ±0.08 => efecto máximo ±2 %.
    ///
    /// Modifier positivo obtiene más energía de la misma biomasa.
    /// Modifier negativo obtiene menos energía de la misma biomasa.
    ///
    /// No modifica la cantidad de plantas consumidas, la elección de
    /// estrategia alimentaria, caza, carroñeo ni preferencias.
    /// </summary>
    public double FeedingSpecializationPlantEnergyMultiplier
    {
        get
        {
            double coupling =
                PhenotypeEcologyContext
                    .FeedingSpecializationCoupling;


            double maximumDeviation =
                0.08
                *
                coupling;


            return Math.Clamp(
                1
                +
                (
                    FeedingSpecializationModifier
                    *
                    coupling
                ),
                1
                -
                maximumDeviation,
                1
                +
                maximumDeviation
            );
        }
    }


    /// <summary>
    /// Factor diagnóstico que combina la digestión vegetal histórica con
    /// el efecto fenotípico experimental de FeedingSpecialization.
    ///
    /// No sustituye PlantDigestionEfficiency; solo permite observar la
    /// eficiencia final que se aplica a la energía vegetal.
    /// </summary>
    public double EffectivePlantEnergyAssimilation =>
        PlantDigestionEfficiency
        *
        FeedingSpecializationPlantEnergyMultiplier;


    /// <summary>
    /// Fase 7.8:
    /// expresión del genoma estructural asignada al canal Locomotion.
    ///
    /// Rango:
    /// -0.08 .. +0.08
    ///
    /// Un valor positivo representa una locomoción energéticamente más
    /// eficiente. En esta primera activación solo modifica ActivityCost.
    /// </summary>
    public double LocomotionModifier =>
        _phenotype.Locomotion;


    /// <summary>
    /// Multiplicador aplicado únicamente al costo energético de actividad.
    ///
    /// Coupling 0:
    ///     siempre 1.0, control ecológico exacto.
    ///
    /// Coupling 0.125:
    ///     modifier ±0.08 => efecto máximo ±1 %.
    ///
    /// Coupling 0.25:
    ///     modifier ±0.08 => efecto máximo ±2 %.
    ///
    /// Modifier positivo reduce ActivityCost.
    /// Modifier negativo aumenta ActivityCost.
    ///
    /// No modifica Speed, movimiento, migración, caza ni ningún otro sistema.
    /// </summary>
    public double LocomotionActivityCostMultiplier
    {
        get
        {
            double coupling =
                PhenotypeEcologyContext
                    .LocomotionCoupling;


            double maximumDeviation =
                0.08
                *
                coupling;


            return Math.Clamp(
                1
                -
                (
                    LocomotionModifier
                    *
                    coupling
                ),
                1
                -
                maximumDeviation,
                1
                +
                maximumDeviation
            );
        }
    }


    /// <summary>
    /// Fase 7.7:
    /// expresión del genoma estructural asignada al canal BodySize.
    ///
    /// Rango:
    /// -0.08 .. +0.08
    ///
    /// En esta primera activación NO modifica el Size genético.
    /// Solo ajusta la capacidad máxima de reserva energética.
    /// </summary>
    public double BodySizeModifier =>
        _phenotype.BodySize;


    /// <summary>
    /// Multiplicador aplicado únicamente a MaxEnergy.
    ///
    /// Coupling 0:
    ///     siempre 1.0, control ecológico exacto.
    ///
    /// Coupling 0.125:
    ///     modifier ±0.08 => efecto máximo ±1 %.
    ///
    /// Coupling 0.25:
    ///     modifier ±0.08 => efecto máximo ±2 %.
    ///
    /// No modifica mantenimiento, actividad, madurez reproductiva,
    /// FeedingCapacity ni ningún otro sistema dependiente del Size genético.
    /// </summary>
    public double BodySizeEnergyCapacityMultiplier
    {
        get
        {
            double coupling =
                PhenotypeEcologyContext
                    .BodySizeCoupling;


            double maximumDeviation =
                0.08
                *
                coupling;


            return Math.Clamp(
                1
                +
                (
                    BodySizeModifier
                    *
                    coupling
                ),
                1
                -
                maximumDeviation,
                1
                +
                maximumDeviation
            );
        }
    }


    /// <summary>
    /// Fase 7.6:
    /// expresión del genoma estructural asignada a ThermalRegulation.
    ///
    /// Rango:
    /// -0.08 .. +0.08
    ///
    /// Los valores positivos reducen el desafío térmico efectivo.
    /// Los valores negativos aumentan el desafío térmico efectivo.
    /// </summary>
    public double ThermalRegulationModifier =>
        _phenotype.ThermalRegulation;


    /// <summary>
    /// Multiplicador aplicado a la diferencia térmica ambiental
    /// ANTES de las ecuaciones existentes de estrés y fitness térmico.
    ///
    /// Coupling 0:
    ///     siempre 1.0, control ecológico exacto.
    ///
    /// Coupling 0.125:
    ///     modifier ±0.08 => efecto máximo ±1 %.
    ///
    /// Coupling 0.25:
    ///     modifier ±0.08 => efecto máximo ±2 %.
    /// </summary>
    public double ThermalRegulationStressMultiplier
    {
        get
        {
            double coupling =
                PhenotypeEcologyContext
                    .ThermalRegulationCoupling;


            double maximumDeviation =
                0.08
                *
                coupling;


            return Math.Clamp(
                1
                -
                (
                    ThermalRegulationModifier
                    *
                    coupling
                ),
                1
                -
                maximumDeviation,
                1
                +
                maximumDeviation
            );
        }
    }


    /// <summary>
    /// Primer canal fenotípico con efecto ecológico real.
    ///
    /// Rango de Phase 7.4:
    /// -0.08 .. +0.08
    ///
    /// Positivo = mejor eficiencia = menor gasto energético.
    /// Negativo = peor eficiencia = mayor gasto energético.
    /// </summary>
    public double MetabolicEfficiencyModifier =>
        _phenotype.MetabolicEfficiency;


    /// <summary>
    /// Multiplicador energético producido por el canal
    /// MetabolicEfficiency.
    ///
    /// La fuerza de acoplamiento proviene del contexto de ejecución
    /// abierto por SimulationEngine a partir de SimulationConfig.
    ///
    /// Con coupling 0.00:
    ///     siempre 1.00 (control pasivo).
    ///
    /// Con coupling 0.125:
    ///     modifier ±0.08 => efecto máximo ±1 %.
    ///
    /// Con coupling 0.25:
    ///     modifier ±0.08 => efecto máximo ±2 %.
    /// </summary>
    public double MetabolicEfficiencyCostMultiplier
    {
        get
        {
            double coupling =
                PhenotypeEcologyContext
                    .MetabolicEfficiencyCoupling;


            double maximumDeviation =
                0.08
                *
                coupling;


            return Math.Clamp(
                1
                -
                (
                    MetabolicEfficiencyModifier
                    *
                    coupling
                ),
                1
                -
                maximumDeviation,
                1
                +
                maximumDeviation
            );
        }
    }


    public double Speed =>
        Genome.Speed;


    public double Size =>
        Genome.Size;


    public double Metabolism =>
        Genome.Metabolism;


    public double OptimalTemperature =>
        Genome.OptimalTemperature;


    public double ThermalTolerance =>
        Genome.ThermalTolerance;


    public double MeatAdaptation =>
        Genome.MeatAdaptation;


    public double PredatoryDrive =>
        Genome.PredatoryDrive;

    public double ExplorationDrive =>
    Genome.ExplorationDrive;
    public double RiskTolerance =>
    Genome.RiskTolerance;

    public double ScavengingDrive =>
    Genome.ScavengingDrive;
    public double MateSelectivity =>
    Genome.MateSelectivity;


    public double Energy
    {
        get;
        private set;
    }


    public int Age
    {
        get;
        private set;
    }


    public int LastReproductionAge
    {
        get;
        private set;
    } = -100;


    /// <summary>
    /// Capacidad energética base producida exclusivamente por el Size genético.
    /// Conserva exactamente la fórmula histórica validada.
    /// </summary>
    public double BaseMaxEnergy =>
        80
        +
        (
            Size
            *
            70
        );


    /// <summary>
    /// Fase 7.7:
    /// BodySize fenotípico actúa únicamente sobre la capacidad máxima
    /// de reserva energética.
    ///
    /// Con coupling 0 esta propiedad es exactamente igual a BaseMaxEnergy.
    /// </summary>
    public double MaxEnergy =>
        BaseMaxEnergy
        *
        BodySizeEnergyCapacityMultiplier;


    public double PhysicalPerformance
    {
        get
        {
            return Math.Clamp(
                0.7
                +
                (
                    0.3
                    *
                    Metabolism
                ),
                0.3,
                1.6
            );
        }
    }


    public double FoodEfficiency
    {
        get
        {
            double saturation =
                Metabolism
                /
                (
                    Metabolism
                    +
                    0.5
                );


            return
                0.25
                +
                (
                    1.125
                    *
                    saturation
                );
        }
    }


    //
    // ==========================================
    // DIGESTIÓN VEGETAL
    // ==========================================
    //

    public double PlantDigestionEfficiency
    {
        get
        {
            //
            // MeatAdaptation = 0
            // -> 100%
            //
            // MeatAdaptation = 1
            // -> 35%
            //
            // El costo de especializarse en
            // carne aparece gradualmente.
            //

            return Math.Clamp(
                1
                -
                (
                    0.65
                    *
                    MeatAdaptation
                ),
                0.35,
                1
            );
        }
    }


    //
    // ==========================================
    // DIGESTIÓN DE CARNE
    // ==========================================
    //

    public double MeatDigestionEfficiency
    {
        get
        {
            //
            // Una pequeña adaptación inicial
            // produce una mejora importante.
            //
            // Después aparecen rendimientos
            // decrecientes.
            //
            // Esto elimina el valle evolutivo
            // de las primeras mutaciones.
            //

            double adaptationBenefit =
                Math.Sqrt(
                    MeatAdaptation
                );


            return Math.Clamp(
                0.05
                +
                (
                    1.20
                    *
                    adaptationBenefit
                ),
                0.05,
                1.25
            );
        }
    }


    public double EnergyAbsorptionCapacity =>
        12
        +
        (
            18
            *
            Metabolism
        );


    public int MaxAge
    {
        get
        {
            double lifespanFactor =
                1
                /
                Math.Sqrt(
                    Metabolism
                );


            lifespanFactor =
                Math.Clamp(
                    lifespanFactor,
                    0.7,
                    1.4
                );


            return Math.Max(
                8,
                (int)Math.Round(
                    40
                    *
                    lifespanFactor
                )
            );
        }
    }


    public int ReproductiveAge
    {
        get
        {
            double baseAge =
                Math.Max(
                    2,
                    Size
                    *
                    5
                );


            double developmentFactor =
                1
                /
                PhysicalPerformance;


            developmentFactor =
                Math.Clamp(
                    developmentFactor,
                    0.8,
                    1.5
                );


            return Math.Max(
                2,
                (int)Math.Ceiling(
                    baseAge
                    *
                    developmentFactor
                )
            );
        }
    }


    public int ReproductionCooldown
    {
        get
        {
            double baseCooldown =
                Math.Max(
                    2,
                    Size
                    *
                    3
                );


            double metabolicFactor =
                1
                /
                Metabolism;


            metabolicFactor =
                Math.Clamp(
                    metabolicFactor,
                    0.7,
                    3
                );


            return Math.Max(
                2,
                (int)Math.Ceiling(
                    baseCooldown
                    *
                    metabolicFactor
                )
            );
        }
    }


    public double BasalMetabolicCost
    {
        get
        {
            double bodyMaintenance =
                4
                +
                (
                    Size
                    *
                    2
                );


            double metabolicFactor =
                0.6
                +
                (
                    0.4
                    *
                    Metabolism
                );


            // =================================================
            // COSTO DE GENERALISMO TÉRMICO
            // =================================================
            //
            // Una tolerancia amplia permite sobrevivir en más
            // ambientes, pero mantener esa plasticidad tiene un
            // costo energético creciente.
            //
            // El componente cuadrático evita que aumentar
            // ThermalTolerance sea siempre la solución óptima.
            // Un especialista bien ajustado a su ambiente puede
            // gastar menos energía que un generalista.
            //

            double baselineToleranceCost =
                ThermalTolerance
                *
                0.05;


            double generalistExcess =
                Math.Max(
                    0,
                    ThermalTolerance - 3.0
                );


            double generalistExpansionCost =
                generalistExcess
                *
                generalistExcess
                *
                0.12;


            double thermalGeneralistCost =
                baselineToleranceCost
                +
                generalistExpansionCost;


            return
                (
                    bodyMaintenance
                    *
                    metabolicFactor
                )
                +
                thermalGeneralistCost;
        }
    }


    /// <summary>
    /// Costo histórico de actividad calculado únicamente a partir de
    /// Speed, Size y Metabolism genéticos.
    ///
    /// Conserva exactamente la fórmula validada antes de la Fase 7.8.
    /// </summary>
    public double BaseActivityCost
    {
        get
        {
            double speedCost =
                Speed
                *
                Speed
                *
                2;


            double bodyFactor =
                Math.Sqrt(
                    Size
                );


            double metabolicFactor =
                0.85
                +
                (
                    0.15
                    *
                    Metabolism
                );


            return
                speedCost
                *
                bodyFactor
                *
                metabolicFactor;
        }
    }


    /// <summary>
    /// Fase 7.8:
    /// Locomotion fenotípico actúa únicamente sobre el costo energético
    /// de actividad.
    ///
    /// Con coupling 0 esta propiedad es exactamente igual a BaseActivityCost.
    /// </summary>
    public double ActivityCost =>
        BaseActivityCost
        *
        LocomotionActivityCostMultiplier;


    public double EnergyCost =>
        BasalMetabolicCost
        +
        ActivityCost;


    public double FeedingCapacity =>
        0.6
        +
        (
            Size
            *
            0.8
        );


    public bool IsAlive =>
        Energy > 0
        &&
        Age < MaxAge;


    public Organism(
        Genome genome,
        int regionId,
        int patchId,
        double initialEnergy = 50,
        Guid? parentAId = null,
        Guid? parentBId = null,
        int generation = 0)
    {
        Id =
            Guid.NewGuid();


        ParentAId =
            parentAId;


        ParentBId =
            parentBId;


        Generation =
            Math.Max(
                0,
                generation
            );


        RegionId =
    regionId;

        PatchId =
            patchId;


        Genome =
            genome;


        _phenotype =
            GenomePhenotypeMapper.Express(
                genome
            );


        Energy =
            Math.Min(
                initialEnergy,
                MaxEnergy
            );


        Age =
            0;
    }


    private double GetEffectiveThermalDifference(
        double ambientTemperature)
    {
        double difference =
            Math.Abs(
                ambientTemperature
                -
                OptimalTemperature
            );


        return
            difference
            *
            ThermalRegulationStressMultiplier;
    }


    public double GetThermalStress(
        double ambientTemperature)
    {
        double effectiveDifference =
            GetEffectiveThermalDifference(
                ambientTemperature
            );


        if (
            effectiveDifference
            <=
            ThermalTolerance
        )
        {
            return 0;
        }


        double excess =
            effectiveDifference
            -
            ThermalTolerance;


        return
            excess
            /
            ThermalTolerance;
    }


    public double GetThermalFitness(
        double ambientTemperature)
    {
        double effectiveDifference =
            GetEffectiveThermalDifference(
                ambientTemperature
            );


        double safeTolerance =
            Math.Max(
                ThermalTolerance,
                0.25
            );


        double normalizedDifference =
            effectiveDifference
            /
            safeTolerance;


        double fitness =
            Math.Exp(
                -0.20
                *
                normalizedDifference
                *
                normalizedDifference
            );


        return Math.Clamp(
            fitness,
            0.15,
            1.0
        );
    }


    public double GetThermalCostMultiplier(
        double ambientTemperature)
    {
        double stress =
            GetThermalStress(
                ambientTemperature
            );


        double multiplier =
            1
            +
            (
                0.50
                *
                stress
                *
                stress
            );


        return Math.Clamp(
            multiplier,
            1,
            4
        );
    }


    public double GetTotalEnergyCost(
        double ambientTemperature)
    {
        return
            EnergyCost
            *
            MetabolicEfficiencyCostMultiplier
            *
            GetThermalCostMultiplier(
                ambientTemperature
            );
    }


    public void ConsumeEnergy(
        double ambientTemperature)
    {
        Energy -=
            GetTotalEnergyCost(
                ambientTemperature
            );


        Age++;
    }


    public void Eat(
        double foodEnergy)
    {
        double potentialEnergy =
            foodEnergy
            *
            FoodEfficiency;


        double absorbedEnergy =
            Math.Min(
                potentialEnergy,
                EnergyAbsorptionCapacity
            );


        Energy =
            Math.Min(
                Energy
                +
                absorbedEnergy,
                MaxEnergy
            );
    }


    public void SpendEnergy(
        double amount)
    {
        Energy -=
            amount;
    }


    public void Die()
    {
        Energy =
            0;
    }


    public double GetForagingAbility(
        Random random,
        double ambientTemperature)
    {
        double luck =
            random.NextDouble()
            *
            0.4
            +
            0.8;


        double thermalFitness =
            GetThermalFitness(
                ambientTemperature
            );


        return
            Speed
            *
            PhysicalPerformance
            *
            thermalFitness
            *
            luck;
    }


    public double GetScavengingAbility(
    Random random,
    double ambientTemperature)
    {
        double thermalFitness =
            GetThermalFitness(
                ambientTemperature
            );


        //
        // Encontrar cadáveres requiere movilidad,
        // pero la velocidad pesa menos que durante
        // una persecución activa.
        //

        double mobilityFactor =
            Math.Sqrt(
                Speed
            );


        //
        // Un cuerpo algo mayor obtiene una pequeña
        // ventaja al competir alrededor de un cadáver.
        //
        // No queremos que Size domine demasiado.
        //

        double bodyDominance =
            Math.Pow(
                Size,
                0.15
            );


        double luck =
            0.80
            +
            (
                random.NextDouble()
                *
                0.40
            );


        return
            mobilityFactor
            *
            PhysicalPerformance
            *
            bodyDominance
            *
            thermalFitness
            *
            luck;
    }


    //
    // ==========================================
    // HABILIDAD DE CAZA
    // ==========================================
    //

    public double GetHuntingAbility(
        Random random,
        double ambientTemperature)
    {
        double thermalFitness =
            GetThermalFitness(
                ambientTemperature
            );


        double bodyPower =
            Math.Pow(
                Size,
                0.35
            );


        //
        // Ahora la habilidad de cazar depende
        // del comportamiento depredador,
        // NO de la digestión de carne.
        //

        double predatoryAdaptation =
            0.20
            +
            (
                1.80
                *
                PredatoryDrive
            );


        double luck =
            0.85
            +
            (
                random.NextDouble()
                *
                0.30
            );


        return
            Speed
            *
            PhysicalPerformance
            *
            bodyPower
            *
            thermalFitness
            *
            predatoryAdaptation
            *
            luck;
    }


    public double GetEscapeAbility(
        Random random,
        double ambientTemperature)
    {
        double thermalFitness =
            GetThermalFitness(
                ambientTemperature
            );


        double agilityFactor =
            1
            /
            Math.Pow(
                Size,
                0.20
            );


        double luck =
            0.85
            +
            (
                random.NextDouble()
                *
                0.30
            );


        return
            Speed
            *
            PhysicalPerformance
            *
            thermalFitness
            *
            agilityFactor
            *
            luck;
    }


    public bool CanReproduce(
        double ambientTemperature)
    {
        if (!IsAlive)
        {
            return false;
        }


        if (
            Age
            <
            ReproductiveAge
        )
        {
            return false;
        }


        double thermalFitness =
            GetThermalFitness(
                ambientTemperature
            );


        int effectiveCooldown =
            (int)Math.Ceiling(
                ReproductionCooldown
                /
                thermalFitness
            );


        int cyclesSinceLastReproduction =
            Age
            -
            LastReproductionAge;


        return
            cyclesSinceLastReproduction
            >=
            effectiveCooldown;
    }


    public void Reproduce(
        double energyCost)
    {
        SpendEnergy(
            energyCost
        );


        LastReproductionAge =
            Age;
    }


    public void MoveToLocation(
    int regionId,
    int patchId)
    {
        RegionId =
            regionId;

        PatchId =
            patchId;
    }

    public void MoveToPatch(
    int patchId)
    {
        PatchId =
            patchId;
    }
}