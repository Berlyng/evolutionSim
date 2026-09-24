namespace EvolutionSim.Output;

public interface ISimulationOutput
{
    void WriteLine();

    void WriteLine(
        string message);
}
