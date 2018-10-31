using UnityEngine;
using System.Collections;

/// <summary>
/// An alien which extends the character class and extends it with an AI
/// </summary>
public class Alien : Character
{

    public float minSpeed;


    public bool test = false;

    /**
	 * The timer when the velocity of the alien will change automaticly
	 */
    public float velocityChangeTimer = 5f;


    /**
	 * The aliens spawn audio clip
	 */
    public AudioClip spawnAudioClip;


    /**
	 * Fires missiles in the players direction if possible
	 */
    private void Update()
    {
        if (test)
        {
            test = false;
            Activate(new Vector2(-10, -10), new Vector2(1,1));
        }
        /*if (IsActive() && CanFireMissile())
        {
            var playerPosition = GameManager.Instance.player.transform.position;
            var alienPosition = weaponTransform.position;

            playerPosition.x -= alienPosition.x;
            playerPosition.y -= alienPosition.y;
            var angle = (Mathf.Atan2(playerPosition.y, playerPosition.x) * Mathf.Rad2Deg) - 90f;
            var quaternion = Quaternion.Euler(new Vector3(0f, 0f, angle));

            FireMissile(alienPosition, quaternion, playerPosition.normalized);
        }*/
    }


    /**
	 * Activates the alien
	 */
    public override void Activate(Vector2 position, Vector2 velocity)
    {
        
        var speed = Random.Range(minSpeed, maxSpeed);
        gameObject.transform.position = position;
        lastSeenInViewport = Time.time; // Ensures that last seen in viewport will work correctly
        lastMissileShot = Time.time; // Ensures that the alien will fire after a short delay

        AudioManager.Instance.Play(spawnAudioClip);
        gameObject.SetActive(true);

        rigidbody2DComponent.velocity = velocity * speed;

        Invoke("ChangeVelocityRandomly", velocityChangeTimer);
        
    }


    /**
	 * Deactivates the alien if he is too long outside of the viewport
	 */
    protected override void TooLongOutsideOfViewport()
    {
        Debug.Log("TooLongOutsideOfViewport!");
        Deactivate();
    }


    /**
	 * Changes the velocity of the alien randomly
	 */
    private void ChangeVelocityRandomly()
    {
        var speed = Random.Range(minSpeed, maxSpeed);
        rigidbody2DComponent.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * speed;

        Invoke("ChangeVelocityRandomly", velocityChangeTimer);
    }
}