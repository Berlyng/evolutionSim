namespace EvolutionSim.Simulation;

public sealed record SimulationStepResult(
    int Cycle,
    int Population,
    int Births,
    int Deaths,
    int HuntingKills,
    int HuntingAttempts,
    int ScavengingActions,
    int GrazingActions,
    double CarcassBiomass,
    double HuntingEnergySpent,
    int Migrations,
    int LocalMovements,
    double AverageGeneration,
    bool SpeciesDetectionRan,
    int PendingSpeciesCandidates,
    IReadOnlyList<SimulationRegionStepResult> Regions,
    IReadOnlyList<SimulationSpeciesStepResult> Species,
    IReadOnlyList<SimulationMigrationRouteStepResult> MigrationRoutes,
    IReadOnlyList<SimulationPatchMovementRouteStepResult> LocalMovementRoutes,
    IReadOnlyList<SimulationRegionConnectionStepResult> RegionConnections,
    IReadOnlyList<SimulationPatchConnectionStepResult> PatchConnections,
    IReadOnlyList<SimulationSpeciesHistoryStepResult> SpeciesHistory,
    IReadOnlyList<SimulationSpeciesHistoryEventStepResult> SpeciesHistoryEvents,
    SimulationPhylogenyStepResult Phylogeny,
    SimulationGenomeStructureStepResult GenomeStructure,
    SimulationPhenotypeExpressionStepResult PhenotypeExpression,
    SimulationMetabolicEfficiencyEffectStepResult MetabolicEfficiencyEffect,
    SimulationMetabolicSelectionDiagnosticsStepResult MetabolicSelectionDiagnostics,
    bool IsExtinct,
    SimulationBoomBustDiagnosticsStepResult? BoomBustDiagnostics = null,
    SimulationThermalRegulationDiagnosticsStepResult? ThermalRegulationDiagnostics = null,
    SimulationBodySizeDiagnosticsStepResult? BodySizeDiagnostics = null,
    SimulationLocomotionDiagnosticsStepResult? LocomotionDiagnostics = null,
    SimulationFeedingSpecializationDiagnosticsStepResult? FeedingSpecializationDiagnostics = null
);

public sealed record SimulationRegionStepResult(
    int RegionId,
    string Name,
    double Temperature,
    int Population,
    int Births,
    int Deaths,
    double PlantBiomass,
    double CarcassBiomass,
    double AverageOptimalTemperature,
    double AverageThermalTolerance,
    double AverageSpeed,
    double AverageSize,
    double AverageMetabolism,
    double AverageScavengingDrive,
    double AverageExplorationDrive,
    double AverageMateSelectivity,
    IReadOnlyList<SimulationPatchStepResult> Patches
);

public sealed record SimulationPatchStepResult(
    int PatchId,
    int RegionId,
    string Name,
    double LocalTemperature,
    int Population,
    double PlantBiomass,
    double PlantEnergyDensity,
    double PlantToughness,
    double CarcassBiomass
);

public sealed record SimulationSpeciesStepResult(
    int SpeciesId,
    int? ParentSpeciesId,
    int FirstDetectedCycle,
    int Population,
    double AverageDistanceToCentroid,
    double Speed,
    double Size,
    double Metabolism,
    double OptimalTemperature,
    double ThermalTolerance,
    double MeatAdaptation,
    double PredatoryDrive,
    double ExplorationDrive,
    double RiskTolerance,
    double ScavengingDrive,
    double MateSelectivity
);


public sealed record SimulationMigrationRouteStepResult(
    int OriginRegionId,
    string OriginRegionName,
    int DestinationRegionId,
    string DestinationRegionName,
    int Count,
    double AverageThermalFitness
);

public sealed record SimulationPatchMovementRouteStepResult(
    int OriginPatchId,
    string OriginPatchName,
    int DestinationPatchId,
    string DestinationPatchName,
    int Count
);

public sealed record SimulationRegionConnectionStepResult(
    int RegionAId,
    string RegionAName,
    int RegionBId,
    string RegionBName,
    double MigrationDifficulty,
    double EnergyCost
);

public sealed record SimulationPatchConnectionStepResult(
    int PatchAId,
    string PatchAName,
    int PatchBId,
    string PatchBName,
    double MovementDifficulty,
    double EnergyCost
);


