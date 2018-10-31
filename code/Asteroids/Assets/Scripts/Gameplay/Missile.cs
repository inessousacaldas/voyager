using UnityEngine;


public class Missile : MonoBehaviour
{
    [SerializeField]
    private float lifetime = 1f;
    [SerializeField]
    private float speed = 500f;
    [SerializeField]
    private LayerMask collisionRaycastLayerMask;
    [SerializeField]
    private FlyingObject.FlyingObjectType ownedBy;
    [SerializeField]
    private float _energyCost = 5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private float despawnTime;
    private Vector3 previousRigidbody2DPosition;

    public FlyingObject.FlyingObjectType OwnedBy
    {
        get
        {
            return ownedBy;
        }

        set
        {
            ownedBy = value;
        }
    }

    public float EnergyCost
    {
        get
        {
            return _energyCost;
        }

        set
        {
            _energyCost = value;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        previousRigidbody2DPosition = rb.position;

        Deactivate();
    }


    private void FixedUpdate()
    {
        if (IsActive())
        {
            Vector3 raycastToPosition = transform.position - previousRigidbody2DPosition;
            RaycastHit2D hit = Physics2D.Raycast(previousRigidbody2DPosition, raycastToPosition,
                    Vector2.Distance(transform.position, previousRigidbody2DPosition), collisionRaycastLayerMask.value);

            if (hit.collider != null && hit.collider.CompareTag(Config.Tags.flyingObject))
            {
                FlyingObject flyingObject = hit.collider.GetComponent<FlyingObject>();

                if (OwnedBy != flyingObject.FlyingObjType)
                {
                    flyingObject.HitByCollider(col);
                    rb.position = hit.point;
                }
            }

            previousRigidbody2DPosition = rb.position;

            if (despawnTime <= Time.time)
            {
                Deactivate();
            }
        }
    }


    public void Deactivate()
    {
        gameObject.SetActive(false);
        rb.velocity = Vector2.zero;
    }


    public void Activate(Vector3 position, Quaternion rotation, Vector2 force)
    {
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        despawnTime = Time.time + lifetime;
        rb.position = position;
        previousRigidbody2DPosition = rb.position;

        gameObject.SetActive(true);

        rb.AddForce(force * speed);
    }


    public bool IsActive()
    {
        return gameObject.activeSelf;
    }
}
