using UnityEngine;

/**
 * The singleton will make every class which extends this singleton class to a singleton
 */
public class Singleton<Type> : MonoBehaviour where Type : MonoBehaviour
{
    /**
	 * The singleton instance
	 */
    protected static Type instance;


    /**
	 * Returns the instance of the class
	 */
    public static Type Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (Type)FindObjectOfType(typeof(Type));

                if (instance == null)
                {
                    throw new UnityException("Couldn't find an instance of " + typeof(Type));
                }
            }

            return instance;
        }
    }
}