using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

/// <summary>
/// Analiza divergencia morfológica entre regiones y entre especies.
///
/// Fase 8.2:
/// completamente diagnóstico y pasivo.
///
/// La distancia se calcula mediante diferencias estandarizadas:
///
/// ZDelta = |MediaA - MediaB| / StdGlobal
///
/// StdEffectDistance = sqrt(promedio(ZDelta²))
///
/// La contribución de cada dimensión se calcula como:
///
/// ZDelta² / suma(ZDelta²)
///
/// De esta forma BodyScale y los modificadores estructurales pueden
/// compararse aunque vivan en escalas numéricas diferentes.
///
/// No utiliza Random.
/// No modifica organismos.
/// No modifica especies.
/// </summary>
public sealed class MorphologyDivergenceCalculator
{
    private const double MinimumStandardDeviation =
        1e-9;


    private readonly GeneticDistanceCalculator
        _geneticDistanceCalculator;


    private static readonly MorphologyDimensionDefinition[]
        Dimensions =
        [
            new(
                Name:
                    "BodyScale",
                Selector:
                    organism =>
                        organism.Morphology.BodyScale
            ),

            new(
                Name:
                    nameof(
                        MorphologyChannel.LimbLength
                    ),
                Selector:
                    organism =>
                        organism
                            .Morphology
                            .LimbLengthModifier
            ),

            new(
                Name:
                    nameof(
                        MorphologyChannel.LimbRobustness
                    ),
                Selector:
                    organism =>
                        organism
                            .Morphology
                            .LimbRobustnessModifier
            ),

            new(
                Name:
                    nameof(
                        MorphologyChannel.Insulation
                    ),
                Selector:
                    organism =>
                        organism
                            .Morphology
                            .InsulationModifier
            ),

            new(
                Name:
                    nameof(
                        MorphologyChannel.JawStrength
                    ),
                Selector:
                    organism =>
                        organism
                            .Morphology
                            .JawStrengthModifier
            ),

            new(
                Name:
                    nameof(
                        MorphologyChannel.DigestiveStructure
                    ),
                Selector:
                    organism =>
                        organism
                            .Morphology
                            .DigestiveStructureModifier
            )
        ];


    public MorphologyDivergenceCalculator(
        GeneticDistanceCalculator geneticDistanceCalculator)
    {
        _geneticDistanceCalculator =
            geneticDistanceCalculator
            ??
            throw new ArgumentNullException(
                nameof(
                    geneticDistanceCalculator
                )
            );
    }


    public MorphologyDivergenceAnalysis Calculate(
        Worldd world,
        IEnumerable<Organism> population,
        IReadOnlyList<SpeciesCluster> detectedSpecies,
        bool calculateSpeciesComparisons)
    {
        ArgumentNullException.ThrowIfNull(
            world
        );

        ArgumentNullException.ThrowIfNull(
            population
        );

        ArgumentNullException.ThrowIfNull(
            detectedSpecies
        );


        List<Organism> living =
            population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .ToList();


        IReadOnlyDictionary<string, double>
            globalStandardDeviations =
                CalculateGlobalStandardDeviations(
                    living
                );


        List<MorphologyGroupSummary>
            regionalGroups =
                world.Regions
                    .Select(
                        region =>
                        {
                            List<Organism> members =
                                living
                                    .Where(
                                        organism =>
                                            organism.RegionId
                                            ==
                                            region.Id
                                    )
                                    .ToList();


                            return BuildGroupSummary(
                                groupId:
                                    region.Id,
                                name:
                                    region.Name,
                                members:
                                    members
                            );
                        }
                    )
                    .ToList();


        IReadOnlyList<MorphologyDivergenceComparison>
            regionalComparisons =
                CompareAllGroups(
                    regionalGroups,
                    globalStandardDeviations
                );


        SpeciesMorphologyAssignmentDiagnostics?
            speciesAssignment =
                null;


        IReadOnlyList<MorphologyGroupSummary>
            speciesGroups =
                Array.Empty<MorphologyGroupSummary>();


        IReadOnlyList<MorphologyDivergenceComparison>
            speciesComparisons =
                Array.Empty<MorphologyDivergenceComparison>();


        if (
            calculateSpeciesComparisons
            &&
            detectedSpecies.Count
            >
            0
        )
        {
            SpeciesAssignmentResult assignment =
                AssignSpeciesMembership(
                    living,
                    detectedSpecies
                );


            speciesAssignment =
                assignment.Diagnostics;


            speciesGroups =
                detectedSpecies
                    .OrderBy(
                        species =>
                            species.SpeciesId
                    )
                    .Select(
                        species =>
                            BuildGroupSummary(
                                groupId:
                                    species.SpeciesId,

                                name:
                                    $"S{species.SpeciesId}",

                                members:
                                    assignment
                                        .MembersBySpeciesId
                                        .GetValueOrDefault(
                                            species.SpeciesId,
                                            new List<Organism>()
                                        )
                            )
                    )
                    .ToList();


            speciesComparisons =
                CompareAllGroups(
                    speciesGroups,
                    globalStandardDeviations
                );
        }


        return new MorphologyDivergenceAnalysis(
            Population:
                living.Count,

            GlobalStandardDeviations:
                globalStandardDeviations,

            RegionalGroups:
                regionalGroups,

            RegionalComparisons:
                regionalComparisons,

            SpeciesGroups:
                speciesGroups,

            SpeciesComparisons:
                speciesComparisons,

            SpeciesAssignment:
                speciesAssignment
        );
    }


