using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityGLTF;
using static CategoryManager;

public class HandTrackingVisualizer : MonoBehaviour
{
    
    [Header("Dependencies")]
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private AROcclusionManager occlusionManager;
    [SerializeField] private GameObject wristAnchorPrefab;

    [Header("Settings")]
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private Vector3 wristOffset = new Vector3(0f, 0.02f, 0f);
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(0f, 0f, 0f);
    [SerializeField] private float baseWatchScale = 0.1f; // Base scale at 0.5m distance

    public GameObject currentWristAnchor;
    public GameObject watchMesh;
    public float watchWidth;
    private bool isWatchSpawned = false;
    private Vector3 currentVelocity;
    private Vector3 rotationVelocity;

    // Scaling
    private float currentScale = 1.0f;
    private float scaleVelocity = 0f;
    private float scaleSmoothTime = 0.2f;

    bool isBackHand = false;

    [Header("Ring Settings")]
    [SerializeField] private RingPlacer ringPlacer;
#if UNITY_IOS
    private void Start()
    {
        if (currentWristAnchor == null && wristAnchorPrefab != null)
        {
            currentWristAnchor = Instantiate(wristAnchorPrefab);
            currentWristAnchor.SetActive(false);
            currentScale = baseWatchScale;

        }

        //MeshRenderer renderer = watchMesh.GetComponentInChildren<MeshRenderer>();
        //watchWidth = renderer.bounds.size.x; // whichever axis represents width


        //if (watchWidth < 0.03f || watchWidth > 0.06f)
        //{
        //    Debug.LogWarning($"Watch width out of spec: {watchWidth:F3}m");
        //}
    }


    public void UpdateHandLandmarks(float[] landmarks, int imageWidth, int imageHeight, string handness)
    {
        
        if (!IsValidLandmarks(landmarks)) return;

        UpdateRingPosition(landmarks, imageWidth, imageHeight, handness);
        
        // Get wrist position using YOUR working coordinate conversion
        Vector2 wristScreen = ConvertImageToScreenSpace(new Vector2(landmarks[0], landmarks[1]), imageWidth, imageHeight);
        Vector2 indexMcpScreen = ConvertImageToScreenSpace(new Vector2(landmarks[5 * 3], landmarks[5 * 3 + 1]), imageWidth, imageHeight);
        Vector2 pinkyMcpScreen = ConvertImageToScreenSpace(new Vector2(landmarks[17 * 3], landmarks[17 * 3 + 1]), imageWidth, imageHeight);
        Vector2 middleMcpScreen = ConvertImageToScreenSpace(new Vector2(landmarks[9 * 3], landmarks[9 * 3 + 1]), imageWidth, imageHeight);

        if (!HasSufficientConfidence(landmarks, new int[] { 0, 5, 17, 9 })) return;

        // Use LiDAR to get ACTUAL 3D position on hand surface
        Vector3 wristWorld = GetHandSurfacePosition(wristScreen);
        if (wristWorld == Vector3.zero) return;

        // Get other hand points in 3D for rotation calculation
        Vector3 indexWorld = GetHandSurfacePosition(indexMcpScreen);
        Vector3 pinkyWorld = GetHandSurfacePosition(pinkyMcpScreen);
        Vector3 middleWorld = GetHandSurfacePosition(middleMcpScreen);

        if(currentWristAnchor.transform.GetChild(0).transform.childCount > 0)
        {
            foreach(Transform obj in currentWristAnchor.transform.GetChild(0).transform)
                if(obj.gameObject.activeInHierarchy)
                {
                    // Calculate rotation from actual 3D hand surface points
                    Quaternion handRotation = CalculateHandRotationFromSurface(wristWorld, indexWorld, pinkyWorld, middleWorld, handness);

                    // Apply offset relative to hand surface
                    //Vector3 offsetPosition = wristWorld + handRotation * wristOffset;
                    Vector3 watchPosition = wristWorld;

                    // Calculate scale based on hand size and distance
                    float handScale = CalculateHandScale(wristWorld, indexWorld, pinkyWorld);
                    UpdateWatchScale(handScale);

                    UpdateWristAnchor(watchPosition, handRotation);
                    break;
                }
        }

        

    }

    private void UpdateRingPosition(float[] landmarks, int imageWidth, int imageHeight, string handness)
    {
        if (ringPlacer == null || ringPlacer.currentRing == null) return;

        Transform ringTransform = ringPlacer.currentRing.transform;
        if (ringTransform.childCount == 0) return;

        Transform firstChild = ringTransform.GetChild(0);
        if (firstChild.childCount == 0) return;

        foreach (Transform obj in firstChild)
        {
            if (obj.gameObject.activeInHierarchy)
            {
                ringPlacer.ProcessHandLandmarks(landmarks, imageWidth, imageHeight, handness);
                GetComponent<IOSHandDetector>().StopTutorial();
                break;
            }
        }
    }

    private float CalculateHandScale(Vector3 wristWorld, Vector3 indexWorld, Vector3 pinkyWorld)
    {
        if (Camera.main == null)
            return baseWatchScale;

        // Wrist width from hand tracking
        float wristWidth = Vector3.Distance(indexWorld, pinkyWorld);

        // MUST match your normalized watch size
        float watchReferenceWidth = 0.04f; // <-- from your bounds (~0.03972)

        float fitFactor = 1.6f; // small buffer so it doesn't clip

        float targetScale = (wristWidth / watchReferenceWidth) * fitFactor;

        return targetScale;
    }

    private void UpdateWatchScale(float targetScale)
    {
        if (currentWristAnchor == null) return;

        // Smooth scale transition
        currentScale = Mathf.SmoothDamp(currentScale, targetScale, ref scaleVelocity, scaleSmoothTime);

        // Apply scale to watch
        currentWristAnchor.transform.localScale = Vector3.one * currentScale;
    }

