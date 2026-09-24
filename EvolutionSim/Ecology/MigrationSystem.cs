using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Ecology;

public sealed record MigrationRouteStatistics(
    int OriginRegionId,
    string OriginRegionName,
    int DestinationRegionId,
    string DestinationRegionName,
    int Count,
    double AverageThermalFitness);


public sealed record MigrationEventStatistics(
    Guid OrganismId,
    int OriginRegionId,
    string OriginRegionName,
    int DestinationRegionId,
    string DestinationRegionName,
    double DestinationThermalFitness);


public sealed class MigrationSystem
{
    private readonly Random _random;

    private readonly CarcassSystem _carcassSystem;


    private const double BaseMigrationChance =
        0.005;


    private const double ResourceScarcityPressure =
        0.06;


    private const double HungerPressure =
        0.05;


    private const double MaximumMigrationChance =
        0.20;


    // =========================================================
    // DIAGNÓSTICO DE LA ÚLTIMA PASADA
    // =========================================================
    //
    // No modifica ninguna decisión de migración.
    // Solamente guarda las migraciones que realmente ocurrieron
    // durante la última llamada a Migrate().
    //
    // Para cada ruta registra:
    //
    // - región de origen
    // - región de destino
    // - cantidad de migrantes exitosos
    // - fitness térmica media de esos inmigrantes al destino
    //

    public IReadOnlyList<MigrationRouteStatistics>
        LastMigrationRoutes
    { get; private set; }
        = Array.Empty<MigrationRouteStatistics>();


    public IReadOnlyList<MigrationEventStatistics>
        LastMigrationEvents
    { get; private set; }
        = Array.Empty<MigrationEventStatistics>();


    public MigrationSystem(
        Random random,
        CarcassSystem carcassSystem)
    {
        _random =
            random;

        _carcassSystem =
            carcassSystem;
    }


    // =========================================================
    // MIGRACIÓN ENTRE REGIONES
    // =========================================================

