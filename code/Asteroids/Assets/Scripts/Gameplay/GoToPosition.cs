using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToPosition : MonoBehaviour {

    [SerializeField]
    private Vector3 _target;
    [SerializeField]
    private float _timeToReachTarget;

    private float _deltaTime;
    private Vector3 _startPosition;

    public bool _move;

    public bool Move
    {
        get
        {
            return _move;
        }

        set
        {
            _move = value;
        }
    }

    private void Start()
    {
        _startPosition = _target = transform.position;
    }

    void Update()
    {
        _deltaTime += Time.deltaTime / _timeToReachTarget;
        transform.position = Vector3.Lerp(_startPosition, _target, _deltaTime);
    }

    public void LockTarget(Vector3 destination)
    {
        _deltaTime = 0;
        _startPosition = transform.position;
        _target = destination;
    }
}
