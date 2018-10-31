using System.Linq;
using UnityEngine;


    public abstract class Vehicle2D : DetectableObject2D
{
    [SerializeField]
    private float _minSpeedForTurning = 0.1f;

    [SerializeField]
    private float _turnTime = 0.25f;

    [SerializeField]
    private float _mass = 1;

    [SerializeField]
    private Vector2 _allowedMovementAxes = Vector2.one;

    /// <summary>
    /// The vehicle's arrival radius.
    /// </summary>
    /// <remarks>The difference between the radius and arrival radius is that
    /// the first is used to determine the area the vehicle covers, whereas the
    /// second one is a value used to determine if a vehicle is close enough
    /// to a desired target.  Unlike the radius, it is not scaled with the vehicle.</remarks>
    [SerializeField]
    private float _arrivalRadius = 0.25f;

    [SerializeField]
    private float _maxSpeed = 1;

    [SerializeField]
    private float _maxForce = 10;

    [SerializeField]
    private bool _canMove = true;

    public Vector2 Forward; //TODO

    public Vector2 AllowedMovementAxes
    {
        get { return _allowedMovementAxes; }
    }

    public bool CanMove
    {
        get { return _canMove; }
        set { _canMove = value; }
    }

    public Vector2 DesiredVelocity { get; protected set; }

    public GameObject GameObject { get; private set; }

    public float Mass
    {
        get { return _mass; }
        set { _mass = Mathf.Max(0, value); }
    }

