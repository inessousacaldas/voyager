using UnityEngine;


public abstract class Character : FlyingObject
{

    public Transform weaponTransform;
    public float delayBetweenMissiles = 0.1f;
    protected float lastMissileShot = 0f;

    protected override void TooLongOutsideOfViewport() { }

    public override void HitByCollider(Collider2D collider)
    {
        base.HitByCollider(collider);

        if (collider.CompareTag(Config.Tags.flyingObject))
        {
            Destroy(false);
        }
    }

    public override void Activate(Vector2 position, Vector2 velocity) { }

    public override void Destroy(bool addScore)
    {
        AudioManager.Instance.Play(destructionAudioClip);
        Deactivate();

        if (addScore == true)
        {
            GameManager.Instance.AddScore(score);
        }
    }


    public override void Deactivate()
    {
        gameObject.SetActive(false);
        CancelInvoke();
    }

    public void FireMissile(Vector3 position, Quaternion rotation, Vector2 force)
    {
        if (CanFireMissile())
        {
            lastMissileShot = Time.time;
            Missile missile = MissileManager.Instance.GetMissile(FlyingObjType);

            missile.Activate(position, rotation, force);
        }
    }

    public bool CanFireMissile()
    {
        return (lastMissileShot + delayBetweenMissiles <= Time.time);
    }
}