    public int Migrate(
        List<Organism> population,
        Worldd world)
    {
        int migrations =
            0;


        // =====================================================
        // ACUMULADORES DE DIAGNÓSTICO
        // =====================================================
        //
        // Key:
        // (RegionOrigen, RegionDestino)
        //
        // Value:
        // (Cantidad, SumaFitnessTermica)
        //

        Dictionary<(int OriginId, int DestinationId),
            (int Count, double ThermalFitnessSum)>
            routeAccumulators =
                new();


        List<MigrationEventStatistics>
            migrationEvents =
                new();


        // =====================================================
        // POBLACIÓN ACTUAL POR REGIÓN
        // =====================================================

        Dictionary<int, int> regionPopulationCounts =
            population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .GroupBy(
                    organism =>
                        organism.RegionId
                )
                .ToDictionary(
                    group =>
                        group.Key,

                    group =>
                        group.Count()
                );


        //
        // Garantizamos que todas las regiones
        // tengan una entrada, incluso si están vacías.
        //

        foreach (
            Region region
            in world.Regions
        )
        {
            regionPopulationCounts.TryAdd(
                region.Id,
                0
            );
        }


        // =====================================================
        // EVALUAR CADA ORGANISMO
        // =====================================================

        foreach (
            Organism organism
            in population
        )
        {
            if (
                !organism.IsAlive
            )
            {
                continue;
            }


            Region currentRegion =
                world.GetRegion(
                    organism.RegionId
                );


            List<RegionConnection> connections =
                world.GetConnections(
                    currentRegion.Id
                );


            if (
                connections.Count == 0
            )
            {
                continue;
            }


            // =================================================
            // CALIDAD DEL HÁBITAT ACTUAL
            // =================================================

            double currentQuality =
                CalculateResourceQuality(
                    organism,
                    currentRegion,
                    regionPopulationCounts[
                        currentRegion.Id
                    ],
                    world
                );


            double resourceScarcity =
                1
                -
                Math.Clamp(
                    currentQuality,
                    0,
                    1
                );


            double hunger =
                CalculateHunger(
                    organism
                );


            // =================================================
            // MOVILIDAD
            // =================================================

            double mobility =
                organism.Speed
                /
                Math.Sqrt(
                    organism.Size
                );


            mobility =
                Math.Clamp(
                    mobility,
                    0.50,
                    1.50
                );


            // =================================================
            // IMPULSO EXPLORATORIO
            // =================================================

            double exploratoryImpulse =
                organism.ExplorationDrive
                *
                0.03;


            // =================================================
            // PROBABILIDAD DE MIGRACIÓN
            // =================================================

            double migrationChance =
                BaseMigrationChance
                +
                (
                    resourceScarcity
                    *
                    ResourceScarcityPressure
                )
                +
                (
                    hunger
                    *
                    HungerPressure
                )
                +
                exploratoryImpulse;


            migrationChance *=
                mobility;


            migrationChance =
                Math.Clamp(
                    migrationChance,
                    0,
                    MaximumMigrationChance
                );


            if (
                _random.NextDouble()
                >
                migrationChance
            )
            {
                continue;
            }


            // =================================================
            // BUSCAR MEJOR DESTINO
            // =================================================

            RegionConnection? bestConnection =
                null;


            Region? bestRegion =
                null;


            double bestRegionQuality =
                double.MinValue;


            double bestScore =
                double.MinValue;


            foreach (
                RegionConnection connection
                in connections
            )
            {
                int targetRegionId =
                    connection.GetOtherRegionId(
                        currentRegion.Id
                    );


                Region targetRegion =
                    world.GetRegion(
                        targetRegionId
                    );


                double targetQuality =
                    CalculateResourceQuality(
                        organism,
                        targetRegion,
                        regionPopulationCounts[
                            targetRegion.Id
                        ],
                        world
                    );


                // ---------------------------------------------
                // ACCESIBILIDAD
                // ---------------------------------------------

                double accessibility =
                    1.0
                    /
                    connection.MigrationDifficulty;


                double score =
                    targetQuality
                    *
                    accessibility;


                //
                // Pequeña incertidumbre ambiental para evitar
                // decisiones perfectamente deterministas.
                //

                score *=
                    0.90
                    +
                    (
                        _random.NextDouble()
                        *
                        0.20
                    );


                if (
                    score
                    >
                    bestScore
                )
                {
                    bestScore =
                        score;


                    bestRegionQuality =
                        targetQuality;


                    bestConnection =
                        connection;


                    bestRegion =
                        targetRegion;
                }
            }


            if (
                bestConnection is null
                ||
                bestRegion is null
            )
            {
                continue;
            }


            // =================================================
            // ¿EL DESTINO ES MEJOR?
            // =================================================

            bool targetIsBetter =
                bestRegionQuality
                >
                currentQuality;


            if (
                !targetIsBetter
            )
            {
                double explorationChance =
                    0.02
                    +
                    (
                        organism.ExplorationDrive
                        *
                        0.48
                    );


                if (
                    _random.NextDouble()
                    >
                    explorationChance
                )
                {
                    continue;
                }
            }


            // =================================================
            // ADECUACIÓN TÉRMICA AL DESTINO
            // =================================================
            //
            // Esta misma variable se utiliza en la barrera
            // ecológica y luego se registra en las estadísticas.
            //
            // Por lo tanto, el diagnóstico NO añade ninguna
            // llamada nueva a Random ni cambia el resultado.
            //

            double targetThermalFitness =
                organism.GetThermalFitness(
                    bestRegion.Temperature
                );


            // =================================================
            // BARRERA GEOGRÁFICA
            // =================================================

            double barrierCrossingChance =
                1.0
                /
                Math.Pow(
                    bestConnection.MigrationDifficulty,
                    0.85
                );


            barrierCrossingChance =
                Math.Clamp(
                    barrierCrossingChance,
                    0.05,
                    1.0
                );


            // =================================================
            // AISLAMIENTO ECOLÓGICO
            // =================================================
            //
            // Organismos mal adaptados térmicamente al destino
            // tienen menor probabilidad de completar una
            // migración exitosa.
            //

            double ecologicalCrossingFactor =
                0.40
                +
                (
                    0.60
                    *
                    targetThermalFitness
                );


            barrierCrossingChance *=
                ecologicalCrossingFactor;


            barrierCrossingChance =
                Math.Clamp(
                    barrierCrossingChance,
                    0.02,
                    1.0
                );


            if (
                _random.NextDouble()
                >
                barrierCrossingChance
            )
            {
                continue;
            }


            // =================================================
            // COSTO DE MOVIMIENTO
            // =================================================

            double difficultyEnergyMultiplier =
                1.0
                +
                (
                    Math.Max(
                        0,
                        bestConnection.MigrationDifficulty
                        -
                        1.0
                    )
                    *
                    0.15
                );


            double movementCost =
                bestConnection.EnergyCost
                *
                difficultyEnergyMultiplier
                *
                Math.Sqrt(
                    organism.Size
                );


            organism.SpendEnergy(
                movementCost
            );


            if (
                !organism.IsAlive
            )
            {
                continue;
            }


            // =================================================
            // SELECCIONAR PATCH DE DESTINO
            // =================================================

            List<Patch> targetPatches =
                bestRegion.Patches
                    .ToList();


            if (
                targetPatches.Count == 0
            )
            {
                continue;
            }


            Patch targetPatch =
                targetPatches[
                    _random.Next(
                        targetPatches.Count
                    )
                ];


            int previousRegionId =
                organism.RegionId;


            string previousRegionName =
                currentRegion.Name;


            // =================================================
            // MIGRACIÓN
            // =================================================

            organism.MoveToLocation(
                bestRegion.Id,
                targetPatch.Id
            );


            regionPopulationCounts[
                previousRegionId
            ]--;


            regionPopulationCounts[
                bestRegion.Id
            ]++;


            migrations++;


            // =================================================
            // REGISTRAR EVENTO INDIVIDUAL DE MIGRACIÓN
            // =================================================
            //
            // Este evento solamente describe una migración que
            // YA ocurrió. No consume Random ni afecta decisiones.
            //

            migrationEvents.Add(
                new MigrationEventStatistics(
                    OrganismId:
                        organism.Id,

                    OriginRegionId:
                        previousRegionId,

                    OriginRegionName:
                        previousRegionName,

                    DestinationRegionId:
                        bestRegion.Id,

                    DestinationRegionName:
                        bestRegion.Name,

                    DestinationThermalFitness:
                        targetThermalFitness
                )
            );


            // =================================================
            // REGISTRAR DIAGNÓSTICO DE RUTA
            // =================================================

            var routeKey =
                (
                    OriginId:
                        previousRegionId,

                    DestinationId:
                        bestRegion.Id
                );


            if (
                routeAccumulators.TryGetValue(
                    routeKey,
                    out var currentRoute
                )
            )
            {
                routeAccumulators[
                    routeKey
                ] =
                    (
                        currentRoute.Count + 1,
                        currentRoute.ThermalFitnessSum
                        +
                        targetThermalFitness
                    );
            }
            else
            {
                routeAccumulators[
                    routeKey
                ] =
                    (
                        1,
                        targetThermalFitness
                    );
            }
        }


        // =====================================================
        // CONSTRUIR RESULTADO DIAGNÓSTICO
        // =====================================================

        LastMigrationEvents =
            migrationEvents;


        LastMigrationRoutes =
            routeAccumulators
                .Select(
                    entry =>
                    {
                        Region originRegion =
                            world.GetRegion(
                                entry.Key.OriginId
                            );


                        Region destinationRegion =
                            world.GetRegion(
                                entry.Key.DestinationId
                            );


                        double averageThermalFitness =
                            entry.Value.Count > 0

                                ?

                                entry.Value.ThermalFitnessSum
                                /
                                entry.Value.Count

                                :

                                0;


                        return new MigrationRouteStatistics(
                            OriginRegionId:
                                originRegion.Id,

                            OriginRegionName:
                                originRegion.Name,

                            DestinationRegionId:
                                destinationRegion.Id,

                            DestinationRegionName:
                                destinationRegion.Name,

                            Count:
                                entry.Value.Count,

                            AverageThermalFitness:
                                averageThermalFitness
                        );
                    }
                )
                .OrderBy(
                    route =>
                        route.OriginRegionName
                )
                .ThenBy(
                    route =>
                        route.DestinationRegionName
                )
                .ToList();


        return migrations;
    }