    /// <summary>
    /// Reconstruye una membresía diagnóstica para las especies confirmadas
    /// usando sus centroides genéticos y sus tamaños oficiales.
    ///
    /// SpeciesCluster no expone actualmente los IDs originales de sus
    /// miembros. Por eso esta fase NO pretende modificar ni reemplazar
    /// SpeciesDetectionSystem.
    ///
    /// En ciclos de detección, la suma de SpeciesCluster.Population debe
    /// coincidir con la población viva. Bajo esa condición asignamos
    /// exactamente la capacidad oficial de cada especie.
    ///
    /// Los organismos con separación genética más clara entre su primera
    /// y segunda opción se asignan primero.
    ///
    /// Esto produce una reconstrucción determinista y conserva los tamaños
    /// oficiales, pero se reporta explícitamente como una asignación
    /// diagnóstica por centroide.
    /// </summary>
    private SpeciesAssignmentResult AssignSpeciesMembership(
        IReadOnlyList<Organism> living,
        IReadOnlyList<SpeciesCluster> species)
    {
        Dictionary<int, List<Organism>>
            membersBySpeciesId =
                species.ToDictionary(
                    item =>
                        item.SpeciesId,

                    _ =>
                        new List<Organism>()
                );


        int officialPopulationSum =
            species.Sum(
                item =>
                    item.Population
            );


        bool capacitiesMatchPopulation =
            officialPopulationSum
            ==
            living.Count;


        if (
            living.Count
            ==
            0
            ||
            species.Count
            ==
            0
        )
        {
            return new SpeciesAssignmentResult(
                MembersBySpeciesId:
                    membersBySpeciesId,

                Diagnostics:
                    new SpeciesMorphologyAssignmentDiagnostics(
                        Population:
                            living.Count,
                        OfficialSpeciesPopulationSum:
                            officialPopulationSum,
                        CapacitiesMatchPopulation:
                            capacitiesMatchPopulation,
                        AssignedPopulation:
                            0,
                        DirectNearestAssignments:
                            0,
                        DirectNearestAssignmentFraction:
                            0,
                        MeanAssignedGeneticDistance:
                            0
                    )
            );
        }


        Dictionary<int, int> remainingCapacity =
            species.ToDictionary(
                item =>
                    item.SpeciesId,

                item =>
                    Math.Max(
                        0,
                        item.Population
                    )
            );


        List<OrganismSpeciesPreference>
            preferences =
                living
                    .Select(
                        organism =>
                        BuildPreference(
                            organism,
                            species
                        )
                    )
                    .OrderByDescending(
                        preference =>
                            preference.ConfidenceMargin
                    )
                    .ThenBy(
                        preference =>
                            preference.Organism.Id
                    )
                    .ToList();


        int directNearestAssignments =
            0;


        double assignedDistanceSum =
            0;


        int assignedPopulation =
            0;


        foreach (
            OrganismSpeciesPreference preference
            in preferences
        )
        {
            SpeciesDistance? selected =
                preference
                    .RankedSpecies
                    .FirstOrDefault(
                        candidate =>
                            remainingCapacity
                                .GetValueOrDefault(
                                    candidate.SpeciesId
                                )
                            >
                            0
                    );


            if (
                selected is null
            )
            {
                continue;
            }


            membersBySpeciesId[
                selected.SpeciesId
            ].Add(
                preference.Organism
            );


            remainingCapacity[
                selected.SpeciesId
            ]--;


            assignedPopulation++;


            assignedDistanceSum +=
                selected.GeneticDistance;


            if (
                preference.RankedSpecies.Count
                >
                0
                &&
                selected.SpeciesId
                ==
                preference
                    .RankedSpecies[
                        0
                    ]
                    .SpeciesId
            )
            {
                directNearestAssignments++;
            }
        }


        // Si las capacidades oficiales no coinciden con la población del
        // ciclo, pueden quedar organismos sin asignar.
        //
        // Esto normalmente solo ocurriría si se solicitara el diagnóstico
        // fuera de un ciclo de detección. SimulationEngine evita ese caso.
        //
        // No forzamos organismos adicionales dentro de una especie porque
        // eso ocultaría una inconsistencia de datos.


        return new SpeciesAssignmentResult(
            MembersBySpeciesId:
                membersBySpeciesId,

            Diagnostics:
                new SpeciesMorphologyAssignmentDiagnostics(
                    Population:
                        living.Count,

                    OfficialSpeciesPopulationSum:
                        officialPopulationSum,

                    CapacitiesMatchPopulation:
                        capacitiesMatchPopulation,

                    AssignedPopulation:
                        assignedPopulation,

                    DirectNearestAssignments:
                        directNearestAssignments,

                    DirectNearestAssignmentFraction:
                        assignedPopulation
                        >
                        0

                            ?

                            directNearestAssignments
                            /
                            (double)assignedPopulation

                            :

                            0,

                    MeanAssignedGeneticDistance:
                        assignedPopulation
                        >
                        0

                            ?

                            assignedDistanceSum
                            /
                            assignedPopulation

                            :

                            0
                )
        );
    }


