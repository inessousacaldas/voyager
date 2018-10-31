using UnityEngine;

public class FollowObject : MonoBehaviour {

    [SerializeField]
    private GameObject followThisObject;

    public GameObject FollowThisObject
    {
        get
        {
            return followThisObject;
        }

        set
        {
            followThisObject = value;
        }
    }

    void Update()
    {
        if(followThisObject != null)
        {
            transform.position = FollowThisObject.transform.position;
        }   
    }
}
