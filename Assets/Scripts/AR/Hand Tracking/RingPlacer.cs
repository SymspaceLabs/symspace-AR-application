using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using System.Collections.Generic;

public class RingPlacer : MonoBehaviour
{

    [Header("Ring Settings")]
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] public FingerType selectedFinger = FingerType.Ring;
    [SerializeField] private float ringSizeMultiplier = 1.2f;

    [Header("Stabilization Settings")]
    [SerializeField] private float positionSmoothTime = 0.1f;
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private float scaleSmoothTime = 0.2f;

    [Header("Dependencies")]
    [SerializeField] private AROcclusionManager occlusionManager;

    public GameObject currentRing;

    // Stabilization variables
    private Vector3 currentVelocity;
    private Vector3 rotationVelocity;
    private float scaleVelocity;
    private Vector3 smoothedPosition;
    private Quaternion smoothedRotation;
    private float smoothedScale = 0.015f;

    public enum FingerType
    {
        Index,
        Middle,
        Ring,
        Pinky,
        Thumb
    }
#if UNITY_IOS
    void Start()
    {
        //InitializeRing();

        // Initialize smoothed values
        if (currentRing != null)
        {
            smoothedPosition = currentRing.transform.position;
            smoothedRotation = currentRing.transform.rotation;
            smoothedScale = currentRing.transform.localScale.x;
        }
    }

    private void InitializeRing()
    {
        //if (currentRing == null && ringPrefab != null)
        //{
        //    //currentRing = Instantiate(ringPrefab);
        //    currentRing.SetActive(false);

        //}
            GetComponent<IOSHandDetector>().secondTutorialPage.SetActive(false);
    }

    public void ProcessHandLandmarks(float[] landmarks, int imageWidth, int imageHeight, string handness)
    {
        if (!IsValidLandmarks(landmarks) || currentRing == null) return;

        // Get both MCP and PIP joints
        int mcpIndex = GetFingerBaseIndex(selectedFinger);
        int pipIndex = GetFingerPIPIndex(selectedFinger);

        if (!HasSufficientConfidence(landmarks, mcpIndex) || !HasSufficientConfidence(landmarks, pipIndex))
            return;

        // Get both joint positions
        Vector2 mcpScreen = ConvertImageToScreenSpace(
            new Vector2(landmarks[mcpIndex * 3], landmarks[mcpIndex * 3 + 1]),
            imageWidth, imageHeight
        );
        Vector2 pipScreen = ConvertImageToScreenSpace(
            new Vector2(landmarks[pipIndex * 3], landmarks[pipIndex * 3 + 1]),
            imageWidth, imageHeight
        );

        Vector3 mcpWorld = GetHandSurfacePosition(mcpScreen);
        Vector3 pipWorld = GetHandSurfacePosition(pipScreen);

        if (mcpWorld == Vector3.zero || pipWorld == Vector3.zero) return;

        // Place ring at 70% between MCP and PIP (closer to PIP)
        Vector3 ringPosition = Vector3.Lerp(mcpWorld, pipWorld, 0.7f);


        // Calculate finger direction for rotation
        Vector3 fingerDirection = (pipWorld - mcpWorld).normalized;

        int fingerBaseIndex = GetFingerBaseIndex(selectedFinger);
        if (!HasSufficientConfidence(landmarks, fingerBaseIndex)) return;

        // Get finger position
        Vector2 fingerScreen = ConvertImageToScreenSpace(
            new Vector2(landmarks[fingerBaseIndex * 3], landmarks[fingerBaseIndex * 3 + 1]),
            imageWidth, imageHeight);

        Vector3 fingerWorld = GetHandSurfacePosition(fingerScreen);
        if (fingerWorld == Vector3.zero) return;

        float fingerWidth = CalculateFingerWidth(landmarks, selectedFinger, imageWidth, imageHeight);
        Quaternion ringRotation = CalculateRingRotation(landmarks, selectedFinger, imageWidth, imageHeight, handness, ringPosition);

        UpdateRingPosition(ringPosition, fingerWidth, ringRotation);
    }

    private int GetFingerPIPIndex(FingerType finger)
    {
        switch (finger)
        {
            case FingerType.Index: return 6;   // Index PIP
            case FingerType.Middle: return 10; // Middle PIP  
            case FingerType.Ring: return 14;   // Ring PIP
            case FingerType.Pinky: return 18;  // Pinky PIP
            case FingerType.Thumb: return 3;   // Thumb IP
            default: return 14;
        }
    }

    private Vector3 CalculateRingOffset(Vector3 fingerDirection, string handness)
    {
        // For ring placement on TOP of finger (not underside)
        if (handness.ToLower() == "right")
        {
            // For right hand: offset to the left side of finger (toward thumb) and slightly up
            return Vector3.Cross(fingerDirection, Vector3.up) * 0.01f + Vector3.up * 0.005f;
        }
        else
        {
            // For left hand: offset to the right side of finger (toward thumb) and slightly up  
            return Vector3.Cross(fingerDirection, Vector3.down) * 0.01f + Vector3.up * 0.005f;
        }
    }

    private bool IsValidLandmarks(float[] landmarks)
    {
        return landmarks != null && landmarks.Length >= 63;
    }

    private bool HasSufficientConfidence(float[] landmarks, int landmarkIndex)
    {
        return landmarkIndex >= 0 && landmarkIndex * 3 + 2 < landmarks.Length &&
               landmarks[landmarkIndex * 3 + 2] >= 0.3f;
    }

    private Vector2 ConvertImageToScreenSpace(Vector2 imagePixelPos, int imageWidth, int imageHeight)
    {
        float normalizedX = imagePixelPos.x / imageWidth;
        float normalizedY = imagePixelPos.y / imageHeight;

        return new Vector2(
            (1 - normalizedY) * Screen.width,
            (1 - normalizedX) * Screen.height
        );
    }

    private Vector3 GetHandSurfacePosition(Vector2 screenPos)
    {
        if (occlusionManager == null) return GetFallbackWorldPosition(screenPos);

        try
        {
            if (occlusionManager.TryAcquireEnvironmentDepthCpuImage(out var depthImage))
            {
                using (depthImage)
                {
                    // Sample multiple points for more stable depth
                    float depth = GetStableDepth(screenPos, depthImage);
                    if (depth > 0.3f && depth < 1.0f)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(screenPos);
                        return ray.origin + ray.direction * depth;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ring LiDAR error: {e.Message}");
        }

        return GetFallbackWorldPosition(screenPos);
    }

    private float GetStableDepth(Vector2 centerPos, XRCpuImage depthImage)
    {
        List<float> depths = new List<float>();

        // Sample 5 points in a small cross pattern
        Vector2[] samplePoints = new Vector2[]
        {
            centerPos,
            centerPos + new Vector2(1, 0),
            centerPos + new Vector2(-1, 0),
            centerPos + new Vector2(0, 1),
            centerPos + new Vector2(0, -1)
        };

        foreach (Vector2 point in samplePoints)
        {
            float depth = GetDepthAtPixel(point, depthImage);
            if (depth > 0.3f && depth < 1.0f)
            {
                depths.Add(depth);
            }
        }

        if (depths.Count == 0) return 0f;

        // Return median depth for stability (reduces outliers)
        depths.Sort();
        return depths[depths.Count / 2];
    }

    private Vector3 GetFallbackWorldPosition(Vector2 screenPos)
    {
        if (Camera.main == null) return Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return ray.origin + ray.direction * 0.6f;
    }

    private float GetDepthAtPixel(Vector2 screenPos, XRCpuImage depthImage)
    {
        try
        {
            int x = Mathf.Clamp((int)screenPos.x, 0, depthImage.width - 1);
            int y = Mathf.Clamp((int)screenPos.y, 0, depthImage.height - 1);

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(x, y, 1, 1),
                outputDimensions = new Vector2Int(1, 1),
                outputFormat = TextureFormat.RFloat
            };

            int bufferSize = depthImage.GetConvertedDataSize(conversionParams);
            NativeArray<byte> buffer = new NativeArray<byte>(bufferSize, Allocator.Temp);

            depthImage.Convert(conversionParams, buffer);

            float depthValue = 0f;
            if (buffer.Length >= 4)
            {
                depthValue = System.BitConverter.ToSingle(buffer.ToArray(), 0);
            }

            buffer.Dispose();
            return depthValue;
        }
        catch
        {
            return 0f;
        }
    }

    private int GetFingerBaseIndex(FingerType finger)
    {
        switch (finger)
        {
            case FingerType.Index: return 5;
            case FingerType.Middle: return 9;
            case FingerType.Ring: return 13;
            case FingerType.Pinky: return 17;
            case FingerType.Thumb: return 2;
            default: return 5;
        }
    }

    private int GetFingerTipIndex(FingerType finger)
    {
        switch (finger)
        {
            case FingerType.Index: return 8;
            case FingerType.Middle: return 12;
            case FingerType.Ring: return 16;
            case FingerType.Pinky: return 20;
            case FingerType.Thumb: return 4;
            default: return 8;
        }
    }

    private int GetAdjacentFingerIndex(FingerType fingerType)
    {
        switch (fingerType)
        {
            case FingerType.Thumb: return 5;
            case FingerType.Index: return 9;
            case FingerType.Middle: return 13;
            case FingerType.Ring: return 17;
            case FingerType.Pinky: return 13;
            default: return 9;
        }
    }

    private float CalculateFingerWidth(float[] landmarks, FingerType fingerType, int imageWidth, int imageHeight)
    {
        int baseIndex = GetFingerBaseIndex(fingerType);
        int adjacentIndex = GetAdjacentFingerIndex(fingerType);

        if (!HasSufficientConfidence(landmarks, baseIndex) || !HasSufficientConfidence(landmarks, adjacentIndex))
            return 0.015f;

        Vector2 baseScreen = ConvertImageToScreenSpace(
            new Vector2(landmarks[baseIndex * 3], landmarks[baseIndex * 3 + 1]),
            imageWidth, imageHeight
        );
        Vector2 adjacentScreen = ConvertImageToScreenSpace(
            new Vector2(landmarks[adjacentIndex * 3], landmarks[adjacentIndex * 3 + 1]),
            imageWidth, imageHeight
        );

        Vector3 baseWorld = GetHandSurfacePosition(baseScreen);
        Vector3 adjacentWorld = GetHandSurfacePosition(adjacentScreen);

        if (baseWorld == Vector3.zero || adjacentWorld == Vector3.zero)
            return 0.015f;

        // Adjust multiplier based on hand orientation
        float adjustedMultiplier = 0.5f;

        // If palm is facing camera, reduce the multiplier (fingers appear wider apart)
        if (IsPalmFacingCamera(landmarks, imageWidth, imageHeight))
        {
            adjustedMultiplier = 0.4f; // Reduce for palm view
        }

        float distance = Vector3.Distance(baseWorld, adjacentWorld);
        return Mathf.Clamp(distance * adjustedMultiplier, 0.01f, 0.03f);
    }

    private bool IsPalmFacingCamera(float[] landmarks, int imageWidth, int imageHeight)
    {
        // Simple check: if wrist is behind finger joints in screen space, palm is facing camera
        Vector2 wristScreen = ConvertImageToScreenSpace(new Vector2(landmarks[0], landmarks[1]), imageWidth, imageHeight);
        Vector2 middleMcpScreen = ConvertImageToScreenSpace(new Vector2(landmarks[9 * 3], landmarks[9 * 3 + 1]), imageWidth, imageHeight);

        // If wrist Y position is lower than middle MCP, palm is likely facing camera
        return wristScreen.y < middleMcpScreen.y;
    }

    private Quaternion CalculateRingRotation(float[] landmarks, FingerType fingerType, int imageWidth, int imageHeight, string handness, Vector3 ringPosition)
    {
        // Use PIP to DIP section for straight finger rotation (where rings actually sit)
        int pipIndex = GetFingerPIPIndex(fingerType);
        int dipIndex = GetFingerDIPIndex(fingerType);
        if (!HasSufficientConfidence(landmarks, pipIndex) || !HasSufficientConfidence(landmarks, dipIndex))
            return GetFallbackRotation(landmarks, imageWidth, imageHeight, handness);
        Vector2 pipScreen = ConvertImageToScreenSpace(new Vector2(landmarks[pipIndex * 3], landmarks[pipIndex * 3 + 1]), imageWidth, imageHeight);
        Vector2 dipScreen = ConvertImageToScreenSpace(new Vector2(landmarks[dipIndex * 3], landmarks[dipIndex * 3 + 1]), imageWidth, imageHeight);
        Vector3 pipWorld = GetHandSurfacePosition(pipScreen);
        Vector3 dipWorld = GetHandSurfacePosition(dipScreen);
        if (pipWorld == Vector3.zero || dipWorld == Vector3.zero)
            return GetFallbackRotation(landmarks, imageWidth, imageHeight, handness);
        // Use the straight section of finger (PIP to DIP) for rotation
        Vector3 fingerDirection = (dipWorld - pipWorld).normalized;
        // Get hand orientation for palm/back detection
        Vector2 wristScreen = ConvertImageToScreenSpace(new Vector2(landmarks[0], landmarks[1]), imageWidth, imageHeight);
        Vector2 indexScreen = ConvertImageToScreenSpace(new Vector2(landmarks[5 * 3], landmarks[5 * 3 + 1]), imageWidth, imageHeight);
        Vector2 pinkyScreen = ConvertImageToScreenSpace(new Vector2(landmarks[17 * 3], landmarks[17 * 3 + 1]), imageWidth, imageHeight);
        Vector3 wristWorld = GetHandSurfacePosition(wristScreen);
        Vector3 indexWorld = GetHandSurfacePosition(indexScreen);
        Vector3 pinkyWorld = GetHandSurfacePosition(pinkyScreen);
        if (wristWorld == Vector3.zero || indexWorld == Vector3.zero || pinkyWorld == Vector3.zero)
            return GetFallbackRotation(landmarks, imageWidth, imageHeight, handness);
        // Calculate hand plane for palm direction
        Vector3 handRight = (indexWorld - pinkyWorld).normalized;
        if (handness.ToLower() == "left") handRight = -handRight;
        Vector3 handForward = (indexWorld - wristWorld).normalized;
        Vector3 handUp = Vector3.Cross(handForward, handRight).normalized;
        // Palm faces opposite to handUp
        Vector3 palmDirection = -handUp;
        // Create rotation: face palm direction, align with straight finger section
        return Quaternion.LookRotation(palmDirection, fingerDirection);
    }
    private Quaternion GetFallbackRotation(float[] landmarks, int imageWidth, int imageHeight, string handness)
    {
        // Fallback to original method if PIP/DIP not available
        Vector2 wristScreen = ConvertImageToScreenSpace(new Vector2(landmarks[0], landmarks[1]), imageWidth, imageHeight);
        Vector2 indexScreen = ConvertImageToScreenSpace(new Vector2(landmarks[5 * 3], landmarks[5 * 3 + 1]), imageWidth, imageHeight);
        Vector2 pinkyScreen = ConvertImageToScreenSpace(new Vector2(landmarks[17 * 3], landmarks[17 * 3 + 1]), imageWidth, imageHeight);
        Vector2 middleScreen = ConvertImageToScreenSpace(new Vector2(landmarks[9 * 3], landmarks[9 * 3 + 1]), imageWidth, imageHeight);
        Vector3 wristWorld = GetHandSurfacePosition(wristScreen);
        Vector3 indexWorld = GetHandSurfacePosition(indexScreen);
        Vector3 pinkyWorld = GetHandSurfacePosition(pinkyScreen);
        Vector3 middleWorld = GetHandSurfacePosition(middleScreen);
        if (wristWorld == Vector3.zero || indexWorld == Vector3.zero || pinkyWorld == Vector3.zero || middleWorld == Vector3.zero)
            return Quaternion.identity;
        Vector3 handRight = (indexWorld - pinkyWorld).normalized;
        Vector3 handForward = (middleWorld - wristWorld).normalized;
        if (handness.ToLower() == "left")
        {
            handRight = -handRight;
        }
        Vector3 handUp = Vector3.Cross(handForward, handRight).normalized;
        handRight = Vector3.Cross(handUp, handForward).normalized;
        return Quaternion.LookRotation(-handUp, handForward);
    }

    private int GetFingerDIPIndex(FingerType finger)
    {
        // Use DIP joints (third knuckle) - for ring placement closer to fingertip
        switch (finger)
        {
            case FingerType.Index: return 7;   // Index DIP
            case FingerType.Middle: return 11; // Middle DIP
            case FingerType.Ring: return 15;   // Ring DIP
            case FingerType.Pinky: return 19;  // Pinky DIP
            case FingerType.Thumb: return 4;   // Thumb Tip (no DIP for thumb)
            default: return 15;
        }
    }

    private float GetHandYRotation(string handness)
    {
        // Simple Y rotation based on hand type
        // Adjust these values based on what looks natural
        if (handness.ToLower() == "right")
        {
            return 0f; // Right hand Y rotation
        }
        else
        {
            return 180f; // Left hand Y rotation (flipped)
        }
    }

    public void UpdateRingPosition(Vector3 targetPosition, float targetFingerWidth, Quaternion targetRotation)
    {
        if (currentRing == null)
        {
            return;
        }

        if (!currentRing.activeInHierarchy)
            currentRing.SetActive(true);

        // Smooth position
        smoothedPosition = Vector3.SmoothDamp(smoothedPosition, targetPosition, ref currentVelocity, positionSmoothTime);

        // Smooth rotation
        smoothedRotation = SmoothDampQuaternion(smoothedRotation, targetRotation, ref rotationVelocity, rotationSmoothTime);

        // Smooth scale
        float targetScale = targetFingerWidth * ringSizeMultiplier;
        smoothedScale = Mathf.SmoothDamp(smoothedScale, targetScale, ref scaleVelocity, scaleSmoothTime);

        currentRing.transform.position = smoothedPosition;
        currentRing.transform.rotation = smoothedRotation;
        currentRing.transform.localScale = Vector3.one * smoothedScale * 250 * 2f;
        Debug.Log($"Ring Pos: {currentRing.transform.position:F3}m, Rotation: {currentRing.transform.eulerAngles:F2}m, Scale: {currentRing.transform.localScale:F3}");
    }

    private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 velocity, float smoothTime)
    {
        Vector3 currentEuler = current.eulerAngles;
        Vector3 targetEuler = target.eulerAngles;

        return Quaternion.Euler(
            Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref velocity.x, smoothTime),
            Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref velocity.y, smoothTime),
            Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref velocity.z, smoothTime)
        );
    }

    public void SetRingFinger(FingerType finger)
    {
        selectedFinger = finger;
    }

    public void SetRingVisible(bool visible)
    {
        if (currentRing != null)
        {
            currentRing.SetActive(visible);
        }
    }
#endif
}