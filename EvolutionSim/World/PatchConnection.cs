namespace EvolutionSim.World;

public sealed class PatchConnection
{
    public int PatchAId { get; }

    public int PatchBId { get; }

    public double MovementDifficulty { get; }

    public double EnergyCost { get; }


    public PatchConnection(
        int patchAId,
        int patchBId,
        double movementDifficulty = 1,
        double energyCost = 1)
    {
        if (patchAId == patchBId)
        {
            throw new ArgumentException(
                "A patch cannot connect to itself."
            );
        }


        PatchAId =
            patchAId;

        PatchBId =
            patchBId;

        MovementDifficulty =
            Math.Max(
                0.1,
                movementDifficulty
            );

        EnergyCost =
            Math.Max(
                0,
                energyCost
            );
    }


    public bool ContainsPatch(
        int patchId)
    {
        return
            PatchAId == patchId
            ||
            PatchBId == patchId;
    }


    public int GetOtherPatchId(
        int patchId)
    {
        if (PatchAId == patchId)
        {
            return PatchBId;
        }


        if (PatchBId == patchId)
        {
            return PatchAId;
        }


        throw new InvalidOperationException(
            $"Patch {patchId} is not part of this connection."
        );
    }
}