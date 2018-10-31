using System;
using UnityEngine;

/// <summary>
/// Vehicle subclass which automatically applies the steering forces from
/// the components attached to the object.  AutonomousVehicle is characterized
/// for the vehicle always moving in the same direction as its forward vector
/// </summary>
public class Test : Vehicle2D
{

    private float _speed;

    /// <summary>
    /// How often will this Vehicle's steering calculations be ticked.
    /// </summary>
    [SerializeField] private float _tickLength = 0.1f;

    /// <summary>
    /// The maximum number of radar update calls processed on the queue per update
    /// </summary>
    /// <remarks>
    /// Notice that this is a limit shared across queue items of the same name, at
    /// least until we have some queue settings, so whatever value is set last for 
    /// the queue will win.  Make sure your settings are consistent for objects of
    /// the same queue.
    /// </remarks>
    [SerializeField] private int _maxQueueProcessedPerUpdate = 20;


    /// <summary>
    /// Acceleration rate - it'll be used as a multiplier for the speed
    /// at which the velocity is interpolated when accelerating. A rate
    /// of 1 means that we interpolate across 1 second; a rate of 5 means
    /// we do it five times as fast.
    /// </summary>
    [SerializeField] private float _accelerationRate = 5;

    /// <summary>
    /// Deceleration rate - it'll be used as a multiplier for the speed
    /// at which the velocity is interpolated when decelerating. A rate
    /// of 1 means that we interpolate across 1 second; a rate of 5 means
    /// we do it five times as fast.
    /// </summary>
    [SerializeField] private float _decelerationRate = 8;


    /// <summary>
    /// Current vehicle speed
    /// </summary>
    public override float Speed
    {
        get { return _speed; }
    }

    /// <summary>
    /// Current vehicle velocity
    /// </summary>
    public override Vector2 Velocity
    {
        get { return Forward * Speed; }
        protected set { throw new NotSupportedException("Cannot set the velocity directly on AutonomousVehicle2D"); }
    }

    /// <summary>
    /// Last time the vehicle's tick was completed.
    /// </summary>
    /// <value>The last tick time.</value>
    public float PreviousTickTime { get; private set; }


    /// <summary>
    /// Current time that the tick was called.
    /// </summary>
    /// <value>The current tick time.</value>
    public float CurrentTickTime { get; private set; }

    /// <summary>
    /// The time delta between now and when the vehicle's previous tick time and the current one.
    /// </summary>
    /// <value>The delta time.</value>
    public override float DeltaTime
    {
        get { return CurrentTickTime - PreviousTickTime; }
    }

    /// <summary>
    /// Velocity vector used to orient the agent.
    /// </summary>
    /// <remarks>
    /// This is expected to be set by the subclasses.
    /// </remarks>
    public Vector2 OrientationVelocity { get; protected set; }


    private void Start()
    {
        PreviousTickTime = Time.time;
        OnEnable();
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        CanMove = true;
        /*
        TickedObject = new TickedObject(OnUpdateSteering);
        TickedObject.TickLength = _tickLength;
        _steeringQueue = UnityTickedQueue.GetInstance(QueueName);
        _steeringQueue.Add(TickedObject);
        _steeringQueue.MaxProcessedPerUpdate = _maxQueueProcessedPerUpdate;
        */
    }

    protected override void OnDisable()
    {
        //DeQueue();
        base.OnDisable();
    }

    /*
    private void DeQueue()
    {
        if (_steeringQueue != null)
        {
            _steeringQueue.Remove(TickedObject);
        }
    }
    */

    public void Update()
    {
        OnUpdateSteering(null);

        if (CanMove)
        {
            ApplySteeringForce(Time.deltaTime);
            AdjustOrientation(Time.deltaTime);
        }

    }

    protected void OnUpdateSteering(object obj)
    {
        if (enabled)
        {
            // We just calculate the forces, and expect the radar updates itself.
            CalculateForces();
        }
        else
        {
            /*
         * This is an interesting edge case.
         * 
         * Because of the way TickedQueue iterates through its items, we may have
         * a case where:
         * - The vehicle's OnUpdateSteering is enqueued into the work queue
         * - An event previous to it being called causes it to be disabled, and de-queued
         * - When the ticked queue gets to it, it executes and re-enqueues it
         * 
         * Therefore we double check that we're not trying to tick it while disabled, and 
         * if so we de-queue it.  Must review TickedQueue to see if there's a way we can 
         * easily handle these sort of issues without a performance hit.
         */
            //DeQueue();
        }
    }