    private OrganismSpeciesPreference BuildPreference(
        Organism organism,
        IReadOnlyList<SpeciesCluster> species)
    {
        List<SpeciesDistance> ranked =
            species
                .Select(
                    item =>
                        new SpeciesDistance(
                            SpeciesId:
                                item.SpeciesId,

                            GeneticDistance:
                                _geneticDistanceCalculator
                                    .Calculate(
                                        organism.Genome,
                                        item.Centroid
                                    )
                        )
                )
                .OrderBy(
                    item =>
                        item.GeneticDistance
                )
                .ThenBy(
                    item =>
                        item.SpeciesId
                )
                .ToList();


        double confidenceMargin =
            ranked.Count
            >=
            2

                ?

                ranked[
                    1
                ].GeneticDistance
                -
                ranked[
                    0
                ].GeneticDistance

                :

                double.PositiveInfinity;


        return new OrganismSpeciesPreference(
            Organism:
                organism,

            RankedSpecies:
                ranked,

            ConfidenceMargin:
                confidenceMargin
        );
    }


    private static IReadOnlyDictionary<string, double>
        CalculateGlobalStandardDeviations(
            IReadOnlyList<Organism> population)
    {
        Dictionary<string, double> result =
            new(
                StringComparer.Ordinal
            );


        foreach (
            MorphologyDimensionDefinition dimension
            in Dimensions
        )
        {
            double standardDeviation =
                StandardDeviation(
                    population
                        .Select(
                            dimension.Selector
                        )
                );


            result[
                dimension.Name
            ] =
                standardDeviation;
        }


        return result;
    }


