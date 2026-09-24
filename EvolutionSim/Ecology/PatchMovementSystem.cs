using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Ecology;

public sealed record PatchMovementRouteStatistics(
    int OriginPatchId,
    string OriginPatchName,
    int DestinationPatchId,
    string DestinationPatchName,
    int Count
);


public sealed class PatchMovementSystem
{
    private readonly Random _random;

    private readonly CarcassSystem _carcassSystem;


    private const double BaseMovementChance =
        0.01;


    private const double ResourceScarcityPressure =
        0.04;


    private const double ExplorationInfluence =
        0.04;


    private const double MaximumMovementChance =
        0.12;


    public IReadOnlyList<PatchMovementRouteStatistics>
        LastMovementRoutes
    { get; private set; }
        =
        Array.Empty<PatchMovementRouteStatistics>();


    public PatchMovementSystem(
        Random random,
        CarcassSystem carcassSystem)
    {
        _random =
            random;

        _carcassSystem =
            carcassSystem;
    }


    // =========================================================
    // MOVIMIENTO LOCAL
    // =========================================================

    public int Move(
        List<Organism> population,
        Worldd world)
    {
        int movements =
            0;


        Dictionary<(int OriginPatchId, int DestinationPatchId), int>
            movementRouteCounts =
                new();


        LastMovementRoutes =
            Array.Empty<PatchMovementRouteStatistics>();


        // =====================================================
        // POBLACIÓN VIVA POR PATCH
        // =====================================================
        //
        // La necesitamos para estimar cuántas presas
        // potenciales existen localmente.
        //

        Dictionary<int, int> patchPopulationCounts =
            population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .GroupBy(
                    organism =>
                        organism.PatchId
                )
                .ToDictionary(
                    group =>
                        group.Key,

                    group =>
                        group.Count()
                );


        //
        // Garantizamos que todos los patches tengan
        // una entrada aunque estén vacíos.
        //

        foreach (
            Patch patch
            in world.Patches
        )
        {
            patchPopulationCounts.TryAdd(
                patch.Id,
                0
            );
        }


        // =====================================================
        // EVALUAR ORGANISMOS
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


            Patch currentPatch =
                world.GetPatch(
                    organism.PatchId
                );


            Region currentRegion =
                world.GetRegion(
                    currentPatch.RegionId
                );


            double regionalTemperature =
                currentRegion.Temperature;


            List<PatchConnection> connections =
                world.GetPatchConnections(
                    organism.PatchId
                );


            if (
                connections.Count == 0
            )
            {
                continue;
            }


            // =================================================
            // CALIDAD DEL PATCH ACTUAL
            // =================================================

            double currentQuality =
                CalculateResourceQuality(
                    organism,
                    currentPatch,
                    patchPopulationCounts[
                        currentPatch.Id
                    ],
                    regionalTemperature
                );


            //
            // Para calcular escasez solo necesitamos
            // una escala 0..1.
            //

            double resourceScarcity =
                1
                -
                Math.Clamp(
                    currentQuality,
                    0,
                    1
                );


            // =================================================
            // PROBABILIDAD DE MOVERSE
            // =================================================
            //
            // Conservamos exactamente las mismas constantes
            // que ya teníamos.
            //
            // Lo que cambia es la definición de "escasez":
            // ahora depende de la dieta del organismo.
            //

            double movementChance =
                BaseMovementChance
                +
                (
                    resourceScarcity
                    *
                    ResourceScarcityPressure
                )
                +
                (
                    organism.ExplorationDrive
                    *
                    ExplorationInfluence
                );


            movementChance =
                Math.Clamp(
                    movementChance,
                    0,
                    MaximumMovementChance
                );


            if (
                _random.NextDouble()
                >
                movementChance
            )
            {
                continue;
            }


            // =================================================
            // BUSCAR EL MEJOR PATCH VECINO
            // =================================================

            PatchConnection? bestConnection =
                null;


            Patch? bestPatch =
                null;


            double bestPatchQuality =
                double.MinValue;


            double bestScore =
                double.MinValue;


            foreach (
                PatchConnection connection
                in connections
            )
            {
                int targetPatchId =
                    connection.GetOtherPatchId(
                        currentPatch.Id
                    );


                Patch targetPatch =
                    world.GetPatch(
                        targetPatchId
                    );


                double targetQuality =
                    CalculateResourceQuality(
                        organism,
                        targetPatch,
                        patchPopulationCounts[
                            targetPatch.Id
                        ],
                        regionalTemperature
                    );


                // ---------------------------------------------
                // ACCESIBILIDAD
                // ---------------------------------------------

                double accessibility =
                    1.0
                    /
                    connection.MovementDifficulty;


                double score =
                    targetQuality
                    *
                    accessibility;


                //
                // Pequeña incertidumbre ambiental.
                //
                // Evita que todos los organismos tomen
                // siempre exactamente la misma decisión.
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


                    bestPatchQuality =
                        targetQuality;


                    bestConnection =
                        connection;


                    bestPatch =
                        targetPatch;
                }
            }


