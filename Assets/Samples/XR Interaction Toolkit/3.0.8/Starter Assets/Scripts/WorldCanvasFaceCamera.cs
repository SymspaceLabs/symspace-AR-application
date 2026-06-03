using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class WorldCanvasFaceCamera : MonoBehaviour
{
    public Transform targetModel;         // The model to follow
    private Camera mainCamera;
    private Canvas canvas;
    public float verticalPadding = 0.1f;    // Extra space above the object

    public ProductDetails pd;
    public ObjectDetail objDetail;

    public CanvasPosition canvasPosition = CanvasPosition.Top; // default Top
    public Vector3 customOffset; // only used if CanvasPosition.Custom is selected

    private Mesh _bakedMesh;
    private float _lastBakeTime;
    private const float bakeInterval = 0.05f; // 20 FPS update for bounds

    void Awake()
    {
        //pd = GetComponentInParent<ProductDetails>();
        //objDetail = GetComponentInParent<ObjectDetail>();
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = mainCamera;
        canvas.enabled = false;

        // Detach canvas so scaling of model does not affect it
        transform.SetParent(null);

        // Set fixed canvas scale
        transform.localScale = Vector3.one * 0.005f;

        if (targetModel != null)
        {
            // Calculate the bounds of the entire model
            Bounds bounds = CalculateBounds(targetModel.gameObject);

            // Position the canvas just above the top of the object
            Vector3 topPosition = bounds.center + Vector3.up * (bounds.extents.y + verticalPadding);
            transform.position = topPosition;

            if (BodyTrackingWithMars.Instance != null)
                BodyTrackingWithMars.Instance.productSelected = pd;
        }

        UIManagerAR.instance.OnResetClick += DestroyItself;
    }

    void LateUpdate()
    {
        if (targetModel == null || mainCamera == null || !targetModel.gameObject.activeInHierarchy)
            return;

        // Calculate bounds of the model
        Bounds bounds = CalculateBounds(targetModel.gameObject);

        // Compute offset based on inspector selection
        Vector3 offset = Vector3.zero;
        float x = bounds.extents.x + verticalPadding;
        float y = bounds.extents.y + verticalPadding;
        float z = bounds.extents.z + verticalPadding;

        //switch (canvasPosition)
        //{
        //    case CanvasPosition.Top:
        //        offset = Vector3.up * y;
        //        break;
        //    case CanvasPosition.Bottom:
        //        offset = Vector3.down * y;
        //        break;
        //    case CanvasPosition.Left:
        //        offset = Vector3.left * x;
        //        break;
        //    case CanvasPosition.Right:
        //        offset = Vector3.right * x;
        //        break;
        //    case CanvasPosition.TopLeft:
        //        offset = new Vector3(-x, y, 0);
        //        break;
        //    case CanvasPosition.TopRight:
        //        offset = new Vector3(x, y, 0);
        //        break;
        //    case CanvasPosition.BottomLeft:
        //        offset = new Vector3(-x, -y, 0);
        //        break;
        //    case CanvasPosition.BottomRight:
        //        offset = new Vector3(x, -y, 0);
        //        break;
        //    case CanvasPosition.Custom:
        //        offset = customOffset;
        //        break;
        //}

        Vector3 camRight = mainCamera.transform.right;
        Vector3 camUp = Vector3.up; // usually better than camera.up in AR

        switch (canvasPosition)
        {
            case CanvasPosition.Top:
                offset = camUp * y;
                break;

            case CanvasPosition.Bottom:
                offset = -camUp * y;
                break;

            case CanvasPosition.Left:
                offset = -camRight * x;
                break;

            case CanvasPosition.Right:
                offset = camRight * x;
                break;

            case CanvasPosition.TopLeft:
                offset = (-camRight * x) + (camUp * y);
                break;

            case CanvasPosition.TopRight:
                offset = (camRight * x) + (camUp * y);
                break;

            case CanvasPosition.BottomLeft:
                offset = (-camRight * x) - (camUp * y);
                break;

            case CanvasPosition.BottomRight:
                offset = (camRight * x) - (camUp * y);
                break;
        }

        //if(CategoryManager.Instance.isDebugMode)Debug.Log("bounds.y : " + y);

        // Update canvas position relative to the model
        //if(CategoryManager.Instance.isDebugMode)Debug.Log("Bounds center: " + bounds.center + ", Offset: " + offset);
        transform.position = bounds.center + offset;

        // Make the canvas face the camera
        transform.LookAt(mainCamera.transform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        float scale = GetCanvasScale(bounds);
        transform.localScale = Vector3.one * scale;

        // Enable canvas if not already
        if (!canvas.enabled)
            canvas.enabled = true;
    }

    // Calculate combined renderer bounds of the object and its children
    Bounds CalculateBounds(GameObject obj)
    {
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
        {
            SkinnedMeshRenderer[] skinnedRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedRenderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.zero);

            bool initialized = false;
            Bounds skinnedBounds = new Bounds();

            if (_bakedMesh == null)
                _bakedMesh = new Mesh();

            foreach (var smr in skinnedRenderers)
            {
                if (smr == null) continue;

                // Bake current deformed mesh
                if (Time.time - _lastBakeTime > bakeInterval)
                {
                    _lastBakeTime = Time.time;
                    smr.BakeMesh(_bakedMesh);
                }
                // Convert vertices to world space
                Vector3[] verts = _bakedMesh.vertices;
                if (verts == null || verts.Length == 0) continue;

                if (!initialized)
                {
                    skinnedBounds = new Bounds(smr.transform.TransformPoint(verts[0]), Vector3.zero);
                    initialized = true;
                }

                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 worldV = smr.transform.TransformPoint(verts[i]);
                    skinnedBounds.Encapsulate(worldV);
                }
            }

            return skinnedBounds;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    float GetCanvasScale(Bounds bounds)
    {
        float maxDimension = Mathf.Max(
            bounds.size.x,
            bounds.size.y,
            bounds.size.z);

        float dynamicPadding = Mathf.Clamp(
            maxDimension * 0.2f,
            0.003f,   // minimum
            0.08f     // maximum
            );
        verticalPadding = dynamicPadding;

        // Adjust multiplier to your liking
        return Mathf.Clamp(maxDimension * 0.01f, 0.0004f, 0.005f);
    }

    public void PlusButton()
    {
        string name = SceneManager.GetActiveScene().name;
        switch(name)
        {
            case SceneNames.ARScene:
                for (int i = 0; i < GhostPlacementController.Instance.spawnedObjects.Count; i++)
                {
                    if (GhostPlacementController.Instance.spawnedObjects[i].name == pd.gameObject.name)
                    {
                        UIManagerAR.instance.objectSelectedIndex = i;
                        break;
                    }
                }
                break;
            case SceneNames.ARBodyTrackingMars:
                BodyTrackingWithMars.Instance.productSelected = pd;
                break;
        }

        //for (int i = 0; i < UIManagerAR.instance.UI_3D_Models.Count; i++)
        //{
        //    if (UIManagerAR.instance.UI_3D_Models[i].name == pd.gameObject.name)
        //    {
        //        UIManagerAR.instance.objectSelectedIndex = i;
        //        break;
        //    }
        //}
        //UIManagerAR.instance.objectSelectedIndex = GetComponent<ObjectDetail>().index;
        if(CategoryManager.Instance.isDebugMode)Debug.Log("Plus button clicked");
        UIManagerAR.instance.PlusBtn(pd/*.product.id, pd.selectedColorIndex*/);
    }

    void DestroyItself()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        UIManagerAR.instance.OnResetClick -= DestroyItself;
    }
    string CleanName(string originalName)
    {
        return originalName.Replace("(Clone)", "").Trim();
    }

    public enum CanvasPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Custom  // optional: if you want to provide a manual Vector3
    }
}