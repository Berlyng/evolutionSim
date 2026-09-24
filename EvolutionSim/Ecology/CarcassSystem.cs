using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Ecology;

public sealed class CarcassSystem
{
    public const double CarcassBiomassPerSizeUnit =
        2.5;


    public const double DecayRate =
        0.20;


    public const double EnergyPerBiomassUnit =
        22.0;


    //
    // Ahora la biomasa de cadáveres pertenece al patch,
    // no a toda la región.
    //

    private readonly Dictionary<int, double>
        _biomassByPatch =
            new();


    //
    // Evita registrar dos veces la misma muerte
    // dentro del mismo ciclo.
    //

    private readonly HashSet<Guid>
        _registeredDeathsThisCycle =
            new();


    // =========================================================
    // AVANZAR CICLO
    // =========================================================

    public void AdvanceCycle(
        Worldd world)
    {
        //
        // Al comenzar un nuevo ciclo permitimos registrar
        // nuevas muertes.
        //

        _registeredDeathsThisCycle.Clear();


        foreach (
            Patch patch
            in world.Patches
        )
        {
            double currentBiomass =
                GetPatchBiomass(
                    patch.Id
                );


            if (
                currentBiomass <= 0
            )
            {
                continue;
            }


            double remainingBiomass =
                currentBiomass
                *
                (
                    1.0
                    -
                    DecayRate
                );


            if (
                remainingBiomass
                <=
                0.0001
            )
            {
                _biomassByPatch.Remove(
                    patch.Id
                );


                continue;
            }


            _biomassByPatch[
                patch.Id
            ]
            =
            remainingBiomass;
        }
    }


    // =========================================================
    // REGISTRAR MUERTE
    // =========================================================

    public void RegisterDeath(
        Organism organism)
    {
        //
        // La misma muerte puede ser observada por varios
        // sistemas durante un ciclo.
        //
        // Solo la convertimos en cadáver una vez.
        //

        if (
            !_registeredDeathsThisCycle.Add(
                organism.Id
            )
        )
        {
            return;
        }


        double carcassBiomass =
            organism.Size
            *
            CarcassBiomassPerSizeUnit;


        if (
            carcassBiomass <= 0
        )
        {
            return;
        }


        int patchId =
            organism.PatchId;


        if (
            !_biomassByPatch.TryGetValue(
                patchId,
                out double existingBiomass
            )
        )
        {
            existingBiomass =
                0;
        }


        _biomassByPatch[
            patchId
        ]
        =
        existingBiomass
        +
        carcassBiomass;
    }


    // =========================================================
    // CONSUMIR CARROÑA DE UN PATCH
    // =========================================================

    public double ConsumeFromPatch(
        int patchId,
        double requestedAmount)
    {
        if (
            requestedAmount <= 0
        )
        {
            return 0;
        }


        double availableBiomass =
            GetPatchBiomass(
                patchId
            );


        if (
            availableBiomass <= 0
        )
        {
            return 0;
        }


        double consumed =
            Math.Min(
                requestedAmount,
                availableBiomass
            );


        double remaining =
            availableBiomass
            -
            consumed;


        if (
            remaining <= 0.0001
        )
        {
            _biomassByPatch.Remove(
                patchId
            );
        }
        else
        {
            _biomassByPatch[
                patchId
            ]
            =
            remaining;
        }


        return consumed;
    }


    // =========================================================
    // BIOMASA DE UN PATCH
    // =========================================================

    public double GetPatchBiomass(
        int patchId)
    {
        return
            _biomassByPatch.TryGetValue(
                patchId,
                out double biomass
            )

                ?

                biomass

                :

                0;
    }


    // =========================================================
    // BIOMASA TOTAL DE UNA REGIÓN
    // =========================================================

    public double GetRegionalBiomass(
        Worldd world,
        int regionId)
    {
        Region region =
            world.GetRegion(
                regionId
            );


        return
            region.Patches.Sum(
                patch =>
                    GetPatchBiomass(
                        patch.Id
                    )
            );
    }


    // =========================================================
    // BIOMASA TOTAL DEL MUNDO
    // =========================================================

    public double GetTotalBiomass()
    {
        return
            _biomassByPatch.Values.Sum();
    }
}