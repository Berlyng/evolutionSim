using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class GeneticDistanceCalculator
{
    public double Calculate(
        Genome genomeA,
        Genome genomeB)
    {
        ArgumentNullException.ThrowIfNull(
            genomeA
        );

        ArgumentNullException.ThrowIfNull(
            genomeB
        );


        double squaredDistance =
            0;


        foreach (
            GenomeTraitId traitId
            in GenomeTraitCatalog.GeneticDistanceOrder
        )
        {
            GenomeTraitDefinition definition =
                GenomeTraitCatalog.Get(
                    traitId
                );

            double difference =
                (
                    genomeA.GetTrait(
                        traitId
                    )
                    -
                    genomeB.GetTrait(
                        traitId
                    )
                )
                /
                definition.GeneticDistanceScale;


            squaredDistance +=
                difference
                *
                difference;
        }


        // Exactamente la misma normalización histórica:
        // raíz de la media de 11 diferencias normalizadas al cuadrado.
        return Math.Sqrt(
            squaredDistance
            /
            GenomeTraitCatalog
                .GeneticDistanceOrder
                .Count
        );
    }
}
