using System;
using System.Collections.Generic;
using System.Collections;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class IOSHandDetector : MonoBehaviour
{
#if UNITY_IOS
    // These now match your actual native plugin functions
    [DllImport("__Internal")]
    private static extern void InitializeHandDetector(HandPoseCallback callback);

    [DllImport("__Internal")]
    private static extern void ProcessFrame(IntPtr pixelData, int width, int height);

    [DllImport("__Internal")]
    private static extern void CleanupHandDetector();

    private delegate void HandPoseCallback(IntPtr landmarks, int landmarkCount, int width, int height, string handness);

    [DllImport("__Internal")]
    private static extern void ProcessFrameForRings(IntPtr pixelData, int width, int height, HandPoseCallback ringCallback);
#endif
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private AROcclusionManager occlusionManager;

    private bool isInitialized = false;

    private Texture2D cameraTexture; // Add this declaration

    // Landmark indices
    private const int WRIST = 0;
    private const int INDEX_MCP = 1;
    private const int PINKY_MCP = 2;
    private const int MIDDLE_MCP = 3;

    public HandTrackingVisualizer HTV;
    public RingPlacer RP;
    public HandItemSelector HIS;

    private Coroutine tutorialRoutine;
    private bool tutorialActive = true;

    public GameObject firstTutorialPage;
    public GameObject secondTutorialPage;

#if UNITY_IOS
    private void Start()
    {
        if (cameraManager == null)
            cameraManager = FindObjectOfType<ARCameraManager>();

        cameraManager.frameReceived += OnCameraFrameReceived;

        DisableOcclusion();

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            InitializeNativePlugin();
        }

        tutorialRoutine = StartCoroutine(TutorialFlow());

        Invoke(nameof(EnableObjects), 5f);
    }

    IEnumerator TutorialFlow()
    {
        tutorialActive = true;

        firstTutorialPage.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (!tutorialActive) yield break;

        firstTutorialPage.SetActive(false);

        yield return StartCoroutine(BlinkSecondPage());
    }

    IEnumerator BlinkSecondPage()
    {
        while (tutorialActive)
        {
            secondTutorialPage.SetActive(true);
            yield return new WaitForSeconds(1f);

            if (!tutorialActive) yield break;

            secondTutorialPage.SetActive(false);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void StopTutorial()
    {
        tutorialActive = false;

        if (tutorialRoutine != null)
            StopCoroutine(tutorialRoutine);

        firstTutorialPage.SetActive(false);
        secondTutorialPage.SetActive(false);
    }

    //IEnumerator HandleTutorials()
    //{
    //    firstTutorialPage.SetActive(true);
    //    yield return new WaitForSeconds(2f);
    //    firstTutorialPage.SetActive(false);
    //    secondTutorialPage.SetActive(true);
    //    yield return new WaitForSeconds(1f);
    //    secondTutorialPage.SetActive(false);
    //    yield return new WaitForSeconds(0.05f);
    //    secondTutorialPage.SetActive(true);
    //    yield return new WaitForSeconds(1f);
    //    secondTutorialPage.SetActive(false);
    //    yield return new WaitForSeconds(0.05f);
    //    secondTutorialPage.SetActive(true);
    //}

    void EnableObjects()
    {
        HTV.enabled = true;
        RP.enabled = true;
        HIS.enabled = true;
    }

    void DisableOcclusion()
    {
        // This stops occlusion rendering but LiDAR still provides depth data
        occlusionManager.enabled = false;

        // OR be more specific:
        occlusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Disabled;
        occlusionManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Disabled;
        occlusionManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Disabled;

        //Camera.main.depthTextureMode = DepthTextureMode.None;
        //List<XROcclusionSubsystem> subsystems = new List<XROcclusionSubsystem>();
        //SubsystemManager.GetSubsystems(subsystems);

        //foreach (var subsystem in subsystems)
        //{
        //    if (subsystem != null && subsystem.running)
        //    {
        //        Debug.Log("Stopping XROcclusionSubsystem in this scene");
        //        subsystem.Stop();
        //    }
        //}
    }

    private void OnDestroy()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            CleanupHandDetector();
        }
    }

    private void InitializeNativePlugin()
    {
        try
        {
            InitializeHandDetector(OnHandPoseDetected);
            isInitialized = true;
            Debug.Log("iOS Hand Detector initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize hand detector: {e.Message}");
        }
    }



    [AOT.MonoPInvokeCallback(typeof(HandPoseCallback))]
    private static void OnHandPoseDetected(IntPtr landmarksPtr, int landmarkCount, int width, int height, string handness)
    {
        float[] landmarks = new float[landmarkCount * 3];
        Marshal.Copy(landmarksPtr, landmarks, 0, landmarkCount * 3);

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            HandTrackingVisualizer visualizer = FindObjectOfType<HandTrackingVisualizer>();
            if (visualizer != null)
            {
                visualizer.UpdateHandLandmarks(landmarks, width, height, handness);
            }
        });
    }

    //[AOT.MonoPInvokeCallback(typeof(HandPoseCallback))]
    //private static void OnRingLandmarksDetected(IntPtr landmarksPtr, int landmarkCount, int width, int height, string handness)
    //{
    //    if (landmarkCount == 21) // Full hand landmarks for rings
    //    {
    //        float[] landmarks = new float[landmarkCount * 3];
    //        Marshal.Copy(landmarksPtr, landmarks, 0, landmarkCount * 3);

    //        UnityMainThreadDispatcher.Instance.Enqueue(() =>
    //        {
    //            RingPlacer ringDetector = FindObjectOfType<RingPlacer>();
    //            if (ringDetector != null && ringDetector.enableRingDetection)
    //            {
    //                ringDetector.UpdateRingPosition(landmarks, width, height);
    //            }
    //        });
    //    }
    //}

    //private void ProcessFrameForRings(IntPtr pixelData, int width, int height)
    //{
    //    if (!enableRingDetection) return;

    //    try
    //    {
    //        // Just pass the pixelData directly - it's already the IntPtr we need!
    //        ProcessFrameForRings(pixelData, width, height, OnRingLandmarksDetected);
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"Ring processing error: {e.Message}");
    //    }
    //}

    // Add this field
    [SerializeField] private bool enableRingDetection = false;

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (!isInitialized) return;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        try
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.BGRA32
            };

            int size = image.GetConvertedDataSize(conversionParams);

            // Create texture and convert directly
            if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
            {
                cameraTexture = new Texture2D(image.width, image.height, TextureFormat.BGRA32, false);
            }

            // Convert directly to texture
            image.Convert(conversionParams, cameraTexture.GetRawTextureData<byte>());
            cameraTexture.Apply();

            // Process texture data for wrist tracking (EXISTING - DON'T CHANGE)
            ProcessTextureData(cameraTexture, image.width, image.height);

            // ADDED: Process for rings using the same texture data
            //byte[] ringPixelData = cameraTexture.GetRawTextureData(); // Renamed to avoid conflict
            //GCHandle handle = GCHandle.Alloc(ringPixelData, GCHandleType.Pinned);
            //try
            //{
            //    ProcessFrameForRings(handle.AddrOfPinnedObject(), image.width, image.height);
            //}
            //finally
            //{
            //    if (handle.IsAllocated)
            //        handle.Free();
            //}
        }
        finally
        {
            image.Dispose();
        }
    }

    private void ProcessTextureData(Texture2D texture, int width, int height)
    {
        // Get raw texture data
        byte[] pixelData = texture.GetRawTextureData();

        // Pin the memory and send to native plugin
        GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
        try
        {
            IntPtr pixelDataPtr = handle.AddrOfPinnedObject();
            ProcessFrame(pixelDataPtr, width, height);
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
    private void ProcessFrameWithBuffer(byte[] buffer, int width, int height)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr bufferPtr = handle.AddrOfPinnedObject();
                ProcessFrame(bufferPtr, width, height);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
#endif
}