using EvolutionSim.Output;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public sealed class SimulationRunner
{
    public void Run()
    {
        // ============================================================
        // SALIDA DE CONSOLA
        // ============================================================

        ISimulationOutput output =
            new ConsoleSimulationOutput();


        // ============================================================
        // CONTROLADOR
        // ============================================================
        //
        // La consola usa la misma capa de control que luego utilizará
        // WPF. El controlador no cambia la lógica del motor: cada
        // Tick() sigue ejecutando exactamente un SimulationEngine.Step().
        //

        SimulationController controller =
            SimulationFactory.CreateControllerDefault(
                output
            );

        SimulationConfig config =
            SimulationConfig.Default;

        SimulationState state =
            controller.State;

        Region startingRegion =
            state.World.GetRegion(
                0
            );


        // ============================================================
        // INFORMACIÓN INICIAL
        // ============================================================

        output.WriteLine(
            $"Random seed: {config.RandomSeed}"
        );

        output.WriteLine(
            $"Initial population: {config.InitialPopulation}"
        );

        output.WriteLine(
            $"Starting region: {startingRegion.Name}"
        );

        output.WriteLine(
            $"Initial detected species: {state.DetectedSpecies.Count}"
        );

        output.WriteLine();


        // ============================================================
        // EJECUCIÓN COMPLETA
        // ============================================================
        //
        // Start() habilita la ejecución y cada Tick() avanza un solo
        // ciclo. En WPF, esos Tick() vendrán de un DispatcherTimer.
        // Aquí simplemente se consumen tan rápido como sea posible.
        //

        controller.Start();

        while (controller.IsRunning)
        {
            controller.Tick();
        }
    }
}
