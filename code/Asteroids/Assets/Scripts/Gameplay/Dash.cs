using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour {

    [SerializeField]
    private float _speed = 1f;
    private Rigidbody2D _rigidbody2DComponent;

    private Animator _animator;

    private float _doubleTapCooler = 0.5f;
    private float _doubleTapTime;
    private int _tapCount;
    private KeyCode _keyTap;



    void Start()
    {
        _rigidbody2DComponent = GetComponent<Rigidbody2D>();

        GameObject ship = GameObject.Find("Spaceship_Master").gameObject;
        _animator = ship.GetComponent<Animator>();

        // For double input
        _doubleTapTime = _doubleTapCooler;
        _tapCount = 0;
    }

    void Update()
    {
        if(GameManager.Instance.CurrGameState == GameManager.GameState.InGame)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {


                if (_doubleTapTime > 0 && _tapCount == 1 && _keyTap == KeyCode.UpArrow)
                {
                    _tapCount = 0;
                    DashMovement(transform.up);
                    _animator.SetTrigger("spin_front");
                }
                else
                {
                    _keyTap = KeyCode.UpArrow;
                    _doubleTapTime = _doubleTapCooler;
                    _tapCount += 1;
                }
            }

            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {

                if (_doubleTapTime > 0 && _tapCount == 1 && _keyTap == KeyCode.DownArrow)
                {
                    _tapCount = 0;
                    DashMovement(-transform.up);
                    _animator.SetTrigger("spin_back");
                }
                else
                {
                    _keyTap = KeyCode.DownArrow;
                    _doubleTapTime = _doubleTapCooler;
                    _tapCount += 1;
                }
            }

            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {

                if (_doubleTapTime > 0 && _tapCount == 1 && _keyTap == KeyCode.RightArrow)
                {
                    _tapCount = 0;
                    DashMovement(transform.right);
                    _animator.SetTrigger("spin_right");
                }
                else
                {
                    _keyTap = KeyCode.RightArrow;
                    _doubleTapTime = _doubleTapCooler;
                    _tapCount += 1;
                }
            }

            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {

                if (_doubleTapTime > 0 && _tapCount == 1 && _keyTap == KeyCode.LeftArrow)
                {
                    _tapCount = 0;
                    DashMovement(-transform.right);
                    _animator.SetTrigger("spin_left");
                }
                else
                {
                    _keyTap = KeyCode.LeftArrow;
                    _doubleTapTime = _doubleTapCooler;
                    _tapCount += 1;
                }
            }

            if (_doubleTapTime > 0)
            {
                _doubleTapTime -= 1 * Time.deltaTime;
            }
            else
            {
                _doubleTapTime = _doubleTapCooler;
                _tapCount = 0;
            }
            if (_tapCount > 1)
            {
                _tapCount = 0;
            }
        } 
    }

    private void DashMovement(Vector2 direction)
    {
        _rigidbody2DComponent.AddForce(_speed*direction, ForceMode2D.Impulse);
    }

    


}
