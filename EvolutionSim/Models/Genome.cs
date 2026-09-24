namespace EvolutionSim.Models;

/// <summary>
/// Genoma extensible.
///
/// Los 11 traits históricos siguen siendo obligatorios y conservan la API
/// tipada validada. Además, desde Phase 7.2 el genoma puede transportar
/// loci estructurales neutrales heredables.
/// </summary>
public sealed class Genome
{
    private readonly IReadOnlyDictionary<
        GenomeTraitId,
        double
    > _traits;

    private readonly IReadOnlyList<
        GenomeLocus
    > _structuralLoci;


    public IReadOnlyDictionary<
        GenomeTraitId,
        double
    > Traits =>
        _traits;


    public IReadOnlyList<
        GenomeLocus
    > StructuralLoci =>
        _structuralLoci;


    public int StructuralLocusCount =>
        _structuralLoci.Count;


    public int ActiveStructuralLocusCount =>
        _structuralLoci.Count(
            locus =>
                locus.IsActive
        );


    // =========================================================
    // API HISTÓRICA
    // =========================================================

    public double Speed =>
        GetTrait(
            GenomeTraits.Speed
        );

    public double Size =>
        GetTrait(
            GenomeTraits.Size
        );

    public double Metabolism =>
        GetTrait(
            GenomeTraits.Metabolism
        );

    public double OptimalTemperature =>
        GetTrait(
            GenomeTraits.OptimalTemperature
        );

    public double ThermalTolerance =>
        GetTrait(
            GenomeTraits.ThermalTolerance
        );

    public double MeatAdaptation =>
        GetTrait(
            GenomeTraits.MeatAdaptation
        );

    public double PredatoryDrive =>
        GetTrait(
            GenomeTraits.PredatoryDrive
        );

    public double ExplorationDrive =>
        GetTrait(
            GenomeTraits.ExplorationDrive
        );

    public double RiskTolerance =>
        GetTrait(
            GenomeTraits.RiskTolerance
        );

    public double ScavengingDrive =>
        GetTrait(
            GenomeTraits.ScavengingDrive
        );

    public double MateSelectivity =>
        GetTrait(
            GenomeTraits.MateSelectivity
        );


    /// <summary>
    /// Constructor histórico. No crea loci estructurales.
    /// InitialPopulationFactory permanece exactamente igual.
    /// </summary>
    public Genome(
        double speed,
        double size,
        double metabolism,
        double optimalTemperature,
        double thermalTolerance,
        double meatAdaptation,
        double predatoryDrive,
        double explorationDrive,
        double riskTolerance,
        double scavengingDrive,
        double mateSelectivity)
        : this(
            new Dictionary<GenomeTraitId, double>
            {
                [GenomeTraits.Speed] =
                    speed,

                [GenomeTraits.Size] =
                    size,

                [GenomeTraits.Metabolism] =
                    metabolism,

                [GenomeTraits.OptimalTemperature] =
                    optimalTemperature,

                [GenomeTraits.ThermalTolerance] =
                    thermalTolerance,

                [GenomeTraits.MeatAdaptation] =
                    meatAdaptation,

                [GenomeTraits.PredatoryDrive] =
                    predatoryDrive,

                [GenomeTraits.ExplorationDrive] =
                    explorationDrive,

                [GenomeTraits.RiskTolerance] =
                    riskTolerance,

                [GenomeTraits.ScavengingDrive] =
                    scavengingDrive,

                [GenomeTraits.MateSelectivity] =
                    mateSelectivity
            },
            Array.Empty<GenomeLocus>()
        )
    {
    }


    public Genome(
        IReadOnlyDictionary<
            GenomeTraitId,
            double
        > traits)
        : this(
            traits,
            Array.Empty<GenomeLocus>()
        )
    {
    }


    public Genome(
        IReadOnlyDictionary<
            GenomeTraitId,
            double
        > traits,
        IReadOnlyList<
            GenomeLocus
        > structuralLoci)
    {
        ArgumentNullException.ThrowIfNull(
            traits
        );

        ArgumentNullException.ThrowIfNull(
            structuralLoci
        );


        Dictionary<GenomeTraitId, double>
            normalized =
                new();


        foreach (
            KeyValuePair<
                GenomeTraitId,
                double
            > pair
            in traits
        )
        {
            double value =
                GenomeTraitCatalog.IsKnown(
                    pair.Key
                )
                    ? GenomeTraitCatalog
                        .Get(
                            pair.Key
                        )
                        .ClampGenomeValue(
                            pair.Value
                        )
                    : pair.Value;


            normalized[
                pair.Key
            ] =
                value;
        }


        EnsureCurrentCoreTraitsExist(
            normalized
        );


        _traits =
            normalized;


        _structuralLoci =
            structuralLoci
                .GroupBy(
                    locus =>
                        locus.LocusId
                )
                .Select(
                    group =>
                        group.First()
                )
                .OrderBy(
                    locus =>
                        locus.LocusId
                )
                .ToArray();
    }


    public double GetTrait(
        GenomeTraitId traitId)
    {
        if (
            !_traits.TryGetValue(
                traitId,
                out double value
            )
        )
        {
            throw new KeyNotFoundException(
                $"El genoma no contiene el trait '{traitId}'."
            );
        }

        return value;
    }


    public bool TryGetTrait(
        GenomeTraitId traitId,
        out double value)
    {
        return _traits.TryGetValue(
            traitId,
            out value
        );
    }


    public bool HasTrait(
        GenomeTraitId traitId)
    {
        return _traits.ContainsKey(
            traitId
        );
    }


    public Genome WithTrait(
        GenomeTraitId traitId,
        double value)
    {
        Dictionary<GenomeTraitId, double>
            copy =
                new(
                    _traits
                );


        copy[
            traitId
        ] =
            value;


        return new Genome(
            copy,
            _structuralLoci
        );
    }


    public Genome WithStructuralLoci(
        IReadOnlyList<
            GenomeLocus
        > structuralLoci)
    {
        return new Genome(
            _traits,
            structuralLoci
        );
    }


    private static void EnsureCurrentCoreTraitsExist(
        IReadOnlyDictionary<
            GenomeTraitId,
            double
        > traits)
    {
        foreach (
            GenomeTraitId traitId
            in GenomeTraitCatalog.InheritanceOrder
        )
        {
            if (
                !traits.ContainsKey(
                    traitId
                )
            )
            {
                throw new ArgumentException(
                    $"Falta el trait núcleo '{traitId}' en el genoma."
                );
            }
        }
    }
}
