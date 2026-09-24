using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Ecology;

public sealed class GrazingSystem
{
    private readonly double
        _halfSaturationBiomass;


    public GrazingSystem(
        double halfSaturationBiomass)
    {
        _halfSaturationBiomass =
            Math.Max(
                1,
                halfSaturationBiomass
            );
    }


    public double FeedOrganism(
        Organism organism,
        PlantPopulation plants,
        double energyPerFoodUnit,
        double hunger)
    {
        if (
            !organism.IsAlive
            ||
            plants.Biomass <= 0
        )
        {
            return 0;
        }


        // =====================================================
        // DISPONIBILIDAD DE PLANTAS
        // =====================================================

        double availabilityFactor =
            plants.Biomass
            /
            (
                plants.Biomass
                +
                _halfSaturationBiomass
            );


        // =====================================================
        // CAPACIDAD DE PROCESAR LA VEGETACIÓN
        // =====================================================
        //
        // No existe un "herbívoro" explícito.
        //
        // La capacidad emerge de:
        //
        // - eficiencia digestiva vegetal
        // - tamaño corporal
        // - metabolismo
        //
        // MeatAdaptation ya influye indirectamente porque
        // PlantDigestionEfficiency disminuye cuando aumenta
        // la especialización hacia carne.
        //

        double plantProcessingAbility =
            organism.PlantDigestionEfficiency
            *
            Math.Sqrt(
                organism.Size
            )
            *
            (
                0.75
                +
                0.25
                *
                organism.Metabolism
            );


        // =====================================================
        // EFECTO DE LA DUREZA
        // =====================================================
        //
        // Si la capacidad del organismo supera la dureza,
        // puede explotar mejor el recurso.
        //
        // Si la vegetación es demasiado dura,
        // reduce la cantidad que puede consumir.
        //
        // Los límites impiden que este único factor
        // domine completamente la ecología.
        //

        double processingModifier =
     Math.Clamp(
         plantProcessingAbility
         /
         plants.Toughness,
         0.50,
         1.00
     );


        // =====================================================
        // HAMBRE
        // =====================================================
        //
        // Un organismo muy hambriento
        // intenta comer más.
        //
        // Uno casi satisfecho consume menos.
        //

        double hungerFactor =
            0.20
            +
            (
                0.80
                *
                hunger
            );


        // =====================================================
        // CANTIDAD QUE INTENTA COMER
        // =====================================================

        double requestedFood =
            organism.FeedingCapacity
            *
            availabilityFactor
            *
            hungerFactor
            *
            processingModifier;


        double eaten =
            plants.Consume(
                requestedFood
            );


        if (
            eaten <= 0
        )
        {
            return 0;
        }


        // =====================================================
        // ENERGÍA VEGETAL BRUTA
        // =====================================================
        //
        // EnergyDensity representa cuánto valor energético
        // contiene cada unidad de biomasa.
        //
        // 1.0 = base
        //
        // > 1.0 = más energética
        // < 1.0 = menos energética
        //

        double rawPlantEnergy =
            eaten
            *
            energyPerFoodUnit
            *
            plants.EnergyDensity;


        // =====================================================
        // ENERGÍA UTILIZABLE
        // =====================================================
        //
        // El organismo todavía necesita ser capaz
        // de digerir fisiológicamente el tejido vegetal.
        //
        // MeatAdaptation alta reduce esta eficiencia
        // mediante PlantDigestionEfficiency.
        //

        // =====================================================
        // ESPECIALIZACIÓN ALIMENTARIA FENOTÍPICA
        // =====================================================
        //
        // Fase 7.9:
        // FeedingSpecialization NO modifica cuánta biomasa consume
        // el organismo. Solo cambia ligeramente cuánta energía obtiene
        // de esa biomasa después de la digestión vegetal histórica.
        //
        // Con coupling 0:
        // FeedingSpecializationPlantEnergyMultiplier = 1.0
        // y el comportamiento es exactamente el histórico.
        //

        double usablePlantEnergy =
            rawPlantEnergy
            *
            organism.PlantDigestionEfficiency
            *
            organism.FeedingSpecializationPlantEnergyMultiplier;


        organism.Eat(
            usablePlantEnergy
        );


        return eaten;
    }
}