    // =========================================================
    // CALIDAD DEL HÁBITAT PARA ESTE ORGANISMO
    // =========================================================

    private double CalculateResourceQuality(
        Organism organism,
        Region region,
        int regionalPopulation,
        Worldd world)
    {
        // =====================================================
        // PLANTAS
        // =====================================================

        double plantAvailability =
            region.PlantAvailability;


        double plantOpportunity =
            plantAvailability
            *
            organism.PlantDigestionEfficiency;


        // =====================================================
        // CARROÑA
        // =====================================================

        double carrionBiomass =
            _carcassSystem.GetRegionalBiomass(
                world,
                region.Id
            );


        double carrionAvailability =
            carrionBiomass
            /
            (
                carrionBiomass
                +
                100.0
            );


        double carrionOpportunity =
            carrionAvailability
            *
            organism.MeatAdaptation
            *
            organism.MeatDigestionEfficiency
            *
            1.25;


        // =====================================================
        // PRESAS
        // =====================================================

        double preyAvailability =
            regionalPopulation
            /
            (
                regionalPopulation
                +
                250.0
            );


        double predatoryOpportunity =
            preyAvailability
            *
            organism.PredatoryDrive
            *
            (
                0.25
                +
                (
                    0.75
                    *
                    organism.MeatAdaptation
                )
            )
            *
            1.50;


        // =====================================================
        // OPORTUNIDAD ALIMENTARIA TOTAL
        // =====================================================

        double foodOpportunity =
            plantOpportunity
            +
            carrionOpportunity
            +
            predatoryOpportunity;


        // =====================================================
        // ADECUACIÓN TÉRMICA
        // =====================================================
        //
        // La temperatura tiene un peso mayor en la decisión
        // regional que en la primera versión del sistema.
        //

        double thermalSuitability =
            organism.GetThermalFitness(
                region.Temperature
            );


        double thermalPreference =
            0.35
            +
            (
                0.65
                *
                thermalSuitability
            );


        // =====================================================
        // CALIDAD FINAL
        // =====================================================

        return
            foodOpportunity
            *
            thermalPreference;
    }


    // =========================================================
    // HAMBRE
    // =========================================================

    private static double CalculateHunger(
        Organism organism)
    {
        if (
            organism.MaxEnergy
            <=
            0
        )
        {
            return 1;
        }


        double energyRatio =
            Math.Clamp(
                organism.Energy
                /
                organism.MaxEnergy,
                0,
                1
            );


        return
            1
            -
            energyRatio;
    }
}
