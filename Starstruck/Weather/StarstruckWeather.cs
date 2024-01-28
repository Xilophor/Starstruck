using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = System.Random;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine.LowLevel;
using UnityEngine.Serialization;

namespace Starstruck.Weather;

public class StarstruckWeather : MonoBehaviour
{
    [SerializeField] private LayerMask layersToIgnore = 0;

    // Range between times
    [SerializeField] private int minTimeBetweenSpawns = 20;
    [SerializeField] private int maxTimeBetweenSpawns = 60;
    
    // Spawn Parameters
    [SerializeField] private int maxToSpawn = 1;
    [SerializeField] private int meteorLandRadius = 6;
    
    private Vector2 _meteorSpawnDirection;
    private Vector3 _meteorSpawnLocationOffset;

    private LethalServerMessage<MeteorSpawnInfo> _serverMessage;
    private LethalClientMessage<MeteorSpawnInfo> _clientMessage;
    
    private float _lastTimeUsed;
    private float _currentTimeOffset;
    private Random _random;
    private GameObject _meteorPrefab;

    private const int RandomSeedOffset = -53;
    
    private void OnEnable()
    {
        _serverMessage ??= new LethalServerMessage<MeteorSpawnInfo>("meteorSpawnSyncedEvent");
        _clientMessage ??= new LethalClientMessage<MeteorSpawnInfo>("meteorSpawnSyncedEvent", SpawnMeteor);

        _meteorPrefab ??= StarstruckMod.Assets["Meteor"] as GameObject;

        if (!PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref loop);
        }
        
#if DEBUG
        StarstruckMod.Logger.LogDebug("Starstruck Weather Effect has been enabled!");
#endif

        if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)) return;
        
        _random = new Random(StartOfRound.Instance.randomMapSeed + RandomSeedOffset);
        TimeOfDay.Instance.onTimeSync.AddListener(OnGlobalTimeSync);

        DecideSpawnArea().Forget();

        // Wait 12-18 seconds before spawning first batch.
        _currentTimeOffset = _random.Next(12, 18);
        
#if DEBUG
        StarstruckMod.Logger.LogDebug("Server Starstruck Weather Effect params have been set!");
#endif
    }

    private async UniTaskVoid DecideSpawnArea()
    {
        try
        {
            await UniTask.SwitchToThreadPool();

            var spawnDirection = (float)_random.NextDouble() * 2 * Mathf.PI;

            _meteorSpawnDirection = new Vector2(Mathf.Sin(spawnDirection), Mathf.Cos(spawnDirection));
            _meteorSpawnLocationOffset = new Vector3(_meteorSpawnDirection.x * _random.Next(540, 1200), 350,
                _meteorSpawnDirection.y * _random.Next(540, 1200));

            if (!await PlanMeteor(maxAttempts: 6, spawn: false))
            {
                await UniTask.Yield();
                await UniTask.NextFrame();

                DecideSpawnArea().Forget();
            }
        }
        finally
        {
            await UniTask.SwitchToMainThread();
        }
    }

    private void SpawnMeteor(MeteorSpawnInfo meteorSpawnInfo)
    {
        SpawnMeteorTask(meteorSpawnInfo).Forget();
    }

    private async UniTaskVoid SpawnMeteorTask(MeteorSpawnInfo meteorSpawnInfo)
    {
        try
        {
            await UniTask.SwitchToThreadPool();
            
            await UniTask.Delay(TimeSpan.FromSeconds(meteorSpawnInfo.timeToSpawnAt - NetworkManager.Singleton.LocalTime.Time));
            
            var meteorObject = Instantiate(_meteorPrefab, new Vector3(0, -1000, 0), Quaternion.identity, StarstruckMod.effectObject.transform);
            meteorObject.GetComponent<Meteor>().SetParams(meteorSpawnInfo.spawnLocation, meteorSpawnInfo.landLocation);
            
#if DEBUG
            StarstruckMod.Logger.LogDebug($"Spawned meteor on client! {meteorSpawnInfo.spawnLocation} -> {meteorSpawnInfo.landLocation}");
#endif
        }
        finally
        {
            await UniTask.SwitchToMainThread();
        }
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
        PlanStrikes();
    }

    private void PlanStrikes()
    {
        _currentTimeOffset = _random.Next(minTimeBetweenSpawns, maxTimeBetweenSpawns);

        var amountToSpawn = _random.Next(1, maxToSpawn);

#if DEBUG
        StarstruckMod.Logger.LogDebug($"Planning {amountToSpawn} meteor strike(s).");
#endif
        
        for (var i = 0; i < amountToSpawn; i++)
        {
            PlanMeteor().Forget();
        }
    }

    private async UniTask<bool> PlanMeteor(int maxAttempts = 4, bool spawn = true)
    {
        try
        {
            await UniTask.SwitchToThreadPool();
            
            for (var i = 0; i < maxAttempts; i++)
            {
                var initialPos = RoundManager.Instance.outsideAINodes[_random.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;

                var landLocation = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(initialPos, radius: meteorLandRadius, navHit: RoundManager.Instance.navHit, randomSeed: _random);
                var spawnLocation = landLocation+_meteorSpawnLocationOffset;

                // See if theres anything in the way excluding foliage & trees, and try a new spawn if so.
                // ReSharper disable once Unity.PreferNonAllocApi
                var raycastHit = Physics.RaycastAll(spawnLocation, landLocation, Mathf.Infinity, ~layersToIgnore);
#if DEBUG
                StarstruckMod.Logger.LogDebug($"Casted ray. {raycastHit}, {raycastHit.Length}");
#endif
                if (raycastHit.Any(hit => hit.transform && hit.transform.tag is not "Wood"))
                {
                    await UniTask.Yield();
                    await UniTask.NextFrame();
                    
                    continue;
                }

                // Skip spawn code if it's only running a check to make sure meteors can spawn.
                if (!spawn) return true;
                
                var timeAtSpawn = NetworkManager.Singleton.LocalTime.Time + (_random.NextDouble() * 10 + 2);

#if DEBUG
                StarstruckMod.Logger.LogDebug(
                    $"Planning meteor strike from {spawnLocation} to {landLocation} in {timeAtSpawn - NetworkManager.Singleton.LocalTime.Time} seconds.");
#endif

                await UniTask.SwitchToMainThread();
                _serverMessage.SendAllClients(new MeteorSpawnInfo(timeAtSpawn, spawnLocation, landLocation));

                return true;
            }
            
            return false;
        }
        finally
        {
            await UniTask.SwitchToMainThread();
        }
    }
}

[Serializable]
public struct MeteorSpawnInfo(double timeToSpawnAt, Vector3 spawnLocation, Vector3 landLocation)
{
    public double timeToSpawnAt = timeToSpawnAt;
    public Vector3 spawnLocation = spawnLocation;
    public Vector3 landLocation = landLocation;
}