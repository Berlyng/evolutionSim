namespace EvolutionSim.Output;

/// <summary>
/// Implementación silenciosa de la salida de simulación.
/// Permite ejecutar EvolutionSim desde hosts que consumen
/// SimulationStepResult directamente y no necesitan texto.
/// </summary>
public sealed class NullSimulationOutput : ISimulationOutput
{
    public static NullSimulationOutput Instance { get; } =
        new();

    private NullSimulationOutput()
    {
    }

    public void WriteLine()
    {
        // Intencionalmente vacío.
    }

    public void WriteLine(
        string message)
    {
        // Intencionalmente vacío.
    }
}
