using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

/// <summary>
/// Diagnóstico del primer efecto fenotípico activo.
/// No usa Random y no modifica organismos.
/// </summary>
public sealed class MetabolicEfficiencyEffectStatisticsCalculator
{
    private const double NeutralTolerance =
        1e-12;


    public MetabolicEfficiencyEffectStatistics Calculate(
        IEnumerable<Organism> population)
    {
        ArgumentNullException.ThrowIfNull(
            population
        );


        List<Organism>
            living =
                population
                    .Where(
                        organism =>
                            organism.IsAlive
                    )
                    .ToList();


        if (
            living.Count
            ==
            0
        )
        {
            return new MetabolicEfficiencyEffectStatistics(
                LivingPopulation:
                    0,
                BeneficialOrganisms:
                    0,
                DetrimentalOrganisms:
                    0,
                NeutralOrganisms:
                    0,
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
            );
        }


        int beneficial =
            living.Count(
                organism =>
                    organism.MetabolicEfficiencyModifier
                    >
                    NeutralTolerance
            );


        int detrimental =
            living.Count(
                organism =>
                    organism.MetabolicEfficiencyModifier
                    <
                    -NeutralTolerance
            );


        int neutral =
            living.Count
            -
            beneficial
            -
            detrimental;


        double averageModifier =
            living.Average(
                organism =>
                    organism.MetabolicEfficiencyModifier
            );


        double averageMultiplier =
            living.Average(
                organism =>
                    organism.MetabolicEfficiencyCostMultiplier
            );


        double minimumMultiplier =
            living.Min(
                organism =>
                    organism.MetabolicEfficiencyCostMultiplier
            );


        double maximumMultiplier =
            living.Max(
                organism =>
                    organism.MetabolicEfficiencyCostMultiplier
            );


        return new MetabolicEfficiencyEffectStatistics(
            LivingPopulation:
                living.Count,
            BeneficialOrganisms:
                beneficial,
            DetrimentalOrganisms:
                detrimental,
            NeutralOrganisms:
                neutral,
            AverageModifier:
                averageModifier,
            AverageCostMultiplier:
                averageMultiplier,
            MinimumCostMultiplier:
                minimumMultiplier,
            MaximumCostMultiplier:
                maximumMultiplier,
            AverageEnergyCostDeltaFraction:
                averageMultiplier
                -
                1
        );
    }
}


public sealed record MetabolicEfficiencyEffectStatistics(
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
