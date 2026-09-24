namespace EvolutionSim.Experiments;

/// <summary>
/// Tracks occupancy transitions for one region without consuming RNG
/// or affecting simulation state.
/// </summary>
internal sealed class RegionalOccupancyExperimentTracker
{
    private bool _everEstablished;
    private bool _wasOccupied;
    private bool _currentlyInPostEstablishmentEmptyPeriod;

    private int _localExtinctionEpisodeCount;
    private int _recolonizationCount;
    private int _currentEmptyPeriod;
    private int _longestEmptyPeriod;


    public RegionalOccupancyExperimentTracker(
        bool initiallyOccupied)
    {
        _everEstablished =
            initiallyOccupied;

        _wasOccupied =
            initiallyOccupied;
    }


    public void Observe(
        bool isOccupied)
    {
        if (
            isOccupied
        )
        {
            if (
                _everEstablished
                &&
                !_wasOccupied
                &&
                _currentlyInPostEstablishmentEmptyPeriod
            )
            {
                _recolonizationCount++;
            }


            _everEstablished =
                true;

            _currentlyInPostEstablishmentEmptyPeriod =
                false;

            _currentEmptyPeriod =
                0;

            _wasOccupied =
                true;

            return;
        }


        if (
            !_everEstablished
        )
        {
            _wasOccupied =
                false;

            return;
        }


        if (
            _wasOccupied
        )
        {
            _localExtinctionEpisodeCount++;

            _currentlyInPostEstablishmentEmptyPeriod =
                true;

            _currentEmptyPeriod =
                1;
        }
        else if (
            _currentlyInPostEstablishmentEmptyPeriod
        )
        {
            _currentEmptyPeriod++;
        }


        if (
            _currentEmptyPeriod
            >
            _longestEmptyPeriod
        )
        {
            _longestEmptyPeriod =
                _currentEmptyPeriod;
        }


        _wasOccupied =
            false;
    }


    public RegionalOccupancyExperimentStatistics Snapshot(
        bool isEmptyAtEnd)
    {
        return new RegionalOccupancyExperimentStatistics(
            EverEstablished:
                _everEstablished,

            LocalExtinctionEpisodeCount:
                _localExtinctionEpisodeCount,

            RecolonizationCount:
                _recolonizationCount,

            LongestEmptyPeriod:
                _longestEmptyPeriod,

            IsEmptyAtEnd:
                isEmptyAtEnd
        );
    }
}
