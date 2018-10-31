using System;
using System.Collections.Generic;
using UnityEngine;

public class Radar2D : MonoBehaviour
{

    private static IDictionary<int, DetectableObject2D> _knownDetectableObjects =
        new SortedDictionary<int, DetectableObject2D>();

    private Transform _transform;


    [SerializeField] private string _queueName = "Radar2D";

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
    /// How often is the radar updated
    /// </summary>
    [SerializeField] private float _tickLength = 0.5f;

    [SerializeField] private float _detectionRadius = 5;

    [SerializeField] private bool _detectDisabledVehicles;

    [SerializeField] private LayerMask _layersChecked;

    [SerializeField] private int _preAllocateSize = 30;

    private Collider2D[] _detectedColliders;
    private List<DetectableObject2D> _detectedObjects;
    private List<Vehicle2D> _vehicles;
    private List<DetectableObject2D> _obstacles;

    /// <summary>
    /// List of currently detected neighbors
    /// </summary>
    public IEnumerable<Collider2D> Detected
    {
        get { return _detectedColliders; }
    }

    /// <summary>
    /// Radar ping detection radius
    /// </summary>
    public float DetectionRadius
    {
        get { return _detectionRadius; }
        set { _detectionRadius = value; }
    }


    /// <summary>
    /// Indicates if the radar will detect disabled vehicles. 
    /// </summary>
    public bool DetectDisabledVehicles
    {
        get { return _detectDisabledVehicles; }
        set { _detectDisabledVehicles = value; }
    }

    public List<DetectableObject2D> Obstacles
    {
        get { return _obstacles; }
    }

    public Vector2 Position
    {
        get { return (Vehicle != null) ? Vehicle.Position : (Vector2)_transform.position; }
    }

    public Action<Radar2D> OnDetected = delegate { };

    public Vehicle2D Vehicle { get; private set; }

    public List<Vehicle2D> Vehicles
    {
        get { return _vehicles; }
    }

    /// <summary>
    /// Layer mask for the object layers checked
    /// </summary>
    public LayerMask LayersChecked
    {
        get { return _layersChecked; }
        set { _layersChecked = value; }
    }

    public static void AddDetectableObject2D(DetectableObject2D obj)
    {
        _knownDetectableObjects[obj.Collider.GetInstanceID()] = obj;
    }

    public static bool RemoveDetectableObject2D(DetectableObject2D obj)
    {
        return _knownDetectableObjects.Remove(obj.Collider.GetInstanceID());
    }

    protected virtual void Awake()
    {
        Vehicle = GetComponent<Vehicle2D>();
        _transform = transform;
        _vehicles = new List<Vehicle2D>(_preAllocateSize);
        _obstacles = new List<DetectableObject2D>(_preAllocateSize);
        _detectedObjects = new List<DetectableObject2D>(_preAllocateSize * 3);
    }


    private void OnEnable()
    {
        /*_tickedObject = new TickedObject(OnUpdateRadar) { TickLength = _tickLength };
        _steeringQueue = UnityTickedQueue.GetInstance(_queueName);
        _steeringQueue.Add(_tickedObject);
        _steeringQueue.MaxProcessedPerUpdate = _maxQueueProcessedPerUpdate;*/
    }


    private void OnDisable()
    {
        /*
        if (_steeringQueue != null)
        {
            _steeringQueue.Remove(_tickedObject);
        }
        */
    }


    private void OnUpdateRadar()
    {
        _detectedColliders = Detect();
        FilterDetected();
        if (OnDetected != null)
        {
            OnDetected(this);
        }

    }

    public void Update()
    {
        OnUpdateRadar();
    }

    protected virtual Collider2D[] Detect()
    {
        return Physics2D.OverlapCircleAll(Position, DetectionRadius, LayersChecked);
    }

    protected virtual void FilterDetected()
    {

        _vehicles.Clear();
        _obstacles.Clear();
        _detectedObjects.Clear();

        for (var i = 0; i < _detectedColliders.Length; i++)
        {
            print("detected " + _detectedColliders.Length);

            var id = _detectedColliders[i].GetInstanceID();
            if (!_knownDetectableObjects.ContainsKey(id))
                continue; // Ignore anything that hadn't previously registered as a detectable object
            var detectable = _knownDetectableObjects[id];
            // It's possible that d != null but that d.Equals(null) if the
            // game object has been marked as destroyed by Unity between
            // detection and filtering.
            if (detectable != null &&
                detectable != Vehicle &&
                !detectable.Equals(null))
            {
                _detectedObjects.Add(detectable);
            }
        }


        for (var i = 0; i < _detectedObjects.Count; i++)
        {
            var d = _detectedObjects[i];
            var v = d as Vehicle2D;
            if (v != null && (v.enabled || _detectDisabledVehicles))
            {
                _vehicles.Add(v);
            }
            else
            {
                _obstacles.Add(d);
            }
        }
    }


}