    private static MorphologyGroupSummary BuildGroupSummary(
        int groupId,
        string name,
        IReadOnlyList<Organism> members)
    {
        Dictionary<string, MorphologyGroupDimensionSummary>
            dimensions =
                new(
                    StringComparer.Ordinal
                );


        foreach (
            MorphologyDimensionDefinition dimension
            in Dimensions
        )
        {
            double[] values =
                members
                    .Select(
                        dimension.Selector
                    )
                    .ToArray();


            dimensions[
                dimension.Name
            ] =
                new MorphologyGroupDimensionSummary(
                    Name:
                        dimension.Name,

                    Average:
                        values.Length
                        >
                        0

                            ?

                            values.Average()

                            :

                            0,

                    StandardDeviation:
                        StandardDeviation(
                            values
                        ),

                    Minimum:
                        values.Length
                        >
                        0

                            ?

                            values.Min()

                            :

                            0,

                    Maximum:
                        values.Length
                        >
                        0

                            ?

                            values.Max()

                            :

                            0
                );
        }


        return new MorphologyGroupSummary(
            GroupId:
                groupId,

            Name:
                name,

            Population:
                members.Count,

            Dimensions:
                dimensions
        );
    }


    private static IReadOnlyList<MorphologyDivergenceComparison>
        CompareAllGroups(
            IReadOnlyList<MorphologyGroupSummary> groups,
            IReadOnlyDictionary<string, double> globalStandardDeviations)
    {
        List<MorphologyDivergenceComparison> comparisons =
            new();


        for (
            int i = 0;
            i < groups.Count;
            i++
        )
        {
            for (
                int j = i + 1;
                j < groups.Count;
                j++
            )
            {
                MorphologyGroupSummary groupA =
                    groups[
                        i
                    ];


                MorphologyGroupSummary groupB =
                    groups[
                        j
                    ];


                comparisons.Add(
                    CompareGroups(
                        groupA,
                        groupB,
                        globalStandardDeviations
                    )
                );
            }
        }


        return comparisons;
    }


    private static MorphologyDivergenceComparison CompareGroups(
        MorphologyGroupSummary groupA,
        MorphologyGroupSummary groupB,
        IReadOnlyDictionary<string, double> globalStandardDeviations)
    {
        List<MutableDimensionComparison>
            workingDimensions =
                new();


        double squaredEffectSum =
            0;


        foreach (
            MorphologyDimensionDefinition dimension
            in Dimensions
        )
        {
            MorphologyGroupDimensionSummary a =
                groupA.Dimensions[
                    dimension.Name
                ];


            MorphologyGroupDimensionSummary b =
                groupB.Dimensions[
                    dimension.Name
                ];


            double delta =
                Math.Abs(
                    a.Average
                    -
                    b.Average
                );


            double populationStandardDeviation =
                globalStandardDeviations
                    .GetValueOrDefault(
                        dimension.Name
                    );


            double zDelta =
                populationStandardDeviation
                >
                MinimumStandardDeviation

                    ?

                    delta
                    /
                    populationStandardDeviation

                    :

                    0;


            double squaredEffect =
                zDelta
                *
                zDelta;


            squaredEffectSum +=
                squaredEffect;


            workingDimensions.Add(
                new MutableDimensionComparison(
                    Name:
                        dimension.Name,

                    AverageA:
                        a.Average,

                    AverageB:
                        b.Average,

                    Delta:
                        delta,

                    PopulationStandardDeviation:
                        populationStandardDeviation,

                    ZDelta:
                        zDelta,

                    SquaredEffect:
                        squaredEffect
                )
            );
        }


        double standardizedEffectDistance =
            workingDimensions.Count
            >
            0

                ?

                Math.Sqrt(
                    squaredEffectSum
                    /
                    workingDimensions.Count
                )

                :

                0;


        IReadOnlyList<MorphologyDimensionDivergence>
            dimensions =
                workingDimensions
                    .Select(
                        item =>
                            new MorphologyDimensionDivergence(
                                Name:
                                    item.Name,

                                AverageA:
                                    item.AverageA,

                                AverageB:
                                    item.AverageB,

                                Delta:
                                    item.Delta,

                                PopulationStandardDeviation:
                                    item.PopulationStandardDeviation,

                                ZDelta:
                                    item.ZDelta,

                                ContributionPercent:
                                    squaredEffectSum
                                    >
                                    0

                                        ?

                                        item.SquaredEffect
                                        /
                                        squaredEffectSum
                                        *
                                        100

                                        :

                                        0
                            )
                    )
                    .OrderByDescending(
                        item =>
                            item.ContributionPercent
                    )
                    .ThenBy(
                        item =>
                            item.Name
                    )
                    .ToList();


        return new MorphologyDivergenceComparison(
            GroupAId:
                groupA.GroupId,

            GroupAName:
                groupA.Name,

            GroupAPopulation:
                groupA.Population,

            GroupBId:
                groupB.GroupId,

            GroupBName:
                groupB.Name,

            GroupBPopulation:
                groupB.Population,

            StandardizedEffectDistance:
                standardizedEffectDistance,

            Dimensions:
                dimensions
        );
    }


