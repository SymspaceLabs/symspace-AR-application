using UnityEngine;

public class BodyMeasurement : MonoBehaviour
{
    public Transform head;
    public Transform leftShoulder;
    public Transform rightShoulder;
    public Transform leftFoot;
    public Transform rightFoot;

    public enum BodySize
    {
        Small,
        Medium,
        Large
    }

    void Update()
    {
        // Safety checks
        if (head == null || leftShoulder == null || rightShoulder == null ||
            leftFoot == null || rightFoot == null)
            return;

        // Measurements
        float shoulderWidth = Vector3.Distance(leftShoulder.position, rightShoulder.position);

        Vector3 feetMid = (leftFoot.position + rightFoot.position) / 2f;
        float height = Vector3.Distance(head.position, feetMid);

        if (height <= 0.0001f)
            return;

        // Ratio
        float shoulderRatio = shoulderWidth / height;

        // Classify
        BodySize size = Classify(shoulderRatio);

        //Debug.Log($"ShoulderWidth: {shoulderWidth}, Height: {height}, Ratio: {shoulderRatio}, Size: {size}");
    }

    BodySize Classify(float ratio)
    {
        // ⚠️ These thresholds must be tuned based on your MARS output
        // Your earlier ratio (~0.03) suggests your values are scaled differently

        if (ratio < 0.03f)
            return BodySize.Small;
        else if (ratio < 0.05f)
            return BodySize.Medium;
        else
            return BodySize.Large;
    }
}