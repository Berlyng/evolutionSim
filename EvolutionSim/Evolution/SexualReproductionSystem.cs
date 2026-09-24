using EvolutionSim.Models;
using EvolutionSim.World;

namespace EvolutionSim.Evolution;

public sealed class SexualReproductionSystem
{
    private readonly Random _random;

    private readonly GenomeRecombiner _recombiner;

    private readonly GenomeMutator _mutator;

    private readonly MateCompatibilityCalculator
        _compatibilityCalculator;

    private readonly EcologicalMateSimilarityCalculator
        _ecologicalSimilarityCalculator;


    private const int MaxMateSearchAttempts =
        12;


    public SexualReproductionSystem(
        Random random,
        GenomeRecombiner recombiner,
        GenomeMutator mutator,
        MateCompatibilityCalculator compatibilityCalculator,
        EcologicalMateSimilarityCalculator ecologicalSimilarityCalculator)
    {
        _random =
            random;

        _recombiner =
            recombiner;

        _mutator =
            mutator;

        _compatibilityCalculator =
            compatibilityCalculator;

        _ecologicalSimilarityCalculator =
            ecologicalSimilarityCalculator;
    }


    // =========================================================
    // REPRODUCCIÓN
    // =========================================================

    public List<Organism> Reproduce(
        List<Organism> population,
        Worldd world,
        double reproductionThreshold,
        double reproductionCostPerParent,
        double childInitialEnergy)
    {
        List<Organism> newborns =
            new();


        // =====================================================
        // REPRODUCCIÓN LOCAL
        // =====================================================
        //
        // La reproducción ocurre dentro del patch.
        //
        // Esto permite que poblaciones locales acumulen
        // diferencias evolutivas sin que toda una región
        // funcione como una única población panmíctica.
        //

        foreach (
            Region region
            in world.Regions
        )
        {
            foreach (
                Patch patch
                in region.Patches
            )
            {
                // =================================================
                // TEMPERATURA LOCAL DEL PATCH
                // =================================================
                //
                // Antes se utilizaba region.Temperature.
                //
                // Ahora utilizamos el microclima real del patch,
                // permitiendo que la reproducción también esté
                // sometida a selección térmica local.
                //

                double localTemperature =
                    patch.GetLocalTemperature(
                        region.Temperature
                    );


                // =================================================
                // CANDIDATOS DEL PATCH
                // =================================================

                List<Organism> candidates =
                    population
                        .Where(
                            organism =>
                                organism.IsAlive
                                &&
                                organism.RegionId
                                ==
                                region.Id
                                &&
                                organism.PatchId
                                ==
                                patch.Id
                                &&
                                organism.Energy
                                >=
                                reproductionThreshold
                                &&
                                organism.CanReproduce(
                                    localTemperature
                                )
                        )
                        .ToList();


                if (
                    candidates.Count
                    <
                    2
                )
                {
                    continue;
                }


                // =================================================
                // ALEATORIZAR ORDEN
                // =================================================
                //
                // Evitamos favorecer sistemáticamente a los
                // organismos que aparecen primero en la lista.
                //

                Shuffle(
                    candidates
                );


                HashSet<Guid> usedParents =
                    new();


                // =================================================
                // BUSCAR PAREJAS
                // =================================================

                foreach (
                    Organism parentA
                    in candidates
                )
                {
                    if (
                        usedParents.Contains(
                            parentA.Id
                        )
                    )
                    {
                        continue;
                    }


                    // =================================================
                    // REVALIDAR PADRE A
                    // =================================================

                    if (
                        !parentA.IsAlive
                        ||
                        parentA.Energy
                        <
                        reproductionThreshold
                        ||
                        !parentA.CanReproduce(
                            localTemperature
                        )
                    )
                    {
                        continue;
                    }


                    // =================================================
                    // BUSCAR PAREJA COMPATIBLE
                    // =================================================

                    Organism? parentB =
                        FindCompatiblePartner(
                            parentA,
                            candidates,
                            usedParents,
                            localTemperature,
                            reproductionThreshold
                        );


                    if (
                        parentB is null
                    )
                    {
                        continue;
                    }


                    // =================================================
                    // RECOMBINACIÓN GENÉTICA
                    // =================================================

                    Genome recombinedGenome =
                        _recombiner.Combine(
                            parentA.Genome,
                            parentB.Genome
                        );


                    // =================================================
                    // MUTACIÓN
                    // =================================================

                    Genome childGenome =
                        _mutator.Mutate(
                            recombinedGenome
                        );


                    // =================================================
                    // GENERACIÓN DEL HIJO
                    // =================================================

                    int childGeneration =
                        Math.Max(
                            parentA.Generation,
                            parentB.Generation
                        )
                        +
                        1;


                    // =================================================
                    // CREAR DESCENDIENTE
                    // =================================================
                    //
                    // El descendiente nace en el mismo patch
                    // donde se encuentran sus padres.
                    //

                    Organism child =
                        new(
                            genome:
                                childGenome,

                            regionId:
                                parentA.RegionId,

                            patchId:
                                parentA.PatchId,

                            initialEnergy:
                                childInitialEnergy,

                            parentAId:
                                parentA.Id,

                            parentBId:
                                parentB.Id,

                            generation:
                                childGeneration
                        );


                    // =================================================
                    // COSTO REPRODUCTIVO
                    // =================================================

                    parentA.Reproduce(
                        reproductionCostPerParent
                    );


                    parentB.Reproduce(
                        reproductionCostPerParent
                    );


                    // =================================================
                    // MARCAR PADRES UTILIZADOS
                    // =================================================
                    //
                    // Cada organismo solamente puede reproducirse
                    // una vez durante esta pasada reproductiva.
                    //

                    usedParents.Add(
                        parentA.Id
                    );


                    usedParents.Add(
                        parentB.Id
                    );


                    newborns.Add(
                        child
                    );
                }
            }
        }


        return newborns;
    }


