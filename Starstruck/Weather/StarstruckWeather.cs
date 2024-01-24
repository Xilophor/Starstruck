using UnityEngine;
using Random = System.Random;

namespace Starstruck.Weather;

public class StarstruckWeather : MonoBehaviour
{
    private float _lastTimeUsed;
    private float _currentTimeOffset;
    private Random _random;

    // Range between times
    private const int MinTimeRange = 8;
    private const int MaxTimeRange = 45;
    
    // Range between distance offsets
    private const int MinOffset = 8;
    private const int MaxOffset = 45;
    
    private const int RandomSeedOffset = -53;
    private const int MeteorLandRadius = 14;

    private Vector3 MeteorSpawnLocation;
    
    private void OnEnable()
    {
        _random = new Random(StartOfRound.Instance.randomMapSeed + RandomSeedOffset);
        TimeOfDay.Instance.onTimeSync.AddListener(OnGlobalTimeSync);
    }

    private void OnDisable()
    {
        TimeOfDay.Instance.onTimeSync.RemoveListener(OnGlobalTimeSync);
    }

    private void OnGlobalTimeSync()
    {
        var time = TimeOfDay.Instance.globalTime;
        if (time <= _lastTimeUsed + _currentTimeOffset)
            return;
        _lastTimeUsed = time;
        PlanStrike();
    }

    private void PlanStrike()
    {
        _currentTimeOffset = _random.Next(MinTimeRange, MaxTimeRange);

        var amountToSpawn = _random.Next(MinOffset, MaxOffset);

        for (var i = 0; i < amountToSpawn; i++)
        {
            SpawnMeteor();
        }
    }

    private void SpawnMeteor()
    {
        var initialPos = RoundManager.Instance.outsideAINodes[_random.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
        var selectedLandPos = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(initialPos, radius: MeteorLandRadius, navHit: RoundManager.Instance.navHit, randomSeed: _random);

        var selectedSpawnPos = GetRandomPosition(MeteorSpawnLocation, 50, 150, 1, 1);
    }

    private Vector3 GetRandomPosition(
        Vector3 pos,
        int minRange,
        int maxRange,
        int directionX,
        int directionZ)
    {
        var x = _random.Next(minRange * directionX, maxRange * directionX);
        var y = _random.Next(minRange * directionZ / 15, maxRange * directionZ / 25);
        var z = _random.Next(minRange * directionZ, maxRange * directionZ);
        
        return pos + new Vector3(x, y, z);
    }
}