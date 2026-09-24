using EvolutionSim.Models;

namespace EvolutionSim.Evolution;

public sealed class EcologicalMateSimilarityCalculator
{
    // =========================================================
    // ESCALAS ECOLÓGICAS
    // =========================================================
    //
    // Indican qué magnitud de diferencia consideramos
    // ecológicamente importante.
    //

    private const double OptimalTemperatureScale =
        6.0;

    private const double ThermalToleranceScale =
        3.5;

    private const double MeatAdaptationScale =
        0.30;

    private const double PredatoryDriveScale =
        0.30;

    private const double ScavengingDriveScale =
        0.30;


    // =========================================================
    // PESOS ECOLÓGICOS
    // =========================================================
    //
    // La adaptación térmica tiene mayor importancia porque
    // actualmente es una de las principales dimensiones
    // ecológicas del mundo.
    //
    // Los rasgos alimentarios siguen influyendo, pero ya no
    // pueden ocultar una divergencia térmica importante.
    //

    private const double OptimalTemperatureWeight =
        0.45;

    private const double ThermalToleranceWeight =
        0.20;

    private const double MeatAdaptationWeight =
        0.15;

    private const double PredatoryDriveWeight =
        0.10;

    private const double ScavengingDriveWeight =
        0.10;


    // =========================================================
    // FUERZA DE DECAIMIENTO
    // =========================================================

    private const double SimilarityDecayStrength =
        1.20;


    public double Calculate(
        Genome genomeA,
        Genome genomeB)
    {
        // =====================================================
        // TEMPERATURA ÓPTIMA
        // =====================================================

        double optimalTemperatureDifference =
            (
                genomeA.OptimalTemperature
                -
                genomeB.OptimalTemperature
            )
            /
            OptimalTemperatureScale;


        // =====================================================
        // TOLERANCIA TÉRMICA
        // =====================================================

        double thermalToleranceDifference =
            (
                genomeA.ThermalTolerance
                -
                genomeB.ThermalTolerance
            )
            /
            ThermalToleranceScale;


        // =====================================================
        // ADAPTACIÓN A CARNE
        // =====================================================

        double meatAdaptationDifference =
            (
                genomeA.MeatAdaptation
                -
                genomeB.MeatAdaptation
            )
            /
            MeatAdaptationScale;


        // =====================================================
        // CONDUCTA DEPREDADORA
        // =====================================================

        double predatoryDriveDifference =
            (
                genomeA.PredatoryDrive
                -
                genomeB.PredatoryDrive
            )
            /
            PredatoryDriveScale;


        // =====================================================
        // CARROÑEO
        // =====================================================

        double scavengingDriveDifference =
            (
                genomeA.ScavengingDrive
                -
                genomeB.ScavengingDrive
            )
            /
            ScavengingDriveScale;


        // =====================================================
        // DISTANCIA ECOLÓGICA PONDERADA
        // =====================================================

        double weightedSquaredDistance =
            (
                OptimalTemperatureWeight
                *
                optimalTemperatureDifference
                *
                optimalTemperatureDifference
            )
            +
            (
                ThermalToleranceWeight
                *
                thermalToleranceDifference
                *
                thermalToleranceDifference
            )
            +
            (
                MeatAdaptationWeight
                *
                meatAdaptationDifference
                *
                meatAdaptationDifference
            )
            +
            (
                PredatoryDriveWeight
                *
                predatoryDriveDifference
                *
                predatoryDriveDifference
            )
            +
            (
                ScavengingDriveWeight
                *
                scavengingDriveDifference
                *
                scavengingDriveDifference
            );


        double ecologicalDistance =
            Math.Sqrt(
                weightedSquaredDistance
            );


        // =====================================================
        // SIMILITUD ECOLÓGICA
        // =====================================================
        //
        // 1.0
        // -> prácticamente idénticos ecológicamente.
        //
        // Valores menores indican nichos progresivamente
        // diferentes.
        //
        // Sigue siendo una curva suave:
        // no existe un corte artificial.
        //

        double similarity =
            Math.Exp(
                -SimilarityDecayStrength
                *
                ecologicalDistance
                *
                ecologicalDistance
            );


        return Math.Clamp(
            similarity,
            0,
            1
        );
    }
}