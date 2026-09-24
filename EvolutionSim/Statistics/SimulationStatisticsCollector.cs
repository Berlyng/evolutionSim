using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Statistics;

public static class SimulationStatisticsCollector
{
    public static SimulationSnapshot Capture(
        int cycle,
        List<Organism> population,
        int births,
        int deaths,
        Worldd world,
        double plantsConsumed)
    {
        int count =
            population.Count;


        //
        // ======================================
        // PLANTAS GLOBALES
        // ======================================
        //
        double totalPlantBiomass =
            world.Patches.Sum(
                patch =>
                    patch.Plants.Biomass
            );


        double totalSeedBank =
            world.Patches.Sum(
                patch =>
                    patch.Plants.SeedBank
            );


        double totalRootBiomass =
            world.Patches.Sum(
                patch =>
                    patch.Plants.RootBiomass
            );


        //
        // ======================================
        // ACUMULADORES
        // ======================================
        //

        double totalSpeed =
            0;

        double totalSpeedSquared =
            0;

        double totalSize =
            0;

        double totalSizeSquared =
            0;

        double totalMetabolism =
            0;

        double totalMetabolismSquared =
            0;

        double totalEnergy =
            0;

        double totalMaxEnergy =
            0;

        double totalFoodEfficiency =
            0;

        double totalPhysicalPerformance =
            0;

        double totalAbsorptionCapacity =
            0;

        double totalBasalMetabolicCost =
            0;

        double totalActivityCost =
            0;

        double totalEnergyCost =
            0;

        double totalAge =
            0;

        double totalGeneration =
            0;

        int maxGeneration =
            0;


        //
        // ======================================
        // MÍNIMOS Y MÁXIMOS
        // ======================================
        //

        double minSpeed =
            double.MaxValue;

        double maxSpeed =
            double.MinValue;

        double minSize =
            double.MaxValue;

        double maxSize =
            double.MinValue;

        double minMetabolism =
            double.MaxValue;

        double maxMetabolism =
            double.MinValue;


        //
        // ======================================
        // RECORRER POBLACIÓN
        // ======================================
        //

        foreach (
            Organism organism
            in population
        )
        {
            //
            // SPEED
            //

            totalSpeed +=
                organism.Speed;

            totalSpeedSquared +=
                organism.Speed
                *
                organism.Speed;

            minSpeed =
                Math.Min(
                    minSpeed,
                    organism.Speed
                );

            maxSpeed =
                Math.Max(
                    maxSpeed,
                    organism.Speed
                );


            //
            // SIZE
            //

            totalSize +=
                organism.Size;

            totalSizeSquared +=
                organism.Size
                *
                organism.Size;

            minSize =
                Math.Min(
                    minSize,
                    organism.Size
                );

            maxSize =
                Math.Max(
                    maxSize,
                    organism.Size
                );


            //
            // METABOLISM
            //

            totalMetabolism +=
                organism.Metabolism;

            totalMetabolismSquared +=
                organism.Metabolism
                *
                organism.Metabolism;

            minMetabolism =
                Math.Min(
                    minMetabolism,
                    organism.Metabolism
                );

            maxMetabolism =
                Math.Max(
                    maxMetabolism,
                    organism.Metabolism
                );


            //
            // ENERGÍA
            //

            totalEnergy +=
                organism.Energy;

            totalMaxEnergy +=
                organism.MaxEnergy;


            //
            // FISIOLOGÍA
            //

            totalFoodEfficiency +=
                organism.FoodEfficiency;

            totalPhysicalPerformance +=
                organism.PhysicalPerformance;

            totalAbsorptionCapacity +=
                organism.EnergyAbsorptionCapacity;

            totalBasalMetabolicCost +=
                organism.BasalMetabolicCost;

            totalActivityCost +=
                organism.ActivityCost;

            totalEnergyCost +=
                organism.EnergyCost;


            //
            // EDAD
            //

            totalAge +=
                organism.Age;


            //
            // GENERACIÓN
            //

            totalGeneration +=
                organism.Generation;

            maxGeneration =
                Math.Max(
                    maxGeneration,
                    organism.Generation
                );
        }


        //
        // ======================================
        // PROMEDIOS
        // ======================================
        //

        double averageSpeed =
            totalSpeed /
            count;

        double averageSize =
            totalSize /
            count;

        double averageMetabolism =
            totalMetabolism /
            count;


        //
        // ======================================
        // VARIANZA
        // ======================================
        //

        double speedVariance =
            totalSpeedSquared / count
            -
            averageSpeed
            *
            averageSpeed;

        double sizeVariance =
            totalSizeSquared / count
            -
            averageSize
            *
            averageSize;

        double metabolismVariance =
            totalMetabolismSquared / count
            -
            averageMetabolism
            *
            averageMetabolism;


        //
        // ======================================
        // DESVIACIÓN ESTÁNDAR
        // ======================================
        //

        double speedStdDev =
            Math.Sqrt(
                Math.Max(
                    0,
                    speedVariance
                )
            );

        double sizeStdDev =
            Math.Sqrt(
                Math.Max(
                    0,
                    sizeVariance
                )
            );

        double metabolismStdDev =
            Math.Sqrt(
                Math.Max(
                    0,
                    metabolismVariance
                )
            );


        //
        // ======================================
        // SNAPSHOT
        // ======================================
        //

        return new SimulationSnapshot(
            Cycle:
                cycle,

            Population:
                count,

            Births:
                births,

            Deaths:
                deaths,

            PlantBiomass:
                totalPlantBiomass,

            SeedBank:
                totalSeedBank,

            RootBiomass:
                totalRootBiomass,

            PlantsConsumed:
                plantsConsumed,

            AverageSpeed:
                averageSpeed,

            AverageSize:
                averageSize,

            AverageMetabolism:
                averageMetabolism,

            SpeedStdDev:
                speedStdDev,

            SizeStdDev:
                sizeStdDev,

            MetabolismStdDev:
                metabolismStdDev,

            MinSpeed:
                minSpeed,

            MaxSpeed:
                maxSpeed,

            MinSize:
                minSize,

            MaxSize:
                maxSize,

            MinMetabolism:
                minMetabolism,

            MaxMetabolism:
                maxMetabolism,

            AverageEnergy:
                totalEnergy /
                count,

            AverageMaxEnergy:
                totalMaxEnergy /
                count,

            AverageFoodEfficiency:
                totalFoodEfficiency /
                count,

            AveragePhysicalPerformance:
                totalPhysicalPerformance /
                count,

            AverageAbsorptionCapacity:
                totalAbsorptionCapacity /
                count,

            AverageBasalMetabolicCost:
                totalBasalMetabolicCost /
                count,

            AverageActivityCost:
                totalActivityCost /
                count,

            AverageEnergyCost:
                totalEnergyCost /
                count,

            AverageAge:
                totalAge /
                count,

            AverageGeneration:
                totalGeneration /
                count,

            MaxGeneration:
                maxGeneration
        );
    }
}