    public float MaxForce
    {
        get { return _maxForce; }
        set { _maxForce = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    /// <summary>
    /// The vehicle's maximum speed
    /// </summary>
    public float MaxSpeed
    {
        get { return _maxSpeed; }
        set { _maxSpeed = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    /// <summary>
    /// Minimum speed necessary for ths vehicle to apply a turn
    /// </summary>
    public float MinSpeedForTurning
    {
        get { return _minSpeedForTurning; }
    }

    /// <summary>
    /// Radar assigned to this vehicle
    /// </summary>
    public Radar2D Radar { get; private set; }

    public Rigidbody2D Rigidbody { get; private set; }

    /// <summary>
    /// Vehicle arrival radius
    /// </summary>
    public float ArrivalRadius
    {
        get { return _arrivalRadius; }
        set
        {
            _arrivalRadius = Mathf.Clamp(value, 0.01f, float.MaxValue);
            SquaredArrivalRadius = _arrivalRadius * _arrivalRadius;
        }
    }

    /// <summary>
    /// Squared arrival radius, for performance purposes
    /// </summary>
    public float SquaredArrivalRadius { get; private set; }

    /// <summary>
    /// Last raw force applied to the vehicle. It is expected to be set 
    /// by the subclases.
    /// </summary>
    public Vector2 LastRawForce { get; protected set; }

    /// <summary>
    /// Current vehicle speed
    /// </summary>
    public abstract float Speed { get; }

    /// <summary>
    /// Across how many seconds is the vehicle's forward orientation smoothed.
    /// </summary>
    /// <value>
    /// The turn speed
    /// </value>
    public float TurnTime
    {
        get { return _turnTime; }
        set { _turnTime = Mathf.Max(0, value); }
    }

    /// <summary>
    /// Array of steering behaviors
    /// </summary>
    public Steering2D[] Steerings { get; private set; }

    /// <summary>
    /// Array of steering post-processor behaviors
    /// </summary>
    public Steering2D[] SteeringPostprocessors { get; private set; }

    /// <summary>
    /// Current vehicle velocity. Subclasses are likely to only actually
    /// implement one of the two methods.
    /// </summary>
    public abstract Vector2 Velocity { get; protected set; }

    /// <summary>
    /// Current magnitude for the vehicle's velocity.
    /// </summary>
    /// <remarks>
    /// It is expected to be set at the same time that the Velocity is 
    /// assigned in one of the descendent classes.  It may or may not
    /// match the vehicle speed, depending on how that is calculated - 
    /// for example, some subclasses can use a Speedometer to calculate
    /// their speed.
    /// </remarks>
    public float TargetSpeed { get; protected set; }

    /// <summary>
    /// The delta time used by this vehicle.
    /// </summary>
    /// <value>The delta time.</value>
    /// <remarks>
    /// Vehicles aren't necessarily ticked every frame, so we keep a
    /// DeltaTime property that steering behaviors can access when
    /// their CalculateForce is called.
    /// </remarks>
    public virtual float DeltaTime
    {
        get { return Time.deltaTime; }
    }


    protected override void Awake()
    {
        base.Awake();
        GameObject = gameObject;
        Rigidbody = GetComponent<Rigidbody2D>();
        var allSteerings = GetComponents<Steering2D>();
        Steerings = allSteerings.Where(x => !x.IsPostProcess).ToArray();
        SteeringPostprocessors = allSteerings.Where(x => x.IsPostProcess).ToArray();


        Radar = GetComponent<Radar2D>();
  
        SquaredArrivalRadius = ArrivalRadius * ArrivalRadius;

        Forward = (Vector2)transform.right;
    }

    /// <summary>
    /// Predicts where the vehicle wants to be at a point in the future
    /// </summary>
    /// <param name="predictionTime">
    /// A time in seconds for the prediction <see cref="System.Single"/>
    /// </param>
    /// <returns>
    /// Vehicle position<see cref="Vector2"/>
    /// </returns>
    public Vector2 PredictFutureDesiredPosition(float predictionTime)
    {
        return (Vector2)transform.position + (DesiredVelocity * predictionTime);
    }

    /// <summary>
    /// Calculates if a vehicle is in the neighborhood of another
    /// </summary>
    /// <param name="other">
    /// Another vehicle to check against<see cref="Vehicle"/>
    /// </param>
    /// <param name="minDistance">
    /// Minimum distance <see cref="System.Single"/>
    /// </param>
    /// <param name="maxDistance">
    /// Maximum distance <see cref="System.Single"/>
    /// </param>
    /// <param name="cosMaxAngle">
    /// Cosine of the maximum angle between vehicles (for performance)<see cref="System.Single"/>
    /// </param>
    /// <returns>
    /// True if the other vehicle can be considered to our neighbor, or false if otherwise<see cref="System.Boolean"/>
    /// </returns>
    /// <remarks>Originally SteerLibrary.inBoidNeighborhood</remarks>
    public bool IsInNeighborhood(Vehicle2D other, float minDistance, float maxDistance, float cosMaxAngle)
    {
        var result = false;
        if (other != this)
        {
            var offset = other.Position - (Vector2)transform.position ;
            var distanceSquared = offset.sqrMagnitude;

            // definitely in neighborhood if inside minDistance sphere
            if (distanceSquared < (minDistance * minDistance))
            {
                result = true;
            }
            else
            {
                // definitely not in neighborhood if outside maxDistance sphere
                if (distanceSquared <= (maxDistance * maxDistance))
                {
                    // otherwise, test angular offset from forward axis
                    var unitOffset = offset / Mathf.Sqrt(distanceSquared);
                    var forwardness = Vector2.Dot(Forward, unitOffset);
                    result = forwardness > cosMaxAngle;
                }
            }
        }
        return result;
    }


    /// <summary>
    /// Returns a vector to seek a target position
    /// </summary>
    /// <param name="target">
    /// Target position <see cref="Vector2"/>
    /// </param>
    /// <param name="considerVelocity">
    /// Should the current velocity be taken into account?
    /// </param>
    /// <returns>
    /// Seek vector <see cref="Vector2"/>
    /// </returns>
    /// <remarks>
    /// If a steering behavior depends on GetSeekVector, passing considerVelocity 
    /// as  true can make the agent wobble as it approaches a target unless its 
    /// force is calculated every frame.
    /// </remarks>
    public Vector2 GetSeekVector(Vector2 target, bool considerVelocity = false)
    {
        /*
	    * First off, we calculate how far we are from the target, If this
	    * distance is smaller than the configured vehicle radius, we tell
	    * the vehicle to stop.
	    */
        var force = Vector2.zero;

        var difference = target - Position;
        var d = difference.sqrMagnitude;
        if (d > SquaredArrivalRadius)
        {
            /*
		    * But suppose we still have some distance to go. The first step
		    * then would be calculating the steering force necessary to orient
		    * ourselves to and walk to that point.
		    * 
		    * It doesn't apply the steering itself, simply returns the value so
		    * we can continue operating on it.
		    */
            force = considerVelocity ? difference - Velocity : difference;
        }
        return force;
    }

    /// <summary>
    /// Returns a returns a maxForce-clipped steering force along the 
    /// up vector that can be used to try to maintain a target speed
    /// </summary>
    /// <returns>
    /// The target speed vector.
    /// </returns>
    /// <param name='targetSpeed'>
    /// Target speed to aim for.
    /// </param>
    public Vector2 GetTargetSpeedVector(float targetSpeed)
    {
        var mf = MaxForce;
        var speedError = targetSpeed - Speed;
        return Forward * Mathf.Clamp(speedError, -mf, +mf);
    }

    /// <summary>
    /// Returns the distance from this vehicle to another
    /// </summary>
    /// <returns>
    /// The distance between both vehicles' positions. If negative, they are overlapping.
    /// </returns>
    /// <param name='other'>
    /// Vehicle to compare against.
    /// </param>
    public float DistanceFromPerimeter(Vehicle2D other)
    {
        var diff = Position - other.Position;
        return diff.magnitude - Radius - other.Radius;
    }

    /// <summary>
    /// Resets the vehicle's orientation.
    /// </summary>
    public void ResetOrientation()
    {
        transform.up = Vector3.up;
        transform.forward = Vector3.forward;
    }

    /// <summary>
    /// Predicts the time until nearest approach between this and another vehicle
    /// </summary>
    /// <returns>
    /// The nearest approach time.
    /// </returns>
    /// <param name='other'>
    /// Other vehicle to compare against
    /// </param>
    public float PredictNearestApproachTime(Vehicle2D other)
    {
        // imagine we are at the origin with no velocity,
        // compute the relative velocity of the other vehicle
        var otherVelocity = other.Velocity;
        var relVelocity = otherVelocity - Velocity;
        var relSpeed = relVelocity.magnitude;

        // for parallel paths, the vehicles will always be at the same distance,
        // so return 0 (aka "now") since "there is no time like the present"
        if (Mathf.Approximately(relSpeed, 0))
        {
            return 0;
        }

        // Now consider the path of the other vehicle in this relative
        // space, a line defined by the relative position and velocity.
        // The distance from the origin (our vehicle) to that line is
        // the nearest approach.

        // Take the unit tangent along the other vehicle's path
        var relTangent = relVelocity / relSpeed;

        // find distance from its path to origin (compute offset from
        // other to us, find length of projection onto path)
        var relPosition = (Vector2)transform.position  - other.Position;
        var projection = Vector2.Dot(relTangent, relPosition);

        return projection / relSpeed;
    }


    /// <summary>
    /// Given the time until nearest approach (predictNearestApproachTime)
    /// determine position of each vehicle at that time, and the distance
    /// between them
    /// </summary>
    /// <returns>
    /// Distance between positions
    /// </returns>
    /// <param name='other'>
    /// Other vehicle to compare against
    /// </param>
    /// <param name='time'>
    /// Time to estimate.
    /// </param>
    /// <param name='ourPosition'>
    /// Our position.
    /// </param>
    /// <param name='hisPosition'>
    /// The other vehicle's position.
    /// </param>
    public float ComputeNearestApproachPositions(Vehicle2D other, float time,
        ref Vector2 ourPosition,
        ref Vector2 hisPosition)
    {
        return ComputeNearestApproachPositions(other, time, ref ourPosition, ref hisPosition, Speed,
            Forward);
    }

    /// <summary>
    /// Given the time until nearest approach (predictNearestApproachTime)
    /// determine position of each vehicle at that time, and the distance
    /// between them
    /// </summary>
    /// <returns>
    /// Distance between positions
    /// </returns>
    /// <param name='other'>
    /// Other vehicle to compare against
    /// </param>
    /// <param name='time'>
    /// Time to estimate.
    /// </param>
    /// <param name='ourPosition'>
    /// Our position.
    /// </param>
    /// <param name='hisPosition'>
    /// The other vehicle's position.
    /// </param>
    /// <param name="ourSpeed">Our speed to use for the calculations</param>
    /// <param name='ourForward'>
    /// Forward vector to use instead of the vehicle's.
    /// </param>
    public float ComputeNearestApproachPositions(Vehicle2D other, float time,
        ref Vector2 ourPosition,
        ref Vector2 hisPosition,
        float ourSpeed,
        Vector2 ourForward)
    {
        var myTravel = ourForward * ourSpeed * time;
        var otherTravel = other.Forward * other.Speed * time;

        //The casts are to make sure they are both the same even when changing from 2D to 3D.
        ourPosition = (Vector2)transform.position  + myTravel;
        hisPosition = other.Position + otherTravel;

        return Vector2.Distance(ourPosition, hisPosition);
    }
}