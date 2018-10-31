using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyLayer : MonoBehaviour {

    [SerializeField]
    private float _rotationSpeed = 0.5f;

    private Player _player;

	void Start () {

        _player = FindObjectOfType<Player>();

	}
	
	void Update () {

        RotateSphere();
    }

    /**
     * Rotates layer sphere according to player's movement 
     */
    public void RotateSphere()
    {
        Vector2 playerVelocity = _player.Velocity();
        Vector3 direction = transform.right * playerVelocity.y - transform.up * playerVelocity.x;
        
        transform.Rotate(direction * Time.deltaTime * _rotationSpeed);
    }
}