    private static double StandardDeviation(
        IEnumerable<double> values)
    {
        double[] array =
            values.ToArray();


        if (
            array.Length
            ==
            0
        )
        {
            return 0;
        }


        double average =
            array.Average();


        double variance =
            array.Average(
                value =>
                {
                    double difference =
                        value
                        -
                        average;


                    return
                        difference
                        *
                        difference;
                }
            );


        return Math.Sqrt(
            variance
        );
    }


    private sealed record MorphologyDimensionDefinition(
        string Name,
        Func<Organism, double> Selector
    );


    private sealed record SpeciesDistance(
        int SpeciesId,
        double GeneticDistance
    );


    private sealed record OrganismSpeciesPreference(
        Organism Organism,
        IReadOnlyList<SpeciesDistance> RankedSpecies,
        double ConfidenceMargin
    );


    private sealed record MutableDimensionComparison(
        string Name,
        double AverageA,
        double AverageB,
        double Delta,
        double PopulationStandardDeviation,
        double ZDelta,
        double SquaredEffect
    );


    private sealed record SpeciesAssignmentResult(
        Dictionary<int, List<Organism>> MembersBySpeciesId,
        SpeciesMorphologyAssignmentDiagnostics Diagnostics
    );
}


public sealed record MorphologyDivergenceAnalysis(
    int Population,
    IReadOnlyDictionary<string, double> GlobalStandardDeviations,
    IReadOnlyList<MorphologyGroupSummary> RegionalGroups,
    IReadOnlyList<MorphologyDivergenceComparison> RegionalComparisons,
    IReadOnlyList<MorphologyGroupSummary> SpeciesGroups,
    IReadOnlyList<MorphologyDivergenceComparison> SpeciesComparisons,
    SpeciesMorphologyAssignmentDiagnostics? SpeciesAssignment
);


public sealed record MorphologyGroupSummary(
    int GroupId,
    string Name,
    int Population,
    IReadOnlyDictionary<string, MorphologyGroupDimensionSummary> Dimensions
);


public sealed record MorphologyGroupDimensionSummary(
    string Name,
    double Average,
    double StandardDeviation,
    double Minimum,
    double Maximum
);


public sealed record MorphologyDivergenceComparison(
    int GroupAId,
    string GroupAName,
    int GroupAPopulation,
    int GroupBId,
    string GroupBName,
    int GroupBPopulation,
    double StandardizedEffectDistance,
    IReadOnlyList<MorphologyDimensionDivergence> Dimensions
);


public sealed record MorphologyDimensionDivergence(
    string Name,
    double AverageA,
    double AverageB,
    double Delta,
    double PopulationStandardDeviation,
    double ZDelta,
    double ContributionPercent
);


public sealed record SpeciesMorphologyAssignmentDiagnostics(
    int Population,
    int OfficialSpeciesPopulationSum,
    bool CapacitiesMatchPopulation,
    int AssignedPopulation,
    int DirectNearestAssignments,
    double DirectNearestAssignmentFraction,
    double MeanAssignedGeneticDistance
);
