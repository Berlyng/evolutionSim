namespace EvolutionSim.Models;

/// <summary>
/// Locus estructural neutral.
///
/// En Phase 7.2 estos loci son heredables y pueden aparecer, duplicarse,
/// activarse/desactivarse o perderse, pero todavía NO modifican el fenotipo.
/// </summary>
public sealed record GenomeLocus(
    long LocusId,
    long FamilyId,
    double Value,
    bool IsActive
)
{
    public GenomeLocus WithValue(
        double value)
    {
        return this with
        {
            Value =
                Math.Clamp(
                    value,
                    0,
                    1
                )
        };
    }
}