public sealed record SimulationSpeciesHistoryStepResult(
    int SpeciesId,
    int? ParentSpeciesId,
    int FirstObservedCycle,
    int ConfirmedCycle,
    int LastConfirmedCycle,
    int? EndCycle,
    bool IsActive,
    int DetectionCount,
    int LatestPopulation,
    int PeakPopulation,
    int PeakPopulationCycle,
    double LatestAverageDistanceToCentroid,
    double LatestSpeed,
    double LatestSize,
    double LatestMetabolism,
    double LatestOptimalTemperature,
    double LatestThermalTolerance,
    double LatestMeatAdaptation,
    double LatestPredatoryDrive,
    double LatestExplorationDrive,
    double LatestRiskTolerance,
    double LatestScavengingDrive,
    double LatestMateSelectivity,
    IReadOnlyList<SimulationSpeciesHistoryObservationStepResult> Observations
);

public sealed record SimulationSpeciesHistoryObservationStepResult(
    int Cycle,
    int Population,
    double AverageDistanceToCentroid,
    double Speed,
    double Size,
    double Metabolism,
    double OptimalTemperature,
    double ThermalTolerance,
    double MeatAdaptation,
    double PredatoryDrive,
    double ExplorationDrive,
    double RiskTolerance,
    double ScavengingDrive,
    double MateSelectivity,
    IReadOnlyDictionary<int, int> RegionPopulations
);

public sealed record SimulationSpeciesHistoryEventStepResult(
    int Cycle,
    string Type,
    int SpeciesId,
    int? RelatedSpeciesId,
    string Description
);


public sealed record SimulationPhylogenyStepResult(
    int CurrentCycle,
    int MaximumDepth,
    IReadOnlyList<int> RootSpeciesIds,
    IReadOnlyList<SimulationPhylogenyNodeStepResult> Nodes,
    IReadOnlyList<SimulationPhylogenyEdgeStepResult> Edges
);

public sealed record SimulationPhylogenyNodeStepResult(
    int SpeciesId,
    int? ParentSpeciesId,
    int Depth,
    int FirstObservedCycle,
    int ConfirmedCycle,
    int LastConfirmedCycle,
    int? EndCycle,
    bool IsActive,
    int LatestPopulation,
    int PeakPopulation,
    int DirectDescendantCount
);

public sealed record SimulationPhylogenyEdgeStepResult(
    int ParentSpeciesId,
    int ChildSpeciesId,
    int BranchCycle
);


public sealed record SimulationGenomeStructureStepResult(
    int LivingPopulation,
    int GenomesWithStructuralLoci,
    int TotalStructuralLoci,
    int ActiveStructuralLoci,
    int InactiveStructuralLoci,
    int UniqueFamilies,
    double AverageLociPerGenome,
    int MaximumLociInGenome,
    IReadOnlyList<SimulationGenomeFamilyStepResult> Families
);

public sealed record SimulationGenomeFamilyStepResult(
    long FamilyId,
    int CarrierCount,
    int CopyCount,
    int ActiveCopyCount,
    double AverageValue
);


public sealed record SimulationPhenotypeExpressionStepResult(
    int LivingPopulation,
    int ExpressedGenomes,
    IReadOnlyList<SimulationPhenotypeChannelStepResult> Channels
);

public sealed record SimulationPhenotypeChannelStepResult(
    string Channel,
    int ExpressingOrganisms,
    double AverageModifier,
    double StandardDeviation,
    double MinimumModifier,
    double MaximumModifier
);


public sealed record SimulationMetabolicEfficiencyEffectStepResult(
    int LivingPopulation,
    int BeneficialOrganisms,
    int DetrimentalOrganisms,
    int NeutralOrganisms,
    double AverageModifier,
    double AverageCostMultiplier,
    double MinimumCostMultiplier,
    double MaximumCostMultiplier,
    double AverageEnergyCostDeltaFraction
);


public sealed record SimulationMetabolicSelectionDiagnosticsStepResult(
    int Population,
    int EffectiveParents,
    SimulationMetabolicCohortStepResult Overall,
    SimulationMetabolicCohortStepResult Parents,
    IReadOnlyList<SimulationMetabolicRegionStepResult> Regions
);

public sealed record SimulationMetabolicRegionStepResult(
    int RegionId,
    string RegionName,
    SimulationMetabolicCohortStepResult Living,
    SimulationMetabolicCohortStepResult Parents
);

public sealed record SimulationMetabolicCohortStepResult(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageCostMultiplier,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier
);


