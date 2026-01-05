using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AROcclusionController : MonoBehaviour
{
    [SerializeField] private AROcclusionManager occlusionManager;

    void Start()
    {
        if (occlusionManager != null)
        {
            // Enable people occlusion for hand isolation
            occlusionManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Best;
            occlusionManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Best;
            occlusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Best;
        }
    }

    void Update()
    {
        // Check if occlusion features are supported
        Debug.Log($"Human Stencil Supported: {occlusionManager.currentHumanStencilMode}");
        Debug.Log($"Human Depth Supported: {occlusionManager.currentHumanDepthMode}");
        Debug.Log($"Environment Depth Supported: {occlusionManager.currentEnvironmentDepthMode}");
    }
}