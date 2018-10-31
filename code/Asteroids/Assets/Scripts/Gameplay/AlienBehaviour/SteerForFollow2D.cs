using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerForFollow2D : Steering2D
{
    /// <summary>
    /// Target transform
    /// </summary>
    [SerializeField]
    private Transform _target;

    /// <summary>
    /// Should the vehicle's own velocity be considered in the seek calculations?
    /// </summary>
    /// <remarks>
    /// If true, the vehicle will slow down as it approaches its target
    /// </remarks>
    [SerializeField]
    private bool _considerVelocity = true;

    /// <summary>
    /// How far behind we should follow the target
    /// </summary>
    [SerializeField]
    private Vector2 _distance;

    /// <summary>
    /// The target.
    /// </summary>
    public Transform Target
    {
        get { return _target; }
        set
        {
            _target = value;
            ReportedArrival = false;
        }
    }


    /// <summary>
    /// Should the vehicle's velocity be considered in the seek calculations?
    /// </summary>
    /// <remarks>
    /// If true, the vehicle will slow down as it approaches its target
    /// </remarks>
    public bool ConsiderVelocity
    {
        get { return _considerVelocity; }
        set { _considerVelocity = value; }
    }

    protected override Vector2 CalculateForce()
    {
        return (Target == null)
            ? Vector2.zero
            : Vehicle.GetSeekVector(Target.TransformPoint(_distance), _considerVelocity);
    }
}

