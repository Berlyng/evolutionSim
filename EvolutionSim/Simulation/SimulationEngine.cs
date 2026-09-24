using EvolutionSim.Ecology;
using EvolutionSim.Evolution;
using EvolutionSim.Models;
using EvolutionSim.Output;
using EvolutionSim.Statistics;
using EvolutionSim.World;

namespace EvolutionSim.Simulation;

public sealed class SimulationEngine
{
    private readonly SimulationConfig _config;
    private readonly SimulationState _state;
    private readonly SimulationSystems _systems;
    private readonly ISimulationOutput _output;
    private readonly SpeciesHistoryTracker _speciesHistoryTracker;
    private readonly PhylogenyBuilder _phylogenyBuilder;
    private readonly GenomeStructuralStatisticsCalculator
        _genomeStructuralStatisticsCalculator;

    private readonly PhenotypeExpressionStatisticsCalculator
        _phenotypeExpressionStatisticsCalculator;

    private readonly MetabolicEfficiencyEffectStatisticsCalculator
        _metabolicEfficiencyEffectStatisticsCalculator;

    private readonly MetabolicSelectionDiagnosticsCalculator
        _metabolicSelectionDiagnosticsCalculator;

    private readonly PopulationBoomBustDiagnosticsTracker
        _populationBoomBustDiagnosticsTracker;

    private readonly ThermalRegulationSelectionDiagnosticsCalculator
        _thermalRegulationSelectionDiagnosticsCalculator;

    private readonly BodySizeSelectionDiagnosticsCalculator
        _bodySizeSelectionDiagnosticsCalculator;

    private readonly LocomotionSelectionDiagnosticsCalculator
        _locomotionSelectionDiagnosticsCalculator;

    private readonly FeedingSpecializationSelectionDiagnosticsCalculator
        _feedingSpecializationSelectionDiagnosticsCalculator;

    private readonly MorphologyStatisticsCalculator
        _morphologyStatisticsCalculator;

    public SimulationState State =>
        _state;

    public bool CanStep =>
        _state.CurrentCycle < _config.Cycles
        &&
        _state.Population.Count > 0;

    public SimulationEngine(
        SimulationConfig config,
        SimulationState state,
        SimulationSystems systems,
        ISimulationOutput output)
    {
        _config = config;
        _state = state;
        _systems = systems;
        _output = output;

        _speciesHistoryTracker =
            new SpeciesHistoryTracker();

        _phylogenyBuilder =
            new PhylogenyBuilder();

        _genomeStructuralStatisticsCalculator =
            new GenomeStructuralStatisticsCalculator();

        _phenotypeExpressionStatisticsCalculator =
            new PhenotypeExpressionStatisticsCalculator();

        _metabolicEfficiencyEffectStatisticsCalculator =
            new MetabolicEfficiencyEffectStatisticsCalculator();

        _metabolicSelectionDiagnosticsCalculator =
            new MetabolicSelectionDiagnosticsCalculator();

        _populationBoomBustDiagnosticsTracker =
            new PopulationBoomBustDiagnosticsTracker();

        _thermalRegulationSelectionDiagnosticsCalculator =
            new ThermalRegulationSelectionDiagnosticsCalculator();

        _bodySizeSelectionDiagnosticsCalculator =
            new BodySizeSelectionDiagnosticsCalculator();

        _locomotionSelectionDiagnosticsCalculator =
            new LocomotionSelectionDiagnosticsCalculator();

        _feedingSpecializationSelectionDiagnosticsCalculator =
            new FeedingSpecializationSelectionDiagnosticsCalculator();

        _morphologyStatisticsCalculator =
            new MorphologyStatisticsCalculator();

        // La especie ancestral ya fue detectada por SimulationFactory
        // antes de construir el motor.
        _speciesHistoryTracker.RecordDetection(
            cycle:
                state.CurrentCycle,
            detectedSpecies:
                state.DetectedSpecies,
            simulationExtinct:
                state.Population.Count == 0
        );
    }

