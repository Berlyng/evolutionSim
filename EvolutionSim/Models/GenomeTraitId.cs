namespace EvolutionSim.Models;

/// <summary>
/// Identificador estable de un trait genómico.
///
/// Se usa un identificador de texto en lugar de un enum para permitir que,
/// en fases posteriores, el genoma pueda contener traits adicionales sin
/// modificar la clase Genome ni sus constructores.
/// </summary>
public readonly record struct GenomeTraitId(
    string Value)
{
    public override string ToString() =>
        Value;
}
