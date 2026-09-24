using EvolutionSim.Models;

namespace EvolutionSim.Ecology;

public readonly record struct HuntingResult(
    bool Attempted,
    bool Success,
    double EnergySpent,
    double MeatBiomassConsumed
);


public sealed class PredationSystem
{
    private readonly Random _random;


    private const int PreySampleSize =
        6;


    public PredationSystem(
        Random random)
    {
        _random =
            random;
    }


    // =========================================================
    // INTENTO DE CAZA
    // =========================================================

    public HuntingResult TryHunt(
        Organism hunter,
        List<Organism> patchPopulation,
        double ambientTemperature,
        double hunger,
        CarcassSystem carcassSystem)
    {
        if (
            !hunter.IsAlive
        )
        {
            return new HuntingResult(
                Attempted: false,
                Success: false,
                EnergySpent: 0,
                MeatBiomassConsumed: 0
            );
        }


        Organism? prey =
            ChoosePrey(
                hunter,
                patchPopulation,
                ambientTemperature
            );


        if (
            prey is null
        )
        {
            return new HuntingResult(
                Attempted: false,
                Success: false,
                EnergySpent: 0,
                MeatBiomassConsumed: 0
            );
        }


        // =====================================================
        // COSTO DE PERSECUCIÓN
        // =====================================================

        double pursuitCost =
            CalculatePursuitCost(
                hunter,
                prey,
                ambientTemperature
            );


        hunter.SpendEnergy(
            pursuitCost
        );


        //
        // El cazador puede morir por el esfuerzo.
        //

        if (
            !hunter.IsAlive
        )
        {
            return new HuntingResult(
                Attempted: true,
                Success: false,
                EnergySpent: pursuitCost,
                MeatBiomassConsumed: 0
            );
        }


        // =====================================================
        // HABILIDAD DE CAZA Y ESCAPE
        // =====================================================

        double huntingAbility =
            hunter.GetHuntingAbility(
                _random,
                ambientTemperature
            );


        double escapeAbility =
            prey.GetEscapeAbility(
                _random,
                ambientTemperature
            );


        // =====================================================
        // VENTAJA DE TAMAÑO
        // =====================================================

        double sizeAdvantage =
            Math.Sqrt(
                hunter.Size
                /
                prey.Size
            );


        sizeAdvantage =
            Math.Clamp(
                sizeAdvantage,
                0.50,
                1.50
            );


        double attackScore =
            huntingAbility
            *
            sizeAdvantage;


        // =====================================================
        // PROBABILIDAD DE CAPTURA
        // =====================================================

        double captureProbability =
            attackScore
            /
            (
                attackScore
                +
                escapeAbility
            );


        captureProbability =
            Math.Clamp(
                captureProbability,
                0.02,
                0.95
            );


        // =====================================================
        // FRACASO
        // =====================================================

        if (
            _random.NextDouble()
            >
            captureProbability
        )
        {
            return new HuntingResult(
                Attempted: true,
                Success: false,
                EnergySpent: pursuitCost,
                MeatBiomassConsumed: 0
            );
        }


        // =====================================================
        // ÉXITO
        // =====================================================

        prey.Die();


        //
        // El cadáver queda exactamente en el patch
        // donde ocurrió la caza.
        //

        carcassSystem.RegisterDeath(
            prey
        );


        // =====================================================
        // CONSUMO INMEDIATO
        // =====================================================

        double hungerFactor =
            0.50
            +
            (
                0.50
                *
                hunger
            );


        double requestedMeat =
            hunter.FeedingCapacity
            *
            hungerFactor;


        double consumedBiomass =
            carcassSystem.ConsumeFromPatch(
                prey.PatchId,
                requestedMeat
            );


        double rawMeatEnergy =
            consumedBiomass
            *
            CarcassSystem.EnergyPerBiomassUnit;


        double usableMeatEnergy =
            rawMeatEnergy
            *
            hunter.MeatDigestionEfficiency;


        hunter.Eat(
            usableMeatEnergy
        );


        return new HuntingResult(
            Attempted: true,
            Success: true,
            EnergySpent: pursuitCost,
            MeatBiomassConsumed:
                consumedBiomass
        );
    }


    // =========================================================
    // COSTO DE CAZA
    // =========================================================