    public SimulationStepResult? Step()
    {
        if (!CanStep)
        {
            return null;
        }

        SimulationConfig config =
            _config;


        using IDisposable metabolicPhenotypeEcologyScope =
            PhenotypeEcologyContext
                .PushMetabolicEfficiencyCoupling(
                    config
                        .MetabolicEfficiencyEcologicalCoupling
                );


        using IDisposable thermalPhenotypeEcologyScope =
            PhenotypeEcologyContext
                .PushThermalRegulationCoupling(
                    config
                        .ThermalRegulationEcologicalCoupling
                );


        using IDisposable bodySizePhenotypeEcologyScope =
            PhenotypeEcologyContext
                .PushBodySizeCoupling(
                    config
                        .BodySizeEcologicalCoupling
                );


        using IDisposable locomotionPhenotypeEcologyScope =
            PhenotypeEcologyContext
                .PushLocomotionCoupling(
                    config
                        .LocomotionEcologicalCoupling
                );


        using IDisposable feedingSpecializationPhenotypeEcologyScope =
            PhenotypeEcologyContext
                .PushFeedingSpecializationCoupling(
                    config
                        .FeedingSpecializationEcologicalCoupling
                );


        SimulationState state =
            _state;

        int cycles =
            config.Cycles;

        double energyPerFoodUnit =
            config.EnergyPerFoodUnit;

        double reproductionThreshold =
            config.ReproductionThreshold;

        double reproductionCostPerParent =
            config.ReproductionCostPerParent;

        double childInitialEnergy =
            config.ChildInitialEnergy;

        int speciesDetectionInterval =
            config.SpeciesDetectionInterval;

        ClimateSystem climateSystem =
            _systems.ClimateSystem;

        RegionalGeneticDivergenceCalculator regionalGeneticDivergenceCalculator =
            _systems.RegionalGeneticDivergenceCalculator;

        RegionalReproductiveIsolationCalculator regionalReproductiveIsolationCalculator =
            _systems.RegionalReproductiveIsolationCalculator;

        SuccessfulMatingStatisticsCalculator successfulMatingStatisticsCalculator =
            _systems.SuccessfulMatingStatisticsCalculator;

        ImmigrantReproductiveSuccessCalculator immigrantReproductiveSuccessCalculator =
            _systems.ImmigrantReproductiveSuccessCalculator;

        SexualReproductionSystem reproductionSystem =
            _systems.ReproductionSystem;

        SpeciesDetectionSystem speciesDetectionSystem =
            _systems.SpeciesDetectionSystem;

        CarcassSystem carcassSystem =
            _systems.CarcassSystem;

        FeedingSystem feedingSystem =
            _systems.FeedingSystem;

        MigrationSystem migrationSystem =
            _systems.MigrationSystem;

        PatchMovementSystem patchMovementSystem =
            _systems.PatchMovementSystem;

        PlantResourceCycleTracker plantResourceCycleTracker =
            _systems.PlantResourceCycleTracker;

        RegionalTraitVariationCalculator regionalTraitVariationCalculator =
            _systems.RegionalTraitVariationCalculator;

        RegionalTraitVariationPrinter regionalTraitVariationPrinter =
            _systems.RegionalTraitVariationPrinter;

        GeneticDistanceBreakdownCalculator geneticDistanceBreakdownCalculator =
            _systems.GeneticDistanceBreakdownCalculator;

        GeneticDistanceBreakdownPrinter geneticDistanceBreakdownPrinter =
            _systems.GeneticDistanceBreakdownPrinter;

        int cycleToRun =
            state.CurrentCycle + 1;

        SimulationStepResult? stepResult =
            null;

        for (
            int cycle = cycleToRun;
            cycle <= cycleToRun;
            cycle++
        )
        {
            state.AdvanceTo(
                cycle
            );

            int populationAtStart =
                state.Population.Count;


            // ========================================================
            // 1. CLIMA
            // ========================================================

            climateSystem.AdvanceCycle(
                state.World,
                cycle
            );


            // ========================================================
            // 2. PLANTAS
            // ========================================================

            // Snapshot pasivo antes de la regeneración.
            // No modifica el estado ni consume Random.
            plantResourceCycleTracker.CaptureBeforeGrowth(
                state.World
            );


            state.World.AdvanceCycle();


            // Snapshot inmediatamente después de Grow() y antes
            // de cualquier consumo vegetal del ciclo.
            plantResourceCycleTracker.CaptureAfterGrowth(
                state.World
            );


            // ========================================================
            // 3. CADÁVERES
            // ========================================================

            carcassSystem.AdvanceCycle(
                state.World
            );


            // ========================================================
            // 4. ALIMENTACIÓN
            // ========================================================
            //
            // EXPERIMENTO CONTROLADO:
            //
            // Los organismos que siguen vivos al inicio del ciclo
            // tienen una oportunidad de alimentarse con los recursos
            // disponibles después de la regeneración vegetal.
            //
            // No se modifica cuánto comen, cuánto obtienen de energía
            // ni ninguna probabilidad. Solo cambia el orden temporal.
            //

            FeedingResult feedingResult =
                feedingSystem.Feed(
                    state.Population,
                    state.World,
                    energyPerFoodUnit
                );


            // ========================================================
            // 5. ENERGÍA Y ENVEJECIMIENTO
            // ========================================================

            foreach (
                Organism organism
                in state.Population
            )
            {
                if (
                    !organism.IsAlive
                )
                {
                    continue;
                }


                Region region =
                    state.World.GetRegion(
                        organism.RegionId
                    );


                Patch patch =
                    state.World.GetPatch(
                        organism.PatchId
                    );


                double localTemperature =
                    patch.GetLocalTemperature(
                        region.Temperature
                    );


                organism.ConsumeEnergy(
                    localTemperature
                );
            }


            // ========================================================
            // 6. MUERTES NATURALES
            // ========================================================

            foreach (
                Organism organism
                in state.Population
            )
            {
                if (
                    organism.IsAlive
                )
                {
                    continue;
                }


                carcassSystem.RegisterDeath(
                    organism
                );
            }


            //
            // El balance vegetal se calcula después de la alimentación
            // y después de determinar los sobrevivientes del ciclo.
            // La biomasa vegetal no cambia durante estos dos últimos
            // pasos; esto mantiene Bio/ind asociado a la población viva.
            //
            List<PlantResourceRegionStatistics>
                plantResourceStatistics =
                    plantResourceCycleTracker.CalculateAfterFeeding(
                        state.Population,
                        state.World
                    );


            // ========================================================
            // 7. MOVIMIENTO LOCAL ENTRE PATCHES
            // ========================================================

            int localMovements =
                patchMovementSystem.Move(
                    state.Population,
                    state.World
                );


            // ========================================================
            // 8. MIGRACIÓN ENTRE REGIONES
            // ========================================================

            int migrations =
                migrationSystem.Migrate(
                    state.Population,
                    state.World
                );


            // ========================================================
            // FAST EXPERIMENT MODE
            // ========================================================
            //
            // En corridas experimentales, los ciclos intermedios
            // conservan TODA la biología pero omiten los diagnósticos
            // costosos. El último ciclo sigue por la ruta completa para
            // producir las métricas finales del experimento.
            //
            // No consume Random adicional.
            // No altera reproducción, muerte ni detección de especies.
            //
            if (
                config.FastExperimentMode
                &&
                cycle
                <
                cycles
            )
            {
                stepResult =
                    RunFastExperimentTail(
                        cycle:
                            cycle,
                        populationAtStart:
                            populationAtStart,
                        feedingResult:
                            feedingResult,
                        migrations:
                            migrations,
                        localMovements:
                            localMovements,
                        reproductionSystem:
                            reproductionSystem,
                        carcassSystem:
                            carcassSystem,
                        speciesDetectionSystem:
                            speciesDetectionSystem
                    );


                continue;
            }


            // ========================================================
            // DIAGNÓSTICO: SEGUIMIENTO INDIVIDUAL DE INMIGRANTES
            // ========================================================
            //
            // Registramos los IDs que REALMENTE migraron en este ciclo.
            // No consume Random ni cambia ninguna decisión de migración.
            // El estado persiste para detectar si un inmigrante logra
            // reproducirse varios ciclos después de haber llegado.
            //

            immigrantReproductiveSuccessCalculator.RegisterMigrations(
                cycle,
                migrationSystem.LastMigrationEvents
            );


            // ========================================================
            // DIAGNÓSTICO: ELEGIBILIDAD REPRODUCTIVA
            // ========================================================
            //
            // Se calcula EXACTAMENTE después de migración y antes de
            // reproducción. Por tanto, representa el estado que ve
            // SexualReproductionSystem al construir sus candidatos.
            //
            // No consume Random ni modifica ningún organismo.
            //
            // Funnel por región:
            //
            // - Vivos
            // - Adultos
            // - Energía >= reproductionThreshold
            // - Adultos con cooldown listo
            // - Candidatos reales = energía suficiente + CanReproduce
            //
            // También contamos los candidatos reales por patch para
            // detectar si existen candidatos en la región pero están
            // demasiado fragmentados para formar parejas locales.
            //

            Dictionary<int,
                (
                    int Alive,
                    int Adults,
                    int EnergyReady,
                    int CooldownReady,
                    int Candidates,
                    double AverageEnergy,
                    double AverageMaxEnergy
                )> reproductiveEligibilityByRegion =
                    new();


            Dictionary<int, Dictionary<int, int>>
                reproductiveCandidatesByPatch =
                    new();


            Dictionary<int, HashSet<Guid>>
                reproductiveCandidateIdsByRegion =
                    state.World.Regions
                        .ToDictionary(
                            region =>
                                region.Id,

                            region =>
                                new HashSet<Guid>()
                        );


            foreach (
                Region diagnosticRegion
                in state.World.Regions
            )
            {
                List<Organism> livingRegionalOrganisms =
                    state.Population
                        .Where(
                            organism =>
                                organism.IsAlive
                                &&
                                organism.RegionId
                                ==
                                diagnosticRegion.Id
                        )
                        .ToList();


                int adultCount =
                    0;

                int energyReadyCount =
                    0;

                int cooldownReadyCount =
                    0;

                int candidateCount =
                    0;


                Dictionary<int, int> patchCandidateCounts =
                    diagnosticRegion.Patches
                        .ToDictionary(
                            patch =>
                                patch.Id,

                            patch =>
                                0
                        );


                foreach (
                    Organism diagnosticOrganism
                    in livingRegionalOrganisms
                )
                {
                    Patch diagnosticPatch =
                        state.World.GetPatch(
                            diagnosticOrganism.PatchId
                        );


                    double diagnosticLocalTemperature =
                        diagnosticPatch.GetLocalTemperature(
                            diagnosticRegion.Temperature
                        );


                    bool isAdult =
                        diagnosticOrganism.Age
                        >=
                        diagnosticOrganism.ReproductiveAge;


                    bool hasEnoughEnergy =
                        diagnosticOrganism.Energy
                        >=
                        reproductionThreshold;


                    double thermalFitness =
                        diagnosticOrganism.GetThermalFitness(
                            diagnosticLocalTemperature
                        );


                    int effectiveCooldown =
                        (int)Math.Ceiling(
                            diagnosticOrganism.ReproductionCooldown
                            /
                            thermalFitness
                        );


                    int cyclesSinceLastReproduction =
                        diagnosticOrganism.Age
                        -
                        diagnosticOrganism.LastReproductionAge;


                    bool isCooldownReady =
                        isAdult
                        &&
                        cyclesSinceLastReproduction
                        >=
                        effectiveCooldown;


                    //
                    // Usamos CanReproduce() para el conteo final de
                    // candidatos, de modo que el diagnóstico permanezca
                    // alineado con la lógica real del organismo.
                    //

                    bool isRealCandidate =
                        hasEnoughEnergy
                        &&
                        diagnosticOrganism.CanReproduce(
                            diagnosticLocalTemperature
                        );


                    if (
                        isAdult
                    )
                    {
                        adultCount++;
                    }


                    if (
                        hasEnoughEnergy
                    )
                    {
                        energyReadyCount++;
                    }


                    if (
                        isCooldownReady
                    )
                    {
                        cooldownReadyCount++;
                    }


                    if (
                        isRealCandidate
                    )
                    {
                        candidateCount++;


                        reproductiveCandidateIdsByRegion[
                            diagnosticRegion.Id
                        ].Add(
                            diagnosticOrganism.Id
                        );


                        if (
                            patchCandidateCounts.ContainsKey(
                                diagnosticOrganism.PatchId
                            )
                        )
                        {
                            patchCandidateCounts[
                                diagnosticOrganism.PatchId
                            ]++;
                        }
                    }
                }


                double averageEnergy =
                    livingRegionalOrganisms.Count > 0

                        ?

                        livingRegionalOrganisms.Average(
                            organism =>
                                organism.Energy
                        )

                        :

                        0;


                double averageMaxEnergy =
                    livingRegionalOrganisms.Count > 0

                        ?

                        livingRegionalOrganisms.Average(
                            organism =>
                                organism.MaxEnergy
                        )

                        :

                        0;


                reproductiveEligibilityByRegion[
                    diagnosticRegion.Id
                ] =
                (
                    Alive:
                        livingRegionalOrganisms.Count,

                    Adults:
                        adultCount,

                    EnergyReady:
                        energyReadyCount,

                    CooldownReady:
                        cooldownReadyCount,

                    Candidates:
                        candidateCount,

                    AverageEnergy:
                        averageEnergy,

                    AverageMaxEnergy:
                        averageMaxEnergy
                );


                reproductiveCandidatesByPatch[
                    diagnosticRegion.Id
                ] =
                    patchCandidateCounts;
            }


            // ========================================================
            // 9. REPRODUCCIÓN
            // ========================================================

            List<Organism> newborns =
                reproductionSystem.Reproduce(
                    state.Population,
                    state.World,
                    reproductionThreshold,
                    reproductionCostPerParent,
                    childInitialEnergy
                );


            int births =
                newborns.Count;


            // ========================================================
            // DIAGNÓSTICO: PAREJAS REPRODUCTIVAS REALES
            // ========================================================
            //
            // Cada recién nacido conserva ParentAId y ParentBId.
            // Reconstruimos exactamente la pareja que produjo cada hijo
            // y calculamos sus métricas SIN consumir Random y SIN
            // modificar ninguna regla de reproducción.
            //

            List<SuccessfulMatingRegionStatistics>
                successfulMatingStatistics =
                    successfulMatingStatisticsCalculator.Calculate(
                        newborns,
                        state.Population,
                        state.World
                    );


            // ========================================================
            // DIAGNÓSTICO: ÉXITO REPRODUCTIVO DE INMIGRANTES
            // ========================================================
            //
            // Clasifica cada nacimiento según el historial real de
            // migración de sus padres:
            //
            // - residente x residente
            // - residente x inmigrante
            // - inmigrante x inmigrante
            //
            // También mide candidatos y padres inmigrantes, incluyendo
            // los que llegaron en ESTE mismo ciclo.
            //

            List<ImmigrantReproductiveRegionStatistics>
                immigrantReproductiveStatistics =
                    immigrantReproductiveSuccessCalculator.Calculate(
                        cycle,
                        newborns,
                        state.Population,
                        state.World,
                        reproductiveCandidateIdsByRegion
                    );


            // ========================================================
            // DIAGNÓSTICO: NACIMIENTOS POR REGIÓN
            // ========================================================
            //
            // Este bloque NO modifica la simulación.
            // Solamente cuenta en qué región nació cada descendiente
            // durante el ciclo actual.
            //

            Dictionary<int, int> birthsByRegion =
                state.World.Regions
                    .ToDictionary(
                        region =>
                            region.Id,

                        region =>
                            0
                    );


            foreach (
                Organism newborn
                in newborns
            )
            {
                birthsByRegion[
                    newborn.RegionId
                ]++;
            }


            // ========================================================
            // DIAGNÓSTICO: PADRES QUE REALMENTE SE REPRODUJERON
            // ========================================================
            //
            // Cada recién nacido ya conserva ParentAId y ParentBId.
            // Por eso podemos identificar a los organismos que
            // efectivamente produjeron descendencia sin tocar
            // SexualReproductionSystem ni consumir números aleatorios.
            //

            HashSet<Guid> successfulParentIds =
                newborns
                    .SelectMany(
                        newborn =>
                            new Guid?[]
                            {
                                newborn.ParentAId,
                                newborn.ParentBId
                            }
                    )
                    .Where(
                        parentId =>
                            parentId.HasValue
                    )
                    .Select(
                        parentId =>
                            parentId!.Value
                    )
                    .ToHashSet();


            Dictionary<int, List<Organism>>
                successfulParentsByRegion =
                    state.World.Regions
                        .ToDictionary(
                            region =>
                                region.Id,

                            region =>
                                state.Population
                                    .Where(
                                        organism =>
                                            organism.RegionId
                                            ==
                                            region.Id
                                            &&
                                            successfulParentIds.Contains(
                                                organism.Id
                                            )
                                    )
                                    .ToList()
                        );


            // ========================================================
            // 10. DESCENDENCIA
            // ========================================================

            state.Population.AddRange(
                newborns
            );


            // ========================================================
            // 11. MUERTES RESTANTES
            // ========================================================

            foreach (
                Organism organism
                in state.Population
            )
            {
                if (
                    organism.IsAlive
                )
                {
                    continue;
                }


                carcassSystem.RegisterDeath(
                    organism
                );
            }


            // ========================================================
            // DIAGNÓSTICO: MUERTES POR REGIÓN
            // ========================================================
            //
            // Agrupamos los organismos muertos por la región donde
            // se encontraban al final del ciclo, justo antes de
            // eliminarlos de la población.
            //
            // No se ejecuta ninguna lógica adicional sobre ellos.
            //

            Dictionary<int, int> deathsByRegion =
                state.World.Regions
                    .ToDictionary(
                        region =>
                            region.Id,

                        region =>
                            0
                    );


            foreach (
                Organism deadOrganism
                in state.Population.Where(
                    organism =>
                        !organism.IsAlive
                )
            )
            {
                if (
                    deathsByRegion.ContainsKey(
                        deadOrganism.RegionId
                    )
                )
                {
                    deathsByRegion[
                        deadOrganism.RegionId
                    ]++;
                }
            }


            // ========================================================
            // 12. REMOVER MUERTOS
            // ========================================================

            state.Population =
                state.Population
                    .Where(
                        organism =>
                            organism.IsAlive
                    )
                    .ToList();


            // ========================================================
            // 13. MUERTES DEL CICLO
            // ========================================================

            int deaths =
                populationAtStart
                +
                births
                -
                state.Population.Count;


            // ========================================================
            // DIAGNÓSTICO PASIVO: BOOM / BUST POBLACIONAL
            // ========================================================
            //
            // Se calcula DESPUÉS de reproducción y remoción de muertos,
            // usando el balance vegetal ya capturado durante este ciclo.
            //
            // No consume Random y no modifica ninguna regla ecológica.
            // Los umbrales son únicamente etiquetas diagnósticas.
            //

            PopulationBoomBustDiagnostics
                boomBustDiagnostics =
                    _populationBoomBustDiagnosticsTracker.Calculate(
                        cycle:
                            cycle,

                        finalPopulation:
                            state.Population.Count,

                        births:
                            births,

                        deaths:
                            deaths,

                        plantStatistics:
                            plantResourceStatistics,

                        birthsByRegion:
                            birthsByRegion,

                        deathsByRegion:
                            deathsByRegion
                    );


            SimulationBoomBustDiagnosticsStepResult
                boomBustDiagnosticsResult =
                    MapBoomBustDiagnostics(
                        boomBustDiagnostics
                    );


            // ========================================================
            // 14. ESPECIES
            // ========================================================

            if (
                cycle
                %
                speciesDetectionInterval
                ==
                0
            )
            {
                state.DetectedSpecies =
                    speciesDetectionSystem.Detect(
                        state.Population,
                        cycle
                    );

                _speciesHistoryTracker.RecordDetection(
                    cycle:
                        cycle,
                    detectedSpecies:
                        state.DetectedSpecies,
                    simulationExtinct:
                        state.Population.Count == 0
                );


                _output.WriteLine(
                    $"[DETECTOR] Ciclo {cycle} | " +
                    $"Especies: {state.DetectedSpecies.Count} | " +
                    $"Candidatos: {speciesDetectionSystem.PendingCandidateCount}"
                );
            }


            if (
                state.Population.Count == 0
                &&
                cycle % speciesDetectionInterval != 0
            )
            {
                _speciesHistoryTracker.RecordSimulationExtinction(
                    cycle
                );
            }


            // ========================================================
            // RESULTADO ESTRUCTURADO DEL CICLO
            // ========================================================
            //
            // Snapshot liviano y pasivo para consumidores externos
            // (WPF, tests, replay, telemetría). No consume Random y no
            // modifica el estado de la simulación.
            //

            double stepAverageGeneration =
                state.Population.Count > 0

                    ?

                    state.Population.Average(
                        organism =>
                            organism.Generation
                    )

                    :

                    0;


            List<SimulationRegionStepResult> regionResults =
                state.World.Regions
                    .Select(
                        region =>
                        {
                            List<Organism> regionalPopulation =
                                state.Population
                                    .Where(
                                        organism =>
                                            organism.RegionId == region.Id
                                    )
                                    .ToList();

                            double averageOptimalTemperature =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.OptimalTemperature)
                                    : 0;

                            double averageThermalTolerance =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.ThermalTolerance)
                                    : 0;

                            double averageSpeed =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.Speed)
                                    : 0;

                            double averageSize =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.Size)
                                    : 0;

