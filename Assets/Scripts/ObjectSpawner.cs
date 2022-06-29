using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private SpawningObject _spawningObject = default;
    [SerializeField] private bool _isSpawning = false;
    [SerializeField] [Range(0.01f, 10f)] private float _spawnRate = 2f;
    
    private readonly List<Transform> _spawnPoints = new List<Transform>();
    private readonly float _epsilon = 0.0001f;
    
    private WaitForSeconds _waitTime = default;
    private WaitUntil _waitIsSpawning = default;
    private Coroutine _spawnObjects = default;
    private float _spawnRateCashe = default;

    private bool IsInactive => !(_spawnObjects is null); 

    private void Awake()
    {
        SetSpawnPoints();
    }

    private void Start()
    {
        _spawnRateCashe = _spawnRate;
        _waitTime = new WaitForSeconds(_spawnRate);
        _waitIsSpawning = new WaitUntil(() => _isSpawning);
        BeginSpawning();
    }

    private void OnValidate()
    {
        if (Mathf.Abs(_spawnRateCashe - _spawnRate) < _epsilon) return;
        
        _spawnRateCashe = _spawnRate;
        _waitTime = new WaitForSeconds(_spawnRate);
    }

    private void BeginSpawning()
    {
        _isSpawning = true;
        _spawnObjects ??= StartCoroutine(SpawnObject());
    }

    private void StopSpawning()
    {
        if (IsInactive) return;
        
        StopCoroutine(_spawnObjects);
        _isSpawning = false;
        _spawnObjects = null;
    }

    private void Pause()
    {
        if (IsInactive) return;
        
        _isSpawning = false;
    }

    private void Resume()
    {
        if (IsInactive) return;

        _isSpawning = true;
    }
    
    private void SetSpawnPoints()
    {
        foreach (Transform spawnPoint in transform)
        {
            _spawnPoints.Add(spawnPoint);
        }
    }

    private IEnumerator SpawnObject()
    {
        if (_spawnPoints.Count == 0) throw new Exception("Не заданы точки спавна");
        
        IEnumerator spawnPointsEnumerator = _spawnPoints.GetEnumerator();
        
        while (_isSpawning)
        {
            if (spawnPointsEnumerator.MoveNext() == false)
            {
                spawnPointsEnumerator.Reset();
                spawnPointsEnumerator.MoveNext();
            }
                
            Transform spawnPoint = (Transform) spawnPointsEnumerator.Current;

            if (spawnPoint is null) throw new NullReferenceException();
            
            Instantiate(_spawningObject, spawnPoint.position, Quaternion.identity);
            yield return _waitTime;
            yield return _waitIsSpawning;
        }
    }
}
