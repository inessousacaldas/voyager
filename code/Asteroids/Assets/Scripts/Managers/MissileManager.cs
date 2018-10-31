using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class the manage the missiles from the player and the alien
/// </summary>
public class MissileManager : Singleton<MissileManager>
{
    [SerializeField]
	private GameObject[] _missilePrefabs;
    [SerializeField]
    private Transform _missileContainer;

	private Dictionary<FlyingObject.FlyingObjectType, List<Missile>> missileObjectPool = new Dictionary<FlyingObject.FlyingObjectType, List<Missile>>();

    /// <summary>
    /// TODOOOOOOOOOOOOOOOOOOOOOOOOO
    /// </summary>
    /// <param name="flyingObject"></param>
    /// <returns></returns>
	private Missile AddMissileToPool(FlyingObject.FlyingObjectType flyingObject)
	{
		int flyingObjectIndex = (int) flyingObject;
		GameObject missileGameObject = Instantiate(_missilePrefabs[flyingObjectIndex]) as GameObject;

        missileGameObject.name = _missilePrefabs[flyingObjectIndex].name;
		missileGameObject.transform.SetParent(_missileContainer, false);

        Missile missile = missileGameObject.GetComponent<Missile>();

		if (!missileObjectPool.ContainsKey(flyingObject))
		{
			missileObjectPool.Add(flyingObject, new List<Missile>());
		}

		missileObjectPool[flyingObject].Add(missile);
		
		return missile;
	}

    /// <summary>
    /// Gets a missile for the flying object from the pool. If no missile available, a new missile is added to to pool.
    /// </summary>
    /// <param name="flyingObject">The flying object to whom the missile belongs</param>
    /// <returns>The missile</returns>
	public Missile GetMissile(FlyingObject.FlyingObjectType flyingObject)
	{
		if (missileObjectPool.ContainsKey(flyingObject))
		{
			for (int index = 0; index < missileObjectPool[flyingObject].Count; index ++)
			{
				if (!missileObjectPool[flyingObject][index].IsActive())
				{
					return missileObjectPool[flyingObject][index];
				}
			}
		}
		
		return AddMissileToPool(flyingObject);
	}
}