                            double averageMetabolism =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.Metabolism)
                                    : 0;

                            double averageScavengingDrive =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.ScavengingDrive)
                                    : 0;

                            double averageExplorationDrive =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.ExplorationDrive)
                                    : 0;

                            double averageMateSelectivity =
                                regionalPopulation.Count > 0
                                    ? regionalPopulation.Average(organism => organism.MateSelectivity)
                                    : 0;

                            List<SimulationPatchStepResult> patchResults =
                                region.Patches
                                    .Select(
                                        patch =>
                                        {
                                            int patchPopulation =
                                                regionalPopulation.Count(
                                                    organism =>
                                                        organism.PatchId == patch.Id
                                                );

                                            double localTemperature =
                                                patch.GetLocalTemperature(
                                                    region.Temperature
                                                );

                                            return new SimulationPatchStepResult(
                                                PatchId: patch.Id,
                                                RegionId: region.Id,
                                                Name: patch.Name,
                                                LocalTemperature: localTemperature,
                                                Population: patchPopulation,
                                                PlantBiomass: patch.Plants.Biomass,
                                                PlantEnergyDensity: patch.Plants.EnergyDensity,
                                                PlantToughness: patch.Plants.Toughness,
                                                CarcassBiomass: carcassSystem.GetPatchBiomass(patch.Id)
                                            );
                                        }
                                    )
                                    .OrderBy(patch => patch.PatchId)
                                    .ToList();

                            return new SimulationRegionStepResult(
                                RegionId: region.Id,
                                Name: region.Name,
                                Temperature: region.Temperature,
                                Population: regionalPopulation.Count,
                                Births: birthsByRegion[region.Id],
                                Deaths: deathsByRegion[region.Id],
                                PlantBiomass: region.TotalPlantBiomass,
                                CarcassBiomass: carcassSystem.GetRegionalBiomass(state.World, region.Id),
                                AverageOptimalTemperature: averageOptimalTemperature,
                                AverageThermalTolerance: averageThermalTolerance,
                                AverageSpeed: averageSpeed,
                                AverageSize: averageSize,
                                AverageMetabolism: averageMetabolism,
                                AverageScavengingDrive: averageScavengingDrive,
                                AverageExplorationDrive: averageExplorationDrive,
                                AverageMateSelectivity: averageMateSelectivity,
                                Patches: patchResults
                            );
                        }
                    )
                    .ToList();


            List<SimulationSpeciesStepResult> speciesResults =
                state.DetectedSpecies
                    .Select(
                        species =>
                            new SimulationSpeciesStepResult(
                                SpeciesId: species.SpeciesId,
                                ParentSpeciesId: species.ParentSpeciesId,
                                FirstDetectedCycle: species.FirstDetectedCycle,
                                Population: species.Population,
                                AverageDistanceToCentroid: species.AverageDistanceToCentroid,
                                Speed: species.Centroid.Speed,
                                Size: species.Centroid.Size,
                                Metabolism: species.Centroid.Metabolism,
                                OptimalTemperature: species.Centroid.OptimalTemperature,
                                ThermalTolerance: species.Centroid.ThermalTolerance,
                                MeatAdaptation: species.Centroid.MeatAdaptation,
                                PredatoryDrive: species.Centroid.PredatoryDrive,
                                ExplorationDrive: species.Centroid.ExplorationDrive,
                                RiskTolerance: species.Centroid.RiskTolerance,
                                ScavengingDrive: species.Centroid.ScavengingDrive,
                                MateSelectivity: species.Centroid.MateSelectivity
                            )
                    )
                    .ToList();


            List<SimulationSpeciesHistoryStepResult>
                speciesHistoryResults =
                    _speciesHistoryTracker
                        .GetHistories()
                        .Select(
                            history =>
                                new SimulationSpeciesHistoryStepResult(
                                    SpeciesId:
                                        history.SpeciesId,
                                    ParentSpeciesId:
                                        history.ParentSpeciesId,
                                    FirstObservedCycle:
                                        history.FirstObservedCycle,
                                    ConfirmedCycle:
                                        history.ConfirmedCycle,
                                    LastConfirmedCycle:
                                        history.LastConfirmedCycle,
                                    EndCycle:
                                        history.EndCycle,
                                    IsActive:
                                        history.IsActive,
                                    DetectionCount:
                                        history.DetectionCount,
                                    LatestPopulation:
                                        history.LatestPopulation,
                                    PeakPopulation:
                                        history.PeakPopulation,
                                    PeakPopulationCycle:
                                        history.PeakPopulationCycle,
                                    LatestAverageDistanceToCentroid:
                                        history.LatestAverageDistanceToCentroid,
                                    LatestSpeed:
                                        history.LatestSpeed,
                                    LatestSize:
                                        history.LatestSize,
                                    LatestMetabolism:
                                        history.LatestMetabolism,
                                    LatestOptimalTemperature:
                                        history.LatestOptimalTemperature,
                                    LatestThermalTolerance:
                                        history.LatestThermalTolerance,
                                    LatestMeatAdaptation:
                                        history.LatestMeatAdaptation,
                                    LatestPredatoryDrive:
                                        history.LatestPredatoryDrive,
                                    LatestExplorationDrive:
                                        history.LatestExplorationDrive,
                                    LatestRiskTolerance:
                                        history.LatestRiskTolerance,
                                    LatestScavengingDrive:
                                        history.LatestScavengingDrive,
                                    LatestMateSelectivity:
                                        history.LatestMateSelectivity,
                                    Observations:
                                        history.Observations
                                            .Select(
                                                observation =>
                                                    new SimulationSpeciesHistoryObservationStepResult(
                                                        Cycle:
                                                            observation.Cycle,
                                                        Population:
                                                            observation.Population,
                                                        AverageDistanceToCentroid:
                                                            observation.AverageDistanceToCentroid,
                                                        Speed:
                                                            observation.Speed,
                                                        Size:
                                                            observation.Size,
                                                        Metabolism:
                                                            observation.Metabolism,
                                                        OptimalTemperature:
                                                            observation.OptimalTemperature,
                                                        ThermalTolerance:
                                                            observation.ThermalTolerance,
                                                        MeatAdaptation:
                                                            observation.MeatAdaptation,
                                                        PredatoryDrive:
                                                            observation.PredatoryDrive,
                                                        ExplorationDrive:
                                                            observation.ExplorationDrive,
                                                        RiskTolerance:
                                                            observation.RiskTolerance,
                                                        ScavengingDrive:
                                                            observation.ScavengingDrive,
                                                        MateSelectivity:
                                                            observation.MateSelectivity,
                                                        RegionPopulations:
                                                            new Dictionary<int, int>(
                                                                observation.RegionPopulations
                                                            )
                                                    )
                                            )
                                            .ToList()
                                )
                        )
                        .ToList();


            List<SimulationSpeciesHistoryEventStepResult>
                speciesHistoryEventResults =
                    _speciesHistoryTracker
                        .GetEvents()
                        .Select(
                            historyEvent =>
                                new SimulationSpeciesHistoryEventStepResult(
                                    Cycle:
                                        historyEvent.Cycle,
                                    Type:
                                        historyEvent.Type.ToString(),
                                    SpeciesId:
                                        historyEvent.SpeciesId,
                                    RelatedSpeciesId:
                                        historyEvent.RelatedSpeciesId,
                                    Description:
                                        historyEvent.Description
                                )
                        )
                        .ToList();



            PhylogenySnapshot phylogenySnapshot =
                _phylogenyBuilder.Build(
                    currentCycle:
                        cycle,
                    histories:
                        _speciesHistoryTracker
                            .GetHistories()
                );


            SimulationPhylogenyStepResult
                phylogenyResult =
                    new(
                        CurrentCycle:
                            phylogenySnapshot.CurrentCycle,
                        MaximumDepth:
                            phylogenySnapshot.MaximumDepth,
                        RootSpeciesIds:
                            phylogenySnapshot.RootSpeciesIds
                                .ToList(),
                        Nodes:
                            phylogenySnapshot.Nodes
                                .Select(
                                    node =>
                                        new SimulationPhylogenyNodeStepResult(
                                            SpeciesId:
                                                node.SpeciesId,
                                            ParentSpeciesId:
                                                node.ParentSpeciesId,
                                            Depth:
                                                node.Depth,
                                            FirstObservedCycle:
                                                node.FirstObservedCycle,
                                            ConfirmedCycle:
                                                node.ConfirmedCycle,
                                            LastConfirmedCycle:
                                                node.LastConfirmedCycle,
                                            EndCycle:
                                                node.EndCycle,
                                            IsActive:
                                                node.IsActive,
                                            LatestPopulation:
                                                node.LatestPopulation,
                                            PeakPopulation:
                                                node.PeakPopulation,
                                            DirectDescendantCount:
                                                node.DirectDescendantCount
                                        )
                                )
                                .ToList(),
                        Edges:
                            phylogenySnapshot.Edges
                                .Select(
                                    edge =>
                                        new SimulationPhylogenyEdgeStepResult(
                                            ParentSpeciesId:
                                                edge.ParentSpeciesId,
                                            ChildSpeciesId:
                                                edge.ChildSpeciesId,
                                            BranchCycle:
                                                edge.BranchCycle
                                        )
                                )
                                .ToList()
                    );


            GenomeStructuralStatistics
                genomeStructuralStatistics =
                    _genomeStructuralStatisticsCalculator
                        .Calculate(
                            state.Population
                        );


            SimulationGenomeStructureStepResult
                genomeStructureResult =
                    new(
                        LivingPopulation:
                            genomeStructuralStatistics
                                .LivingPopulation,
                        GenomesWithStructuralLoci:
                            genomeStructuralStatistics
                                .GenomesWithStructuralLoci,
                        TotalStructuralLoci:
                            genomeStructuralStatistics
                                .TotalStructuralLoci,
                        ActiveStructuralLoci:
                            genomeStructuralStatistics
                                .ActiveStructuralLoci,
                        InactiveStructuralLoci:
                            genomeStructuralStatistics
                                .InactiveStructuralLoci,
                        UniqueFamilies:
                            genomeStructuralStatistics
                                .UniqueFamilies,
                        AverageLociPerGenome:
                            genomeStructuralStatistics
                                .AverageLociPerGenome,
                        MaximumLociInGenome:
                            genomeStructuralStatistics
                                .MaximumLociInGenome,
                        Families:
                            genomeStructuralStatistics
                                .Families
                                .Select(
                                    family =>
                                        new SimulationGenomeFamilyStepResult(
                                            FamilyId:
                                                family.FamilyId,
                                            CarrierCount:
                                                family.CarrierCount,
                                            CopyCount:
                                                family.CopyCount,
                                            ActiveCopyCount:
                                                family.ActiveCopyCount,
                                            AverageValue:
                                                family.AverageValue
                                        )
                                )
                                .ToList()
                    );


            PhenotypeExpressionStatistics
                phenotypeExpressionStatistics =
                    _phenotypeExpressionStatisticsCalculator
                        .Calculate(
                            state.Population
                        );


            SimulationPhenotypeExpressionStepResult
                phenotypeExpressionResult =
                    new(
                        LivingPopulation:
                            phenotypeExpressionStatistics
                                .LivingPopulation,
                        ExpressedGenomes:
                            phenotypeExpressionStatistics
                                .ExpressedGenomes,
                        Channels:
                            phenotypeExpressionStatistics
                                .Channels
                                .Select(
                                    channel =>
                                        new SimulationPhenotypeChannelStepResult(
                                            Channel:
                                                channel.Channel.ToString(),
                                            ExpressingOrganisms:
                                                channel.ExpressingOrganisms,
                                            AverageModifier:
                                                channel.AverageModifier,
                                            StandardDeviation:
                                                channel.StandardDeviation,
                                            MinimumModifier:
                                                channel.MinimumModifier,
                                            MaximumModifier:
                                                channel.MaximumModifier
                                        )
                                )
                                .ToList()
                    );


            MetabolicEfficiencyEffectStatistics
                metabolicEfficiencyEffectStatistics =
                    _metabolicEfficiencyEffectStatisticsCalculator
                        .Calculate(
                            state.Population
                        );


            SimulationMetabolicEfficiencyEffectStepResult
                metabolicEfficiencyEffectResult =
                    new(
                        LivingPopulation:
                            metabolicEfficiencyEffectStatistics
                                .LivingPopulation,
                        BeneficialOrganisms:
                            metabolicEfficiencyEffectStatistics
                                .BeneficialOrganisms,
                        DetrimentalOrganisms:
                            metabolicEfficiencyEffectStatistics
                                .DetrimentalOrganisms,
                        NeutralOrganisms:
                            metabolicEfficiencyEffectStatistics
                                .NeutralOrganisms,
                        AverageModifier:
                            metabolicEfficiencyEffectStatistics
                                .AverageModifier,
                        AverageCostMultiplier:
                            metabolicEfficiencyEffectStatistics
                                .AverageCostMultiplier,
                        MinimumCostMultiplier:
                            metabolicEfficiencyEffectStatistics
                                .MinimumCostMultiplier,
                        MaximumCostMultiplier:
                            metabolicEfficiencyEffectStatistics
                                .MaximumCostMultiplier,
                        AverageEnergyCostDeltaFraction:
                            metabolicEfficiencyEffectStatistics
                                .AverageEnergyCostDeltaFraction
                    );


            HashSet<Guid>
                effectiveParentIds =
                    state.Population
                        .Where(
                            organism =>
                                organism.Generation
                                >
                                0
                                &&
                                organism.Age
                                ==
                                0
                        )
                        .SelectMany(
                            organism =>
                                new[]
                                {
                                    organism.ParentAId,
                                    organism.ParentBId
                                }
                        )
                        .Where(
                            parentId =>
                                parentId.HasValue
                        )
                        .Select(
                            parentId =>
                                parentId!.Value
                        )
                        .ToHashSet();


            List<Organism>
                effectiveParents =
                    state.Population
                        .Where(
                            organism =>
                                effectiveParentIds.Contains(
                                    organism.Id
                                )
                        )
                        .ToList();


            MetabolicSelectionDiagnostics
                metabolicSelectionDiagnostics =
                    _metabolicSelectionDiagnosticsCalculator
                        .Calculate(
                            state.World,
                            state.Population,
                            effectiveParents
                        );


            SimulationMetabolicSelectionDiagnosticsStepResult
                metabolicSelectionDiagnosticsResult =
                    new(
                        Population:
                            metabolicSelectionDiagnostics
                                .Population,
                        EffectiveParents:
                            metabolicSelectionDiagnostics
                                .EffectiveParents,
                        Overall:
                            MapCohort(
                                metabolicSelectionDiagnostics
                                    .Overall
                            ),
                        Parents:
                            MapCohort(
                                metabolicSelectionDiagnostics
                                    .Parents
                            ),
                        Regions:
                            metabolicSelectionDiagnostics
                                .Regions
                                .Select(
                                    region =>
                                        new SimulationMetabolicRegionStepResult(
                                            RegionId:
                                                region.RegionId,
                                            RegionName:
                                                region.RegionName,
                                            Living:
                                                MapCohort(
                                                    region.Living
                                                ),
                                            Parents:
                                                MapCohort(
                                                    region.Parents
                                                )
                                        )
                                )
                                .ToList()
                    );


            ThermalRegulationSelectionDiagnostics
                thermalRegulationDiagnostics =
                    _thermalRegulationSelectionDiagnosticsCalculator
                        .Calculate(
                            state.World,
                            state.Population,
                            effectiveParents
                        );


            SimulationThermalRegulationDiagnosticsStepResult
                thermalRegulationDiagnosticsResult =
                    new(
                        Population:
                            thermalRegulationDiagnostics
                                .Population,

                        EffectiveParents:
                            thermalRegulationDiagnostics
                                .EffectiveParents,

                        Overall:
                            MapThermalRegulationCohort(
                                thermalRegulationDiagnostics
                                    .Overall
                            ),

                        Parents:
                            MapThermalRegulationCohort(
                                thermalRegulationDiagnostics
                                    .Parents
                            ),

                        Regions:
                            thermalRegulationDiagnostics
                                .Regions
                                .Select(
                                    region =>
                                        new SimulationThermalRegulationRegionStepResult(
                                            RegionId:
                                                region.RegionId,

                                            RegionName:
                                                region.RegionName,

                                            Living:
                                                MapThermalRegulationCohort(
                                                    region.Living
                                                ),

                                            Parents:
                                                MapThermalRegulationCohort(
                                                    region.Parents
                                                )
                                        )
                                )
                                .ToList()
                    );


            BodySizeSelectionDiagnostics
                bodySizeDiagnostics =
                    _bodySizeSelectionDiagnosticsCalculator
                        .Calculate(
                            state.World,
                            state.Population,
                            effectiveParents
                        );


            SimulationBodySizeDiagnosticsStepResult
                bodySizeDiagnosticsResult =
                    new(
                        Population:
                            bodySizeDiagnostics
                                .Population,

                        EffectiveParents:
                            bodySizeDiagnostics
                                .EffectiveParents,

                        Overall:
                            MapBodySizeCohort(
                                bodySizeDiagnostics
                                    .Overall
                            ),

                        Parents:
                            MapBodySizeCohort(
                                bodySizeDiagnostics
                                    .Parents
                            ),

                        Regions:
                            bodySizeDiagnostics
                                .Regions
                                .Select(
                                    region =>
                                        new SimulationBodySizeRegionStepResult(
                                            RegionId:
                                                region.RegionId,

                                            RegionName:
                                                region.RegionName,

                                            Living:
                                                MapBodySizeCohort(
                                                    region.Living
                                                ),

                                            Parents:
                                                MapBodySizeCohort(
                                                    region.Parents
                                                )
                                        )
                                )
                                .ToList()
                    );


            LocomotionSelectionDiagnostics
                locomotionDiagnostics =
                    _locomotionSelectionDiagnosticsCalculator
                        .Calculate(
                            state.World,
                            state.Population,
                            effectiveParents
                        );


            SimulationLocomotionDiagnosticsStepResult
                locomotionDiagnosticsResult =
                    new(
                        Population:
                            locomotionDiagnostics
                                .Population,

                        EffectiveParents:
                            locomotionDiagnostics
                                .EffectiveParents,

                        Overall:
                            MapLocomotionCohort(
                                locomotionDiagnostics
                                    .Overall
                            ),

                        Parents:
                            MapLocomotionCohort(
                                locomotionDiagnostics
                                    .Parents
                            ),

                        Regions:
                            locomotionDiagnostics
                                .Regions
                                .Select(
                                    region =>
                                        new SimulationLocomotionRegionStepResult(
                                            RegionId:
                                                region.RegionId,

                                            RegionName:
                                                region.RegionName,

                                            Living:
                                                MapLocomotionCohort(
                                                    region.Living
                                                ),

                                            Parents:
                                                MapLocomotionCohort(
                                                    region.Parents
                                                )
                                        )
                                )
                                .ToList()
                    );


            FeedingSpecializationSelectionDiagnostics
                feedingSpecializationDiagnostics =
                    _feedingSpecializationSelectionDiagnosticsCalculator
                        .Calculate(
                            state.World,
                            state.Population,
                            effectiveParents
                        );


            SimulationFeedingSpecializationDiagnosticsStepResult
                feedingSpecializationDiagnosticsResult =
                    new(
                        Population:
                            feedingSpecializationDiagnostics
                                .Population,

                        EffectiveParents:
                            feedingSpecializationDiagnostics
                                .EffectiveParents,

                        Overall:
                            MapFeedingSpecializationCohort(
                                feedingSpecializationDiagnostics
                                    .Overall
                            ),

                        Parents:
                            MapFeedingSpecializationCohort(
                                feedingSpecializationDiagnostics
                                    .Parents
                            ),

                        Regions:
                            feedingSpecializationDiagnostics
                                .Regions
                                .Select(
                                    region =>
                                        new SimulationFeedingSpecializationRegionStepResult(
                                            RegionId:
                                                region.RegionId,

                                            RegionName:
                                                region.RegionName,

                                            Living:
                                                MapFeedingSpecializationCohort(
                                                    region.Living
                                                ),

                                            Parents:
                                                MapFeedingSpecializationCohort(
                                                    region.Parents
                                                )
                                        )
                                )
                                .ToList()
                    );


            MorphologyStatistics
                morphologyStatistics =
                    _morphologyStatisticsCalculator
                        .Calculate(
                            state.World,
                            state.Population
                        );


            SimulationMorphologyDiagnosticsStepResult
                morphologyDiagnosticsResult =
                    new(
                        LivingPopulation:
                            morphologyStatistics
                                .LivingPopulation,

                        ExpressedOrganisms:
                            morphologyStatistics
                                .ExpressedOrganisms,

                        AverageBodyScale:
                            morphologyStatistics
                                .AverageBodyScale,

                        AverageStructuralExpressionMagnitude:
                            morphologyStatistics
                                .AverageStructuralExpressionMagnitude,

                        Channels:
                            morphologyStatistics
                                .Channels
                                .Select(
                                    MapMorphologyChannel
                                )
                                .ToList(),

                        Regions:
                            morphologyStatistics
                                .Regions
                                .Select(
                                    region =>
                                        new SimulationRegionalMorphologyStepResult(
                                            RegionId:
                                                region.RegionId,

                                            RegionName:
                                                region.RegionName,

                                            Population:
                                                region.Population,

                                            AverageBodyScale:
                                                region.AverageBodyScale,

                                            AverageStructuralExpressionMagnitude:
                                                region.AverageStructuralExpressionMagnitude,

                                            Channels:
                                                region
                                                    .Channels
                                                    .Select(
                                                        MapMorphologyChannel
                                                    )
                                                    .ToList()
                                        )
                                )
                                .ToList()
                    );


            List<SimulationMigrationRouteStepResult>
                migrationRouteResults =
                    migrationSystem.LastMigrationRoutes
                        .Select(
                            route =>
                                new SimulationMigrationRouteStepResult(
                                    OriginRegionId:
                                        route.OriginRegionId,
                                    OriginRegionName:
                                        route.OriginRegionName,
                                    DestinationRegionId:
                                        route.DestinationRegionId,
                                    DestinationRegionName:
                                        route.DestinationRegionName,
                                    Count:
                                        route.Count,
                                    AverageThermalFitness:
                                        route.AverageThermalFitness
                                )
                        )
                        .ToList();


            List<SimulationPatchMovementRouteStepResult>
                localMovementRouteResults =
                    patchMovementSystem.LastMovementRoutes
                        .Select(
                            route =>
                                new SimulationPatchMovementRouteStepResult(
                                    OriginPatchId:
                                        route.OriginPatchId,
                                    OriginPatchName:
                                        route.OriginPatchName,
                                    DestinationPatchId:
                                        route.DestinationPatchId,
                                    DestinationPatchName:
                                        route.DestinationPatchName,
                                    Count:
                                        route.Count
                                )
                        )
                        .ToList();


            List<SimulationRegionConnectionStepResult>
                regionConnectionResults =
                    new();


            foreach (
                Region region
                in state.World.Regions
            )
            {
                foreach (
                    RegionConnection connection
                    in state.World.GetConnections(
                        region.Id
                    )
                )
                {
                    int otherRegionId =
                        connection.GetOtherRegionId(
                            region.Id
                        );


                    if (
                        region.Id
                        >=
                        otherRegionId
                    )
                    {
                        continue;
                    }


                    Region otherRegion =
                        state.World.GetRegion(
                            otherRegionId
                        );


                    regionConnectionResults.Add(
                        new SimulationRegionConnectionStepResult(
                            RegionAId:
                                region.Id,
                            RegionAName:
                                region.Name,
                            RegionBId:
                                otherRegion.Id,
                            RegionBName:
                                otherRegion.Name,
                            MigrationDifficulty:
                                connection.MigrationDifficulty,
                            EnergyCost:
                                connection.EnergyCost
                        )
                    );
                }
            }


            List<SimulationPatchConnectionStepResult>
                patchConnectionResults =
                    new();


            foreach (
                Patch patch
                in state.World.Patches
            )
            {
                foreach (
                    PatchConnection connection
                    in state.World.GetPatchConnections(
                        patch.Id
                    )
                )
                {
                    int otherPatchId =
                        connection.GetOtherPatchId(
                            patch.Id
                        );


                    if (
                        patch.Id
                        >=
                        otherPatchId
                    )
                    {
                        continue;
                    }


                    Patch otherPatch =
                        state.World.GetPatch(
                            otherPatchId
                        );


                    patchConnectionResults.Add(
                        new SimulationPatchConnectionStepResult(
                            PatchAId:
                                patch.Id,
                            PatchAName:
                                patch.Name,
                            PatchBId:
                                otherPatch.Id,
                            PatchBName:
                                otherPatch.Name,
                            MovementDifficulty:
                                connection.MovementDifficulty,
                            EnergyCost:
                                connection.EnergyCost
                        )
                    );
                }
            }


            stepResult =
                new SimulationStepResult(
                    Cycle: cycle,
                    Population: state.Population.Count,
                    Births: births,
                    Deaths: deaths,
                    HuntingKills: feedingResult.HuntingKills,
                    HuntingAttempts: feedingResult.HuntingAttempts,
                    ScavengingActions: feedingResult.ScavengingActions,
                    GrazingActions: feedingResult.GrazingActions,
                    CarcassBiomass: carcassSystem.GetTotalBiomass(),
                    HuntingEnergySpent: feedingResult.HuntingEnergySpent,
                    Migrations: migrations,
                    LocalMovements: localMovements,
                    AverageGeneration: stepAverageGeneration,
                    SpeciesDetectionRan: cycle % speciesDetectionInterval == 0,
                    PendingSpeciesCandidates: speciesDetectionSystem.PendingCandidateCount,
                    Regions: regionResults,
                    Species: speciesResults,
                    MigrationRoutes: migrationRouteResults,
                    LocalMovementRoutes: localMovementRouteResults,
                    RegionConnections: regionConnectionResults,
                    PatchConnections: patchConnectionResults,
                    SpeciesHistory: speciesHistoryResults,
                    SpeciesHistoryEvents: speciesHistoryEventResults,
                    Phylogeny: phylogenyResult,
                    GenomeStructure: genomeStructureResult,
                    PhenotypeExpression: phenotypeExpressionResult,
                    MetabolicEfficiencyEffect: metabolicEfficiencyEffectResult,
                    MetabolicSelectionDiagnostics: metabolicSelectionDiagnosticsResult,
                    IsExtinct: state.Population.Count == 0,
                    BoomBustDiagnostics: boomBustDiagnosticsResult,
                    ThermalRegulationDiagnostics: thermalRegulationDiagnosticsResult,
                    BodySizeDiagnostics: bodySizeDiagnosticsResult,
                    LocomotionDiagnostics: locomotionDiagnosticsResult,
                    FeedingSpecializationDiagnostics: feedingSpecializationDiagnosticsResult,
                    MorphologyDiagnostics: morphologyDiagnosticsResult
                );


            // ========================================================
            // 15. CONSOLA
            // ========================================================

            if (
                cycle % 10 == 0
            )
            {
                double averageGeneration =
                    stepAverageGeneration;


                _output.WriteLine(
                    $"Ciclo: {cycle} | " +
                    $"Poblacion: {state.Population.Count} | " +
                    $"Nacimientos: {births} | " +
                    $"Muertes: {deaths} | " +
                    $"Caza: {feedingResult.HuntingKills}/{feedingResult.HuntingAttempts} | " +
                    $"Carroñeo: {feedingResult.ScavengingActions} | " +
                    $"Pastoreo: {feedingResult.GrazingActions} | " +
                    $"Cadaveres: {carcassSystem.GetTotalBiomass():F0} | " +
                    $"CostoCaza: {feedingResult.HuntingEnergySpent:F0} | " +
                    $"Migraciones: {migrations} | " +
                    $"MovLocal: {localMovements} | " +
                    $"Generacion: {averageGeneration:F1}"
                );


                _output.WriteLine(
                    $"   Boom/Bust | " +
                    $"PlantCap: {boomBustDiagnostics.TotalPlantCapacityFraction:P1} | " +
                    $"Bio/ind: {boomBustDiagnostics.TotalPlantBiomassPerCapita:F2} | " +
                    $"Cons/Regen: {boomBustDiagnostics.ConsumptionToRegrowthRatio:F2} | " +
                    $"SinNac: {boomBustDiagnostics.NoBirthStreak} | " +
                    $"Escasez severa: {boomBustDiagnostics.RegionsWithSevereScarcity} | " +
                    $"Overshoot: {boomBustDiagnostics.RegionsWithOvershootSignal}"
                );


                foreach (
                    PopulationBoomBustRegionDiagnostics boomBustRegion
                    in boomBustDiagnostics.Regions
                        .Where(
                            region =>
                                region.NoBirthStreak >= 5
                                ||
                                region.IsSeverelyResourceScarce
                                ||
                                region.HasOvershootSignal
                                ||
                                region.IsScarcityRecoveryActive
                        )
                )
                {
                    _output.WriteLine(
                        $"      {boomBustRegion.RegionName,-18} | " +
                        $"Cap: {boomBustRegion.PlantCapacityFraction:P1} | " +
                        $"Bio/ind: {boomBustRegion.PlantBiomassPerCapita:F2} | " +
                        $"Cons/Regen: {boomBustRegion.ConsumptionToRegrowthRatio:F2} | " +
                        $"SinNac: {boomBustRegion.NoBirthStreak} | " +
                        $"ScarcityRun: {boomBustRegion.CurrentScarcityRecoveryDuration} | " +
                        $"Episodes: {boomBustRegion.ScarcityEpisodeCount}"
                    );
                }


                _output.WriteLine(
                    $"   Efecto metabolico activo | " +
                    $"ModAvg: {metabolicEfficiencyEffectStatistics.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"CostMultAvg: {metabolicEfficiencyEffectStatistics.AverageCostMultiplier:F4} | " +
                    $"Benef: {metabolicEfficiencyEffectStatistics.BeneficialOrganisms} | " +
                    $"Perjud: {metabolicEfficiencyEffectStatistics.DetrimentalOrganisms} | " +
                    $"Neutros: {metabolicEfficiencyEffectStatistics.NeutralOrganisms} | " +
                    $"RangoMult: {metabolicEfficiencyEffectStatistics.MinimumCostMultiplier:F4}-{metabolicEfficiencyEffectStatistics.MaximumCostMultiplier:F4}"
                );


                _output.WriteLine(
                    $"   Seleccion metabolica | " +
                    $"Living Avg: {metabolicSelectionDiagnostics.Overall.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"Parents Avg: {metabolicSelectionDiagnostics.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"Parents: {metabolicSelectionDiagnostics.EffectiveParents} | " +
                    $"P10/P50/P90: " +
                    $"{metabolicSelectionDiagnostics.Overall.P10Modifier:+0.0000;-0.0000;0.0000}/" +
                    $"{metabolicSelectionDiagnostics.Overall.P50Modifier:+0.0000;-0.0000;0.0000}/" +
                    $"{metabolicSelectionDiagnostics.Overall.P90Modifier:+0.0000;-0.0000;0.0000}"
                );


                foreach (
                    MetabolicRegionSelectionDiagnostics regionDiagnostics
                    in metabolicSelectionDiagnostics.Regions
                )
                {
                    _output.WriteLine(
                        $"      {regionDiagnostics.RegionName,-18} | " +
                        $"Living: {regionDiagnostics.Living.Count,5} | " +
                        $"LAvg: {regionDiagnostics.Living.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"Parents: {regionDiagnostics.Parents.Count,4} | " +
                        $"PAvg: {regionDiagnostics.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"B/D/N: {regionDiagnostics.Living.Beneficial}/" +
                        $"{regionDiagnostics.Living.Detrimental}/" +
                        $"{regionDiagnostics.Living.Neutral}"
                    );
                }


                _output.WriteLine(
                    $"   Regulacion termica | " +
                    $"Living Avg: {thermalRegulationDiagnostics.Overall.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"Parents Avg: {thermalRegulationDiagnostics.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"StressMult: {thermalRegulationDiagnostics.Overall.AverageStressMultiplier:F4} | " +
                    $"Fit: {thermalRegulationDiagnostics.Overall.AverageThermalFitness:F3} | " +
                    $"Stress: {thermalRegulationDiagnostics.Overall.AverageThermalStress:F3} | " +
                    $"Parents: {thermalRegulationDiagnostics.EffectiveParents}"
                );


                foreach (
                    ThermalRegulationRegionSelectionDiagnostics thermalRegion
                    in thermalRegulationDiagnostics.Regions
                )
                {
                    _output.WriteLine(
                        $"      {thermalRegion.RegionName,-18} | " +
                        $"Living: {thermalRegion.Living.Count,5} | " +
                        $"LAvg: {thermalRegion.Living.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"PAvg: {thermalRegion.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"Mult: {thermalRegion.Living.AverageStressMultiplier:F4} | " +
                        $"Fit: {thermalRegion.Living.AverageThermalFitness:F3} | " +
                        $"Stress: {thermalRegion.Living.AverageThermalStress:F3}"
                    );
                }


                _output.WriteLine(
                    $"   BodySize energia | " +
                    $"Living Avg: {bodySizeDiagnostics.Overall.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"Parents Avg: {bodySizeDiagnostics.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"CapMult: {bodySizeDiagnostics.Overall.AverageEnergyCapacityMultiplier:F4} | " +
                    $"BaseMax: {bodySizeDiagnostics.Overall.AverageBaseMaxEnergy:F1} | " +
                    $"Max: {bodySizeDiagnostics.Overall.AverageMaxEnergy:F1} | " +
                    $"Fill: {bodySizeDiagnostics.Overall.AverageEnergyFillFraction:P1} | " +
                    $"Parents: {bodySizeDiagnostics.EffectiveParents}"
                );


                foreach (
                    BodySizeRegionSelectionDiagnostics bodySizeRegion
                    in bodySizeDiagnostics.Regions
                )
                {
                    _output.WriteLine(
                        $"      {bodySizeRegion.RegionName,-18} | " +
                        $"Living: {bodySizeRegion.Living.Count,5} | " +
                        $"LAvg: {bodySizeRegion.Living.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"PAvg: {bodySizeRegion.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"CapMult: {bodySizeRegion.Living.AverageEnergyCapacityMultiplier:F4} | " +
                        $"GenSize: {bodySizeRegion.Living.AverageGeneticSize:F3} | " +
                        $"MaxE: {bodySizeRegion.Living.AverageMaxEnergy:F1}"
                    );
                }


                _output.WriteLine(
                    $"   Locomotion energia | " +
                    $"Living Avg: {locomotionDiagnostics.Overall.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"Parents Avg: {locomotionDiagnostics.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"ActMult: {locomotionDiagnostics.Overall.AverageActivityCostMultiplier:F4} | " +
                    $"BaseAct: {locomotionDiagnostics.Overall.AverageBaseActivityCost:F3} | " +
                    $"Act: {locomotionDiagnostics.Overall.AverageActivityCost:F3} | " +
                    $"EnergyCost: {locomotionDiagnostics.Overall.AverageEnergyCost:F3} | " +
                    $"Parents: {locomotionDiagnostics.EffectiveParents}"
                );


                foreach (
                    LocomotionRegionSelectionDiagnostics locomotionRegion
                    in locomotionDiagnostics.Regions
                )
                {
                    _output.WriteLine(
                        $"      {locomotionRegion.RegionName,-18} | " +
                        $"Living: {locomotionRegion.Living.Count,5} | " +
                        $"LAvg: {locomotionRegion.Living.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"PAvg: {locomotionRegion.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"ActMult: {locomotionRegion.Living.AverageActivityCostMultiplier:F4} | " +
                        $"BaseAct: {locomotionRegion.Living.AverageBaseActivityCost:F3} | " +
                        $"Act: {locomotionRegion.Living.AverageActivityCost:F3}"
                    );
                }


                _output.WriteLine(
                    $"   FeedingSpecialization vegetal | " +
                    $"Living Avg: {feedingSpecializationDiagnostics.Overall.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"Parents Avg: {feedingSpecializationDiagnostics.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                    $"EnergyMult: {feedingSpecializationDiagnostics.Overall.AveragePlantEnergyMultiplier:F4} | " +
                    $"PlantDig: {feedingSpecializationDiagnostics.Overall.AveragePlantDigestionEfficiency:F3} | " +
                    $"Assim: {feedingSpecializationDiagnostics.Overall.AverageEffectivePlantEnergyAssimilation:F3} | " +
                    $"Fill: {feedingSpecializationDiagnostics.Overall.AverageEnergyFillFraction:P1} | " +
                    $"Parents: {feedingSpecializationDiagnostics.EffectiveParents}"
                );


                foreach (
                    FeedingSpecializationRegionSelectionDiagnostics feedingRegion
                    in feedingSpecializationDiagnostics.Regions
                )
                {
                    _output.WriteLine(
                        $"      {feedingRegion.RegionName,-18} | " +
                        $"Living: {feedingRegion.Living.Count,5} | " +
                        $"LAvg: {feedingRegion.Living.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"PAvg: {feedingRegion.Parents.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"EnergyMult: {feedingRegion.Living.AveragePlantEnergyMultiplier:F4} | " +
                        $"PlantDig: {feedingRegion.Living.AveragePlantDigestionEfficiency:F3} | " +
                        $"Assim: {feedingRegion.Living.AverageEffectivePlantEnergyAssimilation:F3}"
                    );
                }


                _output.WriteLine(
                    $"   Morfologia | " +
                    $"Expresados: {morphologyStatistics.ExpressedOrganisms}/{morphologyStatistics.LivingPopulation} | " +
                    $"BodyScale: {morphologyStatistics.AverageBodyScale:F3} | " +
                    $"StructMag: {morphologyStatistics.AverageStructuralExpressionMagnitude:F4}"
                );


                foreach (
                    MorphologyChannelStatistics morphologyChannel
                    in morphologyStatistics.Channels
                )
                {
                    _output.WriteLine(
                        $"      {morphologyChannel.Channel,-20} | " +
                        $"Expr: {morphologyChannel.ExpressingOrganisms,5} | " +
                        $"Avg: {morphologyChannel.AverageModifier:+0.0000;-0.0000;0.0000} | " +
                        $"Std: {morphologyChannel.StandardDeviation:F4} | " +
                        $"Min: {morphologyChannel.MinimumModifier:+0.0000;-0.0000;0.0000} | " +
                        $"Max: {morphologyChannel.MaximumModifier:+0.0000;-0.0000;0.0000}"
                    );
                }


                foreach (
                    RegionalMorphologyStatistics morphologyRegion
                    in morphologyStatistics.Regions
                )
                {
                    _output.WriteLine(
                        $"      {morphologyRegion.RegionName,-18} | " +
                        $"Pop: {morphologyRegion.Population,5} | " +
                        $"BodyScale: {morphologyRegion.AverageBodyScale:F3} | " +
                        $"StructMag: {morphologyRegion.AverageStructuralExpressionMagnitude:F4}"
                    );
                }


                // ====================================================
                // REGIONES
                // ====================================================

                foreach (
                    Region region
                    in state.World.Regions
                )
                {
                    List<Organism> regionalPopulation =
                        state.Population
                            .Where(
                                organism =>
                                    organism.RegionId
                                    ==
                                    region.Id
                            )
                            .ToList();


                    if (
                        regionalPopulation.Count == 0
                    )
                    {
                        _output.WriteLine(
                            $"   {region.Name,-16} | " +
                            $"Temp: {region.Temperature,5:F1} C | " +
                            $"Pop: {0,6} | " +
                            $"Nac: {birthsByRegion[region.Id],4} | " +
                            $"Muert: {deathsByRegion[region.Id],4} | " +
                            $"Plantas: {region.TotalPlantBiomass,8:F0} | " +
                            $"Carne: {carcassSystem.GetRegionalBiomass(state.World, region.Id),7:F0}"
                        );


                        continue;
                    }


                    double averageOptimalTemperature =
                        regionalPopulation.Average(
                            organism =>
                                organism.OptimalTemperature
                        );


                    double averageThermalTolerance =
                        regionalPopulation.Average(
                            organism =>
                                organism.ThermalTolerance
                        );


                    double optimalTemperatureStdDev =
                        Math.Sqrt(
                            regionalPopulation.Average(
                                organism =>
                                    Math.Pow(
                                        organism.OptimalTemperature
                                        -
                                        averageOptimalTemperature,
                                        2
                                    )
                            )
                        );


                    double thermalToleranceStdDev =
                        Math.Sqrt(
                            regionalPopulation.Average(
                                organism =>
                                    Math.Pow(
                                        organism.ThermalTolerance
                                        -
                                        averageThermalTolerance,
                                        2
                                    )
                            )
                        );


                    double averageThermalStress =
                        regionalPopulation.Average(
                            organism =>
                            {
                                Patch organismPatch =
                                    state.World.GetPatch(
                                        organism.PatchId
                                    );


                                double localTemperature =
                                    organismPatch.GetLocalTemperature(
                                        region.Temperature
                                    );


                                return organism.GetThermalStress(
                                    localTemperature
                                );
                            }
                        );


                    double averageSpeed =
                        regionalPopulation.Average(
                            organism =>
                                organism.Speed
                        );


                    double averageSize =
                        regionalPopulation.Average(
                            organism =>
                                organism.Size
                        );


                    double averageMetabolism =
                        regionalPopulation.Average(
                            organism =>
                                organism.Metabolism
                        );


                    double averageMeatAdaptation =
                        regionalPopulation.Average(
                            organism =>
                                organism.MeatAdaptation
                        );


                    double maximumMeatAdaptation =
                        regionalPopulation.Max(
                            organism =>
                                organism.MeatAdaptation
                        );


                    double averagePredatoryDrive =
                        regionalPopulation.Average(
                            organism =>
                                organism.PredatoryDrive
                        );


                    double maximumPredatoryDrive =
                        regionalPopulation.Max(
                            organism =>
                                organism.PredatoryDrive
                        );


                    double averageExplorationDrive =
                        regionalPopulation.Average(
                            organism =>
                                organism.ExplorationDrive
                        );


                    double maximumExplorationDrive =
                        regionalPopulation.Max(
                            organism =>
                                organism.ExplorationDrive
                        );


                    double averageRiskTolerance =
                        regionalPopulation.Average(
                            organism =>
                                organism.RiskTolerance
                        );


                    double averageScavengingDrive =
                        regionalPopulation.Average(
                            organism =>
                                organism.ScavengingDrive
                        );


                    double averageMateSelectivity =
                        regionalPopulation.Average(
                            organism =>
                                organism.MateSelectivity
                        );


                    int meatAdaptedCount =
                        regionalPopulation.Count(
                            organism =>
                                organism.MeatAdaptation
                                >=
                                0.10
                        );


                    int predatoryCount =
                        regionalPopulation.Count(
                            organism =>
                                organism.PredatoryDrive
                                >=
                                0.10
                        );


                    int explorerCount =
                        regionalPopulation.Count(
                            organism =>
                                organism.ExplorationDrive
                                >=
                                0.20
                        );


                    _output.WriteLine(
                        $"   {region.Name,-16} | " +
                        $"Temp: {region.Temperature,5:F1} C | " +
                        $"Pop: {regionalPopulation.Count,6} | " +
                        $"Nac: {birthsByRegion[region.Id],4} | " +
                        $"Muert: {deathsByRegion[region.Id],4} | " +
                        $"Plantas: {region.TotalPlantBiomass,8:F0} | " +
                        $"Carne: {carcassSystem.GetRegionalBiomass(state.World, region.Id),7:F0} | " +
                        $"OptTemp: {averageOptimalTemperature,5:F2} | " +
                        $"OptStd: {optimalTemperatureStdDev,4:F2} | " +
                        $"Tol: {averageThermalTolerance,4:F2} | " +
                        $"TolStd: {thermalToleranceStdDev,4:F2} | " +
                        $"Stress: {averageThermalStress,5:F2} | " +
                        $"Speed: {averageSpeed,4:F2} | " +
                        $"Size: {averageSize,4:F2} | " +
                        $"Met: {averageMetabolism,4:F2} | " +
                        $"Meat: {averageMeatAdaptation,4:F2} | " +
                        $"MeatMax: {maximumMeatAdaptation,4:F2} | " +
                        $"Pred: {averagePredatoryDrive,4:F2} | " +
                        $"PredMax: {maximumPredatoryDrive,4:F2} | " +
                        $"Explore: {averageExplorationDrive,4:F2} | " +
                        $"ExploreMax: {maximumExplorationDrive,4:F2} | " +
                        $"Meat10+: {meatAdaptedCount,4} | " +
                        $"Pred10+: {predatoryCount,4} | " +
                        $"Explore20+: {explorerCount,4} | " +
                        $"Risk: {averageRiskTolerance,4:F2} | " +
                        $"Scav: {averageScavengingDrive,4:F2} | " +
                        $"MateSel: {averageMateSelectivity,4:F2}"
                    );
                }


                // ====================================================
                // BALANCE VEGETAL POR REGIÓN
                // ====================================================

                _output.WriteLine();

                _output.WriteLine(
                    "   Balance vegetal:"
                );


                foreach (
                    PlantResourceRegionStatistics plantStats
                    in plantResourceStatistics
                )
                {
                    string consumeRegrowthText =
                        double.IsPositiveInfinity(
                            plantStats.ConsumptionToRegrowthRatio
                        )

                            ?

                            "INF"

                            :

                            plantStats
                                .ConsumptionToRegrowthRatio
                                .ToString(
                                    "F2"
                                );


                    _output.WriteLine(
                        $"      {plantStats.RegionName,-16} | " +
                        $"Inicio: {plantStats.StartBiomass,7:F0} | " +
                        $"Regen: +{plantStats.Regrowth,6:F0} | " +
                        $"PreFeed: {plantStats.PreFeedingBiomass,7:F0} | " +
                        $"Consumido: {plantStats.ConsumedBiomass,7:F0} | " +
                        $"Final: {plantStats.EndBiomass,7:F0} | " +
                        $"Cap: {plantStats.EndCapacityFraction,6:P1} | " +
                        $"Bio/ind: {plantStats.EndBiomassPerCapita,6:F2} | " +
                        $"Cons/Regen: {consumeRegrowthText}"
                    );


                    _output.WriteLine(
                        $"         Reservas -> " +
                        $"Raices: {plantStats.RootBiomass,7:F0}/" +
                        $"{plantStats.RootCapacity,7:F0} " +
                        $"({plantStats.RootCapacityFraction:P1}) | " +
                        $"Semillas: {plantStats.SeedBank,7:F0}/" +
                        $"{plantStats.SeedBankCapacity,7:F0} " +
                        $"({plantStats.SeedBankCapacityFraction:P1})"
                    );
                }


                // ====================================================
                // ELEGIBILIDAD REPRODUCTIVA POR REGIÓN
                // ====================================================
                //
                // Este diagnóstico representa el estado inmediatamente
                // anterior a la reproducción del ciclo.
                //
                // EnergyOK cuenta organismos vivos con energía >= al
                // threshold, independientemente de su edad.
                //
                // CooldownOK cuenta únicamente adultos cuyo cooldown
                // efectivo ya estaba listo.
                //
                // Candidatos aplica exactamente:
                // Energy >= threshold && CanReproduce(localTemperature).
                //

                _output.WriteLine();

                _output.WriteLine(
                    "   Elegibilidad reproductiva:"
                );


                foreach (
                    Region eligibilityRegion
                    in state.World.Regions
                )
                {
                    var eligibility =
                        reproductiveEligibilityByRegion[
                            eligibilityRegion.Id
                        ];


                    _output.WriteLine(
                        $"      {eligibilityRegion.Name,-16} | " +
                        $"Vivos: {eligibility.Alive,5} | " +
                        $"Adultos: {eligibility.Adults,5} | " +
                        $"EnergyOK: {eligibility.EnergyReady,5} | " +
                        $"CooldownOK: {eligibility.CooldownReady,5} | " +
                        $"Candidatos: {eligibility.Candidates,5} | " +
                        $"Energia media: {eligibility.AverageEnergy,6:F1} | " +
                        $"Max media: {eligibility.AverageMaxEnergy,6:F1}"
                    );


                    string patchCandidateSummary =
                        string.Join(
                            " | ",

                            eligibilityRegion.Patches
                                .Select(
                                    patch =>
                                        $"{patch.Name}: " +
                                        reproductiveCandidatesByPatch[
                                            eligibilityRegion.Id
                                        ][
                                            patch.Id
                                        ]
                                )
                        );


                    _output.WriteLine(
                        $"         Candidatos por patch -> " +
                        patchCandidateSummary
                    );
                }


                // ====================================================
                // REPRODUCTORES EFECTIVOS POR REGIÓN
                // ====================================================
                //
                // Aquí no contamos candidatos potenciales.
                // Contamos únicamente padres que realmente produjeron
                // al menos un descendiente durante ESTE ciclo.
                //
                // FitTerm media se calcula usando el microclima del
                // patch donde se encontraba cada padre.
                //

                _output.WriteLine();

                _output.WriteLine(
                    "   Reproductores efectivos:"
                );


                foreach (
                    Region reproductiveRegion
                    in state.World.Regions
                )
                {
                    List<Organism> successfulParents =
                        successfulParentsByRegion[
                            reproductiveRegion.Id
                        ];


                    double parentAverageOptimalTemperature =
                        successfulParents.Count > 0

                            ?

                            successfulParents.Average(
                                organism =>
                                    organism.OptimalTemperature
                            )

                            :

                            0;


                    double parentAverageThermalFitness =
                        successfulParents.Count > 0

                            ?

                            successfulParents.Average(
                                organism =>
                                {
                                    Patch parentPatch =
                                        state.World.GetPatch(
                                            organism.PatchId
                                        );


                                    double parentLocalTemperature =
                                        parentPatch.GetLocalTemperature(
                                            reproductiveRegion.Temperature
                                        );


                                    return organism.GetThermalFitness(
                                        parentLocalTemperature
                                    );
                                }
                            )

                            :

                            0;


                    _output.WriteLine(
                        $"      {reproductiveRegion.Name,-16} | " +
                        $"Nac: {birthsByRegion[reproductiveRegion.Id],4} | " +
                        $"Padres: {successfulParents.Count,4} | " +
                        $"OptTemp padres: {parentAverageOptimalTemperature,5:F2} | " +
                        $"FitTerm padres: {parentAverageThermalFitness:F3}"
                    );
                }


                // ====================================================
                // PAREJAS REPRODUCTIVAS REALES
                // ====================================================
                //
                // Estos valores describen únicamente parejas que
                // realmente produjeron descendencia durante el ciclo.
                //
                // EcoSim < 0.70 y MateAccept < 0.50 NO son reglas del
                // modelo. Son cortes diagnósticos para detectar cruces
                // entre organismos ecológicamente muy distintos.
                //

                _output.WriteLine();

                _output.WriteLine(
                    "   Parejas reproductivas reales:"
                );


                foreach (
                    Region matingRegion
                    in state.World.Regions
                )
                {
                    SuccessfulMatingRegionStatistics? matingStats =
                        successfulMatingStatistics
                            .FirstOrDefault(
                                stats =>
                                    stats.RegionId
                                    ==
                                    matingRegion.Id
                            );


                    if (
                        matingStats is null
                        ||
                        matingStats.ResolvedPairCount == 0
                    )
                    {
                        _output.WriteLine(
                            $"      {matingRegion.Name,-16} | " +
                            $"Parejas:    0 | " +
                            $"GenDist avg/max: 0.000/0.000 | " +
                            $"EcoSim avg/min: 0.000/0.000 | " +
                            $"MateAccept avg/min: 0.000/0.000 | " +
                            $"Eco<.70:   0 | Accept<.50:   0"
                        );

                        continue;
                    }


                    _output.WriteLine(
                        $"      {matingRegion.Name,-16} | " +
                        $"Parejas: {matingStats.ResolvedPairCount,4} | " +
                        $"GenDist avg/max: " +
                        $"{matingStats.AverageGeneticDistance:F3}/" +
                        $"{matingStats.MaximumGeneticDistance:F3} | " +
                        $"EcoSim avg/min: " +
                        $"{matingStats.AverageEcologicalSimilarity:F3}/" +
                        $"{matingStats.MinimumEcologicalSimilarity:F3} | " +
                        $"MateAccept avg/min: " +
                        $"{matingStats.AverageMateAcceptance:F3}/" +
                        $"{matingStats.MinimumMateAcceptance:F3} | " +
                        $"Eco<.70: {matingStats.LowEcologicalSimilarityPairs,3} | " +
                        $"Accept<.50: {matingStats.LowMateAcceptancePairs,3}"
                    );


                    _output.WriteLine(
                        $"         Delta OptTemp avg/max: " +
                        $"{matingStats.AverageOptimalTemperatureDifference:F2}/" +
                        $"{matingStats.MaximumOptimalTemperatureDifference:F2} C | " +
                        $"No resueltas: {matingStats.UnresolvedBirthCount}"
                    );
                }


                // ====================================================
                // ÉXITO REPRODUCTIVO DE INMIGRANTES
                // ====================================================
                //
                // Un organismo se considera inmigrante si su migración
                // interregional más reciente terminó en su región actual.
                // Sus descendientes NO se marcan automáticamente como
                // inmigrantes: aquí medimos flujo génico directo causado
                // por individuos que realmente cruzaron entre regiones.
                //

                _output.WriteLine();

                _output.WriteLine(
                    "   Exito reproductivo de inmigrantes:"
                );


                foreach (
                    Region immigrantRegion
                    in state.World.Regions
                )
                {
                    ImmigrantReproductiveRegionStatistics? immigrantStats =
                        immigrantReproductiveStatistics
                            .FirstOrDefault(
                                stats =>
                                    stats.RegionId
                                    ==
                                    immigrantRegion.Id
                            );


                    if (
                        immigrantStats is null
                    )
                    {
                        continue;
                    }


                    _output.WriteLine(
                        $"      {immigrantRegion.Name,-16} | " +
                        $"Inm vivos: {immigrantStats.TrackedImmigrantsAlive,4} | " +
                        $"Cand inm: {immigrantStats.TrackedImmigrantCandidates,4} | " +
                        $"Padres inm: {immigrantStats.UniqueImmigrantParents,3} | " +
                        $"Nac c/inm: {immigrantStats.BirthsWithImmigrantParent,3} | " +
                        $"RR: {immigrantStats.ResidentResidentBirths,3} | " +
                        $"RxI: {immigrantStats.ResidentImmigrantBirths,3} | " +
                        $"IxI: {immigrantStats.ImmigrantImmigrantBirths,3}"
                    );


                    _output.WriteLine(
                        $"         Llegaron ciclo: {immigrantStats.SameCycleArrivals,3} | " +
                        $"Cand nuevos: {immigrantStats.SameCycleImmigrantCandidates,3} | " +
                        $"Padres nuevos: {immigrantStats.SameCycleImmigrantParents,3} | " +
                        $"OptTemp padres inm/res: " +
                        $"{immigrantStats.AverageImmigrantParentOptimalTemperature:F2}/" +
                        $"{immigrantStats.AverageResidentParentOptimalTemperature:F2} | " +
                        $"No resueltas: {immigrantStats.UnresolvedBirths}"
                    );


                    if (
                        immigrantStats.ParentOrigins.Count == 0
                    )
                    {
                        _output.WriteLine(
                            "         Origen padres inmigrantes -> ninguno"
                        );
                    }
                    else
                    {
                        string originSummary =
                            string.Join(
                                " | ",
                                immigrantStats.ParentOrigins
                                    .Select(
                                        origin =>
                                            $"{origin.OriginRegionName}: " +
                                            $"Padres {origin.UniqueParentCount}, " +
                                            $"Nac {origin.BirthsInvolvingOrigin}"
                                    )
                            );


                        _output.WriteLine(
                            $"         Origen padres inmigrantes -> {originSummary}"
                        );
                    }
                }


                // ====================================================
                // MIGRACIONES POR RUTA
                // ====================================================
                //
                // Diagnóstico únicamente.
                //
                // Muestra las migraciones exitosas ocurridas durante
                // ESTE ciclo y la fitness térmica media que tenían los
                // inmigrantes respecto a la región de destino.
                //

                IReadOnlyList<MigrationRouteStatistics>
                    migrationRoutes =
                        migrationSystem.LastMigrationRoutes;


                _output.WriteLine();

                _output.WriteLine(
                    "   Migraciones por ruta:"
                );


                if (
                    migrationRoutes.Count == 0
                )
                {
                    _output.WriteLine(
                        "      Ninguna migracion exitosa en este ciclo."
                    );
                }
                else
                {
                    foreach (
                        MigrationRouteStatistics route
                        in migrationRoutes
                    )
                    {
                        _output.WriteLine(
                            $"      {route.OriginRegionName} -> " +
                            $"{route.DestinationRegionName}: " +
                            $"{route.Count,4} | " +
                            $"FitTerm media: {route.AverageThermalFitness:F3}"
                        );
                    }
                }


                // ====================================================
                // INMIGRACIÓN RECIBIDA POR REGIÓN
                // ====================================================
                //
                // Esto nos permite detectar rápidamente si una región
                // pequeña está siendo recolonizada desde afuera.
                //

                _output.WriteLine(
                    "   Inmigracion recibida:"
                );


                foreach (
                    Region destinationRegion
                    in state.World.Regions
                )
                {
                    List<MigrationRouteStatistics> incomingRoutes =
                        migrationRoutes
                            .Where(
                                route =>
                                    route.DestinationRegionId
                                    ==
                                    destinationRegion.Id
                            )
                            .ToList();


                    int incomingCount =
                        incomingRoutes.Sum(
                            route =>
                                route.Count
                        );


                    double incomingThermalFitness =
                        incomingCount > 0

                            ?

                            incomingRoutes.Sum(
                                route =>
                                    route.AverageThermalFitness
                                    *
                                    route.Count
                            )
                            /
                            incomingCount

                            :

                            0;


                    _output.WriteLine(
                        $"      {destinationRegion.Name,-16} | " +
                        $"Inmigrantes: {incomingCount,4} | " +
                        $"FitTerm media: {incomingThermalFitness:F3}"
                    );
                }


                // ====================================================
                // PATCHES
                // ====================================================

                if (
                    cycle % 50 == 0
                )
                {
                    foreach (
                        Region patchRegion
                        in state.World.Regions
                    )
                    {
                        string patchSummary =
                            string.Join(
                                " | ",
                                patchRegion.Patches.Select(
                                    patch =>
                                    {
                                        int count =
                                            state.Population.Count(
                                                organism =>
                                                    organism.RegionId
                                                    ==
                                                    patchRegion.Id
                                                    &&
                                                    organism.PatchId
                                                    ==
                                                    patch.Id
                                            );


                                        double localTemperature =
                                     patch.GetLocalTemperature(
                                         patchRegion.Temperature
                                     );


                                        return
                                            $"{patch.Name}: " +
                                            $"Pop {count}, " +
                                            $"Temp {localTemperature:F1}C, " +
                                            $"Plant {patch.Plants.Biomass:F0}, " +
                                            $"ED {patch.Plants.EnergyDensity:F2}, " +
                                            $"Tough {patch.Plants.Toughness:F2}, " +
                                            $"Meat {carcassSystem.GetPatchBiomass(patch.Id):F0}";
                                    }
                                )
                            );


                        _output.WriteLine(
                            $"      Patches [{patchRegion.Name}] -> " +
                            patchSummary
                        );
                    }
                }


                // ====================================================
                // VARIACIÓN DE LOS 11 RASGOS POR REGIÓN
                // ====================================================
                //
                // Diagnóstico pasivo. Se imprime cada 100 ciclos y
                // también en el ciclo final. No consume Random.
                //
                if (
                    cycle % 100 == 0
                    ||
                    cycle == cycles
                )
                {
                    IReadOnlyList<RegionalTraitVariationReport>
                        traitVariationReports =
                            regionalTraitVariationCalculator.Calculate(
                                state.Population,
                                state.World.Regions
                            );


                    regionalTraitVariationPrinter.Print(
                        cycle,
                        traitVariationReports
                    );


                    IReadOnlyList<RegionalGeneticDistanceBreakdown>
                        geneticDistanceBreakdownReports =
                            geneticDistanceBreakdownCalculator.Calculate(
                                state.Population,
                                state.World.Regions
                            );


                    geneticDistanceBreakdownPrinter.PrintRegional(
                        cycle,
                        geneticDistanceBreakdownReports
                    );


                    IReadOnlyList<SpeciesGeneticDistanceBreakdown>
                        speciesGeneticDistanceBreakdownReports =
                            geneticDistanceBreakdownCalculator.CalculateSpecies(
                                state.Population,
                                state.DetectedSpecies
                            );


                    geneticDistanceBreakdownPrinter.PrintSpecies(
                        cycle,
                        speciesGeneticDistanceBreakdownReports
                    );
                }


                // ====================================================
                // DIVERGENCIA GENÉTICA ENTRE REGIONES
                // ====================================================

                if (
                    cycle
                    %
                    speciesDetectionInterval
                    ==
                    0
                )
                {
                    List<RegionalGeneticDistance> regionalDistances =
                        regionalGeneticDivergenceCalculator.Calculate(
                            state.Population,
                            state.World
                        );


                    if (
                        regionalDistances.Count > 0
                    )
                    {
                        _output.WriteLine();

                        _output.WriteLine(
                            "   Divergencia genetica regional:"
                        );


                        foreach (
                            RegionalGeneticDistance regionalDistance
                            in regionalDistances
                        )
                        {
                            _output.WriteLine(
                                $"      {regionalDistance.RegionAName} <-> " +
                                $"{regionalDistance.RegionBName}: " +
                                $"{regionalDistance.Distance:F3}"
                            );
                        }


                        List<RegionalReproductiveIsolation>
                            regionalIsolation =
                                regionalReproductiveIsolationCalculator.Calculate(
                                    state.Population,
                                    state.World
                                );


                        if (
                            regionalIsolation.Count > 0
                        )
                        {
                            _output.WriteLine();

                            _output.WriteLine(
                                "   Aislamiento reproductivo regional:"
                            );


                            foreach (
                                RegionalReproductiveIsolation isolation
                                in regionalIsolation
                            )
                            {
                                _output.WriteLine(
                                    $"      {isolation.RegionAName} <-> " +
                                    $"{isolation.RegionBName} | " +
                                    $"GenDist: {isolation.GeneticDistance:F3} | " +
                                    $"GenCompat: {isolation.GeneticCompatibility:F3} | " +
                                    $"EcoSim: {isolation.EcologicalSimilarity:F3} | " +
                                    $"MateSel: {isolation.PairMateSelectivity:F3} | " +
                                    $"EcoPref: {isolation.EcologicalPreference:F3} | " +
                                    $"MateAccept: {isolation.MateAcceptance:F3}"
                                );
                            }
                        }
                    }
                }


                // ====================================================
                // ESPECIES
                // ====================================================

                if (
                    cycle
                    %
                    speciesDetectionInterval
                    ==
                    0
                )
                {
                    _output.WriteLine();


                    _output.WriteLine(
                        $"   Especies confirmadas: {state.DetectedSpecies.Count} | " +
                        $"Candidatos: {speciesDetectionSystem.PendingCandidateCount}"
                    );


                    foreach (
                        SpeciesCluster species
                        in state.DetectedSpecies
                    )
                    {
                        string parent =
                            species.ParentSpeciesId
                            is null

                                ?

                                "S-"

                                :

                                $"S{species.ParentSpeciesId}";


                        Genome centroid =
                            species.Centroid;


                        _output.WriteLine(
                            $"      S{species.SpeciesId} | " +
                            $"Pop: {species.Population,6} | " +
                            $"Parent: {parent,-3} | " +
                            $"Desde: {species.FirstDetectedCycle,4} | " +
                            $"Dist: {species.AverageDistanceToCentroid:F3} | " +
                            $"Speed: {centroid.Speed:F2} | " +
                            $"Size: {centroid.Size:F2} | " +
                            $"Met: {centroid.Metabolism:F2} | " +
                            $"OptTemp: {centroid.OptimalTemperature:F2} | " +
                            $"Tol: {centroid.ThermalTolerance:F2} | " +
                            $"Meat: {centroid.MeatAdaptation:F2} | " +
                            $"Pred: {centroid.PredatoryDrive:F2} | " +
                            $"Scav: {centroid.ScavengingDrive:F2} | " +
                            $"Explore: {centroid.ExplorationDrive:F2} | " +
                            $"Risk: {centroid.RiskTolerance:F2} | " +
                            $"MateSel: {centroid.MateSelectivity:F2}"
                        );
                    }
                }


                _output.WriteLine();
            }


            // ========================================================
            // EXTINCIÓN
            // ========================================================

            if (
                state.Population.Count == 0
            )
            {
                _output.WriteLine(
                    $"EXTINCION TOTAL EN EL CICLO {cycle}"
                );


                break;
            }
        }

        return stepResult;
    }

    private SimulationStepResult RunFastExperimentTail(
        int cycle,
        int populationAtStart,
        FeedingResult feedingResult,
        int migrations,
        int localMovements,
        SexualReproductionSystem reproductionSystem,
        CarcassSystem carcassSystem,
        SpeciesDetectionSystem speciesDetectionSystem)
    {
        SimulationConfig config =
            _config;

        SimulationState state =
            _state;


        // ============================================================
        // 9. REPRODUCCIÓN — MISMA LLAMADA DEL MOTOR NORMAL
        // ============================================================

        List<Organism> newborns =
            reproductionSystem.Reproduce(
                state.Population,
                state.World,
                config.ReproductionThreshold,
                config.ReproductionCostPerParent,
                config.ChildInitialEnergy
            );


        int births =
            newborns.Count;


        // ============================================================
        // 10. DESCENDENCIA
        // ============================================================

        state.Population.AddRange(
            newborns
        );


        // ============================================================
        // 11. MUERTES RESTANTES
        // ============================================================
        //
        // Deliberadamente preserva el comportamiento del motor normal,
        // incluyendo este segundo recorrido de registro antes de
        // eliminar los muertos.
        //

        foreach (
            Organism organism
            in state.Population
        )
        {
            if (
                organism.IsAlive
            )
            {
                continue;
            }


            carcassSystem.RegisterDeath(
                organism
            );
        }


        // ============================================================
        // 12. REMOVER MUERTOS
        // ============================================================

        state.Population =
            state.Population
                .Where(
                    organism =>
                        organism.IsAlive
                )
                .ToList();


        // ============================================================
        // 13. MUERTES DEL CICLO
        // ============================================================

        int deaths =
            populationAtStart
            +
            births
            -
            state.Population.Count;


        // ============================================================
        // 14. ESPECIES — MISMA FRECUENCIA
        // ============================================================

        if (
            cycle
            %
            config.SpeciesDetectionInterval
            ==
            0
        )
        {
            state.DetectedSpecies =
                speciesDetectionSystem.Detect(
                    state.Population,
                    cycle
                );

            _speciesHistoryTracker.RecordDetection(
                cycle:
                    cycle,
                detectedSpecies:
                    state.DetectedSpecies,
                simulationExtinct:
                    state.Population.Count == 0
            );
        }


        if (
            state.Population.Count == 0
            &&
            cycle % config.SpeciesDetectionInterval != 0
        )
        {
            _speciesHistoryTracker.RecordSimulationExtinction(
                cycle
            );
        }


        // ============================================================
        // RESULTADO MÍNIMO
        // ============================================================
        //
        // El runner solo necesita Population/Births/Deaths/Cycle en
        // ciclos intermedios. Los diagnósticos completos se calculan
        // en C1600 por la ruta normal.
        //

        double averageGeneration =
            state.Population.Count > 0
                ?
                state.Population.Average(
                    organism =>
                        organism.Generation
                )
                :
                0;


        SimulationMetabolicCohortStepResult
            emptyCohort =
                new(
                    Name:
                        "Fast",
                    Count:
                        0,
                    Beneficial:
                        0,
                    Detrimental:
                        0,
                    Neutral:
                        0,
                    AverageModifier:
                        0,
                    AverageCostMultiplier:
                        1,
                    P10Modifier:
                        0,
                    P50Modifier:
                        0,
                    P90Modifier:
                        0
                );


        return new SimulationStepResult(
            Cycle:
                cycle,
            Population:
                state.Population.Count,
            Births:
                births,
            Deaths:
                deaths,
            HuntingKills:
                feedingResult.HuntingKills,
            HuntingAttempts:
                feedingResult.HuntingAttempts,
            ScavengingActions:
                feedingResult.ScavengingActions,
            GrazingActions:
                feedingResult.GrazingActions,
            CarcassBiomass:
                carcassSystem.GetTotalBiomass(),
            HuntingEnergySpent:
                feedingResult.HuntingEnergySpent,
            Migrations:
                migrations,
            LocalMovements:
                localMovements,
            AverageGeneration:
                averageGeneration,
            SpeciesDetectionRan:
                cycle
                %
                config.SpeciesDetectionInterval
                ==
                0,
            PendingSpeciesCandidates:
                speciesDetectionSystem.PendingCandidateCount,
            Regions:
                Array.Empty<SimulationRegionStepResult>(),
            Species:
                Array.Empty<SimulationSpeciesStepResult>(),
            MigrationRoutes:
                Array.Empty<SimulationMigrationRouteStepResult>(),
            LocalMovementRoutes:
                Array.Empty<SimulationPatchMovementRouteStepResult>(),
            RegionConnections:
                Array.Empty<SimulationRegionConnectionStepResult>(),
            PatchConnections:
                Array.Empty<SimulationPatchConnectionStepResult>(),
            SpeciesHistory:
                Array.Empty<SimulationSpeciesHistoryStepResult>(),
            SpeciesHistoryEvents:
                Array.Empty<SimulationSpeciesHistoryEventStepResult>(),
            Phylogeny:
                new SimulationPhylogenyStepResult(
                    CurrentCycle:
                        cycle,
                    MaximumDepth:
                        0,
                    RootSpeciesIds:
                        Array.Empty<int>(),
                    Nodes:
                        Array.Empty<SimulationPhylogenyNodeStepResult>(),
                    Edges:
                        Array.Empty<SimulationPhylogenyEdgeStepResult>()
                ),
            GenomeStructure:
                new SimulationGenomeStructureStepResult(
                    LivingPopulation:
                        state.Population.Count,
                    GenomesWithStructuralLoci:
                        0,
                    TotalStructuralLoci:
                        0,
                    ActiveStructuralLoci:
                        0,
                    InactiveStructuralLoci:
                        0,
                    UniqueFamilies:
                        0,
                    AverageLociPerGenome:
                        0,
                    MaximumLociInGenome:
                        0,
                    Families:
                        Array.Empty<SimulationGenomeFamilyStepResult>()
                ),
            PhenotypeExpression:
                new SimulationPhenotypeExpressionStepResult(
                    LivingPopulation:
                        state.Population.Count,
                    ExpressedGenomes:
                        0,
                    Channels:
                        Array.Empty<SimulationPhenotypeChannelStepResult>()
                ),
            MetabolicEfficiencyEffect:
                new SimulationMetabolicEfficiencyEffectStepResult(
                    LivingPopulation:
                        state.Population.Count,
                    BeneficialOrganisms:
                        0,
                    DetrimentalOrganisms:
                        0,
                    NeutralOrganisms:
                        state.Population.Count,
                    AverageModifier:
                        0,
                    AverageCostMultiplier:
                        1,
                    MinimumCostMultiplier:
                        1,
                    MaximumCostMultiplier:
                        1,
                    AverageEnergyCostDeltaFraction:
                        0
                ),
            MetabolicSelectionDiagnostics:
                new SimulationMetabolicSelectionDiagnosticsStepResult(
                    Population:
                        state.Population.Count,
                    EffectiveParents:
                        0,
                    Overall:
                        emptyCohort,
                    Parents:
                        emptyCohort,
                    Regions:
                        Array.Empty<SimulationMetabolicRegionStepResult>()
                ),
            IsExtinct:
                state.Population.Count
                ==
                0
        );
    }


    private static SimulationMorphologyChannelStepResult
        MapMorphologyChannel(
            MorphologyChannelStatistics channel)
    {
        return new SimulationMorphologyChannelStepResult(
            Channel:
                channel.Channel.ToString(),

            ExpressingOrganisms:
                channel.ExpressingOrganisms,

            AverageModifier:
                channel.AverageModifier,

            StandardDeviation:
                channel.StandardDeviation,

            MinimumModifier:
                channel.MinimumModifier,

            MaximumModifier:
                channel.MaximumModifier
        );
    }


    private static SimulationFeedingSpecializationCohortStepResult
        MapFeedingSpecializationCohort(
            FeedingSpecializationCohortDiagnostics cohort)
    {
        return new SimulationFeedingSpecializationCohortStepResult(
            Name:
                cohort.Name,

            Count:
                cohort.Count,

            Beneficial:
                cohort.Beneficial,

            Detrimental:
                cohort.Detrimental,

            Neutral:
                cohort.Neutral,

            AverageModifier:
                cohort.AverageModifier,

            AveragePlantEnergyMultiplier:
                cohort.AveragePlantEnergyMultiplier,

            AveragePlantDigestionEfficiency:
                cohort.AveragePlantDigestionEfficiency,

            AverageEffectivePlantEnergyAssimilation:
                cohort.AverageEffectivePlantEnergyAssimilation,

            AverageFeedingCapacity:
                cohort.AverageFeedingCapacity,

            AverageCurrentEnergy:
                cohort.AverageCurrentEnergy,

            AverageEnergyFillFraction:
                cohort.AverageEnergyFillFraction,

            P10Modifier:
                cohort.P10Modifier,

            P50Modifier:
                cohort.P50Modifier,

            P90Modifier:
                cohort.P90Modifier
        );
    }


    private static SimulationLocomotionCohortStepResult
        MapLocomotionCohort(
            LocomotionCohortDiagnostics cohort)
    {
        return new SimulationLocomotionCohortStepResult(
            Name:
                cohort.Name,

            Count:
                cohort.Count,

            Beneficial:
                cohort.Beneficial,

            Detrimental:
                cohort.Detrimental,

            Neutral:
                cohort.Neutral,

            AverageModifier:
                cohort.AverageModifier,

            AverageActivityCostMultiplier:
                cohort.AverageActivityCostMultiplier,

            AverageBaseActivityCost:
                cohort.AverageBaseActivityCost,

            AverageActivityCost:
                cohort.AverageActivityCost,

            AverageBasalMetabolicCost:
                cohort.AverageBasalMetabolicCost,

            AverageEnergyCost:
                cohort.AverageEnergyCost,

            P10Modifier:
                cohort.P10Modifier,

            P50Modifier:
                cohort.P50Modifier,

            P90Modifier:
                cohort.P90Modifier
        );
    }


    private static SimulationBodySizeCohortStepResult
        MapBodySizeCohort(
            BodySizeCohortDiagnostics cohort)
    {
        return new SimulationBodySizeCohortStepResult(
            Name:
                cohort.Name,

            Count:
                cohort.Count,

            Beneficial:
                cohort.Beneficial,

            Detrimental:
                cohort.Detrimental,

            Neutral:
                cohort.Neutral,

            AverageModifier:
                cohort.AverageModifier,

            AverageEnergyCapacityMultiplier:
                cohort.AverageEnergyCapacityMultiplier,

            AverageGeneticSize:
                cohort.AverageGeneticSize,

            AverageBaseMaxEnergy:
                cohort.AverageBaseMaxEnergy,

            AverageMaxEnergy:
                cohort.AverageMaxEnergy,

            AverageCurrentEnergy:
                cohort.AverageCurrentEnergy,

            AverageEnergyFillFraction:
                cohort.AverageEnergyFillFraction,

            P10Modifier:
                cohort.P10Modifier,

            P50Modifier:
                cohort.P50Modifier,

            P90Modifier:
                cohort.P90Modifier
        );
    }


    private static SimulationThermalRegulationCohortStepResult
        MapThermalRegulationCohort(
            ThermalRegulationCohortDiagnostics cohort)
    {
        return new SimulationThermalRegulationCohortStepResult(
            Name:
                cohort.Name,

            Count:
                cohort.Count,

            Beneficial:
                cohort.Beneficial,

            Detrimental:
                cohort.Detrimental,

            Neutral:
                cohort.Neutral,

            AverageModifier:
                cohort.AverageModifier,

            AverageStressMultiplier:
                cohort.AverageStressMultiplier,

            AverageThermalFitness:
                cohort.AverageThermalFitness,

            AverageThermalStress:
                cohort.AverageThermalStress,

            P10Modifier:
                cohort.P10Modifier,

            P50Modifier:
                cohort.P50Modifier,

            P90Modifier:
                cohort.P90Modifier
        );
    }


    private static SimulationBoomBustDiagnosticsStepResult
        MapBoomBustDiagnostics(
            PopulationBoomBustDiagnostics diagnostics)
    {
        return new SimulationBoomBustDiagnosticsStepResult(
            Cycle:
                diagnostics.Cycle,

            Population:
                diagnostics.Population,

            Births:
                diagnostics.Births,

            Deaths:
                diagnostics.Deaths,

            NoBirthStreak:
                diagnostics.NoBirthStreak,

            TotalPlantBiomass:
                diagnostics.TotalPlantBiomass,

            TotalPlantCapacity:
                diagnostics.TotalPlantCapacity,

            TotalPlantCapacityFraction:
                diagnostics.TotalPlantCapacityFraction,

            TotalPlantBiomassPerCapita:
                diagnostics.TotalPlantBiomassPerCapita,

            ConsumptionToRegrowthRatio:
                diagnostics.ConsumptionToRegrowthRatio,

            RegionsWithSevereScarcity:
                diagnostics.RegionsWithSevereScarcity,

            RegionsWithOvershootSignal:
                diagnostics.RegionsWithOvershootSignal,

            Regions:
                diagnostics.Regions
                    .Select(
                        region =>
                            new SimulationBoomBustRegionStepResult(
                                RegionId:
                                    region.RegionId,

                                RegionName:
                                    region.RegionName,

                                Population:
                                    region.Population,

                                Births:
                                    region.Births,

                                Deaths:
                                    region.Deaths,

                                PlantCapacityFraction:
                                    region.PlantCapacityFraction,

                                PlantBiomassPerCapita:
                                    region.PlantBiomassPerCapita,

                                ConsumptionToRegrowthRatio:
                                    region.ConsumptionToRegrowthRatio,

                                NoBirthStreak:
                                    region.NoBirthStreak,

                                IsSeverelyResourceScarce:
                                    region.IsSeverelyResourceScarce,

                                HasOvershootSignal:
                                    region.HasOvershootSignal,

                                ScarcityEpisodeCount:
                                    region.ScarcityEpisodeCount,

                                IsScarcityRecoveryActive:
                                    region.IsScarcityRecoveryActive,

                                CurrentScarcityRecoveryDuration:
                                    region.CurrentScarcityRecoveryDuration,

                                LastScarcityRecoveryDuration:
                                    region.LastScarcityRecoveryDuration,

                                LongestScarcityRecoveryDuration:
                                    region.LongestScarcityRecoveryDuration
                            )
                    )
                    .ToList()
        );
    }


    private static SimulationMetabolicCohortStepResult MapCohort(
        MetabolicCohortDiagnostics cohort)
    {
        return new SimulationMetabolicCohortStepResult(
            Name:
                cohort.Name,
            Count:
                cohort.Count,
            Beneficial:
                cohort.Beneficial,
            Detrimental:
                cohort.Detrimental,
            Neutral:
                cohort.Neutral,
            AverageModifier:
                cohort.AverageModifier,
            AverageCostMultiplier:
                cohort.AverageCostMultiplier,
            P10Modifier:
                cohort.P10Modifier,
            P50Modifier:
                cohort.P50Modifier,
            P90Modifier:
                cohort.P90Modifier
        );
    }


}
