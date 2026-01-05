using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DelayOnObjects : MonoBehaviour
{
    public float delay = 2f;
    public float extraDelay = 5f;
    public List<GameObject> objectsToEnable;
    public List<GameObject> objectsToEnableLater;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       StartCoroutine(ObjectsOn());
    }

    IEnumerator ObjectsOn()
    {
        yield return new WaitForSeconds(delay);
        foreach (GameObject obj in objectsToEnable)
            obj.SetActive(true);
        yield return new WaitForSeconds(extraDelay);
        foreach (GameObject obj in objectsToEnableLater)
            obj.SetActive(true);
    }


}
