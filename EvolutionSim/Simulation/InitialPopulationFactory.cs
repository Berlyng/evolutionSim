using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public static class InitialPopulationFactory
{
    public static List<Organism> Create(
        Random random,
        int populationSize,
        Region startingRegion,
        double initialEnergy)
    {
        List<Organism> population =
            new();

        for (
            int i = 0;
            i < populationSize;
            i++
        )
        {
            //
            // IMPORTANTE PARA REPRODUCIBILIDAD:
            //
            // Este orden de llamadas a Random es exactamente
            // el mismo que tenía el Program/SimulationRunner
            // validado con seed 98765.
            //
            // 1 Next para el patch.
            // 11 NextDouble para los 11 rasgos del genoma.
            //

            int initialPatchId =
                random.Next(
                    0,
                    3
                );

            Genome genome =
                new(
                    speed:
                        0.8
                        +
                        random.NextDouble()
                        *
                        0.4,

                    size:
                        0.7
                        +
                        random.NextDouble()
                        *
                        0.6,

                    metabolism:
                        0.8
                        +
                        random.NextDouble()
                        *
                        0.4,

                    optimalTemperature:
                        23
                        +
                        random.NextDouble()
                        *
                        2,

                    thermalTolerance:
                        4
                        +
                        random.NextDouble()
                        *
                        2,

                    meatAdaptation:
                        0.02
                        +
                        random.NextDouble()
                        *
                        0.06,

                    predatoryDrive:
                        0.01
                        +
                        random.NextDouble()
                        *
                        0.03,

                    explorationDrive:
                        0.05
                        +
                        random.NextDouble()
                        *
                        0.10,

                    riskTolerance:
                        0.35
                        +
                        random.NextDouble()
                        *
                        0.30,

                    scavengingDrive:
                        0.35
                        +
                        random.NextDouble()
                        *
                        0.30,

                    mateSelectivity:
                        0.35
                        +
                        random.NextDouble()
                        *
                        0.30
                );

            Organism organism =
                new(
                    genome:
                        genome,

                    regionId:
                        startingRegion.Id,

                    patchId:
                        initialPatchId,

                    initialEnergy:
                        initialEnergy,

                    parentAId:
                        null,

                    parentBId:
                        null,

                    generation:
                        0
                );

            population.Add(
                organism
            );
        }

        return population;
    }
}
