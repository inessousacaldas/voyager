using UnityEngine;

/// <summary>
/// Class to handle the player's hyperspace mode
/// </summary>
public class HyperSpace : MonoBehaviour {

    [SerializeField]
    private float _timeUntilNextHyperSpace = 5f;
    [SerializeField]
    private float _timeInHyperSpace = 1f;
    private float _lastHyperSpace;

    private Collider2D[] _colliders2D;
    private Animator _animatorShip;

    public bool IsInHyperSpace { get; set; }

    private void Start()
    {
        _colliders2D = GetComponents<Collider2D>();
        _lastHyperSpace = Time.time;
        IsInHyperSpace = false;
        GameObject ship = GameObject.Find("Spaceship_Master").gameObject;
        _animatorShip = ship.GetComponent<Animator>();
    }

    private void Update () {

        if(Input.GetKeyDown(KeyCode.Q) && _lastHyperSpace < Time.time)
        {
            EnterHyperSpaceMode();
            Invoke("LeaveHyperSpaceMode", _timeInHyperSpace);
        }
		
	}
    
    /// <summary>
    /// The player enters in hyperspace mode
    /// </summary>
    private void EnterHyperSpaceMode()
    {
        _lastHyperSpace = Time.time + _timeUntilNextHyperSpace + _timeInHyperSpace;
        IsInHyperSpace = true;
        _animatorShip.SetTrigger("hiperspace_ON");
        HyperSpaceMode();
    }

    /// <summary>
    /// The player leaves the hyperspace mode
    /// </summary>
    private void LeaveHyperSpaceMode ()
    {
        HyperSpaceNewLocation();
        IsInHyperSpace = false;
        _animatorShip.SetTrigger("hiperspace_OFF");
        HyperSpaceMode();
    }

    /// <summary>
    /// Generates and sets the new player's position after he leaves the hyperspace mode
    /// </summary>
    private void HyperSpaceNewLocation()
    {
        Vector2 newPosition = GameManager.Instance.SpawnInsideViewport(false);
        gameObject.transform.position = newPosition;
    }

    /// <summary>
    /// Applies the necessaries changes to the player's gameobject in/out of hyperspace mode
    /// </summary>
    private void HyperSpaceMode ()
    {
        foreach (Collider2D col in _colliders2D)
        {
            col.enabled = !IsInHyperSpace;
        }
    }
}
