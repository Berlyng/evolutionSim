using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class MateCompatibilityCalculator
{
    private readonly GeneticDistanceCalculator
        _distanceCalculator;

    //
    // Determina qué tan rápidamente cae
    // la compatibilidad con la distancia.
    //

    private const double IsolationStrength =
        0.7;


    public MateCompatibilityCalculator(
        GeneticDistanceCalculator
            distanceCalculator)
    {
        _distanceCalculator =
            distanceCalculator;
    }


    public double CalculateCompatibility(
        Genome genomeA,
        Genome genomeB)
    {
        double geneticDistance =
            _distanceCalculator.Calculate(
                genomeA,
                genomeB
            );

        //
        // Función suave:
        //
        // compatibility =
        // e ^ (-k * distance²)
        //
        // No existe un corte mágico donde
        // repentinamente dejan de reproducirse.
        //

        double compatibility =
            Math.Exp(
                -IsolationStrength
                *
                geneticDistance
                *
                geneticDistance
            );

        return Math.Clamp(
            compatibility,
            0,
            1
        );
    }
}