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
    
    // Range between distance offsets
    [SerializeField] private int minOffset = 1;
    [SerializeField] private int maxOffset = 25;
    
    // Spawn Parameters
    [SerializeField] private int maxToSpawn = 1;
    [SerializeField] private int meteorLandRadius = 6;
    
    private Vector2 MeteorSpawnDirection = Vector2.one;
    private Vector3 MeteorSpawnLocationOffset = new(300,500,300);

    private LethalServerMessage<MeteorSpawnInfo> _serverMessage;
    private LethalClientMessage<MeteorSpawnInfo> _clientMessage;
    
    private float _lastTimeUsed;
    private float _currentTimeOffset;
    private Random _random;
    private GameObject _meteorPrefab;
    private static readonly int Tod = Animator.StringToHash("timeOfDay");

    private const int RandomSeedOffset = -53;
    
    private void OnEnable()
    {
        _random = new Random(StartOfRound.Instance.randomMapSeed + RandomSeedOffset);
        TimeOfDay.Instance.onTimeSync.AddListener(OnGlobalTimeSync);

        _serverMessage ??= new LethalServerMessage<MeteorSpawnInfo>("meteorSpawnSyncedEvent");
        _clientMessage ??= new LethalClientMessage<MeteorSpawnInfo>("meteorSpawnSyncedEvent", SpawnMeteor);

        _meteorPrefab ??= StarstruckMod.Assets["Meteor"] as GameObject;

        var spawnDirection = (float)_random.NextDouble()*2*Mathf.PI;
        
        MeteorSpawnDirection = new Vector2(Mathf.Sin(spawnDirection), Mathf.Cos(spawnDirection));

        MeteorSpawnLocationOffset = new Vector3(MeteorSpawnDirection.x*_random.Next(540,1200), 350, MeteorSpawnDirection.y*_random.Next(540,1200));

        if (!PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref loop);
        }

#if DEBUG
        StarstruckMod.Logger.LogDebug("Starstruck Weather Effect has been enabled!");
#endif
    }

    private void SpawnMeteor(MeteorSpawnInfo meteorSpawnInfo)
    {
        SpawnMeteorTask(meteorSpawnInfo).Forget();
    }

    private async UniTaskVoid SpawnMeteorTask(MeteorSpawnInfo meteorSpawnInfo)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(meteorSpawnInfo.TimeToSpawnAt - NetworkManager.Singleton.LocalTime.Time));
        
        var meteorObject = Instantiate(_meteorPrefab, new Vector3(0, -1000, 0), Quaternion.identity);
        meteorObject.GetComponent<Meteor>().SetParams(meteorSpawnInfo.SpawnLocation, meteorSpawnInfo.LandLocation);
        
#if DEBUG
        StarstruckMod.Logger.LogDebug($"Spawned meteor on client! {meteorSpawnInfo.SpawnLocation} -> {meteorSpawnInfo.LandLocation}");
#endif
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
            PlanMeteor();
        }
    }

    private void PlanMeteor()
    {
        while (true)
        {
            var initialPos = RoundManager.Instance.outsideAINodes[_random.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;

            var landLocation = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(initialPos, radius: meteorLandRadius, navHit: RoundManager.Instance.navHit, randomSeed: _random);
            var spawnLocation = landLocation+MeteorSpawnLocationOffset;

            // See if theres anything in the way excluding foliage & trees, and try a new spawn if so.
            // ReSharper disable once Unity.PreferNonAllocApi
            var raycastHit = Physics.RaycastAll(spawnLocation, landLocation, Mathf.Infinity, ~layersToIgnore);
#if DEBUG
            StarstruckMod.Logger.LogDebug($"Casted ray. {raycastHit}, {raycastHit.Length}");
#endif
            if (raycastHit.Any(hit => hit.transform && hit.transform.tag is not "Wood"))
                continue;
            
            var timeAtSpawn = NetworkManager.Singleton.LocalTime.Time + (_random.NextDouble() * 10 + 2);
            
#if DEBUG
            StarstruckMod.Logger.LogDebug($"Planning meteor strike from {spawnLocation} to {landLocation} in {timeAtSpawn-NetworkManager.Singleton.LocalTime.Time} seconds.");
#endif
            
            _serverMessage.SendAllClients(new MeteorSpawnInfo(timeAtSpawn, spawnLocation, landLocation));
            
            break;
        }
    }
}

[Serializable]
public struct MeteorSpawnInfo(double timeToSpawnAt, Vector3 spawnLocation, Vector3 landLocation)
{
    public double TimeToSpawnAt = timeToSpawnAt;
    public Vector3 SpawnLocation = spawnLocation;
    public Vector3 LandLocation = landLocation;
}