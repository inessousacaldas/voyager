using System;
using System.Collections;
using UnityEngine;

public class Player : Character
{
    [SerializeField]
    private float _thrust = 10f;
    [SerializeField]
    private float _thrustLossRatio = 0.02f;

    [SerializeField]
    private float _turnThrust = 200f;
    [SerializeField]
    private float _maxAngularVelocity = 200f;
    [SerializeField]
    private float _angularVelocityLossRatio = 0.5f;
    [SerializeField]
    private float _maxIncrementAngularVelocity = 0.1f;

    [SerializeField]
    private int _maxRoundMissile = 10;
    [SerializeField]
    private float _timeReloadMissiles = 2f;

    [SerializeField]
    private int _maxLife = 3;
    [SerializeField]
    private float _indestructibleTime = 3f;

    [SerializeField]
    private GameObject _shield;

    [SerializeField]
    private AudioClip _reloadAudioClip;
    
    private bool _hasThrust = false;
    private int _dirThrust;

    private bool _hasAngularVelocity = false;
    private int _dirAngularVelocity;
    private float _prevIncrementAngularVelocity = 0f;

    private int _currMissiles;
    private float _lastMissileRound;
    private bool _reloadMissiles;

    private int _life = 0;
    private float _indestructibleUntil = 0f;

    private HyperSpace _hyperSpace;
    private EnergyFuel _energyFuel;

    private Animator _animatorShip;
    private Animator _animatorCannon;
    private Animator _animatorCamera;

    private AudioSource[] _audioSource;

    private enum audioClipsSources
    {
        Thrust = 0,
        Effects = 1
    }

    public bool HasThrust
    {
        get
        {
            return _hasThrust;
        }
        set
        {
            if (value == true)
            {
                _audioSource[(int)audioClipsSources.Thrust].Play();
            }
            else
            {
                _audioSource[(int)audioClipsSources.Thrust].Stop();
            }

            _hasThrust = value;
        }
    }

    public bool HasAngularVelocity
    {
        get
        {
            return _hasAngularVelocity;
        }

        set
        {
            _hasAngularVelocity = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponents<AudioSource>();
        GetAnimators();
        _hyperSpace = GetComponent<HyperSpace>();
        _energyFuel = GetComponent<EnergyFuel>();
    }

    private void Start()
    {
        _dirThrust = 1;
        _dirAngularVelocity = 1;
        _prevIncrementAngularVelocity = 0f;
        _lastMissileRound = Time.time;
        _reloadMissiles = false;
    }

    private void Update()
    {
        
        if (_hyperSpace.IsInHyperSpace)
            return;

        // Read Inputs
        // Thrust
        if(GameManager.Instance.CurrGameState == GameManager.GameState.InGame)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                HasThrust = true;
                _dirThrust = 1;
            }
            else if (HasThrust && Input.GetKeyUp(KeyCode.UpArrow))
            {
                HasThrust = false;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                HasThrust = true;
                _dirThrust = -1;
            }
            else if (HasThrust && Input.GetKeyUp(KeyCode.DownArrow))
            {
                HasThrust = false;
            }

            // Angular Velocity
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                HasAngularVelocity = true;
                _dirAngularVelocity = 1;
            }
            else if (HasAngularVelocity && Input.GetKeyUp(KeyCode.LeftArrow))
            {
                HasAngularVelocity = false;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                HasAngularVelocity = true;
                _dirAngularVelocity = -1;
            }
            else if (HasAngularVelocity && Input.GetKeyUp(KeyCode.RightArrow))
            {
                HasAngularVelocity = false;
            }

            // Fire missiles
            if (Input.GetKey(KeyCode.Space))
            {
                FireMissile();
            }

            /*
            if (_currMissiles == 0 && _lastMissileRound + _timeReloadMissiles <= Time.time && _reloadMissiles)
            {
                _currMissiles = _maxRoundMissile;
                _reloadMissiles = false;
            }

            // Reload just once
            if (Input.GetKeyUp(KeyCode.Space) && _currMissiles == 0 && !_reloadMissiles)
            {
                _lastMissileRound = Time.time;
                _reloadMissiles = true;
                _audioSource[(int)audioClipsSources.Effects].PlayOneShot(_reloadAudioClip);
            }
            */

            // Apply animatons
            _animatorShip.SetFloat("rotation", rigidbody2DComponent.angularVelocity / _maxAngularVelocity);

            float direction = Vector2.Dot(rigidbody2DComponent.velocity.normalized, gameObject.transform.up);
            _animatorShip.SetFloat("motion", direction * rigidbody2DComponent.velocity.magnitude / maxSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (IsActive() && HasThrust)
        {
            Thrust(_dirThrust);
        }
        else
        {
            ReduceThrust();
        }

        if(IsActive() && HasAngularVelocity)
        {
            AddAngularVelocity(_dirAngularVelocity);
        }
        else
        {
            ReduceAngularVelocity();
        }
    }

    /**
     * Gets the animators related to the player
     */
    private void GetAnimators()
    {
        GameObject ship = GameObject.Find("Spaceship_Master").gameObject;
        _animatorShip = ship.GetComponent<Animator>();

        GameObject cannon = GameObject.Find("Cannon").gameObject;
        _animatorCannon = cannon.GetComponent<Animator>();

        GameObject camera = GameObject.Find("Camera_container").gameObject;
        _animatorCamera = camera.GetComponent<Animator>();
    }


