using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Starstruck.Weather;

/// <summary>
///     Client-side meteor to create a smooth landing, once it lands a server-owned item is spawned.
/// </summary>
public class Meteor : MonoBehaviour
{
    [SerializeField] private float timeToLand = 4.7f;
    
    private float _actualTimeToLand;

    private Vector3 _spawnLocation;
    private Vector3 _landLocation;

    private float _timeRemaining;

    internal void SetParams(Vector3 spawnLocation, Vector3 landLocation)
    {
        _timeRemaining = 1;

        _spawnLocation = spawnLocation;
        _landLocation = landLocation;

        _actualTimeToLand = Vector3.Distance(_spawnLocation, _landLocation) / 185;
        
#if DEBUG
        StarstruckMod.Logger.LogDebug($"Actual Time to Land: {_actualTimeToLand}");
#endif
    }

    private void Update()
    {
        if (_timeRemaining > 0)
        {
            gameObject.transform.position = Vector3.Lerp(_landLocation, _spawnLocation, _timeRemaining);

            // Dived to Normalize the Time Remaining to 0-1
            _timeRemaining -= Time.deltaTime/_actualTimeToLand;
        }
        // Ensure the meteor has landed
        else if (Vector3.Distance(gameObject.transform.position, _landLocation) < 5)
        {
            Landmine.SpawnExplosion(_landLocation, true, 1.8f, 3.6f);
            
            Destroy(gameObject);
        }
    }
}