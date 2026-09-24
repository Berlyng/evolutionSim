namespace EvolutionSim.Models;

/// <summary>
/// Couplings phenotype -> ecología con alcance de ejecución.
///
/// SimulationEngine abre scopes independientes por cada Step().
///
/// AsyncLocal mantiene aisladas entre sí las corridas experimentales
/// que se ejecutan en paralelo.
///
/// No se utiliza Random.
/// </summary>
public static class PhenotypeEcologyContext
{
    private static readonly AsyncLocal<double?>
        MetabolicEfficiencyCouplingSlot =
            new();


    private static readonly AsyncLocal<double?>
        ThermalRegulationCouplingSlot =
            new();


    private static readonly AsyncLocal<double?>
        BodySizeCouplingSlot =
            new();


    private static readonly AsyncLocal<double?>
        LocomotionCouplingSlot =
            new();


    private static readonly AsyncLocal<double?>
        FeedingSpecializationCouplingSlot =
            new();


    public static double MetabolicEfficiencyCoupling =>
        MetabolicEfficiencyCouplingSlot.Value
        ??
        0.125;


    public static double ThermalRegulationCoupling =>
        ThermalRegulationCouplingSlot.Value
        ??
        0.0;


    public static double BodySizeCoupling =>
        BodySizeCouplingSlot.Value
        ??
        0.0;


    public static double LocomotionCoupling =>
        LocomotionCouplingSlot.Value
        ??
        0.0;


    public static double FeedingSpecializationCoupling =>
        FeedingSpecializationCouplingSlot.Value
        ??
        0.0;


    public static IDisposable PushMetabolicEfficiencyCoupling(
        double coupling)
    {
        double normalized =
            Math.Clamp(
                coupling,
                0,
                1
            );


        double? previous =
            MetabolicEfficiencyCouplingSlot.Value;


        MetabolicEfficiencyCouplingSlot.Value =
            normalized;


        return new Scope(
            restore:
                () =>
                    MetabolicEfficiencyCouplingSlot.Value =
                        previous
        );
    }


    public static IDisposable PushThermalRegulationCoupling(
        double coupling)
    {
        double normalized =
            Math.Clamp(
                coupling,
                0,
                1
            );


        double? previous =
            ThermalRegulationCouplingSlot.Value;


        ThermalRegulationCouplingSlot.Value =
            normalized;


        return new Scope(
            restore:
                () =>
                    ThermalRegulationCouplingSlot.Value =
                        previous
        );
    }


    public static IDisposable PushBodySizeCoupling(
        double coupling)
    {
        double normalized =
            Math.Clamp(
                coupling,
                0,
                1
            );


        double? previous =
            BodySizeCouplingSlot.Value;


        BodySizeCouplingSlot.Value =
            normalized;


        return new Scope(
            restore:
                () =>
                    BodySizeCouplingSlot.Value =
                        previous
        );
    }


    public static IDisposable PushLocomotionCoupling(
        double coupling)
    {
        double normalized =
            Math.Clamp(
                coupling,
                0,
                1
            );


        double? previous =
            LocomotionCouplingSlot.Value;


        LocomotionCouplingSlot.Value =
            normalized;


        return new Scope(
            restore:
                () =>
                    LocomotionCouplingSlot.Value =
                        previous
        );
    }


    public static IDisposable PushFeedingSpecializationCoupling(
        double coupling)
    {
        double normalized =
            Math.Clamp(
                coupling,
                0,
                1
            );


        double? previous =
            FeedingSpecializationCouplingSlot.Value;


        FeedingSpecializationCouplingSlot.Value =
            normalized;


        return new Scope(
            restore:
                () =>
                    FeedingSpecializationCouplingSlot.Value =
                        previous
        );
    }


    private sealed class Scope : IDisposable
    {
        private readonly Action
            _restore;

        private bool
            _disposed;


        public Scope(
            Action restore)
        {
            _restore =
                restore;
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }


            _restore();


            _disposed =
                true;
        }
    }
}
