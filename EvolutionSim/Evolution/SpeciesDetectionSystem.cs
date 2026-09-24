using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class SpeciesDetectionSystem
{
    private readonly GeneticDistanceCalculator
        _distanceCalculator;

    private readonly MateCompatibilityCalculator
        _compatibilityCalculator;

    private readonly EcologicalMateSimilarityCalculator
        _ecologicalSimilarityCalculator;


    //
    // ==========================================
    // CONFIGURACIÓN
    // ==========================================
    //

    //
    // La detección preliminar usa la misma
    // aceptación reproductiva que el sistema
    // real de apareamiento.
    //
    // No define por sí sola una especie:
    // solamente decide cuándo vale la pena
    // intentar separar un grupo.
    //

    private const double MinimumWithinClusterMateAcceptance =
        0.70;


    //
    // Usamos el percentil 10 de aceptación
    // respecto al centroide para que uno o dos
    // individuos extremos no partan por sí solos
    // toda la población.
    //

    private const double WithinClusterAcceptanceQuantile =
        0.10;


    //
    // Evita que una mutación rara de
    // unos pocos organismos sea tratada
    // como un grupo evolutivo independiente.
    //

    private const int MinimumClusterPopulation =
        30;


    //
    // Después de formar grupos preliminares,
    // si dos centroides todavía tendrían más
    // de 60% de aceptación de pareja, seguimos
    // considerándolos conectados reproductivamente
    // y los fusionamos.
    //
    // Un valor <= 0.60 NO confirma una especie:
    // solo permite que el grupo sobreviva como
    // candidato. Todavía debe persistir varias
    // detecciones consecutivas.
    //

    private const double MaximumMateAcceptanceForSeparateSpecies =
        0.60;


    //
    // Distancia máxima para considerar que
    // una especie detectada actualmente
    // corresponde a una especie que ya
    // conocíamos del ciclo anterior.
    //

    private const double SpeciesContinuityDistance =
        0.80;


    //
    // Lo mismo, pero para candidatos
    // todavía no confirmados.
    //

    private const double CandidateContinuityDistance =
        0.60;


    //
    // Un aislamiento debe aparecer en varias
    // detecciones consecutivas antes de recibir
    // un SpeciesId real.
    //
    // Con detección cada 25 ciclos:
    //
    // 4 detecciones ≈ 100 ciclos.
    //

    private const int RequiredPersistenceDetections =
        4;


    //
    // ==========================================
    // ESTADO
    // ==========================================
    //

    private int _nextSpeciesId =
        1;


    private List<ConfirmedSpeciesState>
        _confirmedSpecies =
            new();


    private List<PendingCandidateState>
        _pendingCandidates =
            new();


    public int PendingCandidateCount =>
        _pendingCandidates.Count;


    public SpeciesDetectionSystem(
        GeneticDistanceCalculator
            distanceCalculator,

        MateCompatibilityCalculator
            compatibilityCalculator,

        EcologicalMateSimilarityCalculator
            ecologicalSimilarityCalculator)
    {
        _distanceCalculator =
            distanceCalculator;

        _compatibilityCalculator =
            compatibilityCalculator;

        _ecologicalSimilarityCalculator =
            ecologicalSimilarityCalculator;
    }


    //
    // ==========================================
    // DETECTAR
    // ==========================================
    //

    public List<SpeciesCluster> Detect(
        List<Organism> population,
        int cycle)
    {
        List<Organism> livingPopulation =
            population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .ToList();


        if (
            livingPopulation.Count == 0
        )
        {
            _confirmedSpecies.Clear();

            _pendingCandidates.Clear();

            return new List<SpeciesCluster>();
        }


        //
        // ======================================
        // POBLACIÓN ANCESTRAL
        // ======================================
        //
        // En el ciclo inicial sabemos que
        // comenzamos con una sola población
        // reproductiva ancestral.
        //

        if (
            _confirmedSpecies.Count == 0
        )
        {
            Genome centroid =
                CalculateCentroid(
                    livingPopulation
                );


            int speciesId =
                _nextSpeciesId++;


            ConfirmedSpeciesState rootSpecies =
                new(
                    SpeciesId:
                        speciesId,

                    ParentSpeciesId:
                        null,

                    FirstDetectedCycle:
                        cycle,

                    Centroid:
                        centroid
                );


            _confirmedSpecies.Add(
                rootSpecies
            );


            return new List<SpeciesCluster>
            {
                BuildSpeciesCluster(
                    speciesId:
                        speciesId,

                    parentSpeciesId:
                        null,

                    firstDetectedCycle:
                        cycle,

                    members:
                        livingPopulation
                )
            };
        }


        //
        // ======================================
        // 1. GRUPOS REPRODUCTIVOS PRELIMINARES
        // ======================================
        //

        List<WorkingCluster> rawClusters =
            new();


        SplitRecursively(
            livingPopulation,
            rawClusters
        );


        //
        // ======================================
        // 2. ABSORBER GRUPOS MUY PEQUEÑOS
        // ======================================
        //

        List<WorkingCluster> geneticClusters =
            ConsolidateSmallClusters(
                rawClusters
            );


        //
        // ======================================
        // 3. FUSIONAR GRUPOS QUE TODAVÍA
        //    PUEDEN INTERCAMBIAR GENES
        // ======================================
        //

        List<WorkingCluster> reproductiveGroups =
            MergeReproductivelyConnectedClusters(
                geneticClusters
            );


        //
        // En este punto los grupos que quedan
        // tienen aceptación reproductiva
        // suficientemente baja entre sí.
        //
        // Ahora sí son candidatos serios.
        //


        //
        // ======================================
        // 4. EMPAREJAR ESPECIES YA CONFIRMADAS
        //    CON LOS GRUPOS ACTUALES
        // ======================================
        //

        Dictionary<int, WorkingCluster>
            confirmedAssignments =
                new();


        HashSet<int> assignedGroupIndexes =
            new();


        //
        // La continuidad se resuelve GLOBALMENTE por cercanía.
        //
        // Antes dábamos prioridad a las especies más antiguas.
        // Eso podía provocar un error de identidad cuando una
        // especie desaparecía y quedaba un único grupo actual:
        // el grupo sobreviviente heredaba el ID antiguo aunque
        // estuviera mucho más cerca de otra especie confirmada.
        //
        // Ahora construimos todas las parejas especie-grupo que
        // cumplen la distancia de continuidad y elegimos primero
        // las de MENOR distancia. La asignación sigue siendo 1:1.
        //

        List<(
            ConfirmedSpeciesState Species,
            int GroupIndex,
            double Distance
        )> continuityCandidates =
            new();


        foreach (
            ConfirmedSpeciesState species
            in _confirmedSpecies
        )
        {
            for (
                int groupIndex = 0;
                groupIndex < reproductiveGroups.Count;
                groupIndex++
            )
            {
                double distance =
                    _distanceCalculator
                        .Calculate(
                            species.Centroid,
                            reproductiveGroups[
                                groupIndex
                            ].Centroid
                        );


                if (
                    distance
                    <=
                    SpeciesContinuityDistance
                )
                {
                    continuityCandidates.Add(
                        (
                            Species: species,
                            GroupIndex: groupIndex,
                            Distance: distance
                        )
                    );
                }
            }
        }


        HashSet<int> assignedSpeciesIds =
            new();


        foreach (
            var candidate
            in continuityCandidates
                .OrderBy(
                    candidate =>
                        candidate.Distance
                )
                .ThenBy(
                    candidate =>
                        candidate.Species.FirstDetectedCycle
                )
                .ThenBy(
                    candidate =>
                        candidate.Species.SpeciesId
                )
                .ThenBy(
                    candidate =>
                        candidate.GroupIndex
                )
        )
        {
            if (
                assignedSpeciesIds.Contains(
                    candidate.Species.SpeciesId
                )
                ||
                assignedGroupIndexes.Contains(
                    candidate.GroupIndex
                )
            )
            {
                continue;
            }


            confirmedAssignments[
                candidate.Species.SpeciesId
            ] =
                reproductiveGroups[
                    candidate.GroupIndex
                ];


            assignedSpeciesIds.Add(
                candidate.Species.SpeciesId
            );


            assignedGroupIndexes.Add(
                candidate.GroupIndex
            );
        }


        //
        // ======================================
        // 5. GRUPOS NO ASIGNADOS =
        //    CANDIDATOS A NUEVA ESPECIE
        // ======================================
        //

        List<PendingCandidateState>
            nextPendingCandidates =
                new();


        HashSet<int> usedPreviousCandidates =
            new();


        //
        // Guardamos qué candidatos deben
        // seguir temporalmente integrados
        // con la especie parental.
        //

        Dictionary<int, List<Organism>>
            pendingMembersByParent =
                new();


        //
        // Metadatos de nuevas especies
        // confirmadas en esta detección.
        //

        Dictionary<
            int,
            NewSpeciesMetadata
        > newSpeciesMetadata =
            new();


        for (
            int groupIndex = 0;
            groupIndex <
            reproductiveGroups.Count;
            groupIndex++
        )
        {
            if (
                assignedGroupIndexes.Contains(
                    groupIndex
                )
            )
            {
                continue;
            }


            WorkingCluster candidateGroup =
                reproductiveGroups[
                    groupIndex
                ];


            //
            // ==================================
            // BUSCAR ESPECIE PARENTAL
            // ==================================
            //

            int parentSpeciesId =
                FindNearestCurrentSpecies(
                    candidateGroup.Centroid,
                    confirmedAssignments
                );


            //
            // ==================================
            // BUSCAR EL MISMO CANDIDATO
            // EN LA DETECCIÓN ANTERIOR
            // ==================================
            //

            int previousCandidateIndex =
                FindMatchingPendingCandidate(
                    candidateGroup,
                    parentSpeciesId,
                    usedPreviousCandidates
                );


            PendingCandidateState
                candidateState;


            if (
                previousCandidateIndex >= 0
            )
            {
                PendingCandidateState previous =
                    _pendingCandidates[
                        previousCandidateIndex
                    ];


                usedPreviousCandidates.Add(
                    previousCandidateIndex
                );


                candidateState =
                    new PendingCandidateState(
                        ParentSpeciesId:
                            parentSpeciesId,

                        FirstObservedCycle:
                            previous
                                .FirstObservedCycle,

                        ConsecutiveDetections:
                            previous
                                .ConsecutiveDetections
                            +
                            1,

                        Centroid:
                            candidateGroup
                                .Centroid
                    );
            }
            else
            {
                candidateState =
                    new PendingCandidateState(
                        ParentSpeciesId:
                            parentSpeciesId,

                        FirstObservedCycle:
                            cycle,

                        ConsecutiveDetections:
                            1,

                        Centroid:
                            candidateGroup
                                .Centroid
                    );
            }


            //
            // ==================================
            // ¿YA PERSISTIÓ LO SUFICIENTE?
            // ==================================
            //

            if (
                candidateState
                    .ConsecutiveDetections
                >=
                RequiredPersistenceDetections
            )
            {
                int newSpeciesId =
                    _nextSpeciesId++;


                confirmedAssignments[
                    newSpeciesId
                ] =
                    candidateGroup;


                newSpeciesMetadata[
                    newSpeciesId
                ] =
                    new NewSpeciesMetadata(
                        ParentSpeciesId:
                            parentSpeciesId,

                        FirstDetectedCycle:
                            candidateState
                                .FirstObservedCycle
                    );


                //
                // Ya no es candidato.
                //
                // Desde ahora es especie
                // confirmada.
                //
            }
            else
            {
                nextPendingCandidates.Add(
                    candidateState
                );


                //
                // Mientras no esté confirmado,
                // todavía lo mostramos como
                // parte de la especie parental.
                //

                if (
                    !pendingMembersByParent
                        .TryGetValue(
                            parentSpeciesId,
                            out List<Organism>?
                                parentMembers
                        )
                )
                {
                    parentMembers =
                        new List<Organism>();


                    pendingMembersByParent[
                        parentSpeciesId
                    ] =
                        parentMembers;
                }


                parentMembers.AddRange(
                    candidateGroup.Members
                );
            }
        }


        //
        // ======================================
        // 6. CONSTRUIR ESPECIES CONFIRMADAS
        // ======================================
        //

        List<SpeciesCluster> detectedSpecies =
            new();


        List<ConfirmedSpeciesState>
            nextConfirmedSpecies =
                new();


        foreach (
            KeyValuePair<
                int,
                WorkingCluster
            > assignment
            in confirmedAssignments
        )
        {
            int speciesId =
                assignment.Key;


            WorkingCluster coreGroup =
                assignment.Value;


            List<Organism> displayMembers =
                new(
                    coreGroup.Members
                );


            //
            // Añadimos candidatos no confirmados
            // a su especie parental para que
            // la población total siga cuadrando.
            //

            if (
                pendingMembersByParent.TryGetValue(
                    speciesId,
                    out List<Organism>?
                        pendingMembers
                )
            )
            {
                displayMembers.AddRange(
                    pendingMembers
                );
            }


            int? parentSpeciesId;

            int firstDetectedCycle;


            //
            // ¿Es una especie recién confirmada?
            //

            if (
                newSpeciesMetadata.TryGetValue(
                    speciesId,
                    out NewSpeciesMetadata?
                        metadata
                )
            )
            {
                parentSpeciesId =
                    metadata.ParentSpeciesId;

                firstDetectedCycle =
                    metadata.FirstDetectedCycle;
            }
            else
            {
                ConfirmedSpeciesState previous =
                    _confirmedSpecies
                        .First(
                            species =>
                                species.SpeciesId
                                ==
                                speciesId
                        );


                parentSpeciesId =
                    previous.ParentSpeciesId;

                firstDetectedCycle =
                    previous.FirstDetectedCycle;
            }


            //
            // Para continuidad futura usamos
            // solamente el núcleo de la especie,
            // no los candidatos todavía dudosos.
            //

            ConfirmedSpeciesState state =
                new(
                    SpeciesId:
                        speciesId,

                    ParentSpeciesId:
                        parentSpeciesId,

                    FirstDetectedCycle:
                        firstDetectedCycle,

                    Centroid:
                        coreGroup.Centroid
                );


            nextConfirmedSpecies.Add(
                state
            );


            SpeciesCluster cluster =
                BuildSpeciesCluster(
                    speciesId:
                        speciesId,

                    parentSpeciesId:
                        parentSpeciesId,

                    firstDetectedCycle:
                        firstDetectedCycle,

                    members:
                        displayMembers
                );


            detectedSpecies.Add(
                cluster
            );
        }


        //
        // ======================================
        // 7. ACTUALIZAR ESTADO
        // ======================================
        //

        _confirmedSpecies =
            nextConfirmedSpecies;


        _pendingCandidates =
            nextPendingCandidates;


        return detectedSpecies
            .OrderBy(
                species =>
                    species.SpeciesId
            )
            .ToList();
    }


    //
    // ==========================================
    // DIVISIÓN GENÉTICA PRELIMINAR
    // ==========================================
    //

    private void SplitRecursively(
        List<Organism> organisms,
        List<WorkingCluster> result)
    {
        if (
            organisms.Count == 0
        )
        {
            return;
        }


        Genome centroid =
            CalculateCentroid(
                organisms
            );


        List<(
            Organism Organism,
            double Acceptance
        )> acceptanceProfile =
            organisms
                .Select(
                    organism =>
                        (
                            Organism:
                                organism,

                            Acceptance:
                                CalculateMateAcceptance(
                                    organism.Genome,
                                    centroid
                                )
                        )
                )
                .OrderBy(
                    item =>
                        item.Acceptance
                )
                .ToList();


        int quantileIndex =
            (int)Math.Floor(
                (
                    acceptanceProfile.Count
                    -
                    1
                )
                *
                WithinClusterAcceptanceQuantile
            );


        quantileIndex =
            Math.Clamp(
                quantileIndex,
                0,
                acceptanceProfile.Count - 1
            );


        double lowAcceptanceQuantile =
            acceptanceProfile[
                quantileIndex
            ].Acceptance;


        //
        // Si incluso la zona baja de la
        // distribución sigue siendo bastante
        // compatible con el centroide,
        // mantenemos el grupo unido.
        //

        if (
            lowAcceptanceQuantile
            >=
            MinimumWithinClusterMateAcceptance
        )
        {
            result.Add(
                new WorkingCluster(
                    organisms
                )
            );

            return;
        }


        //
        // No tiene sentido partir infinitamente
        // un grupo extremadamente pequeño.
        //

        if (
            organisms.Count
            <
            MinimumClusterPopulation
            *
            2
        )
        {
            result.Add(
                new WorkingCluster(
                    organisms
                )
            );

            return;
        }


        //
        // ======================================
        // DOS EXTREMOS REPRODUCTIVOS
        // ======================================
        //
        // seedA es uno de los individuos menos
        // compatibles con el centroide.
        //
        // seedB es el individuo con menor
        // aceptación esperada frente a seedA.
        //

        Organism seedA =
            acceptanceProfile
                .First()
                .Organism;


        Organism seedB =
            organisms
                .OrderBy(
                    organism =>
                        CalculateMateAcceptance(
                            organism.Genome,
                            seedA.Genome
                        )
                )
                .First();


        if (
            seedA.Id
            ==
            seedB.Id
        )
        {
            result.Add(
                new WorkingCluster(
                    organisms
                )
            );

            return;
        }


        List<Organism> groupA =
            new();


        List<Organism> groupB =
            new();


        foreach (
            Organism organism
            in organisms
        )
        {
            double acceptanceA =
                CalculateMateAcceptance(
                    organism.Genome,
                    seedA.Genome
                );


            double acceptanceB =
                CalculateMateAcceptance(
                    organism.Genome,
                    seedB.Genome
                );


            if (
                acceptanceA
                >=
                acceptanceB
            )
            {
                groupA.Add(
                    organism
                );
            }
            else
            {
                groupB.Add(
                    organism
                );
            }
        }


        //
        // Protección ante una separación
        // degenerada.
        //

        if (
            groupA.Count == 0
            ||
            groupB.Count == 0
        )
        {
            result.Add(
                new WorkingCluster(
                    organisms
                )
            );

            return;
        }


        SplitRecursively(
            groupA,
            result
        );


        SplitRecursively(
            groupB,
            result
        );
    }


    //
    // ==========================================
    // ABSORBER GRUPOS PEQUEÑOS
    // ==========================================
    //

    private List<WorkingCluster>
        ConsolidateSmallClusters(
            List<WorkingCluster> clusters)
    {
        List<WorkingCluster> largeClusters =
            clusters
                .Where(
                    cluster =>
                        cluster.Members.Count
                        >=
                        MinimumClusterPopulation
                )
                .ToList();


        List<WorkingCluster> smallClusters =
            clusters
                .Where(
                    cluster =>
                        cluster.Members.Count
                        <
                        MinimumClusterPopulation
                )
                .ToList();


        //
        // Si absolutamente todos son pequeños,
        // los tratamos como una sola población.
        //

        if (
            largeClusters.Count == 0
        )
        {
            List<Organism> everyone =
                clusters
                    .SelectMany(
                        cluster =>
                            cluster.Members
                    )
                    .ToList();


            return new List<WorkingCluster>
            {
                new(
                    everyone
                )
            };
        }


        foreach (
            WorkingCluster small
            in smallClusters
        )
        {
            int bestIndex =
                -1;


            double highestAcceptance =
                double.MinValue;


            for (
                int i = 0;
                i < largeClusters.Count;
                i++
            )
            {
                double mateAcceptance =
                    CalculateMateAcceptance(
                        small.Centroid,
                        largeClusters[
                            i
                        ].Centroid
                    );


                if (
                    mateAcceptance
                    >
                    highestAcceptance
                )
                {
                    highestAcceptance =
                        mateAcceptance;

                    bestIndex =
                        i;
                }
            }


            List<Organism> mergedMembers =
                new(
                    largeClusters[
                        bestIndex
                    ].Members
                );


            mergedMembers.AddRange(
                small.Members
            );


            largeClusters[
                bestIndex
            ] =
                new WorkingCluster(
                    mergedMembers
                );
        }


        return largeClusters;
    }


    //
    // ==========================================
    // FUSIÓN REPRODUCTIVA
    // ==========================================
    //

    private List<WorkingCluster>
        MergeReproductivelyConnectedClusters(
            List<WorkingCluster> clusters)
    {
        List<WorkingCluster> working =
            clusters
                .Select(
                    cluster =>
                        new WorkingCluster(
                            new List<Organism>(
                                cluster.Members
                            )
                        )
                )
                .ToList();


        while (
            working.Count > 1
        )
        {
            int bestA =
                -1;


            int bestB =
                -1;


            double highestMateAcceptance =
                double.MinValue;


            //
            // Buscamos los dos grupos con
            // mayor aceptación reproductiva
            // esperada usando la MISMA fórmula
            // que el apareamiento real.
            //

            for (
                int i = 0;
                i < working.Count;
                i++
            )
            {
                for (
                    int j = i + 1;
                    j < working.Count;
                    j++
                )
                {
                    double mateAcceptance =
                        CalculateMateAcceptance(
                            working[
                                i
                            ].Centroid,

                            working[
                                j
                            ].Centroid
                        );


                    if (
                        mateAcceptance
                        >
                        highestMateAcceptance
                    )
                    {
                        highestMateAcceptance =
                            mateAcceptance;

                        bestA =
                            i;

                        bestB =
                            j;
                    }
                }
            }


            //
            // Si incluso los dos grupos MÁS
            // conectados ya están en 60% o menos,
            // dejamos de fusionar.
            //
            // Todavía NO se confirman especies:
            // estos grupos deben persistir durante
            // varias detecciones consecutivas.
            //

            if (
                highestMateAcceptance
                <=
                MaximumMateAcceptanceForSeparateSpecies
            )
            {
                break;
            }


            List<Organism> mergedMembers =
                new(
                    working[
                        bestA
                    ].Members
                );


            mergedMembers.AddRange(
                working[
                    bestB
                ].Members
            );


            int largerIndex =
                Math.Max(
                    bestA,
                    bestB
                );


            int smallerIndex =
                Math.Min(
                    bestA,
                    bestB
                );


            working.RemoveAt(
                largerIndex
            );


            working.RemoveAt(
                smallerIndex
            );


            working.Add(
                new WorkingCluster(
                    mergedMembers
                )
            );
        }


        return working;
    }


    //
    // ==========================================
    // ACEPTACIÓN REPRODUCTIVA
    // ==========================================
    //
    // Debe mantenerse sincronizada con
    // SexualReproductionSystem y con el
    // diagnóstico regional.
    //
    // Configuración actual validada:
    // EcologicalSelectionStrength = 3.0
    //

    private double CalculateMateAcceptance(
        Genome genomeA,
        Genome genomeB)
    {
        double geneticCompatibility =
            _compatibilityCalculator
                .CalculateCompatibility(
                    genomeA,
                    genomeB
                );


        double ecologicalSimilarity =
            _ecologicalSimilarityCalculator
                .Calculate(
                    genomeA,
                    genomeB
                );


        double pairMateSelectivity =
            (
                genomeA.MateSelectivity
                +
                genomeB.MateSelectivity
            )
            /
            2.0;


        pairMateSelectivity =
            Math.Clamp(
                pairMateSelectivity,
                0,
                1
            );


        double ecologicalSelectionStrength =
            3.0
            *
            pairMateSelectivity;


        double ecologicalPreference =
            Math.Pow(
                ecologicalSimilarity,
                ecologicalSelectionStrength
            );


        ecologicalPreference =
            Math.Clamp(
                ecologicalPreference,
                0.05,
                1.0
            );


        double mateAcceptance =
            geneticCompatibility
            *
            ecologicalPreference;


        return Math.Clamp(
            mateAcceptance,
            0,
            1
        );
    }


    //
    // ==========================================
    // BUSCAR ESPECIE PARENTAL
    // ==========================================
    //

    private int FindNearestCurrentSpecies(
        Genome candidateCentroid,
        Dictionary<int, WorkingCluster>
            confirmedAssignments)
    {
        //
        // En condiciones normales siempre
        // existirá al menos una especie
        // confirmada actual.
        //

        if (
            confirmedAssignments.Count == 0
        )
        {
            return
                _confirmedSpecies
                    .OrderBy(
                        species =>
                            species.FirstDetectedCycle
                    )
                    .First()
                    .SpeciesId;
        }


        return confirmedAssignments
            .OrderBy(
                pair =>
                    _distanceCalculator
                        .Calculate(
                            candidateCentroid,
                            pair.Value
                                .Centroid
                        )
            )
            .First()
            .Key;
    }


    //
    // ==========================================
    // BUSCAR CANDIDATO PREVIO
    // ==========================================
    //

    private int FindMatchingPendingCandidate(
        WorkingCluster candidateGroup,
        int parentSpeciesId,
        HashSet<int> alreadyUsed)
    {
        int bestIndex =
            -1;


        double bestDistance =
            double.MaxValue;


        for (
            int i = 0;
            i < _pendingCandidates.Count;
            i++
        )
        {
            if (
                alreadyUsed.Contains(
                    i
                )
            )
            {
                continue;
            }


            PendingCandidateState previous =
                _pendingCandidates[
                    i
                ];


            if (
                previous.ParentSpeciesId
                !=
                parentSpeciesId
            )
            {
                continue;
            }


            double distance =
                _distanceCalculator
                    .Calculate(
                        candidateGroup.Centroid,
                        previous.Centroid
                    );


            if (
                distance
                <
                bestDistance
            )
            {
                bestDistance =
                    distance;

                bestIndex =
                    i;
            }
        }


        if (
            bestIndex >= 0
            &&
            bestDistance
            <=
            CandidateContinuityDistance
        )
        {
            return bestIndex;
        }


        return -1;
    }


    //
    // ==========================================
    // CREAR SPECIES CLUSTER
    // ==========================================
    //

    private SpeciesCluster BuildSpeciesCluster(
        int speciesId,
        int? parentSpeciesId,
        int firstDetectedCycle,
        List<Organism> members)
    {
        Genome centroid =
            CalculateCentroid(
                members
            );


        double averageDistance =
            members.Average(
                organism =>
                    _distanceCalculator
                        .Calculate(
                            organism.Genome,
                            centroid
                        )
            );


        Dictionary<int, int>
            regionPopulations =
                members
                    .GroupBy(
                        organism =>
                            organism.RegionId
                    )
                    .ToDictionary(
                        group =>
                            group.Key,

                        group =>
                            group.Count()
                    );


        return new SpeciesCluster(
            speciesId:
                speciesId,

            parentSpeciesId:
                parentSpeciesId,

            firstDetectedCycle:
                firstDetectedCycle,

            population:
                members.Count,

            centroid:
                centroid,

            averageDistanceToCentroid:
                averageDistance,

            regionPopulations:
                regionPopulations
        );
    }


    //
    // ==========================================
    // CENTROIDE
    // ==========================================
    //

    private static Genome CalculateCentroid(
        List<Organism> organisms)
    {
        return new Genome(
            speed:
                organisms.Average(
                    organism =>
                        organism.Speed
                ),

            size:
                organisms.Average(
                    organism =>
                        organism.Size
                ),

            metabolism:
                organisms.Average(
                    organism =>
                        organism.Metabolism
                ),

            optimalTemperature:
                organisms.Average(
                    organism =>
                        organism.OptimalTemperature
                ),

            thermalTolerance:
                organisms.Average(
                    organism =>
                        organism.ThermalTolerance
                ),

            meatAdaptation:
                organisms.Average(
                    organism =>
                        organism.MeatAdaptation
                ),

            predatoryDrive:
                organisms.Average(
                    organism =>
                        organism.PredatoryDrive
                ),

            explorationDrive:
                organisms.Average(
                    organism =>
                        organism.ExplorationDrive
                ),

            riskTolerance:
                organisms.Average(
                    organism =>
                        organism.RiskTolerance
                ),

            scavengingDrive:
                organisms.Average(
                    organism =>
                        organism.ScavengingDrive
                ),

            mateSelectivity:
                organisms.Average(
                    organism =>
                        organism.MateSelectivity
                )
        );
    }


    //
    // ==========================================
    // CLUSTER INTERNO
    // ==========================================
    //

    private sealed class WorkingCluster
    {
        public List<Organism> Members
        {
            get;
        }


        public Genome Centroid
        {
            get;
        }


        public WorkingCluster(
            List<Organism> members)
        {
            Members =
                members;


            Centroid =
                CalculateCentroid(
                    members
                );
        }
    }


    //
    // ==========================================
    // ESPECIE CONFIRMADA
    // ==========================================
    //

    private sealed record ConfirmedSpeciesState(
        int SpeciesId,
        int? ParentSpeciesId,
        int FirstDetectedCycle,
        Genome Centroid
    );


    //
    // ==========================================
    // CANDIDATO
    // ==========================================
    //

    private sealed record PendingCandidateState(
        int ParentSpeciesId,
        int FirstObservedCycle,
        int ConsecutiveDetections,
        Genome Centroid
    );


    //
    // ==========================================
    // METADATOS DE NUEVA ESPECIE
    // ==========================================
    //

    private sealed record NewSpeciesMetadata(
        int ParentSpeciesId,
        int FirstDetectedCycle
    );
}