            if (
                bestConnection is null
                ||
                bestPatch is null
            )
            {
                continue;
            }


            // =================================================
            // ¿EL DESTINO ES MEJOR?
            // =================================================

            bool targetIsBetter =
                bestPatchQuality
                >
                currentQuality;


            //
            // Si no es mejor todavía puede explorarlo.
            //

            if (
                !targetIsBetter
            )
            {
                double explorationChance =
                    0.05
                    +
                    (
                        organism.ExplorationDrive
                        *
                        0.45
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
            // COSTO DE MOVIMIENTO
            // =================================================

            double movementCost =
                bestConnection.EnergyCost
                *
                Math.Sqrt(
                    organism.Size
                );


            organism.SpendEnergy(
                movementCost
            );


            //
            // Puede morir durante el desplazamiento.
            //

            if (
                !organism.IsAlive
            )
            {
                //
                // Ya no cuenta como población viva del patch.
                //

                patchPopulationCounts[
                    currentPatch.Id
                ]--;


                continue;
            }


            // =================================================
            // MOVIMIENTO
            // =================================================

            int previousPatchId =
                currentPatch.Id;


            organism.MoveToPatch(
                bestPatch.Id
            );


            // =================================================
            // ACTUALIZAR POBLACIONES
            // =================================================
            //
            // Esto permite que los siguientes organismos
            // evalúen una situación local más actualizada.
            //

            patchPopulationCounts[
                previousPatchId
            ]--;


            patchPopulationCounts[
                bestPatch.Id
            ]++;


            movements++;


            (int OriginPatchId, int DestinationPatchId) routeKey =
                (
                    previousPatchId,
                    bestPatch.Id
                );


            movementRouteCounts.TryGetValue(
                routeKey,
                out int routeCount
            );


            movementRouteCounts[
                routeKey
            ] =
                routeCount + 1;
        }


        LastMovementRoutes =
            movementRouteCounts
                .OrderByDescending(
                    route =>
                        route.Value
                )
                .ThenBy(
                    route =>
                        route.Key.OriginPatchId
                )
                .ThenBy(
                    route =>
                        route.Key.DestinationPatchId
                )
                .Select(
                    route =>
                    {
                        Patch originPatch =
                            world.GetPatch(
                                route.Key.OriginPatchId
                            );

                        Patch destinationPatch =
                            world.GetPatch(
                                route.Key.DestinationPatchId
                            );

                        return new PatchMovementRouteStatistics(
                            OriginPatchId:
                                originPatch.Id,
                            OriginPatchName:
                                originPatch.Name,
                            DestinationPatchId:
                                destinationPatch.Id,
                            DestinationPatchName:
                                destinationPatch.Name,
                            Count:
                                route.Value
                        );
                    }
                )
                .ToList();


        return movements;
    }


