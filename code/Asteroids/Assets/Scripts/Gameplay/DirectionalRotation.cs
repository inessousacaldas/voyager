using UnityEngine;


public class DirectionalRotation : MonoBehaviour {


    [SerializeField]
    private Vector3 _direction;
    [SerializeField]
    private float _speed = 0f;

    /**
     * Direction of the rotation (not normalized -> for different speeds in different axis)
     */
    public Vector3 Direction
    {
        get
        {
            return _direction;
        }

        set
        {
            _direction = value;
        }
    }

    /**
     * Speed of the rotation (Speed x direction)
     */
    public float Speed
    {
        get
        {
            return _speed;
        }

        set
        {
            _speed = value;
        }
    }

    void Update () {
        
        transform.Rotate(Speed * Direction * Time.deltaTime);

    }
}
