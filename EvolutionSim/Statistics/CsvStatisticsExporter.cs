using System.Globalization;
using System.Text;

namespace EvolutionSim.Statistics;

public static class CsvStatisticsExporter
{
    public static void Export(
        IEnumerable<SimulationSnapshot> snapshots,
        string path)
    {
        using StreamWriter writer =
            new(
                path,
                false,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: true
                )
            );

        writer.WriteLine(
            "Cycle;" +
            "Population;" +
            "Births;" +
            "Deaths;" +

            "PlantBiomass;" +
            "SeedBank;" +
            "RootBiomass;" +
            "PlantsConsumed;" +

            "AverageSpeed;" +
            "AverageSize;" +
            "AverageMetabolism;" +

            "SpeedStdDev;" +
            "SizeStdDev;" +
            "MetabolismStdDev;" +

            "MinSpeed;" +
            "MaxSpeed;" +

            "MinSize;" +
            "MaxSize;" +

            "MinMetabolism;" +
            "MaxMetabolism;" +

            "AverageEnergy;" +
            "AverageMaxEnergy;" +

            "AverageFoodEfficiency;" +
            "AveragePhysicalPerformance;" +
            "AverageAbsorptionCapacity;" +

            "AverageBasalMetabolicCost;" +
            "AverageActivityCost;" +
            "AverageEnergyCost;" +

            "AverageAge;" +

            "AverageGeneration;" +
            "MaxGeneration;" +

            "FounderLineages;" +
            "DominantFounderShare"
        );

        foreach (
            SimulationSnapshot snapshot
            in snapshots
        )
        {
            writer.WriteLine(
                $"{snapshot.Cycle};" +
                $"{snapshot.Population};" +
                $"{snapshot.Births};" +
                $"{snapshot.Deaths};" +

                $"{Format(snapshot.PlantBiomass)};" +
                $"{Format(snapshot.SeedBank)};" +
                $"{Format(snapshot.RootBiomass)};" +
                $"{Format(snapshot.PlantsConsumed)};" +

                $"{Format(snapshot.AverageSpeed)};" +
                $"{Format(snapshot.AverageSize)};" +
                $"{Format(snapshot.AverageMetabolism)};" +

                $"{Format(snapshot.SpeedStdDev)};" +
                $"{Format(snapshot.SizeStdDev)};" +
                $"{Format(snapshot.MetabolismStdDev)};" +

                $"{Format(snapshot.MinSpeed)};" +
                $"{Format(snapshot.MaxSpeed)};" +

                $"{Format(snapshot.MinSize)};" +
                $"{Format(snapshot.MaxSize)};" +

                $"{Format(snapshot.MinMetabolism)};" +
                $"{Format(snapshot.MaxMetabolism)};" +

                $"{Format(snapshot.AverageEnergy)};" +
                $"{Format(snapshot.AverageMaxEnergy)};" +

                $"{Format(snapshot.AverageFoodEfficiency)};" +
                $"{Format(snapshot.AveragePhysicalPerformance)};" +
                $"{Format(snapshot.AverageAbsorptionCapacity)};" +

                $"{Format(snapshot.AverageBasalMetabolicCost)};" +
                $"{Format(snapshot.AverageActivityCost)};" +
                $"{Format(snapshot.AverageEnergyCost)};" +

                $"{Format(snapshot.AverageAge)};" +

                $"{Format(snapshot.AverageGeneration)};" +
                $"{snapshot.MaxGeneration};" 

            );
        }
    }

    private static string Format(
        double value)
    {
        return value.ToString(
            "F6",
            CultureInfo.InvariantCulture
        );
    }
}