using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Ecology;

public sealed record PlantResourceRegionStatistics(
    int RegionId,
    string RegionName,
    int LivingPopulation,
    double StartBiomass,
    double Regrowth,
    double PreFeedingBiomass,
    double ConsumedBiomass,
    double EndBiomass,
    double CarryingCapacity,
    double EndCapacityFraction,
    double RootBiomass,
    double RootCapacity,
    double RootCapacityFraction,
    double SeedBank,
    double SeedBankCapacity,
    double SeedBankCapacityFraction,
    double EndBiomassPerCapita,
    double ConsumptionToRegrowthRatio);

public sealed class PlantResourceCycleTracker
{
    private sealed record PatchPlantSnapshot(
        double Biomass,
        double RootBiomass,
        double SeedBank);

    private readonly Dictionary<int, PatchPlantSnapshot>
        _beforeGrowth = new();

    private readonly Dictionary<int, PatchPlantSnapshot>
        _afterGrowth = new();

    public void CaptureBeforeGrowth(
        Worldd world)
    {
        _beforeGrowth.Clear();
        _afterGrowth.Clear();

        foreach (
            Patch patch
            in world.Patches
        )
        {
            _beforeGrowth[patch.Id] =
                CapturePatch(
                    patch
                );
        }
    }

    public void CaptureAfterGrowth(
        Worldd world)
    {
        _afterGrowth.Clear();

        foreach (
            Patch patch
            in world.Patches
        )
        {
            _afterGrowth[patch.Id] =
                CapturePatch(
                    patch
                );
        }
    }

    public List<PlantResourceRegionStatistics>
        CalculateAfterFeeding(
            List<Organism> population,
            Worldd world)
    {
        List<PlantResourceRegionStatistics> results =
            new();

        foreach (
            Region region
            in world.Regions
        )
        {
            double startBiomass = 0;
            double preFeedingBiomass = 0;
            double endBiomass = 0;
            double carryingCapacity = 0;
            double rootBiomass = 0;
            double rootCapacity = 0;
            double seedBank = 0;
            double seedBankCapacity = 0;

            foreach (
                Patch patch
                in region.Patches
            )
            {
                if (
                    !_beforeGrowth.TryGetValue(
                        patch.Id,
                        out PatchPlantSnapshot? before
                    )
                    ||
                    !_afterGrowth.TryGetValue(
                        patch.Id,
                        out PatchPlantSnapshot? after
                    )
                )
                {
                    continue;
                }

                startBiomass +=
                    before.Biomass;

                preFeedingBiomass +=
                    after.Biomass;

                endBiomass +=
                    patch.Plants.Biomass;

                carryingCapacity +=
                    patch.Plants.CarryingCapacity;

                rootBiomass +=
                    patch.Plants.RootBiomass;

                rootCapacity +=
                    patch.Plants.RootCapacity;

                seedBank +=
                    patch.Plants.SeedBank;

                seedBankCapacity +=
                    patch.Plants.SeedBankCapacity;
            }

            double regrowth =
                Math.Max(
                    0,
                    preFeedingBiomass
                    -
                    startBiomass
                );

            double consumedBiomass =
                Math.Max(
                    0,
                    preFeedingBiomass
                    -
                    endBiomass
                );

            int livingPopulation =
                population.Count(
                    organism =>
                        organism.IsAlive
                        &&
                        organism.RegionId
                        ==
                        region.Id
                );

            double endCapacityFraction =
                carryingCapacity > 0
                    ? endBiomass / carryingCapacity
                    : 0;

            double rootCapacityFraction =
                rootCapacity > 0
                    ? rootBiomass / rootCapacity
                    : 0;

            double seedBankCapacityFraction =
                seedBankCapacity > 0
                    ? seedBank / seedBankCapacity
                    : 0;

            double endBiomassPerCapita =
                livingPopulation > 0
                    ? endBiomass / livingPopulation
                    : 0;

            double consumptionToRegrowthRatio =
                regrowth > 0
                    ? consumedBiomass / regrowth
                    : consumedBiomass > 0
                        ? double.PositiveInfinity
                        : 0;

            results.Add(
                new PlantResourceRegionStatistics(
                    RegionId:
                        region.Id,

                    RegionName:
                        region.Name,

                    LivingPopulation:
                        livingPopulation,

                    StartBiomass:
                        startBiomass,

                    Regrowth:
                        regrowth,

                    PreFeedingBiomass:
                        preFeedingBiomass,

                    ConsumedBiomass:
                        consumedBiomass,

                    EndBiomass:
                        endBiomass,

                    CarryingCapacity:
                        carryingCapacity,

                    EndCapacityFraction:
                        endCapacityFraction,

                    RootBiomass:
                        rootBiomass,

                    RootCapacity:
                        rootCapacity,

                    RootCapacityFraction:
                        rootCapacityFraction,

                    SeedBank:
                        seedBank,

                    SeedBankCapacity:
                        seedBankCapacity,

                    SeedBankCapacityFraction:
                        seedBankCapacityFraction,

                    EndBiomassPerCapita:
                        endBiomassPerCapita,

                    ConsumptionToRegrowthRatio:
                        consumptionToRegrowthRatio
                )
            );
        }

        return results;
    }

    private static PatchPlantSnapshot CapturePatch(
        Patch patch)
    {
        return new PatchPlantSnapshot(
            Biomass:
                patch.Plants.Biomass,

            RootBiomass:
                patch.Plants.RootBiomass,

            SeedBank:
                patch.Plants.SeedBank
        );
    }
}