    // =========================================================
    // CALIDAD DEL PATCH PARA ESTE ORGANISMO
    // =========================================================
    //
    // No existen tipos:
    //
    // Herbivore
    // Predator
    // Scavenger
    //
    // La valoración del hábitat emerge directamente
    // de los genes.
    //

    private double CalculateResourceQuality(
        Organism organism,
        Patch patch,
        int patchPopulation,
        double regionalTemperature)
    {
        // =====================================================
        // 1. PLANTAS
        // =====================================================

        double plantAvailability =
            GetPlantAvailability(
                patch
            );


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


        double processingModifier =
            Math.Clamp(
                plantProcessingAbility
                /
                patch.Plants.Toughness,
                0.50,
                1.00
            );


        double plantOpportunity =
            plantAvailability
            *
            patch.Plants.EnergyDensity
            *
            organism.PlantDigestionEfficiency
            *
            processingModifier;


        // =====================================================
        // 2. CARROÑA
        // =====================================================

        double carrionBiomass =
            _carcassSystem.GetPatchBiomass(
                patch.Id
            );


        //
        // Utilizamos la misma escala local que FeedingSystem.
        //

        double carrionAvailability =
            carrionBiomass
            /
            (
                carrionBiomass
                +
                10.0
            );


        double scavengingBehaviorModifier =
            0.50
            +
            organism.ScavengingDrive;


        double carrionOpportunity =
            carrionAvailability
            *
            organism.MeatAdaptation
            *
            organism.MeatDigestionEfficiency
            *
            scavengingBehaviorModifier;


        // =====================================================
        // 3. PRESAS
        // =====================================================
        //
        // Si estamos evaluando el patch donde ya vive
        // el organismo, debemos excluirlo como presa.
        //
        // Si estamos evaluando otro patch, todavía no está
        // allí, por lo que todos sus habitantes son presas
        // potenciales.
        //

        int potentialPreyCount =
            patchPopulation;


        if (
            patch.Id
            ==
            organism.PatchId
        )
        {
            potentialPreyCount--;
        }


        potentialPreyCount =
            Math.Max(
                potentialPreyCount,
                0
            );


        //
        // Misma escala utilizada por FeedingSystem
        // para la disponibilidad local de presas.
        //

        double preyAvailability =
            Math.Clamp(
                potentialPreyCount
                /
                100.0,
                0,
                1
            );


        double predatoryOpportunity =
            preyAvailability
            *
            organism.PredatoryDrive
            *
            (
                0.20
                +
                (
                    0.80
                    *
                    organism.MeatAdaptation
                )
            );


        // =====================================================
        // 4. ADECUACIÓN TÉRMICA LOCAL
        // =====================================================
        //
        // La comida sigue siendo el factor principal, pero dos
        // patches con recursos similares ya no son equivalentes
        // para organismos adaptados a temperaturas distintas.
        // Esto favorece clasificación espacial y, como la
        // reproducción ocurre dentro del patch, ayuda a conservar
        // adaptaciones locales.
        //

        double localTemperature =
            patch.GetLocalTemperature(
                regionalTemperature
            );


        double thermalSuitability =
            organism.GetThermalFitness(
                localTemperature
            );


        double thermalPreference =
            0.75
            +
            (
                0.25
                *
                thermalSuitability
            );


        // =====================================================
        // 5. CALIDAD TOTAL
        // =====================================================

        double foodOpportunity =
            plantOpportunity
            +
            carrionOpportunity
            +
            predatoryOpportunity;


        return
            foodOpportunity
            *
            thermalPreference;
    }


    // =========================================================
    // DISPONIBILIDAD VEGETAL
    // =========================================================

    private static double GetPlantAvailability(
        Patch patch)
    {
        if (
            patch.Plants.CarryingCapacity
            <=
            0
        )
        {
            return 0;
        }


        return Math.Clamp(
            patch.Plants.Biomass
            /
            patch.Plants.CarryingCapacity,
            0,
            1
        );
    }
}