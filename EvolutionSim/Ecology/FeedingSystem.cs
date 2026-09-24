using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Ecology;

public sealed class FeedingResult
{
    public int GrazingActions { get; set; }

    public int ScavengingActions { get; set; }

    public int HuntingAttempts { get; set; }

    public int HuntingKills { get; set; }

    public double PlantsConsumed { get; set; }

    public double CarcassBiomassConsumed { get; set; }

    public double HuntingEnergySpent { get; set; }
}


public sealed class FeedingSystem
{
    private readonly Random _random;

    private readonly GrazingSystem _grazingSystem;

    private readonly PredationSystem _predationSystem;

    private readonly CarcassSystem _carcassSystem;


    public FeedingSystem(
        Random random,
        GrazingSystem grazingSystem,
        PredationSystem predationSystem,
        CarcassSystem carcassSystem)
    {
        _random =
            random;

        _grazingSystem =
            grazingSystem;

        _predationSystem =
            predationSystem;

        _carcassSystem =
            carcassSystem;
    }


    public FeedingResult Feed(
        List<Organism> population,
        Worldd world,
        double energyPerFoodUnit)
    {
        FeedingResult result =
            new();


        // =====================================================
        // PROCESAR REGIONES
        // =====================================================
        //
        // La temperatura base sigue siendo regional,
        // pero cada patch aplica su TemperatureOffset.
        //
        // Dentro de cada región:
        //
        // - plantas   -> patch
        // - carroña   -> patch
        // - presas    -> patch
        // - competencia alimentaria -> patch
        //

        foreach (
            Region region
            in world.Regions
        )
        {
            List<Organism> regionalPopulation =
                population
                    .Where(
                        organism =>
                            organism.IsAlive
                            &&
                            organism.RegionId
                            ==
                            region.Id
                    )
                    .ToList();


            if (
                regionalPopulation.Count == 0
            )
            {
                continue;
            }


            // =================================================
            // POBLACIÓN POR PATCH
            // =================================================

            Dictionary<int, List<Organism>>
                populationByPatch =
                    regionalPopulation
                        .GroupBy(
                            organism =>
                                organism.PatchId
                        )
                        .ToDictionary(
                            group =>
                                group.Key,

                            group =>
                                group.ToList()
                        );


            // =================================================
            // ESTRATEGIAS
            // =================================================

            Dictionary<int, List<Organism>>
                huntersByPatch =
                    new();


            Dictionary<int, List<Organism>>
                scavengersByPatch =
                    new();


            Dictionary<int, List<Organism>>
                grazersByPatch =
                    new();


            // =================================================
            // ELECCIÓN DE ESTRATEGIA
            // =================================================

            foreach (
                Organism organism
                in regionalPopulation
            )
            {
                if (
                    !organism.IsAlive
                )
                {
                    continue;
                }


                Patch patch =
                    world.GetPatch(
                        organism.PatchId
                    );


                double hunger =
                    CalculateHunger(
                        organism
                    );


                if (
                    hunger <= 0
                )
                {
                    continue;
                }


                // =============================================
                // PLANTAS LOCALES
                // =============================================

                double plantAvailability =
                    GetPlantAvailability(
                        patch
                    );


                double plantScarcity =
                    1
                    -
                    plantAvailability;


                // =============================================
                // CARROÑA LOCAL
                // =============================================

                double carcassBiomass =
                    _carcassSystem.GetPatchBiomass(
                        patch.Id
                    );


                double carcassAvailability =
                    carcassBiomass
                    /
                    (
                        carcassBiomass
                        +
                        10.0
                    );


                // =============================================
                // PRESAS LOCALES
                // =============================================

                int patchPopulationCount =
                    populationByPatch.TryGetValue(
                        patch.Id,
                        out List<Organism>? localPopulation
                    )

                        ?

                        localPopulation.Count

                        :

                        0;


                double preyAvailability =
                    Math.Clamp(
                        (
                            patchPopulationCount
                            -
                            1
                        )
                        /
                        100.0,
                        0,
                        1
                    );


                // =============================================
                // CARROÑEO
                // =============================================

                double scavengingBehaviorModifier =
                    0.50
                    +
                    organism.ScavengingDrive;


                double scavengeChance =
                    organism.MeatAdaptation
                    *
                    scavengingBehaviorModifier
                    *
                    carcassAvailability
                    *
                    (
                        0.40
                        +
                        1.80
                        *
                        plantScarcity
                    )
                    *
                    (
                        0.50
                        +
                        0.50
                        *
                        hunger
                    );


                scavengeChance =
                    Math.Clamp(
                        scavengeChance,
                        0,
                        0.85
                    );


                // =============================================
                // CAZA
                // =============================================

                double riskModifier =
                    0.50
                    +
                    organism.RiskTolerance;


                double huntingChance =
                    organism.PredatoryDrive
                    *
                    (
                        0.20
                        +
                        0.80
                        *
                        organism.MeatAdaptation
                    )
                    *
                    hunger
                    *
                    preyAvailability
                    *
                    (
                        0.50
                        +
                        1.50
                        *
                        plantScarcity
                    );


                huntingChance *=
                    riskModifier;


                huntingChance =
                    Math.Clamp(
                        huntingChance,
                        0,
                        0.75
                    );


                // =============================================
                // ELEGIR UNA SOLA ESTRATEGIA
                // =============================================

                double huntingRoll =
                    _random.NextDouble();


                if (
                    huntingRoll
                    <
                    huntingChance
                )
                {
                    AddToPatchGroup(
                        huntersByPatch,
                        patch.Id,
                        organism
                    );


                    continue;
                }


                double scavengingRoll =
                    _random.NextDouble();


                if (
                    scavengingRoll
                    <
                    scavengeChance
                )
                {
                    AddToPatchGroup(
                        scavengersByPatch,
                        patch.Id,
                        organism
                    );


                    continue;
                }


                //
                // Si no eligió carne, intenta plantas
                // exclusivamente en su patch.
                //

                if (
                    patch.Plants.Biomass
                    >
                    0
                )
                {
                    AddToPatchGroup(
                        grazersByPatch,
                        patch.Id,
                        organism
                    );
                }
            }


            // =================================================
            // 1. CAZA LOCAL
            // =================================================

            foreach (
                KeyValuePair<int, List<Organism>>
                patchGroup
                in huntersByPatch
            )
            {
                int patchId =
                    patchGroup.Key;


                Patch hunterPatch =
                    world.GetPatch(
                        patchId
                    );


                double localTemperature =
                    hunterPatch.GetLocalTemperature(
                        region.Temperature
                    );


                if (
                    !populationByPatch.TryGetValue(
                        patchId,
                        out List<Organism>? patchPopulation
                    )
                )
                {
                    continue;
                }


                List<Organism> hunters =
                    patchGroup.Value
                        .Where(
                            organism =>
                                organism.IsAlive
                        )
                        .OrderByDescending(
                            organism =>
                                organism.GetHuntingAbility(
                                    _random,
                                    localTemperature
                                )
                        )
                        .ToList();


                foreach (
                    Organism hunter
                    in hunters
                )
                {
                    if (
                        !hunter.IsAlive
                    )
                    {
                        continue;
                    }


                    double hunger =
                        CalculateHunger(
                            hunter
                        );


                    HuntingResult huntingResult =
                        _predationSystem.TryHunt(
                            hunter,
                            patchPopulation,
                            localTemperature,
                            hunger,
                            _carcassSystem
                        );


                    if (
                        !huntingResult.Attempted
                    )
                    {
                        continue;
                    }


                    result.HuntingAttempts++;


                    result.HuntingEnergySpent +=
                        huntingResult.EnergySpent;


                    result.CarcassBiomassConsumed +=
                        huntingResult.MeatBiomassConsumed;


                    if (
                        huntingResult.Success
                    )
                    {
                        result.HuntingKills++;
                    }
                }
            }


            // =================================================
            // 2. CARROÑEO LOCAL
            // =================================================

            foreach (
                KeyValuePair<int, List<Organism>>
                patchGroup
                in scavengersByPatch
            )
            {
                int patchId =
                    patchGroup.Key;


                Patch scavengerPatch =
                    world.GetPatch(
                        patchId
                    );


                double localTemperature =
                    scavengerPatch.GetLocalTemperature(
                        region.Temperature
                    );


                List<Organism> scavengers =
                    patchGroup.Value
                        .Where(
                            organism =>
                                organism.IsAlive
                        )
                        .OrderByDescending(
                            organism =>
                                organism.GetScavengingAbility(
                                    _random,
                                    localTemperature
                                )
                        )
                        .ToList();


                foreach (
                    Organism scavenger
                    in scavengers
                )
                {
                    if (
                        !scavenger.IsAlive
                    )
                    {
                        continue;
                    }


                    double availableCarcass =
                        _carcassSystem.GetPatchBiomass(
                            patchId
                        );


                    if (
                        availableCarcass
                        <=
                        0
                    )
                    {
                        break;
                    }


                    double hunger =
                        CalculateHunger(
                            scavenger
                        );


                    double requestedBiomass =
                        scavenger.FeedingCapacity
                        *
                        (
                            0.50
                            +
                            0.50
                            *
                            hunger
                        );


                    double consumedBiomass =
                        _carcassSystem.ConsumeFromPatch(
                            patchId,
                            requestedBiomass
                        );


                    if (
                        consumedBiomass
                        <=
                        0
                    )
                    {
                        continue;
                    }


                    double rawEnergy =
                        consumedBiomass
                        *
                        CarcassSystem.EnergyPerBiomassUnit;


                    double usableEnergy =
                        rawEnergy
                        *
                        scavenger.MeatDigestionEfficiency;


                    scavenger.Eat(
                        usableEnergy
                    );


                    result.ScavengingActions++;


                    result.CarcassBiomassConsumed +=
                        consumedBiomass;
                }
            }


            // =================================================
            // 3. PASTOREO LOCAL
            // =================================================

            foreach (
                KeyValuePair<int, List<Organism>>
                patchGroup
                in grazersByPatch
            )
            {
                Patch patch =
                    world.GetPatch(
                        patchGroup.Key
                    );


                double localTemperature =
                    patch.GetLocalTemperature(
                        region.Temperature
                    );


                List<Organism> grazers =
                    patchGroup.Value
                        .Where(
                            organism =>
                                organism.IsAlive
                        )
                        .OrderByDescending(
                            organism =>
                                organism.GetForagingAbility(
                                    _random,
                                    localTemperature
                                )
                        )
                        .ToList();


                foreach (
                    Organism grazer
                    in grazers
                )
                {
                    if (
                        !grazer.IsAlive
                    )
                    {
                        continue;
                    }


                    if (
                        patch.Plants.Biomass
                        <=
                        0
                    )
                    {
                        break;
                    }


                    double hunger =
                        CalculateHunger(
                            grazer
                        );


                    double consumedPlants =
                        _grazingSystem.FeedOrganism(
                            grazer,
                            patch.Plants,
                            energyPerFoodUnit,
                            hunger
                        );


                    if (
                        consumedPlants
                        <=
                        0
                    )
                    {
                        continue;
                    }


                    result.GrazingActions++;


                    result.PlantsConsumed +=
                        consumedPlants;
                }
            }
        }


        return result;
    }


    // =========================================================
    // AGREGAR ORGANISMO A GRUPO DE PATCH
    // =========================================================

    private static void AddToPatchGroup(
        Dictionary<int, List<Organism>> groups,
        int patchId,
        Organism organism)
    {
        if (
            !groups.TryGetValue(
                patchId,
                out List<Organism>? group
            )
        )
        {
            group =
                new();


            groups.Add(
                patchId,
                group
            );
        }


        group.Add(
            organism
        );
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
            organism.Energy
            /
            organism.MaxEnergy;


        return Math.Clamp(
            1
            -
            energyRatio,
            0,
            1
        );
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