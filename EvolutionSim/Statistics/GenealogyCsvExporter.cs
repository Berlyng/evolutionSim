using System.Globalization;
using System.Text;

namespace EvolutionSim.Statistics;

public static class GenealogyCsvExporter
{
    public static void Export(
        IEnumerable<GenealogyRecord> records,
        string path)
    {
        using StreamWriter writer =
            new(
                path,
                false,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier:
                        true
                )
            );

        writer.WriteLine(
            "Id;" +
            "ParentAId;" +
            "ParentBId;" +
            "Generation;" +
            "BirthCycle;" +
            "Speed;" +
            "Size;" +
            "Metabolism;" +
            "OptimalTemperature;" +
            "ThermalTolerance"
        );

        foreach (
            GenealogyRecord record
            in records
        )
        {
            writer.WriteLine(
                $"{record.Id};" +
                $"{record.ParentAId};" +
                $"{record.ParentBId};" +
                $"{record.Generation};" +
                $"{record.BirthCycle};" +
                $"{Format(record.Speed)};" +
                $"{Format(record.Size)};" +
                $"{Format(record.Metabolism)};" +
                $"{Format(record.OptimalTemperature)};" +
                $"{Format(record.ThermalTolerance)}"
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