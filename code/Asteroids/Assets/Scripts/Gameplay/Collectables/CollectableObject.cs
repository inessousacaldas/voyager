using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollectableObject : FlyingObject
{
    [SerializeField]
    private float _lifeTime = 10f;
    [SerializeField]
    private float _minSpeed;
    [SerializeField]
    private AudioClip _spawnAudioClip;

    public bool start = false;
	
	void Update () {
        //TODO REMOVE
        if (start)
        {
            start = false;
            Activate(transform.position, Vector2.zero);
        }
		
	}

    /**
	 * Deactivates the collectable
	 */
    protected override void TooLongOutsideOfViewport()
    {
        Debug.Log("Collectable TooLongOutsideOfViewport!");
        Deactivate();
    }

    public override void Activate(Vector2 position, Vector2 velocity)
    {
        float speed = Random.Range(_minSpeed, maxSpeed);
        gameObject.transform.position = position;
        lastSeenInViewport = Time.time;

        AudioManager.Instance.Play(_spawnAudioClip);
        gameObject.SetActive(true);

        rigidbody2DComponent.velocity = velocity * speed;

        Invoke("Deactivate", _lifeTime);

    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
        CancelInvoke();
    }

    public override void HitByCollider(Collider2D collider)
    {
        if (collider.CompareTag(Config.Tags.flyingObject))
        {
            FlyingObject flyingObject = collider.GetComponent<FlyingObject>();
            if (flyingObject.FlyingObjType == FlyingObjectType.Player)
            {
                Player player = collider.GetComponent<Player>();
                Effect(player);
                Destroy();
            }

            
        }
    }

    public abstract void Effect(Player player);

    public override void Destroy(bool addScore = false)
    {
        AudioManager.Instance.Play(destructionAudioClip);
        Deactivate();

        if (addScore == true)
        {
            GameManager.Instance.AddScore(Score);
        }
    }
}
