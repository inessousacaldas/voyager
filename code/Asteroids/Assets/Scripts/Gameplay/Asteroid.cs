using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent (typeof (DirectionalRotation))]
public class Asteroid : FlyingObject
{

    private static int rendererOrder = 0;
    private static List<Rigidbody2D> asteroidsRigidBodys;

    public enum AsteroidType
    {
        Rock = 0,
        Ice = 1,
        Metal = 2
    }

    public enum AsteroidSize
    {
        Big = 2,
        Middle = 1,
        Small = 0
    }

    [SerializeField]
    private float _minSpeed = 1f;
    [SerializeField]
    private AsteroidType _asteroidType;
    [SerializeField] 
    private AsteroidSize _currSize;
    [SerializeField]
    private int _spawnAmountAfterDestruction = 2;
    [SerializeField]
    private AsteroidSize _spawnAsteroidSize = AsteroidSize.Big;

    [SerializeField]
    private DirectionalRotation _directionalRotation;
    [SerializeField]
    private float _maxDirectionalRotation = 2f;
    [SerializeField]
    private float _minDirectionalSpeed = 20f;
    [SerializeField]
    private float _maxDirectionalSpeed = 50f;

    private float _speed;
    private float _smoothSpeedRate = 0.1f;
    private int _rendererOrder;

    protected override void Awake()
    {
        base.Awake();

        if (asteroidsRigidBodys == null)
        {
            asteroidsRigidBodys = new List<Rigidbody2D>();
        }

        _directionalRotation = GetComponent<DirectionalRotation>();

        rendererOrder++;
        _rendererOrder = rendererOrder;

        // Renderer order of asteroids (for collisions)
        Renderer renderer = GetComponentInChildren<Renderer>();
        renderer.material.renderQueue += _rendererOrder;

    }

    private void FixedUpdate()
    {
        if(rigidbody2DComponent.velocity.magnitude > _speed)
        {
            ReduceSpeed();
        }
    }

    private void ReduceSpeed()
    {
        float speed = rigidbody2DComponent.velocity.magnitude;
        speed -= _smoothSpeedRate * Time.deltaTime;

        rigidbody2DComponent.velocity = rigidbody2DComponent.velocity.normalized * speed;
    }

    protected override void TooLongOutsideOfViewport()
    {
        
        Deactivate();
        AsteroidManager.Instance.SpawnAsteroid(_asteroidType, CurrSize);
        
        
    }

    public override void HitByCollider(Collider2D collider)
    {
        base.HitByCollider(collider);

        if (collider.CompareTag(Config.Tags.missile))
        {
            var missile = collider.GetComponent<Missile>();

            if (missile.OwnedBy != FlyingObjectType.Player)
            {
                Destroy(false);
            }
        }
    }

    public override void Destroy(bool addScore = true)
    {
        AudioManager.Instance.Play(destructionAudioClip);
        asteroidsRigidBodys.Remove(rigidbody2DComponent);

        if (_spawnAmountAfterDestruction > 0)
        {
            // Spawn directions
            float incrementAngle = 360 / _spawnAmountAfterDestruction;
            float currAngle = UnityEngine.Random.Range(0, 360);
            
            for (int index = 0; index < _spawnAmountAfterDestruction; index++)
            {
                
                Vector2 velocity = transform.forward;
                velocity = Quaternion.AngleAxis(currAngle, Vector3.forward) * velocity;

                Vector2 newPosition = rigidbody2DComponent.position + velocity;

                AsteroidManager.Instance.SpawnAsteroid(_asteroidType, _spawnAsteroidSize, newPosition, velocity);

                currAngle += incrementAngle;
            }
        }
        else
        {
            GameManager.Instance.SpawnNewAsteroids();
        }

        if (addScore == true)
        {
            GameManager.Instance.AddScore(Score);
        }

        Deactivate();
    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public AsteroidSize CurrSize
    {
        get
        {
            return _currSize;
        }

        set
        {
            _currSize = value;
        }
    }

    public override void Activate(Vector2 position, Vector2 velocity)
    {
        asteroidsRigidBodys.Add(rigidbody2DComponent);
        
        _speed = UnityEngine.Random.Range(_minSpeed, maxSpeed);
        float rotation = UnityEngine.Random.Range(0f, 359.99f);

        gameObject.transform.position = position;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));

        gameObject.SetActive(true);

        rigidbody2DComponent.velocity = velocity.normalized * _speed;

        // Set random rotation of the asteroid
        float dirX = UnityEngine.Random.Range(-_maxDirectionalRotation, _maxDirectionalRotation);
        float dirY = UnityEngine.Random.Range(-_maxDirectionalRotation, _maxDirectionalRotation);
        float dirZ = UnityEngine.Random.Range(-_maxDirectionalRotation, _maxDirectionalRotation);

        _directionalRotation.Speed = UnityEngine.Random.Range(_minDirectionalSpeed, _maxDirectionalSpeed);
        _directionalRotation.Direction = new Vector3(dirX, dirY, dirZ);

        // Last time seen in the viewport, to respwan if to long outside
        lastSeenInViewport = Time.time;
    }
}
