using EvolutionSim.Experiments;
using EvolutionSim.Simulation;

bool runLocomotionExperiment =
    args.Any(
        argument =>
            string.Equals(
                argument,
                "--locomotion-experiment",
                StringComparison.OrdinalIgnoreCase
            )
    );


bool runBodySizeExperiment =
    args.Any(
        argument =>
            string.Equals(
                argument,
                "--body-size-experiment",
                StringComparison.OrdinalIgnoreCase
            )
    );


bool runThermalExperiment =
    args.Any(
        argument =>
            string.Equals(
                argument,
                "--thermal-experiment",
                StringComparison.OrdinalIgnoreCase
            )
    );


bool runMetabolicExperiment =
    args.Any(
        argument =>
            string.Equals(
                argument,
                "--metabolic-experiment",
                StringComparison.OrdinalIgnoreCase
            )
    );


if (runLocomotionExperiment)
{
    LocomotionExperimentRunner experiment =
        new();


    experiment.RunDefault();


    return;
}


if (runBodySizeExperiment)
{
    BodySizeExperimentRunner experiment =
        new();


    experiment.RunDefault();


    return;
}


if (runThermalExperiment)
{
    ThermalRegulationExperimentRunner experiment =
        new();


    experiment.RunDefault();


    return;
}


if (runMetabolicExperiment)
{
    MetabolicCouplingExperimentRunner experiment =
        new();


    experiment.RunDefault();


    return;
}


SimulationRunner simulation =
    new();


simulation.Run();
