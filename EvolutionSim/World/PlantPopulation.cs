namespace EvolutionSim.World;

public class PlantPopulation
{
    public double Biomass { get; private set; }

    public double RootBiomass { get; private set; }

    public double SeedBank { get; private set; }

    public double CarryingCapacity { get; }

    public double RootCapacity { get; }

    public double SeedBankCapacity { get; }

    public double GrowthRate { get; }

    public double RootRegrowthRate { get; }

    public double GerminationRate { get; }

    public double SeedProductionRate { get; }


    // =========================================================
    // PROPIEDADES ALIMENTARIAS
    // =========================================================

    //
    // Cantidad relativa de energía disponible
    // por unidad de biomasa vegetal.
    //
    // 1.0 = valor base
    //
    // < 1.0 = menos energética
    // > 1.0 = más energética
    //

    public double EnergyDensity { get; }


    //
    // Dificultad relativa para procesar
    // físicamente la vegetación.
    //
    // 1.0 = valor base
    //
    // < 1.0 = vegetación más fácil
    // > 1.0 = vegetación más difícil
    //

    public double Toughness { get; }


    public PlantPopulation(
        double initialBiomass,
        double initialRootBiomass,
        double initialSeedBank,
        double carryingCapacity,
        double rootCapacity,
        double growthRate,
        double rootRegrowthRate,
        double germinationRate,
        double seedProductionRate,
        double seedBankCapacity,
        double energyDensity = 1.0,
        double toughness = 1.0)
    {
        CarryingCapacity =
            Math.Max(
                1,
                carryingCapacity
            );

        RootCapacity =
            Math.Max(
                1,
                rootCapacity
            );

        SeedBankCapacity =
            Math.Max(
                1,
                seedBankCapacity
            );

        Biomass =
            Math.Clamp(
                initialBiomass,
                0,
                CarryingCapacity
            );

        RootBiomass =
            Math.Clamp(
                initialRootBiomass,
                0,
                RootCapacity
            );

        SeedBank =
            Math.Clamp(
                initialSeedBank,
                0,
                SeedBankCapacity
            );

        GrowthRate =
            Math.Max(
                0,
                growthRate
            );

        RootRegrowthRate =
            Math.Max(
                0,
                rootRegrowthRate
            );

        GerminationRate =
            Math.Max(
                0,
                germinationRate
            );

        SeedProductionRate =
            Math.Max(
                0,
                seedProductionRate
            );


        // =====================================================
        // PROPIEDADES ALIMENTARIAS
        // =====================================================

        EnergyDensity =
            Math.Max(
                0.10,
                energyDensity
            );

        Toughness =
            Math.Max(
                0.10,
                toughness
            );
    }


    public void Grow()
    {
        //
        // 1. Crecimiento de biomasa existente
        //

        if (Biomass > 0)
        {
            double growth =
                GrowthRate
                *
                Biomass
                *
                (
                    1
                    -
                    Biomass
                    /
                    CarryingCapacity
                );

            Biomass +=
                growth;
        }


        //
        // 2. Rebrote desde raíces
        //

        if (
            RootBiomass > 0
            &&
            Biomass < CarryingCapacity
        )
        {
            double regrowth =
                RootBiomass
                *
                RootRegrowthRate;


            double availableSpace =
                CarryingCapacity
                -
                Biomass;


            regrowth =
                Math.Min(
                    regrowth,
                    availableSpace
                );


            Biomass +=
                regrowth;
        }


        //
        // 3. Germinación
        //

        if (
            SeedBank > 0
            &&
            Biomass < CarryingCapacity
        )
        {
            double germinated =
                SeedBank
                *
                GerminationRate;


            double availableSpace =
                CarryingCapacity
                -
                Biomass;


            germinated =
                Math.Min(
                    germinated,
                    availableSpace
                );


            Biomass +=
                germinated;


            SeedBank -=
                germinated;
        }


        //
        // 4. Producción de semillas
        //

        if (Biomass > 0)
        {
            double producedSeeds =
                Biomass
                *
                SeedProductionRate;


            SeedBank +=
                producedSeeds;
        }


        //
        // 5. Crecimiento de raíces
        //

        if (Biomass > 0)
        {
            double rootGrowth =
                Biomass
                *
                0.01;


            RootBiomass +=
                rootGrowth;
        }


        //
        // 6. Mortalidad de raíces
        //

        double rootMortality =
            RootBiomass
            *
            0.005;


        RootBiomass -=
            rootMortality;


        //
        // 7. Límites
        //

        Biomass =
            Math.Clamp(
                Biomass,
                0,
                CarryingCapacity
            );


        RootBiomass =
            Math.Clamp(
                RootBiomass,
                0,
                RootCapacity
            );


        SeedBank =
            Math.Clamp(
                SeedBank,
                0,
                SeedBankCapacity
            );
    }


    public double Consume(
        double requestedAmount)
    {
        if (
            requestedAmount <= 0
        )
        {
            return 0;
        }


        double consumed =
            Math.Min(
                requestedAmount,
                Biomass
            );


        Biomass -=
            consumed;


        return consumed;
    }
}