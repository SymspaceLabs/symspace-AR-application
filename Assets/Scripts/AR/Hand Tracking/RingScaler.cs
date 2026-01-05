using UnityEngine;

public class RingScaler : MonoBehaviour
{
    [SerializeField] private Transform ringTransform;
    [SerializeField] private float defaultRingSize = 0.02f; // 2cm diameter

    public void UpdateRingScale(Vector3 fingerWorldPosition, Vector2[] fingerLandmarks)
    {
        if (ringTransform == null) return;

        // Estimate finger width from landmarks (e.g., PIP to DIP joints)
        float fingerWidth = EstimateFingerWidth(fingerLandmarks);

        // Scale ring to match finger width
        float scaleFactor = fingerWidth / defaultRingSize;

        ringTransform.localScale = Vector3.one * scaleFactor;

        //Debug.Log($"Finger Width: {fingerWidth:F3}m | Ring Scale: {scaleFactor:F2}");
    }

    private float EstimateFingerWidth(Vector2[] fingerLandmarks)
    {
        // Use distance between PIP and DIP joints to estimate finger thickness
        // This would require specific landmark indices for each finger
        float pixelDistance = Vector2.Distance(fingerLandmarks[1], fingerLandmarks[2]);
        float worldWidth = (pixelDistance / 1000f) * 0.01f; // Rough conversion

        return Mathf.Clamp(worldWidth, 0.015f, 0.03f); // 1.5cm to 3cm reasonable range
    }
}
