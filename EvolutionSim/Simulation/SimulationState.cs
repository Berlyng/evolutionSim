using EvolutionSim.Evolution;
using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public sealed class SimulationState
{
    public Worldd World { get; }

    public List<Organism> Population
    {
        get;
        set;
    }

    public List<SpeciesCluster> DetectedSpecies
    {
        get;
        set;
    }

    public int CurrentCycle
    {
        get;
        private set;
    }

    public SimulationState(
        Worldd world,
        List<Organism> population,
        List<SpeciesCluster> detectedSpecies)
    {
        World = world;
        Population = population;
        DetectedSpecies = detectedSpecies;
        CurrentCycle = 0;
    }

    public void AdvanceTo(
        int cycle)
    {
        if (cycle < CurrentCycle)
        {
            throw new InvalidOperationException(
                "Simulation cycle cannot move backwards."
            );
        }

        CurrentCycle = cycle;
    }
}