    // =========================================================
    // ENCONTRAR PAREJA
    // =========================================================

    private Organism? FindCompatiblePartner(
        Organism parent,
        List<Organism> candidates,
        HashSet<Guid> usedParents,
        double ambientTemperature,
        double reproductionThreshold)
    {
        if (
            candidates.Count
            <
            2
        )
        {
            return null;
        }


        // =====================================================
        // INTENTOS DE BÚSQUEDA
        // =====================================================

        for (
            int attempt = 0;
            attempt < MaxMateSearchAttempts;
            attempt++
        )
        {
            Organism candidate =
                candidates[
                    _random.Next(
                        candidates.Count
                    )
                ];


            // =====================================================
            // NO PUEDE APAREARSE CONSIGO MISMO
            // =====================================================

            if (
                candidate.Id
                ==
                parent.Id
            )
            {
                continue;
            }


            // =====================================================
            // NO REUTILIZAR PADRES
            // =====================================================

            if (
                usedParents.Contains(
                    candidate.Id
                )
            )
            {
                continue;
            }


            // =====================================================
            // DEBE ESTAR VIVO
            // =====================================================

            if (
                !candidate.IsAlive
            )
            {
                continue;
            }


            // =====================================================
            // ENERGÍA SUFICIENTE
            // =====================================================

            if (
                candidate.Energy
                <
                reproductionThreshold
            )
            {
                continue;
            }


            // =====================================================
            // CAPACIDAD REPRODUCTIVA
            // =====================================================

            if (
                !candidate.CanReproduce(
                    ambientTemperature
                )
            )
            {
                continue;
            }


            // =====================================================
            // COMPATIBILIDAD GENÉTICA
            // =====================================================
            //
            // Representa cuánto se han separado genéticamente
            // ambos organismos.
            //
            // Sigue utilizando MateCompatibilityCalculator.
            //
            // NO estamos modificando aquí el aislamiento genético.
            //

            double compatibility =
                _compatibilityCalculator
                    .CalculateCompatibility(
                        parent.Genome,
                        candidate.Genome
                    );


            // =====================================================
            // SIMILITUD ECOLÓGICA
            // =====================================================
            //
            // Utiliza:
            //
            // - temperatura óptima
            // - tolerancia térmica
            // - adaptación a carne
            // - conducta depredadora
            // - carroñeo
            //
            // La fórmula concreta pertenece a
            // EcologicalMateSimilarityCalculator.
            //

            double ecologicalSimilarity =
                _ecologicalSimilarityCalculator
                    .Calculate(
                        parent.Genome,
                        candidate.Genome
                    );


            ecologicalSimilarity =
                Math.Clamp(
                    ecologicalSimilarity,
                    0,
                    1
                );


            // =====================================================
            // SELECTIVIDAD PROMEDIO DE LA PAREJA
            // =====================================================
            //
            // Ambos organismos contribuyen a la preferencia.
            //

            double pairSelectivity =
                (
                    parent.MateSelectivity
                    +
                    candidate.MateSelectivity
                )
                /
                2.0;


            pairSelectivity =
                Math.Clamp(
                    pairSelectivity,
                    0,
                    1
                );


            // =====================================================
            // FUERZA DE SELECCIÓN ECOLÓGICA
            // =====================================================
            //
            // MateSelectivity = 0
            //
            // ecologicalSelectionStrength = 0
            //
            // similarity ^ 0 = 1
            //
            // Por tanto:
            // la ecología NO influye en la elección.
            //
            //
            // MateSelectivity = 0.5
            //
            // ecologicalSelectionStrength = 1
            //
            // La similitud ecológica se utiliza directamente
            // como preferencia.
            //
            //
            // MateSelectivity = 1
            //
            // ecologicalSelectionStrength = 2
            //
            // Las diferencias ecológicas tienen una influencia
            // considerable en la aceptación.
            //

            double ecologicalSelectionStrength =
                3.0
                *
                pairSelectivity;


            // =====================================================
            // PREFERENCIA ECOLÓGICA
            // =====================================================
            //
            // Esta fórmula reemplaza la penalización anterior.
            //
            // Es importante porque:
            //
            // 1. pequeñas diferencias producen poca penalización;
            //
            // 2. diferencias grandes tienen consecuencias reales;
            //
            // 3. la selectividad puede evolucionar naturalmente;
            //
            // 4. no existe un corte artificial de apareamiento.
            //

            double ecologicalPreference =
                Math.Pow(
                    ecologicalSimilarity,
                    ecologicalSelectionStrength
                );


            ecologicalPreference =
                Math.Clamp(
                    ecologicalPreference,
                    0.05,
                    1.0
                );


            // =====================================================
            // ACEPTACIÓN FINAL DE LA PAREJA
            // =====================================================
            //
            // Para reproducirse deben superar conjuntamente:
            //
            // 1. compatibilidad genética
            // 2. preferencia ecológica
            //
            // Ambas barreras pueden fortalecerse gradualmente
            // durante la evolución.
            //

            double mateAcceptance =
                compatibility
                *
                ecologicalPreference;


            mateAcceptance =
                Math.Clamp(
                    mateAcceptance,
                    0,
                    1
                );


            // =====================================================
            // DECISIÓN DE APAREAMIENTO
            // =====================================================

            if (
                _random.NextDouble()
                <=
                mateAcceptance
            )
            {
                return candidate;
            }
        }


        return null;
    }


    // =========================================================
    // SHUFFLE
    // =========================================================

    private void Shuffle(
        List<Organism> organisms)
    {
        for (
            int i = organisms.Count - 1;
            i > 0;
            i--
        )
        {
            int j =
                _random.Next(
                    i + 1
                );


            (
                organisms[i],
                organisms[j]
            )
            =
            (
                organisms[j],
                organisms[i]
            );
        }
    }
}