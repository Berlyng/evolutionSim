using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class GenomeRecombiner
{
    private readonly Random _random;
    private readonly Random? _structuralRandom;


    public GenomeRecombiner(
        Random random)
    {
        ArgumentNullException.ThrowIfNull(
            random
        );

        _random =
            random;

        _structuralRandom =
            null;
    }


    public GenomeRecombiner(
        Random random,
        Random structuralRandom)
    {
        ArgumentNullException.ThrowIfNull(
            random
        );

        ArgumentNullException.ThrowIfNull(
            structuralRandom
        );

        _random =
            random;

        _structuralRandom =
            structuralRandom;
    }


    public Genome Combine(
        Genome parentA,
        Genome parentB)
    {
        ArgumentNullException.ThrowIfNull(
            parentA
        );

        ArgumentNullException.ThrowIfNull(
            parentB
        );


        Dictionary<GenomeTraitId, double>
            childTraits =
                new();


        // =====================================================
        // TRAITS HISTÓRICOS
        // =====================================================
        //
        // Exactamente 11 NextDouble del RNG principal.
        //

        foreach (
            GenomeTraitId traitId
            in GenomeTraitCatalog.InheritanceOrder
        )
        {
            childTraits[
                traitId
            ] =
                Inherit(
                    parentA.GetTrait(
                        traitId
                    ),
                    parentB.GetTrait(
                        traitId
                    )
                );
        }


        IReadOnlyList<GenomeLocus>
            inheritedStructuralLoci =
                _structuralRandom is null
                    ? InheritSharedStructuralLociDeterministically(
                        parentA,
                        parentB
                    )
                    : InheritStructuralLoci(
                        parentA,
                        parentB
                    );


        return new Genome(
            childTraits,
            inheritedStructuralLoci
        );
    }


    private IReadOnlyList<GenomeLocus>
        InheritStructuralLoci(
            Genome parentA,
            Genome parentB)
    {
        Random structuralRandom =
            _structuralRandom!;


        Dictionary<long, GenomeLocus>
            lociA =
                parentA.StructuralLoci
                    .ToDictionary(
                        locus =>
                            locus.LocusId
                    );


        Dictionary<long, GenomeLocus>
            lociB =
                parentB.StructuralLoci
                    .ToDictionary(
                        locus =>
                            locus.LocusId
                    );


        long[] allIds =
            lociA.Keys
                .Concat(
                    lociB.Keys
                )
                .Distinct()
                .OrderBy(
                    id =>
                        id
                )
                .ToArray();


        List<GenomeLocus>
            inherited =
                new();


        foreach (
            long locusId
            in allIds
        )
        {
            bool hasA =
                lociA.TryGetValue(
                    locusId,
                    out GenomeLocus? locusA
                );

            bool hasB =
                lociB.TryGetValue(
                    locusId,
                    out GenomeLocus? locusB
                );


            if (
                hasA
                &&
                hasB
            )
            {
                inherited.Add(
                    structuralRandom.NextDouble()
                    <
                    0.5

                        ?

                        locusA!

                        :

                        locusB!
                );

                continue;
            }


            if (
                structuralRandom.NextDouble()
                <
                0.5
            )
            {
                inherited.Add(
                    hasA
                        ? locusA!
                        : locusB!
                );
            }
        }


        return inherited;
    }


    private static IReadOnlyList<GenomeLocus>
        InheritSharedStructuralLociDeterministically(
            Genome parentA,
            Genome parentB)
    {
        Dictionary<long, GenomeLocus>
            lociB =
                parentB.StructuralLoci
                    .ToDictionary(
                        locus =>
                            locus.LocusId
                    );


        return parentA.StructuralLoci
            .Where(
                locusA =>
                    lociB.TryGetValue(
                        locusA.LocusId,
                        out GenomeLocus? locusB
                    )
                    &&
                    locusA.Equals(
                        locusB
                    )
            )
            .OrderBy(
                locus =>
                    locus.LocusId
            )
            .ToArray();
    }


    private double Inherit(
        double parentA,
        double parentB)
    {
        return
            _random.NextDouble()
            <
            0.5

                ?

                parentA

                :

                parentB;
    }
}
