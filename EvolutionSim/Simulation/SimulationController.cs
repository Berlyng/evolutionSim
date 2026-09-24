namespace EvolutionSim.Simulation;

public sealed class SimulationController
{
    private readonly Func<SimulationEngine> _engineFactory;

    private SimulationEngine _engine;

    public SimulationEngine Engine =>
        _engine;

    public SimulationState State =>
        _engine.State;

    public SimulationStepResult? LastResult
    {
        get;
        private set;
    }

    public bool IsRunning
    {
        get;
        private set;
    }

    public bool CanStep =>
        _engine.CanStep;

    public bool IsCompleted =>
        !_engine.CanStep;

    public SimulationController(
        Func<SimulationEngine> engineFactory)
    {
        ArgumentNullException.ThrowIfNull(
            engineFactory
        );

        _engineFactory =
            engineFactory;

        _engine =
            _engineFactory();
    }

    // ============================================================
    // START / PAUSE / RESUME
    // ============================================================
    //
    // El controlador no crea threads ni timers.
    // Start/Resume solo habilitan la ejecución.
    // El host (Console, WPF, tests, etc.) decide cuándo llamar Tick().
    //

    public void Start()
    {
        if (!CanStep)
        {
            IsRunning = false;
            return;
        }

        IsRunning = true;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        Start();
    }

    // ============================================================
    // TICK
    // ============================================================
    //
    // Avanza exactamente un ciclo SOLO si el controlador está
    // corriendo. Esto encaja directamente con un DispatcherTimer
    // futuro en WPF.
    //

    public SimulationStepResult? Tick()
    {
        if (!IsRunning)
        {
            return null;
        }

        return ExecuteOneStep();
    }

    // ============================================================
    // STEP MANUAL
    // ============================================================
    //
    // Un paso manual deja la simulación pausada y ejecuta un único
    // ciclo. No cambia ninguna regla interna del SimulationEngine.
    //

    public SimulationStepResult? Step()
    {
        IsRunning = false;

        return ExecuteOneStep();
    }

    // ============================================================
    // RESET
    // ============================================================
    //
    // Reconstruye completamente el motor usando la factory recibida.
    // Con la misma SimulationConfig vuelve a la misma seed inicial.
    //

    public void Reset()
    {
        IsRunning = false;

        _engine =
            _engineFactory();

        LastResult =
            null;
    }

    private SimulationStepResult? ExecuteOneStep()
    {
        if (!CanStep)
        {
            IsRunning = false;
            return null;
        }

        SimulationStepResult? result =
            _engine.Step();

        LastResult =
            result;

        if (!CanStep)
        {
            IsRunning = false;
        }

        return result;
    }
}
