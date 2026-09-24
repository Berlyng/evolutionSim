using EvolutionSim.Evolution;
using EvolutionSim.Models;
using EvolutionSim.Output;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public static class SimulationFactory
{
    // ============================================================
    // MOTOR SILENCIOSO
    // ============================================================
    //
    // Entrada pensada para hosts que consumen SimulationStepResult
    // directamente (por ejemplo WPF). No escribe texto.
    //

    public static SimulationEngine CreateDefault()
    {
        return Create(
            SimulationConfig.Default,
            NullSimulationOutput.Instance
        );
    }

    public static SimulationEngine Create(
        SimulationConfig config)
    {
        return Create(
            config,
            NullSimulationOutput.Instance
        );
    }


    // ============================================================
    // MOTOR CON SALIDA EXPLÍCITA
    // ============================================================
    //
    // Se conserva para Console, logging y futuras implementaciones
    // de ISimulationOutput.
    //

    public static SimulationEngine CreateDefault(
        ISimulationOutput output)
    {
        return Create(
            SimulationConfig.Default,
            output
        );
    }

    public static SimulationEngine Create(
        SimulationConfig config,
        ISimulationOutput output)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(output);

        // ============================================================
        // RANDOM
        // ============================================================
        //
        // Conservamos exactamente las dos secuencias aleatorias
        // validadas. Ninguna llamada a Random se agrega aquí; solo se
        // crean las mismas instancias que antes vivían en Runner.
        //

        Random random =
            new(
                config.RandomSeed
            );

        Random climateRandom =
            new(
                config.RandomSeed + 1000
            );

        // RNG independiente para evolución estructural del genoma.
        // Nunca consume la secuencia ecológica principal.
        Random structuralGenomeRandom =
            new(
                config.RandomSeed + 2000
            );


        // ============================================================
        // MUNDO
        // ============================================================

        Worldd world =
            WorldFactory.CreateDefault();

        Region startingRegion =
            world.GetRegion(
                0
            );


        // ============================================================
        // SISTEMAS
        // ============================================================

        SimulationSystems systems =
            SimulationSystemsFactory.Create(
                random,
                climateRandom,
                structuralGenomeRandom,
                output
            );


        // ============================================================
        // POBLACIÓN INICIAL
        // ============================================================
        //
        // Se usa la MISMA instancia principal de Random y el factory
        // conserva el orden validado de consumo del RNG.
        //

        List<Organism> population =
            InitialPopulationFactory.Create(
                random,
                config.InitialPopulation,
                startingRegion,
                config.InitialOrganismEnergy
            );


        // ============================================================
        // ESPECIE ANCESTRAL
        // ============================================================

        List<SpeciesCluster> detectedSpecies =
            systems.SpeciesDetectionSystem.Detect(
                population,
                cycle:
                    0
            );


        // ============================================================
        // ESTADO + MOTOR
        // ============================================================

        SimulationState state =
            new(
                world,
                population,
                detectedSpecies
            );

        return new SimulationEngine(
            config,
            state,
            systems,
            output
        );
    }


    // ============================================================
    // CONTROLADOR
    // ============================================================
    //
    // El controlador conserva una factory del motor para que Reset()
    // pueda reconstruir exactamente el mismo escenario y seed.
    //

    public static SimulationController CreateControllerDefault()
    {
        return new SimulationController(
            () => CreateDefault()
        );
    }

    public static SimulationController CreateController(
        SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new SimulationController(
            () => Create(config)
        );
    }

    public static SimulationController CreateControllerDefault(
        ISimulationOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        return new SimulationController(
            () => CreateDefault(output)
        );
    }

    public static SimulationController CreateController(
        SimulationConfig config,
        ISimulationOutput output)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(output);

        return new SimulationController(
            () => Create(config, output)
        );
    }
}
