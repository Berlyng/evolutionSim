namespace EvolutionSim.World;

public sealed class ClimateSystem
{
    private readonly Random _random;

    private readonly Dictionary<int, ClimateProfile>
        _profiles;


    //
    // Una estación completa dura 120 ciclos.
    //
    // No representa necesariamente 120 días.
    // Es simplemente nuestra escala ecológica.
    //

    private const int SeasonalCycleLength =
        120;


    //
    // Guardamos una anomalía meteorológica por
    // región para que el clima no cambie de forma
    // totalmente aleatoria entre dos ciclos.
    //

    private readonly Dictionary<int, double>
        _weatherAnomalies =
            new();


    public ClimateSystem(
        Random random,
        IEnumerable<ClimateProfile> profiles)
    {
        _random =
            random;


        _profiles =
            profiles.ToDictionary(
                profile =>
                    profile.RegionId
            );
    }


    public void AdvanceCycle(
        Worldd world,
        int cycle)
    {
        foreach (
            Region region
            in world.Regions
        )
        {
            ClimateProfile profile =
                GetProfile(
                    region.Id
                );


            //
            // ==================================
            // ESTACIÓN
            // ==================================
            //

            double seasonalPosition =
                (
                    cycle
                    %
                    SeasonalCycleLength
                )
                /
                (double)
                SeasonalCycleLength;


            double angle =
                (
                    seasonalPosition
                    *
                    Math.PI
                    *
                    2
                )
                +
                profile.PhaseOffset;


            double seasonalVariation =
                Math.Sin(
                    angle
                )
                *
                profile.SeasonalAmplitude;


            //
            // ==================================
            // METEOROLOGÍA
            // ==================================
            //
            // No hacemos:
            //
            // temperatura += Random(-5, +5)
            //
            // porque produciría ruido absurdo.
            //
            // La anomalía actual conserva 85 %
            // de la del ciclo anterior.
            //

            double previousAnomaly =
                _weatherAnomalies
                    .GetValueOrDefault(
                        region.Id,
                        0
                    );


            double randomWeather =
                (
                    (
                        _random.NextDouble()
                        *
                        2
                    )
                    -
                    1
                )
                *
                profile.WeatherVariation;


            double weatherAnomaly =
                (
                    previousAnomaly
                    *
                    0.85
                )
                +
                (
                    randomWeather
                    *
                    0.15
                );


            _weatherAnomalies[
                region.Id
            ] =
                weatherAnomaly;


            //
            // ==================================
            // TEMPERATURA FINAL
            // ==================================
            //

            double temperature =
                region.BaseTemperature
                +
                seasonalVariation
                +
                weatherAnomaly;


            region.SetTemperature(
                temperature
            );
        }
    }


    private ClimateProfile GetProfile(
        int regionId)
    {
        if (
            _profiles.TryGetValue(
                regionId,
                out ClimateProfile? profile
            )
        )
        {
            return profile;
        }


        //
        // Una región sin perfil explícito
        // permanece prácticamente estable.
        //

        return new ClimateProfile(
            RegionId:
                regionId,

            SeasonalAmplitude:
                0,

            WeatherVariation:
                0
        );
    }
}