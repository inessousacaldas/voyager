using UnityEngine;


/**
 * Encodes the viewport behaviour. 
 */
[RequireComponent (typeof(BoxCollider2D))]
public class Viewport : MonoBehaviour
{
    [SerializeField]
    private float _offset;

    private void Start()
    {
        AdjustViewportToScreen();
    }

    private void Update()
    {
        AdjustViewportToScreen();
    }

    /**
     * Ajusts the viewport scale and position according to main camera. 
     */
    private void AdjustViewportToScreen()
    {
        float distanceFromCamera = Camera.main.transform.position.z;

        Vector3 viewPort;
        Vector3 bottomLeft;
        Vector3 topRight;

        viewPort = new Vector3(0, 0, distanceFromCamera);
        bottomLeft = Camera.main.ViewportToWorldPoint(viewPort);

        viewPort.Set(1, 1, distanceFromCamera);
        topRight = Camera.main.ViewportToWorldPoint(viewPort);

        transform.localScale = new Vector3(bottomLeft.x - topRight.x - _offset, bottomLeft.y - topRight.y - _offset, 1);
        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -distanceFromCamera));
    }

    /**
    * Notifies a flying object if it leaves the viewport.
    */
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag(Config.Tags.flyingObject))
        {
            collider.GetComponent<FlyingObject>().OnLeaveViewport();
        }
    }
}
