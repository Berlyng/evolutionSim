namespace EvolutionSim.Output;

public sealed class ConsoleSimulationOutput : ISimulationOutput
{
    public void WriteLine()
    {
        Console.WriteLine();
    }

    public void WriteLine(
        string message)
    {
        Console.WriteLine(
            message
        );
    }
}
