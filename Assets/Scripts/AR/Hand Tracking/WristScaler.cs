using UnityEngine;

public class WristScaler : MonoBehaviour
{
    [SerializeField] private float baseScale = 0.15f;
    
    private void Start()
    {
        // Set consistent scale - LiDAR will handle positioning
        transform.localScale = Vector3.one * baseScale;
    }
    
    // Optional: Adjust scale based on distance for visual consistency
    public void EstimateWristSize(Vector3 wristWorldPosition)
    {
        if (Camera.main != null)
        {
            float distance = Vector3.Distance(wristWorldPosition, Camera.main.transform.position);
            float scaleMultiplier = Mathf.Clamp(0.5f / distance, 0.8f, 1.2f);
            transform.localScale = Vector3.one * baseScale * scaleMultiplier;
        }
    }
}