    protected void CalculateForces()
    {
        PreviousTickTime = CurrentTickTime;
        CurrentTickTime = Time.time;

        if (!CanMove || Mathf.Approximately(MaxForce, 0) || Mathf.Approximately(MaxSpeed, 0))
        {
            return;
        }

        var force = Vector2.zero;

        for (var i = 0; i < Steerings.Length; i++)
        {
            var s = Steerings[i];
            if (s.enabled)
            {
                //Cast to make sure everything fits nicely
                force += s.WeighedForce;
            }
        }

        LastRawForce = force;

        // Enforce speed limit.  Steering behaviors are expected to return a
        // final desired velocity, not a acceleration, so we apply them directly.
        var newVelocity = Vector2.ClampMagnitude(force / Mass, MaxForce);

        if (newVelocity.sqrMagnitude == 0)
        {
            ZeroVelocity();
            DesiredVelocity = Vector2.zero;
        }
        else
        {
            DesiredVelocity = newVelocity;
        }

        // Adjusts the velocity by applying the post-processing behaviors.
        //
        // This currently is not also considering the maximum force, nor 
        // blending the new velocity into an accumulator. We *could* do that,
        // but things are working just fine for now, and it seems like
        // overkill. 
        var adjustedVelocity = Vector2.zero;

        for (var i = 0; i < SteeringPostprocessors.Length; i++)
        {
            var s = SteeringPostprocessors[i];
            if (s.enabled)
            {
                //Cast to make sure everyone works together
                adjustedVelocity += s.WeighedForce;
            }
        }


        if (adjustedVelocity != Vector2.zero)
        {
            adjustedVelocity = Vector2.ClampMagnitude(adjustedVelocity, MaxSpeed);

            newVelocity = adjustedVelocity;
        }

        // Update vehicle velocity
        SetCalculatedVelocity(newVelocity);

    }


    /// <summary>
    /// Applies a steering force to this vehicle
    /// </summary>
    /// <param name="elapsedTime">
    /// How long has elapsed since the last update<see cref="System.Single"/>
    /// </param>
    private void ApplySteeringForce(float elapsedTime)
    {
        // Euler integrate (per frame) velocity into position
        var acceleration = CalculatePositionDelta(elapsedTime);
        acceleration = Vector2.Scale(acceleration, AllowedMovementAxes);

        if (Rigidbody == null || Rigidbody.isKinematic)
        {
            Transform.position += (Vector3)acceleration;
        }
        else
        {
            //Cast to make sure the rigidbody doesn't die on switch
            Rigidbody.MovePosition(Rigidbody.position + acceleration);
        }
    }


    /// <summary>
    /// Turns the vehicle towards his velocity vector. Previously called
    /// LookTowardsVelocity.
    /// </summary>
    /// <param name="deltaTime">Time delta to use for turn calculations</param>
    protected void AdjustOrientation(float deltaTime)
    {
     /* 
     * Avoid adjusting if we aren't applying any velocity. We also
     * disregard very small velocities, to avoid jittery movement on
     * rounding errors.
     */
        if (TargetSpeed > MinSpeedForTurning && Velocity != Vector2.zero)
        {
            var newForward = Vector2.Scale(OrientationVelocity, AllowedMovementAxes).normalized; //TODO need to check how this does in 2d, may need to change to quaternion
            if (TurnTime > 0)
            {
                newForward = Vector3.Slerp(Forward, newForward, deltaTime / TurnTime);
            }

            Forward = newForward;
        }
    }

    /// <summary>
    /// Uses a desired velocity vector to adjust the vehicle's target speed and 
    /// orientation velocity.
    /// </summary>
    /// <param name="velocity">Newly calculated velocity</param>
    protected void SetCalculatedVelocity(Vector2 velocity)
    {
        TargetSpeed = velocity.magnitude;
        //More casts to make sure the if statement goes nicely
        OrientationVelocity = Mathf.Approximately(_speed, 0) ? Forward : velocity / TargetSpeed;
    }

    /// <summary>
    /// Calculates how much the agent's position should change in a manner that
    /// is specific to the vehicle's implementation.
    /// </summary>
    /// <param name="deltaTime">Time delta to use in position calculations</param>
    protected Vector2 CalculatePositionDelta(float deltaTime)
    {
        /*
		* Notice that we clamp the target speed and not the speed itself, 
		* because the vehicle's maximum speed might just have been lowered
		* and we don't want its actual speed to suddenly drop.
		*/
        var targetSpeed = Mathf.Clamp(TargetSpeed, 0, MaxSpeed);
        if (Mathf.Approximately(_speed, targetSpeed))
        {
            _speed = targetSpeed;
        }
        else
        {
            var rate = TargetSpeed > _speed ? _accelerationRate : _decelerationRate;
            _speed = Mathf.Lerp(_speed, targetSpeed, deltaTime * rate);
        }

        return Velocity * deltaTime;
    }

    /// <summary>
    /// Zeros this vehicle's target speed, which results on its desired velocity
    /// being zero.
    /// </summary>
    protected void ZeroVelocity()
    {
        TargetSpeed = 0;
    }

    public void Stop()
    {
        CanMove = false;
        ZeroVelocity();
    }

}