    /**
     * Adds thurst to player
     */
    private void Thrust(int dir)
    {
   
        if (rigidbody2DComponent.velocity.magnitude > maxSpeed)
        {
            rigidbody2DComponent.velocity = rigidbody2DComponent.velocity.normalized * maxSpeed;
        }
        else
        {
            rigidbody2DComponent.AddForce(dir * transform.up * _thrust);
        }
    }

    /**
     * Reduces gradually the player's velocity
     */
    private void ReduceThrust()
    {
        if (rigidbody2DComponent.velocity.magnitude > 0)
        {
            rigidbody2DComponent.velocity *= 1f - _thrustLossRatio;
        }
    }

    /**
     * Turns player's direction by adding torque
     */
    private void AddAngularVelocity(int dir)
    {

        if (rigidbody2DComponent.angularVelocity > _maxAngularVelocity)
        {
            rigidbody2DComponent.angularVelocity = _maxAngularVelocity;
            _prevIncrementAngularVelocity = 0f;
        }
        else if (rigidbody2DComponent.angularVelocity < -_maxAngularVelocity)
        {
            rigidbody2DComponent.angularVelocity = -_maxAngularVelocity;
            _prevIncrementAngularVelocity = 0f;
        }
        else
        {
            float angularVelocity = dir * Time.deltaTime * _turnThrust;
            
            //To avoid sudden change of movement
            if ((_prevIncrementAngularVelocity < 0 && dir > 0) 
                || (_prevIncrementAngularVelocity > 0 && dir < 0))
            {
                float increment = Mathf.Abs(angularVelocity - _prevIncrementAngularVelocity);
                angularVelocity = dir * Mathf.Min(increment, _maxIncrementAngularVelocity);
            }

            rigidbody2DComponent.AddTorque(angularVelocity);

            _prevIncrementAngularVelocity = angularVelocity;
        }
    }

    /**
     * Reduces the player's angular velocity 
     */
    private void ReduceAngularVelocity()
    {
        if (rigidbody2DComponent.angularVelocity != 0)
        {
            _prevIncrementAngularVelocity = rigidbody2DComponent.angularVelocity * (1f - _angularVelocityLossRatio);
            rigidbody2DComponent.angularVelocity *= 1f - _angularVelocityLossRatio;

        }
    }

    private void FireMissile()
    {
        /*
        if (IsActive() && lastMissileShot + delayBetweenMissiles <= Time.time && _currMissiles > 0)
        {
            lastMissileShot = Time.time;
            Missile missile = MissileManager.Instance.GetMissile(FlyingObjType);

            missile.Activate(weaponTransform.position, transform.rotation, transform.up);
            ReduceMissiles();
            _animatorCannon.SetBool("shoot", true);
        }
        */
        Missile missile = MissileManager.Instance.GetMissile(FlyingObjType);
        if (IsActive() && lastMissileShot + delayBetweenMissiles <= Time.time && _energyFuel.CanPerformAction(missile.EnergyCost))
        {
            lastMissileShot = Time.time;
            _energyFuel.PerformAction(missile.EnergyCost);
            missile.Activate(weaponTransform.position, transform.rotation, transform.up);
            _animatorCannon.SetBool("shoot", true);
        }
    }

    public bool IsIndistructable()
    {
        return _indestructibleUntil > Time.time;
    }

    /**
     * Reduces player's life by amount
     */
    public void ReduceLife(int life = 1)
    {
        SetLife(_life - life);
    }

    /**
    * Sets player's life and updates UI
    */
    private void SetLife(int life)
    {
        _life = life;

        UIManager.Instance.GetPanel<IngamePanel>(Panel.PanelState.Ingame).SetLife(_life);
    }


    public override void Destroy(bool addScore = false)
    {
        if (!IsIndistructable())
        {
            AudioManager.Instance.Play(destructionAudioClip);

            ReduceLife();

            if (_life == 0)
            {
                GameManager.Instance.GameOver();
                Deactivate();
                
            }
            else
            {
                _indestructibleUntil = Time.time + _indestructibleTime;
                rigidbody2DComponent.velocity = Vector2.zero;
                ActivateShield(_indestructibleTime);
                HasThrust = false;

                _animatorShip.SetTrigger("impact");
                _animatorCamera.SetTrigger("impact");
                Invoke("RemoveIndistructable", _indestructibleTime);
            }
        }
    }


    /**
	 * Activates the player
	 */
    public void Activate()
    {
        gameObject.SetActive(true);
        Reset();
    }

    public override void Deactivate()
    {
        // TODO
        GetComponent<Collider2D>().enabled = false;
        _animatorShip.SetTrigger("destroyed");
        _animatorCamera.SetTrigger("destroyed");
    }


    /**
	 * Removes the indistructable effect
	 */
    private void RemoveIndistructable()
    {
        _animatorShip.SetBool("Indistructable", false);
    }


    /**
	 * Resets the player object
	 */
    private void Reset()
    {
        GameManager.Instance.SetScore(0);
        SetLife(_maxLife);

        _indestructibleUntil = 0f;
        transform.position = Vector3.zero;
        rigidbody2DComponent.velocity = Vector2.zero;
    }

    /**
     * Returns player's velocity
     */
     public Vector2 Velocity()
    {
        return rigidbody2DComponent.velocity;
    }

    /**
     * Activates the shield
     */
    public void ActivateShield(float duration)
    {
        _shield.SetActive(true);
        _indestructibleUntil = Time.time + duration;
        Invoke("DeactivateShield", duration);
    }

    /**
     * Deactives the shield
     */
    public void DeactivateShield()
    {
        _shield.SetActive(false);
    }

    public void EnergyBoost(float rechargeRate, float duration)
    {
        _energyFuel.EnergyPowerUp(rechargeRate, duration);
    }
}
