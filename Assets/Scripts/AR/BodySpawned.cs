using UnityEngine;

public class BodySpawned : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Debug.Log("1st Body Position : " + transform.position);
    }

    private void Update()
    {
        Debug.Log("Body Position : " + transform.position);
    }
}