public sealed record SimulationBoomBustDiagnosticsStepResult(
    int Cycle,
    int Population,
    int Births,
    int Deaths,
    int NoBirthStreak,
    double TotalPlantBiomass,
    double TotalPlantCapacity,
    double TotalPlantCapacityFraction,
    double TotalPlantBiomassPerCapita,
    double ConsumptionToRegrowthRatio,
    int RegionsWithSevereScarcity,
    int RegionsWithOvershootSignal,
    IReadOnlyList<SimulationBoomBustRegionStepResult> Regions
);


public sealed record SimulationBoomBustRegionStepResult(
    int RegionId,
    string RegionName,
    int Population,
    int Births,
    int Deaths,
    double PlantCapacityFraction,
    double PlantBiomassPerCapita,
    double ConsumptionToRegrowthRatio,
    int NoBirthStreak,
    bool IsSeverelyResourceScarce,
    bool HasOvershootSignal,
    int ScarcityEpisodeCount,
    bool IsScarcityRecoveryActive,
    int CurrentScarcityRecoveryDuration,
    int LastScarcityRecoveryDuration,
    int LongestScarcityRecoveryDuration
);



public sealed record SimulationThermalRegulationDiagnosticsStepResult(
    int Population,
    int EffectiveParents,
    SimulationThermalRegulationCohortStepResult Overall,
    SimulationThermalRegulationCohortStepResult Parents,
    IReadOnlyList<SimulationThermalRegulationRegionStepResult> Regions
);


public sealed record SimulationThermalRegulationRegionStepResult(
    int RegionId,
    string RegionName,
    SimulationThermalRegulationCohortStepResult Living,
    SimulationThermalRegulationCohortStepResult Parents
);


public sealed record SimulationThermalRegulationCohortStepResult(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageStressMultiplier,
    double AverageThermalFitness,
    double AverageThermalStress,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier
);



public sealed record SimulationBodySizeDiagnosticsStepResult(
    int Population,
    int EffectiveParents,
    SimulationBodySizeCohortStepResult Overall,
    SimulationBodySizeCohortStepResult Parents,
    IReadOnlyList<SimulationBodySizeRegionStepResult> Regions
);


public sealed record SimulationBodySizeRegionStepResult(
    int RegionId,
    string RegionName,
    SimulationBodySizeCohortStepResult Living,
    SimulationBodySizeCohortStepResult Parents
);


public sealed record SimulationBodySizeCohortStepResult(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageEnergyCapacityMultiplier,
    double AverageGeneticSize,
    double AverageBaseMaxEnergy,
    double AverageMaxEnergy,
    double AverageCurrentEnergy,
    double AverageEnergyFillFraction,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier
);



public sealed record SimulationLocomotionDiagnosticsStepResult(
    int Population,
    int EffectiveParents,
    SimulationLocomotionCohortStepResult Overall,
    SimulationLocomotionCohortStepResult Parents,
    IReadOnlyList<SimulationLocomotionRegionStepResult> Regions
);


public sealed record SimulationLocomotionRegionStepResult(
    int RegionId,
    string RegionName,
    SimulationLocomotionCohortStepResult Living,
    SimulationLocomotionCohortStepResult Parents
);


public sealed record SimulationLocomotionCohortStepResult(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AverageActivityCostMultiplier,
    double AverageBaseActivityCost,
    double AverageActivityCost,
    double AverageBasalMetabolicCost,
    double AverageEnergyCost,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier
);



public sealed record SimulationFeedingSpecializationDiagnosticsStepResult(
    int Population,
    int EffectiveParents,
    SimulationFeedingSpecializationCohortStepResult Overall,
    SimulationFeedingSpecializationCohortStepResult Parents,
    IReadOnlyList<SimulationFeedingSpecializationRegionStepResult> Regions
);


public sealed record SimulationFeedingSpecializationRegionStepResult(
    int RegionId,
    string RegionName,
    SimulationFeedingSpecializationCohortStepResult Living,
    SimulationFeedingSpecializationCohortStepResult Parents
);


public sealed record SimulationFeedingSpecializationCohortStepResult(
    string Name,
    int Count,
    int Beneficial,
    int Detrimental,
    int Neutral,
    double AverageModifier,
    double AveragePlantEnergyMultiplier,
    double AveragePlantDigestionEfficiency,
    double AverageEffectivePlantEnergyAssimilation,
    double AverageFeedingCapacity,
    double AverageCurrentEnergy,
    double AverageEnergyFillFraction,
    double P10Modifier,
    double P50Modifier,
    double P90Modifier
);
