using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class GenomeStructuralStatisticsCalculator
{
    public GenomeStructuralStatistics Calculate(
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


        List<GenomeLocus>
            allLoci =
                living
                    .SelectMany(
                        organism =>
                            organism.Genome.StructuralLoci
                    )
                    .ToList();


        int genomesWithStructuralLoci =
            living.Count(
                organism =>
                    organism.Genome
                        .StructuralLocusCount
                    >
                    0
            );


        List<GenomeStructuralFamilyStatistics>
            families =
                allLoci
                    .GroupBy(
                        locus =>
                            locus.FamilyId
                    )
                    .Select(
                        family =>
                        {
                            long familyId =
                                family.Key;


                            HashSet<Guid> carriers =
                                living
                                    .Where(
                                        organism =>
                                            organism.Genome
                                                .StructuralLoci
                                                .Any(
                                                    locus =>
                                                        locus.FamilyId
                                                        ==
                                                        familyId
                                                )
                                    )
                                    .Select(
                                        organism =>
                                            organism.Id
                                    )
                                    .ToHashSet();


                            return new GenomeStructuralFamilyStatistics(
                                FamilyId:
                                    familyId,
                                CarrierCount:
                                    carriers.Count,
                                CopyCount:
                                    family.Count(),
                                ActiveCopyCount:
                                    family.Count(
                                        locus =>
                                            locus.IsActive
                                    ),
                                AverageValue:
                                    family.Average(
                                        locus =>
                                            locus.Value
                                    )
                            );
                        }
                    )
                    .OrderByDescending(
                        family =>
                            family.CarrierCount
                    )
                    .ThenBy(
                        family =>
                            family.FamilyId
                    )
                    .ToList();


        return new GenomeStructuralStatistics(
            LivingPopulation:
                living.Count,
            GenomesWithStructuralLoci:
                genomesWithStructuralLoci,
            TotalStructuralLoci:
                allLoci.Count,
            ActiveStructuralLoci:
                allLoci.Count(
                    locus =>
                        locus.IsActive
                ),
            InactiveStructuralLoci:
                allLoci.Count(
                    locus =>
                        !locus.IsActive
                ),
            UniqueFamilies:
                families.Count,
            AverageLociPerGenome:
                living.Count == 0
                    ? 0
                    : allLoci.Count
                    /
                    (double)living.Count,
            MaximumLociInGenome:
                living.Count == 0
                    ? 0
                    : living.Max(
                        organism =>
                            organism.Genome
                                .StructuralLocusCount
                    ),
            Families:
                families
        );
    }
}


public sealed record GenomeStructuralStatistics(
    int LivingPopulation,
    int GenomesWithStructuralLoci,
    int TotalStructuralLoci,
    int ActiveStructuralLoci,
    int InactiveStructuralLoci,
    int UniqueFamilies,
    double AverageLociPerGenome,
    int MaximumLociInGenome,
    IReadOnlyList<GenomeStructuralFamilyStatistics> Families
);


public sealed record GenomeStructuralFamilyStatistics(
    long FamilyId,
    int CarrierCount,
    int CopyCount,
    int ActiveCopyCount,
    double AverageValue
);
