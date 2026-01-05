using UnityEngine;

public class HandDebugVisualizer : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject debugPointPrefab; // Small sphere/cube prefab
    [SerializeField] private bool showDebugPoints = true;

    private GameObject wristDebugPoint;
    private TextMesh debugText;

    private void Start()
    {
        // Create debug point for wrist
        if (debugPointPrefab != null && showDebugPoints)
        {
            wristDebugPoint = Instantiate(debugPointPrefab);
            wristDebugPoint.name = "WristDebug";
            wristDebugPoint.transform.localScale = Vector3.one * 0.1f; // Changed from 0.02 to 0.1 (10cm sphere)

            // Create debug text
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(wristDebugPoint.transform);
            textObj.transform.localPosition = Vector3.up * 0.1f;
            debugText = textObj.AddComponent<TextMesh>();
            debugText.fontSize = 20;
            debugText.characterSize = 0.01f;
            debugText.anchor = TextAnchor.MiddleCenter;
        }

        if (arCamera == null)
            arCamera = Camera.main;
    }

    public void UpdateWristPosition(Vector2 screenPosition, Vector3 worldPosition, float confidence, string status = "")
    {
        if (!showDebugPoints || wristDebugPoint == null) return;

        // Update debug point position
        wristDebugPoint.transform.position = worldPosition;

        // Update debug text
        if (debugText != null)
        {
            debugText.text = $"Wrist\nScreen: {screenPosition.x:F0},{screenPosition.y:F0}\nWorld: {worldPosition}\nConf: {confidence:F2}\n{status}";
        }

        // Change color based on confidence
        Renderer renderer = wristDebugPoint.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = confidence > 0.7f ? Color.green :
                                     confidence > 0.4f ? Color.yellow : Color.red;
        }
    }

    public void ShowMessage(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        Debug.Log($"Hand Debug: {message}");
    }
}