    private Vector3 GetHandSurfacePosition(Vector2 screenPos)
    {
        if (occlusionManager == null)
            return GetFallbackWorldPosition(screenPos);

        try
        {
            if (occlusionManager.TryAcquireEnvironmentDepthCpuImage(out XRCpuImage depthImage))
            {
                using (depthImage)
                {
                    // Sample multiple points around the target for stable surface detection
                    Vector3 surfacePosition = GetStableSurfacePosition(screenPos, depthImage);
                    if (surfacePosition != Vector3.zero)
                    {
                        return surfacePosition;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"LiDAR surface detection error: {e.Message}");
        }

        return GetFallbackWorldPosition(screenPos);
    }

    private Vector3 GetStableSurfacePosition(Vector2 centerPos, XRCpuImage depthImage)
    {
        List<Vector3> surfacePoints = new List<Vector3>();

        // Sample a 3x3 grid around the center point to find the hand surface
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 samplePos = centerPos + new Vector2(x * 3, y * 3);
                float depth = GetDepthAtPixel(samplePos, depthImage);

                // Filter for hand-like distances (0.3m to 1.0m)
                if (depth > 0.3f && depth < 1.0f)
                {
                    Ray ray = Camera.main.ScreenPointToRay(samplePos);
                    Vector3 worldPos = ray.origin + ray.direction * depth;
                    surfacePoints.Add(worldPos);
                }
            }
        }

        if (surfacePoints.Count == 0) return Vector3.zero;

        // Return the average position to get a stable surface point
        Vector3 averagePosition = Vector3.zero;
        foreach (Vector3 point in surfacePoints)
        {
            averagePosition += point;
        }
        averagePosition /= surfacePoints.Count;

        return averagePosition;
    }

    private Vector3 GetFallbackWorldPosition(Vector2 screenPos)
    {
        if (Camera.main == null) return Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return ray.origin + ray.direction * 0.6f;
    }

    private Quaternion CalculateHandRotationFromSurface(Vector3 wrist, Vector3 indexMcp, Vector3 pinkyMcp, Vector3 middleMcp, string handness)
    {
        // Calculate hand orientation from ACTUAL 3D surface points
        Vector3 handRight = (indexMcp - pinkyMcp).normalized;
        Vector3 handForward = (middleMcp - wrist).normalized;

        // Adjust for left/right hand
        if (handness.ToLower() == "left")
        {
            handRight = -handRight;
        }

        // Calculate surface normal (up vector)
        Vector3 handUp = Vector3.Cross(handForward, handRight).normalized;
        //Vector3 handUp = handForward;

        // Ensure orthonormal basis
        handRight = Vector3.Cross(handUp, handForward).normalized;

        // Vector3 capsuleUp = -handForward;
        // Vector3 capsuleForward = -handUp; // or -handUp depending on palm direction

        if (handUp.y > 0)
            isBackHand = true;
        else
            isBackHand = false;
            
        return Quaternion.LookRotation(handForward, handUp);
    }

    // Your original working coordinate conversion
    private Vector2 ConvertImageToScreenSpace(Vector2 imagePixelPos, int imageWidth, int imageHeight)
    {
        float normalizedX = imagePixelPos.x / imageWidth;
        float normalizedY = imagePixelPos.y / imageHeight;

        Vector2 screenPos = new Vector2(
            (1 - normalizedY) * Screen.width,
            (1 - normalizedX) * Screen.height
        );

        return screenPos;
    }

    private bool IsValidLandmarks(float[] landmarks)
    {
        return landmarks != null && landmarks.Length >= 63;
    }

    private bool HasSufficientConfidence(float[] landmarks, int[] landmarkIndices)
    {
        foreach (int index in landmarkIndices)
        {
            if (landmarks[index * 3 + 2] < 0.3f)
                return false;
        }
        return true;
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

    private void UpdateWristAnchor(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!isWatchSpawned)
        {
            SpawnWristAnchor(targetPosition, targetRotation);
            return;
        }

        if (currentWristAnchor == null) return;

        // Smooth position
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            currentWristAnchor.transform.position,
            targetPosition + wristOffset,
            ref currentVelocity,
            smoothTime
        );

        Quaternion offsetRotation = Quaternion.Euler(rotationOffsetEuler);

        if (!isBackHand)
            offsetRotation = Quaternion.Euler(-rotationOffsetEuler);

        // Smooth rotation
        Quaternion smoothedRotation = SmoothDampQuaternion(
            currentWristAnchor.transform.localRotation,
            targetRotation * offsetRotation,
            ref rotationVelocity,
            smoothTime
        );

        currentWristAnchor.transform.position = smoothedPosition;
        currentWristAnchor.transform.localRotation = smoothedRotation;
        //currentWristAnchor.transform.SetLocalPositionAndRotation(smoothedPosition, smoothedRotation);
    }

    private void SpawnWristAnchor(Vector3 position, Quaternion rotation)
    {
        if (currentWristAnchor != null)
        {
            currentWristAnchor.SetActive(true);
            currentWristAnchor.transform.SetPositionAndRotation(position, rotation);
            currentWristAnchor.transform.localScale = Vector3.one * baseWatchScale;
            isWatchSpawned = true;
            GetComponent<IOSHandDetector>().StopTutorial();
        }
        else if (wristAnchorPrefab != null)
        {
            currentWristAnchor = Instantiate(wristAnchorPrefab, position, rotation);
            currentWristAnchor.transform.localScale = Vector3.one * baseWatchScale;
            isWatchSpawned = true;

        }
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
#endif
}