using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PersistentARSession : MonoBehaviour
{
    private static ARSession instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = GetComponent<ARSession>();
        DontDestroyOnLoad(gameObject);
    }
}