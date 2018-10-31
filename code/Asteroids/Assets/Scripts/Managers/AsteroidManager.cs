using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class to manage the different asteroids
/// </summary>
public class AsteroidManager : Singleton<AsteroidManager>
{
    [SerializeField]
    private Asteroid[] _rockAsteroidPrefabs;
    [SerializeField]
    private Asteroid[] _iceAsteroidPrefabs;
    [SerializeField]
    private Asteroid[] _metalAsteroidPrefabs;
    [SerializeField]
    private Transform _asteroidContainer;

    private Dictionary<Vector2Int, List<Asteroid>> _asteroidObjectPool = new Dictionary<Vector2Int, List<Asteroid>>();

    private void Start()
    {
        // Make sure the prefabs are in order of size
        _rockAsteroidPrefabs = _rockAsteroidPrefabs.OrderBy(asteroid => asteroid.CurrSize).ToArray();
        _iceAsteroidPrefabs = _iceAsteroidPrefabs.OrderBy(asteroid => asteroid.CurrSize).ToArray();
        _metalAsteroidPrefabs = _metalAsteroidPrefabs.OrderBy(asteroid => asteroid.CurrSize).ToArray();
    }

    /// <summary>
    /// Adds an asteroid of type and size to the asteroid's pool
    /// </summary>
    /// <param name="type">Type of the asteroid (Rock, Ice, Metal)</param>
    /// <param name="size">Size of the asteroid (Big, Middle, Small)</param>
    /// <returns>The asteroid object added to the pool</returns>
    private Asteroid AddAsteroidToObjectPool(Asteroid.AsteroidType type, Asteroid.AsteroidSize size)
    {
        GameObject asteroidGameObject = null;

        switch (type)
        {
            case Asteroid.AsteroidType.Rock:
                asteroidGameObject = Instantiate(_rockAsteroidPrefabs[(int)size].gameObject) as GameObject;
                asteroidGameObject.name = _rockAsteroidPrefabs[(int)size].name;
                break;

            case Asteroid.AsteroidType.Ice:
                asteroidGameObject = Instantiate(_iceAsteroidPrefabs[(int)size].gameObject) as GameObject;
                asteroidGameObject.name = _iceAsteroidPrefabs[(int)size].name;
                break;

            case Asteroid.AsteroidType.Metal:
                asteroidGameObject = Instantiate(_metalAsteroidPrefabs[(int)size].gameObject) as GameObject;
                asteroidGameObject.name = _metalAsteroidPrefabs[(int)size].name;
                break;
        }

        asteroidGameObject.transform.SetParent(_asteroidContainer, false);
        Asteroid asteroid = asteroidGameObject.GetComponent<Asteroid>();

        Vector2Int key = new Vector2Int((int)type, (int)size);

        if (!_asteroidObjectPool.ContainsKey(key))
        {
            _asteroidObjectPool.Add(key, new List<Asteroid>());
        }

        _asteroidObjectPool[key].Add(asteroid);

        return asteroid;
    }

    /// <summary>
    /// Deactivates all the asteroids in the pool
    /// </summary>
    public void DeactivateAllAsteroids()
    {
        if (GetActiveAsteroidAmount() > 0)
        {
            foreach (KeyValuePair<Vector2Int, List<Asteroid>> asteroid in _asteroidObjectPool)
            {
                for (int index = 0; index < asteroid.Value.Count; index++)
                {
                    asteroid.Value[index].Deactivate();
                }
            }
        }
    }


    /// <summary>
    /// Gets the number of active asteroids in the pool
    /// </summary>
    /// <returns>The number of active asteroids in the pool</returns>
    public int GetActiveAsteroidAmount()
    {
        int activeAsteroidAmount = 0;

        foreach (KeyValuePair<Vector2Int, List<Asteroid>> asteroid in _asteroidObjectPool)
        {
            for (int i = 0; i < asteroid.Value.Count; i++)
            {
                if (asteroid.Value[i].IsActive())
                {
                    activeAsteroidAmount++;
                }
            }
        }

        return activeAsteroidAmount;
    }


    /// <summary>
    /// Gets an asteroid of type and size from the pool. If there's no free asteroid, a new is added to the pool.
    /// </summary>
    /// <param name="type">Type of the asteroid (Rock, Ice, Metal)</param>
    /// <param name="size">Size of the asteroid (Big, Middle, Small)</param>
    /// <returns></returns>
    public Asteroid GetAsteroid(Asteroid.AsteroidType type, Asteroid.AsteroidSize size)
    {
        Vector2Int key = new Vector2Int((int)type, (int)size);

        if (_asteroidObjectPool.ContainsKey(key))
        {
            for (int i = 0; i < _asteroidObjectPool[key].Count; i++)
            {
                if (!_asteroidObjectPool[key][i].IsActive())
                {
                    return _asteroidObjectPool[key][i];
                }
            }
        }
        return AddAsteroidToObjectPool(type, size);
    }

    /// <summary>
    /// Spwans a new asteroid into the game.
    /// </summary>
    /// <param name="type">Type of the asteroid (Rock, Ice, Metal)</param>
    /// <param name="size">Size of the asteroid (Big, Middle, Small)</param>
    /// <param name="position">Position of the asteroid</param>
    /// <param name="velocity">Velocity of the asteroid</param>
    public void SpawnAsteroid(Asteroid.AsteroidType type, Asteroid.AsteroidSize size, Vector2 position = default(Vector2), Vector2 velocity = default(Vector2))
    {
        if (position == default(Vector2) && velocity == default(Vector2))
        {
            Vector3 playerPosition = GameManager.Instance.Player.transform.position;
            Vector3 spawnPosition = GameManager.Instance.SpawnOutsideViewport();

            playerPosition.x -= spawnPosition.x;
            playerPosition.y -= spawnPosition.y;

            position = spawnPosition;
            velocity = playerPosition.normalized;
        }
        else if (velocity == default(Vector2))
        {
            velocity = new Vector2(Random.Range(-1, 1f), Random.Range(-1f, 1f));
        }

        Asteroid asteroid = Instance.GetAsteroid(type, size);

        asteroid.Activate(position, velocity);
    }
}