    private static double CalculatePursuitCost(
        Organism hunter,
        Organism prey,
        double ambientTemperature)
    {
        //
        // Costo físico base del cazador.
        //

        double movementCost =
            hunter.Speed
            *
            hunter.Speed
            *
            Math.Sqrt(
                hunter.Size
            );


        double baseCost =
            1.5
            +
            (
                movementCost
                *
                1.5
            );


        //
        // Una presa relativamente más rápida exige
        // una persecución más costosa.
        //
        // Si ambos tienen velocidades similares,
        // el multiplicador queda cerca de 1.0.
        //

        double relativeSpeed =
            prey.Speed
            /
            Math.Max(
                hunter.Speed,
                0.10
            );


        double speedChallenge =
            Math.Clamp(
                0.75
                +
                (
                    0.25
                    *
                    relativeSpeed
                ),
                0.80,
                1.35
            );


        //
        // Una presa mayor que el cazador también
        // exige más esfuerzo para someterla.
        //
        // Una presa pequeña no reduce artificialmente
        // el costo físico base de la persecución.
        //

        double relativeSize =
            prey.Size
            /
            Math.Max(
                hunter.Size,
                0.10
            );


        double sizeChallenge =
            1.0
            +
            (
                Math.Max(
                    0,
                    relativeSize
                    -
                    1.0
                )
                *
                0.35
            );


        sizeChallenge =
            Math.Clamp(
                sizeChallenge,
                1.0,
                1.50
            );


        double preyChallenge =
            speedChallenge
            *
            sizeChallenge;


        double thermalMultiplier =
            hunter.GetThermalCostMultiplier(
                ambientTemperature
            );


        return
            baseCost
            *
            preyChallenge
            *
            thermalMultiplier;
    }


    // =========================================================
    // SELECCIÓN DE PRESA
    // =========================================================

    private Organism? ChoosePrey(
        Organism hunter,
        List<Organism> patchPopulation,
        double ambientTemperature)
    {
        if (
            patchPopulation.Count
            <=
            1
        )
        {
            return null;
        }


        Organism? bestPrey =
            null;


        double bestScore =
            double.MinValue;


        for (
            int attempt = 0;
            attempt < PreySampleSize;
            attempt++
        )
        {
            Organism candidate =
                patchPopulation[
                    _random.Next(
                        patchPopulation.Count
                    )
                ];


            if (
                candidate.Id
                ==
                hunter.Id
            )
            {
                continue;
            }


            if (
                !candidate.IsAlive
            )
            {
                continue;
            }


            //
            // Protección adicional:
            // nunca atacar fuera del patch.
            //

            if (
                candidate.PatchId
                !=
                hunter.PatchId
            )
            {
                continue;
            }


            // -------------------------------------------------
            // VALOR ALIMENTARIO
            // -------------------------------------------------

            double foodValue =
                candidate.Size;


            // -------------------------------------------------
            // RIESGO POR TAMAÑO
            // -------------------------------------------------

            double relativeSize =
                candidate.Size
                /
                hunter.Size;


            double sizeRiskWeight =
                2.50
                -
                (
                    1.50
                    *
                    hunter.RiskTolerance
                );


            double sizePenalty =
                Math.Max(
                    0,
                    relativeSize
                    -
                    1
                )
                *
                sizeRiskWeight;


            // -------------------------------------------------
            // CAPACIDAD DE ESCAPE
            // -------------------------------------------------

            double escapeAbility =
                candidate.GetEscapeAbility(
                    _random,
                    ambientTemperature
                );


            // -------------------------------------------------
            // PUNTUACIÓN DE PRESA
            // -------------------------------------------------
            //
            // RiskTolerance no crea un tipo de depredador.
            // Solo cambia cuánto penaliza este individuo
            // una presa difícil o peligrosa.
            //

            double escapeRiskWeight =
                1.40
                -
                (
                    0.60
                    *
                    hunter.RiskTolerance
                );


            double score =
                foodValue
                /
                (
                    0.5
                    +
                    (
                        escapeAbility
                        *
                        escapeRiskWeight
                    )
                )
                /
                (
                    1
                    +
                    sizePenalty
                );


            //
            // Pequeña variación para evitar una selección
            // completamente determinista.
            //

            score *=
                0.85
                +
                (
                    _random.NextDouble()
                    *
                    0.30
                );


            if (
                score
                >
                bestScore
            )
            {
                bestScore =
                    score;

                bestPrey =
                    candidate;
            }
        }


        return bestPrey;
    }
}