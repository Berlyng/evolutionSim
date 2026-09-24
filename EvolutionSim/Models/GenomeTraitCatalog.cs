namespace EvolutionSim.Models;

/// <summary>
/// Catálogo central de traits actualmente usados por EvolutionSim.
///
/// En Phase 7.1 todavía tenemos exactamente los mismos 11 traits.
/// La diferencia es que sus metadatos ya no están dispersos entre Genome,
/// GenomeMutator y GeneticDistanceCalculator.
///
/// Los órdenes de MutationOrder e InheritanceOrder son deliberadamente
/// explícitos para preservar la secuencia histórica de llamadas al RNG.
/// </summary>
public static class GenomeTraitCatalog
{
    private static readonly IReadOnlyDictionary<
        GenomeTraitId,
        GenomeTraitDefinition
    > DefinitionsById =
        new Dictionary<GenomeTraitId, GenomeTraitDefinition>
        {
            [GenomeTraits.Speed] =
                new(
                    Id:
                        GenomeTraits.Speed,
                    GenomeMinimum:
                        0.1,
                    GenomeMaximum:
                        double.PositiveInfinity,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0.1,
                    MutationMaximum:
                        5,
                    GeneticDistanceScale:
                        0.50
                ),

            [GenomeTraits.Size] =
                new(
                    Id:
                        GenomeTraits.Size,
                    GenomeMinimum:
                        0.1,
                    GenomeMaximum:
                        double.PositiveInfinity,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0.1,
                    MutationMaximum:
                        10,
                    GeneticDistanceScale:
                        0.75
                ),

            [GenomeTraits.Metabolism] =
                new(
                    Id:
                        GenomeTraits.Metabolism,
                    GenomeMinimum:
                        0.1,
                    GenomeMaximum:
                        double.PositiveInfinity,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0.1,
                    MutationMaximum:
                        5,
                    GeneticDistanceScale:
                        0.50
                ),

            [GenomeTraits.OptimalTemperature] =
                new(
                    Id:
                        GenomeTraits.OptimalTemperature,
                    GenomeMinimum:
                        -30,
                    GenomeMaximum:
                        60,
                    MutationStrength:
                        2.0,
                    MutationMinimum:
                        -30,
                    MutationMaximum:
                        60,
                    GeneticDistanceScale:
                        8.0
                ),

            [GenomeTraits.ThermalTolerance] =
                new(
                    Id:
                        GenomeTraits.ThermalTolerance,
                    GenomeMinimum:
                        0.5,
                    GenomeMaximum:
                        30,
                    MutationStrength:
                        1.0,
                    MutationMinimum:
                        0.5,
                    MutationMaximum:
                        30,
                    GeneticDistanceScale:
                        4.0
                ),

            [GenomeTraits.MeatAdaptation] =
                new(
                    Id:
                        GenomeTraits.MeatAdaptation,
                    GenomeMinimum:
                        0,
                    GenomeMaximum:
                        1,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0,
                    MutationMaximum:
                        1,
                    GeneticDistanceScale:
                        0.30
                ),

            [GenomeTraits.PredatoryDrive] =
                new(
                    Id:
                        GenomeTraits.PredatoryDrive,
                    GenomeMinimum:
                        0,
                    GenomeMaximum:
                        1,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0,
                    MutationMaximum:
                        1,
                    GeneticDistanceScale:
                        0.30
                ),

            [GenomeTraits.ExplorationDrive] =
                new(
                    Id:
                        GenomeTraits.ExplorationDrive,
                    GenomeMinimum:
                        0,
                    GenomeMaximum:
                        1,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0,
                    MutationMaximum:
                        1,
                    GeneticDistanceScale:
                        0.30
                ),

            [GenomeTraits.RiskTolerance] =
                new(
                    Id:
                        GenomeTraits.RiskTolerance,
                    GenomeMinimum:
                        0,
                    GenomeMaximum:
                        1,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0,
                    MutationMaximum:
                        1,
                    GeneticDistanceScale:
                        0.30
                ),

            [GenomeTraits.ScavengingDrive] =
                new(
                    Id:
                        GenomeTraits.ScavengingDrive,
                    GenomeMinimum:
                        0,
                    GenomeMaximum:
                        1,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0,
                    MutationMaximum:
                        1,
                    GeneticDistanceScale:
                        0.30
                ),

            [GenomeTraits.MateSelectivity] =
                new(
                    Id:
                        GenomeTraits.MateSelectivity,
                    GenomeMinimum:
                        0,
                    GenomeMaximum:
                        1,
                    MutationStrength:
                        0.10,
                    MutationMinimum:
                        0,
                    MutationMaximum:
                        1,
                    GeneticDistanceScale:
                        0.30
                )
        };


    /// <summary>
    /// Orden histórico del constructor/recombinación.
    ///
    /// GenomeRecombiner consume exactamente 11 NextDouble en este orden.
    /// NO cambiar sin invalidar la reproducibilidad del experimento base.
    /// </summary>
    public static IReadOnlyList<GenomeTraitId>
        InheritanceOrder
    {
        get;
    } =
        new[]
        {
            GenomeTraits.Speed,
            GenomeTraits.Size,
            GenomeTraits.Metabolism,
            GenomeTraits.OptimalTemperature,
            GenomeTraits.ThermalTolerance,
            GenomeTraits.MeatAdaptation,
            GenomeTraits.PredatoryDrive,
            GenomeTraits.ExplorationDrive,
            GenomeTraits.RiskTolerance,
            GenomeTraits.ScavengingDrive,
            GenomeTraits.MateSelectivity
        };


    /// <summary>
    /// Orden histórico EXACTO de GenomeMutator.
    ///
    /// Nota importante:
    /// ExplorationDrive históricamente se muta ANTES de MeatAdaptation.
    /// Esto es distinto al orden del constructor.
    ///
    /// Mantener este orden preserva la secuencia de llamadas al RNG.
    /// </summary>
    public static IReadOnlyList<GenomeTraitId>
        MutationOrder
    {
        get;
    } =
        new[]
        {
            GenomeTraits.Speed,
            GenomeTraits.Size,
            GenomeTraits.Metabolism,
            GenomeTraits.OptimalTemperature,
            GenomeTraits.ThermalTolerance,
            GenomeTraits.ExplorationDrive,
            GenomeTraits.MeatAdaptation,
            GenomeTraits.PredatoryDrive,
            GenomeTraits.RiskTolerance,
            GenomeTraits.ScavengingDrive,
            GenomeTraits.MateSelectivity
        };


    /// <summary>
    /// Orden de los 11 traits usados por la métrica genética validada.
    /// </summary>
    public static IReadOnlyList<GenomeTraitId>
        GeneticDistanceOrder
    {
        get;
    } =
        InheritanceOrder;


    public static IReadOnlyCollection<
        GenomeTraitDefinition
    > Definitions =>
        DefinitionsById
            .Values
            .ToArray();


    public static bool IsKnown(
        GenomeTraitId traitId)
    {
        return DefinitionsById.ContainsKey(
            traitId
        );
    }


    public static GenomeTraitDefinition Get(
        GenomeTraitId traitId)
    {
        if (
            !DefinitionsById.TryGetValue(
                traitId,
                out GenomeTraitDefinition? definition
            )
        )
        {
            throw new KeyNotFoundException(
                $"No existe una definición registrada para el trait '{traitId}'."
            );
        }

        return definition;
    }
}
