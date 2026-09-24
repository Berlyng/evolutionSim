using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class GenomeMutator
{
    private readonly Random _random;
    private readonly Random? _structuralRandom;
    private readonly GenomeStructuralMutationSettings
        _structuralSettings;

    private const double MutationChance =
        0.05;


    public GenomeMutator(
        Random random)
    {
        ArgumentNullException.ThrowIfNull(
            random
        );

        _random =
            random;

        _structuralRandom =
            null;

        _structuralSettings =
            GenomeStructuralMutationSettings.Default;
    }


    public GenomeMutator(
        Random random,
        Random structuralRandom,
        GenomeStructuralMutationSettings? structuralSettings = null)
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

        _structuralSettings =
            structuralSettings
            ??
            GenomeStructuralMutationSettings.Default;
    }


    public Genome Mutate(
        Genome parent)
    {
        ArgumentNullException.ThrowIfNull(
            parent
        );


        Dictionary<GenomeTraitId, double>
            mutatedTraits =
                new(
                    parent.Traits
                );


        // =====================================================
        // MUTACIÓN DE LOS 11 TRAITS VALIDADOS
        // =====================================================
        //
        // Usa exclusivamente _random y conserva exactamente el
        // orden/cantidad histórica de llamadas al RNG principal.
        //

        foreach (
            GenomeTraitId traitId
            in GenomeTraitCatalog.MutationOrder
        )
        {
            GenomeTraitDefinition definition =
                GenomeTraitCatalog.Get(
                    traitId
                );


            double currentValue =
                parent.GetTrait(
                    traitId
                );


            mutatedTraits[
                traitId
            ] =
                MutateValue(
                    currentValue,
                    definition
                );
        }


        IReadOnlyList<GenomeLocus>
            structuralLoci =
                _structuralRandom is null
                    ? parent.StructuralLoci
                    : MutateStructuralLoci(
                        parent.StructuralLoci
                    );


        return new Genome(
            mutatedTraits,
            structuralLoci
        );
    }


    private double MutateValue(
        double value,
        GenomeTraitDefinition definition)
    {
        if (
            _random.NextDouble()
            >
            MutationChance
        )
        {
            return value;
        }


        double mutation =
            (
                _random.NextDouble()
                -
                0.5
            )
            *
            definition.MutationStrength;


        return definition.ClampMutationValue(
            value
            +
            mutation
        );
    }


    private IReadOnlyList<GenomeLocus>
        MutateStructuralLoci(
            IReadOnlyList<GenomeLocus> inheritedLoci)
    {
        Random structuralRandom =
            _structuralRandom!;


        List<GenomeLocus>
            result =
                new();


        // -----------------------------------------------------
        // MUTACIONES POR LOCUS
        // -----------------------------------------------------

        foreach (
            GenomeLocus inherited
            in inheritedLoci
                .OrderBy(
                    locus =>
                        locus.LocusId
                )
        )
        {
            if (
                structuralRandom.NextDouble()
                <
                _structuralSettings
                    .LossChancePerLocus
            )
            {
                continue;
            }


            GenomeLocus locus =
                inherited;


            if (
                structuralRandom.NextDouble()
                <
                _structuralSettings
                    .ActivationToggleChancePerLocus
            )
            {
                locus =
                    locus with
                    {
                        IsActive =
                            !locus.IsActive
                    };
            }


            if (
                structuralRandom.NextDouble()
                <
                _structuralSettings
                    .ValueMutationChancePerLocus
            )
            {
                double delta =
                    (
                        structuralRandom.NextDouble()
                        -
                        0.5
                    )
                    *
                    _structuralSettings
                        .ValueMutationStrength;


                locus =
                    locus.WithValue(
                        locus.Value
                        +
                        delta
                    );
            }


            result.Add(
                locus
            );
        }


        // -----------------------------------------------------
        // DUPLICACIÓN
        // -----------------------------------------------------

        if (
            result.Count > 0
            &&
            result.Count
            <
            _structuralSettings
                .MaximumStructuralLoci
            &&
            structuralRandom.NextDouble()
            <
            _structuralSettings
                .DuplicationChance
        )
        {
            GenomeLocus source =
                result[
                    structuralRandom.Next(
                        result.Count
                    )
                ];


            long newLocusId =
                CreateUniqueLocusId(
                    structuralRandom,
                    result
                );


            result.Add(
                new GenomeLocus(
                    LocusId:
                        newLocusId,
                    FamilyId:
                        source.FamilyId,
                    Value:
                        source.Value,
                    IsActive:
                        source.IsActive
                )
            );
        }


        // -----------------------------------------------------
        // INNOVACIÓN DE NOVO
        // -----------------------------------------------------

        if (
            result.Count
            <
            _structuralSettings
                .MaximumStructuralLoci
            &&
            structuralRandom.NextDouble()
            <
            _structuralSettings
                .InnovationChance
        )
        {
            long locusId =
                CreateUniqueLocusId(
                    structuralRandom,
                    result
                );


            result.Add(
                new GenomeLocus(
                    LocusId:
                        locusId,
                    FamilyId:
                        locusId,
                    Value:
                        structuralRandom.NextDouble(),
                    IsActive:
                        true
                )
            );
        }


        return result
            .OrderBy(
                locus =>
                    locus.LocusId
            )
            .ToArray();
    }


    private static long CreateUniqueLocusId(
        Random random,
        IReadOnlyCollection<GenomeLocus> existing)
    {
        HashSet<long> existingIds =
            existing
                .Select(
                    locus =>
                        locus.LocusId
                )
                .ToHashSet();


        long id;


        do
        {
            id =
                random.NextInt64(
                    1,
                    long.MaxValue
                );
        }
        while (
            existingIds.Contains(
                id
            )
        );


        return id;
    }
}
