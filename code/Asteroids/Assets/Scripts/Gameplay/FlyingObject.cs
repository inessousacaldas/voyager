using UnityEngine;


public abstract class FlyingObject : MonoBehaviour
{
    public enum FlyingObjectType
    {
        Player = 0,
        Alien = 1,
        Asteroid = 2,
        Collectable = 3
    }

    [SerializeField]
    protected FlyingObjectType flyingObjType;
    //TODO REMOVE
    [SerializeField]
    protected int score = 0;
    [SerializeField]
    protected float maxSpeed = 1f;

    [SerializeField]
    protected AudioClip destructionAudioClip;

    protected Rigidbody2D rigidbody2DComponent;
    protected float lastSeenInViewport;

    protected Renderer rendererObj;

    public FlyingObjectType FlyingObjType
    {
        get
        {
            return flyingObjType;
        }

        set
        {
            flyingObjType = value;
        }
    }

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;
        }
    }

    protected virtual void Awake()
    {
        rigidbody2DComponent = GetComponent<Rigidbody2D>();

        Invoke("VerifyIsInViewport", 1f);
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public virtual void HitByCollider(Collider2D collider)
    {
        if (collider.CompareTag(Config.Tags.missile))
        {
            var missile = collider.GetComponent<Missile>();

            if (missile.OwnedBy != FlyingObjType)
            {
                bool addScore = (missile.OwnedBy == FlyingObject.FlyingObjectType.Player);

                missile.Deactivate();

                Destroy(addScore);
            }
        }
    }

    protected abstract void TooLongOutsideOfViewport();

    public abstract void Destroy(bool addScore);

    public abstract void Activate(Vector2 position, Vector2 velocity);

    public abstract void Deactivate();

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        HitByCollider(collider);
    }

    /**
     * Screen warp if object leaves viewport 
     */
    public virtual void OnLeaveViewport()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 position = transform.position;

        if (Screen.width < screenPosition.x || screenPosition.x < 0)
        {
            position.x *= -1;
        }
        if (Screen.height < screenPosition.y || screenPosition.y < 0)
        {
            position.y *= -1;
        }

        transform.position = position;
    }

    /**
    * Verifies that the flying object is in the viewport after a teleport
    */
    private void VerifyIsInViewport()
    {
        if (IsActive())
        {
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, -Vector2.up, Mathf.Infinity, Config.Layer.viewport);
            if (hit.collider != null && hit.collider.CompareTag(Config.Tags.viewport))
            {
                lastSeenInViewport = Time.time;
            }
            else if (this.lastSeenInViewport + 5f < Time.time)
            {
                TooLongOutsideOfViewport();
            }
        }

        Invoke("VerifyIsInViewport", 1f);
    }
}