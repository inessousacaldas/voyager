using UnityEngine;

public class Explosion : MonoBehaviour {

    [SerializeField]
    private float _explosionRadius = 5f;
    [SerializeField]
    private float _explosionPower = 5f;

    public bool explode = false;

    private void Update()
    {
        if(explode)
        {
            explode = false;
            Explode();
        }
    }

    private void Explode()
    {
        Vector2 explosionPos = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionPos, _explosionRadius);

        foreach (Collider2D hit in colliders)
        {
            if (hit.CompareTag(Config.Tags.flyingObject))
            {
                FlyingObject obj = hit.GetComponent<FlyingObject>();

                if(obj.FlyingObjType == FlyingObject.FlyingObjectType.Player)
                {
                    Player player = hit.GetComponent<Player>();
                    
                    // If player is not indistructable dies
                    if (!player.IsIndistructable())
                        player.Destroy();
                    // If player is indistructable is affected by explosion force
                    else
                    {
                        Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                        AddExplosionForce2D(rb, _explosionPower / 2, explosionPos, _explosionRadius);
                    }
                }
                else
                {
                    Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                    AddExplosionForce2D(rb, _explosionPower, explosionPos, _explosionRadius);
                }
                
            }
            
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {

        if (collider.CompareTag(Config.Tags.missile))
        {
             Explode();
        }
    }

    public void AddExplosionForce2D(Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier)
    {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        Vector3 baseForce = dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff;
        body.AddForce(baseForce);

        float upliftWearoff = 1 - upliftModifier / explosionRadius;
        Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
        body.AddForce(upliftForce);
    }

    public void AddExplosionForce2D(Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        Vector2 force = dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff;
        body.AddForce(dir.normalized * (wearoff <= 0f ? 0f : explosionForce) * wearoff);
    }
}
