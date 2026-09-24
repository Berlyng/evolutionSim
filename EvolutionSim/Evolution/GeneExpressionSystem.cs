using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

/// <summary>
/// Fachada de expresión genética usada por diagnósticos.
///
/// La implementación canónica vive en GenomePhenotypeMapper para que
/// Organism y las estadísticas compartan exactamente la misma regla.
/// </summary>
public sealed class GeneExpressionSystem
{
    public PhenotypeProfile Express(
        Genome genome)
    {
        return GenomePhenotypeMapper.Express(
            genome
        );
    }


    public PhenotypeChannel ResolveChannel(
        long familyId)
    {
        return GenomePhenotypeMapper.ResolveChannel(
            familyId
        );
    }
}
