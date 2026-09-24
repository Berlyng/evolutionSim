using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public static class WorldFactory
{
    public static Worldd CreateDefault()
    {
        Patch valleOeste =
            new(
                id: 0,
                regionId: 0,
                name: "Valle Oeste",
                plants: CreateVallePlants(
                    resourceScale: 1.10,
                    growthScale: 1.06,
                    energyDensity: 0.90,
                    toughness: 1.15
                ),
                temperatureOffset: -1.0
            );

        Patch valleCentro =
            new(
                id: 1,
                regionId: 0,
                name: "Valle Centro",
                plants: CreateVallePlants(
                    resourceScale: 1.00,
                    growthScale: 1.00,
                    energyDensity: 1.00,
                    toughness: 1.00
                ),
                temperatureOffset: 0.0
            );

        Patch valleEste =
            new(
                id: 2,
                regionId: 0,
                name: "Valle Este",
                plants: CreateVallePlants(
                    resourceScale: 0.90,
                    growthScale: 0.94,
                    energyDensity: 1.10,
                    toughness: 0.85
                ),
                temperatureOffset: 1.0
            );

        Patch bosqueOeste =
            new(
                id: 3,
                regionId: 1,
                name: "Bosque Oeste",
                plants: CreateBosquePlants(
                    resourceScale: 0.90,
                    growthScale: 0.94,
                    energyDensity: 1.10,
                    toughness: 0.90
                ),
                temperatureOffset: 1.0
            );

        Patch bosqueCentro =
            new(
                id: 4,
                regionId: 1,
                name: "Bosque Centro",
                plants: CreateBosquePlants(
                    resourceScale: 1.00,
                    growthScale: 1.00,
                    energyDensity: 1.00,
                    toughness: 1.00
                ),
                temperatureOffset: 0.0
            );

        Patch bosqueEste =
            new(
                id: 5,
                regionId: 1,
                name: "Bosque Este",
                plants: CreateBosquePlants(
                    resourceScale: 1.10,
                    growthScale: 1.06,
                    energyDensity: 0.90,
                    toughness: 1.10
                ),
                temperatureOffset: -1.0
            );

        Patch llanuraOeste =
            new(
                id: 6,
                regionId: 2,
                name: "Llanura Oeste",
                plants: CreateLlanuraPlants(
                    resourceScale: 0.95,
                    growthScale: 0.97,
                    energyDensity: 1.10,
                    toughness: 1.00
                ),
                temperatureOffset: 1.0
            );

        Patch llanuraCentro =
            new(
                id: 7,
                regionId: 2,
                name: "Llanura Centro",
                plants: CreateLlanuraPlants(
                    resourceScale: 1.10,
                    growthScale: 1.06,
                    energyDensity: 0.90,
                    toughness: 1.15
                ),
                temperatureOffset: -1.0
            );

        Patch llanuraEste =
            new(
                id: 8,
                regionId: 2,
                name: "Llanura Este",
                plants: CreateLlanuraPlants(
                    resourceScale: 0.95,
                    growthScale: 0.97,
                    energyDensity: 1.00,
                    toughness: 0.85
                ),
                temperatureOffset: 0.0
            );

        Region valleCentral =
            new(
                id: 0,
                name: "Valle Central",
                temperature: 24,
                patches: new[]
                {
                    valleOeste,
                    valleCentro,
                    valleEste
                }
            );

        Region bosqueFrio =
            new(
                id: 1,
                name: "Bosque Frio",
                temperature: 16,
                patches: new[]
                {
                    bosqueOeste,
                    bosqueCentro,
                    bosqueEste
                }
            );

        Region llanuraCalida =
            new(
                id: 2,
                name: "Llanura Calida",
                temperature: 32,
                patches: new[]
                {
                    llanuraOeste,
                    llanuraCentro,
                    llanuraEste
                }
            );

        Worldd world =
            new(
                new[]
                {
                    valleCentral,
                    bosqueFrio,
                    llanuraCalida
                }
            );

        world.ConnectPatches(0, 1, movementDifficulty: 1, energyCost: 1);
        world.ConnectPatches(1, 2, movementDifficulty: 1, energyCost: 1);
        world.ConnectPatches(3, 4, movementDifficulty: 1, energyCost: 1);
        world.ConnectPatches(4, 5, movementDifficulty: 1, energyCost: 1);
        world.ConnectPatches(6, 7, movementDifficulty: 1, energyCost: 1);
        world.ConnectPatches(7, 8, movementDifficulty: 1, energyCost: 1);

        world.ConnectRegions(
            regionAId: 0,
            regionBId: 1,
            migrationDifficulty: 3,
            energyCost: 5
        );

        world.ConnectRegions(
            regionAId: 1,
            regionBId: 2,
            migrationDifficulty: 6,
            energyCost: 8
        );

        return world;
    }

    private static PlantPopulation CreateVallePlants(
        double resourceScale,
        double growthScale,
        double energyDensity,
        double toughness)
    {
        return new PlantPopulation(
            initialBiomass: (25000.0 / 3.0) * resourceScale,
            initialRootBiomass: (15000.0 / 3.0) * resourceScale,
            initialSeedBank: (10000.0 / 3.0) * resourceScale,
            carryingCapacity: (40000.0 / 3.0) * resourceScale,
            rootCapacity: (25000.0 / 3.0) * resourceScale,
            growthRate: 0.12 * growthScale,
            rootRegrowthRate: 0.02 * growthScale,
            germinationRate: 0.01 * growthScale,
            seedProductionRate: 0.02 * growthScale,
            seedBankCapacity: (50000.0 / 3.0) * resourceScale,
            energyDensity: energyDensity,
            toughness: toughness
        );
    }

    private static PlantPopulation CreateBosquePlants(
        double resourceScale,
        double growthScale,
        double energyDensity,
        double toughness)
    {
        return new PlantPopulation(
            initialBiomass: (32000.0 / 3.0) * resourceScale,
            initialRootBiomass: (20000.0 / 3.0) * resourceScale,
            initialSeedBank: (12000.0 / 3.0) * resourceScale,
            carryingCapacity: (50000.0 / 3.0) * resourceScale,
            rootCapacity: (30000.0 / 3.0) * resourceScale,
            growthRate: 0.14 * growthScale,
            rootRegrowthRate: 0.025 * growthScale,
            germinationRate: 0.012 * growthScale,
            seedProductionRate: 0.022 * growthScale,
            seedBankCapacity: (60000.0 / 3.0) * resourceScale,
            energyDensity: energyDensity,
            toughness: toughness
        );
    }

    private static PlantPopulation CreateLlanuraPlants(
        double resourceScale,
        double growthScale,
        double energyDensity,
        double toughness)
    {
        return new PlantPopulation(
            initialBiomass: (12000.0 / 3.0) * resourceScale,
            initialRootBiomass: (10000.0 / 3.0) * resourceScale,
            initialSeedBank: (8000.0 / 3.0) * resourceScale,
            carryingCapacity: (25000.0 / 3.0) * resourceScale,
            rootCapacity: (18000.0 / 3.0) * resourceScale,
            growthRate: 0.08 * growthScale,
            rootRegrowthRate: 0.015 * growthScale,
            germinationRate: 0.007 * growthScale,
            seedProductionRate: 0.015 * growthScale,
            seedBankCapacity: (40000.0 / 3.0) * resourceScale,
            energyDensity: energyDensity,
            toughness: toughness
        );
    }
}
