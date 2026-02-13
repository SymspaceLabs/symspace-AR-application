using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class WorldCanvasFaceCamera : MonoBehaviour
{
    public Transform targetModel;         // The model to follow
    //public Vector3 normalizedOffset = new Vector3(-1f, 1f, 0f);  // Direction to offset, not a fixed distance
    private Camera mainCamera;
    private Canvas canvas;
    //private Vector3 boundsOffset;
    public float verticalPadding = 0.1f;    // Extra space above the object

    public ProductDetails pd;
    public ObjectDetail objDetail;

    void Awake()
    {
        pd = GetComponentInParent<ProductDetails>();
        objDetail = GetComponentInParent<ObjectDetail>();
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = mainCamera;
        canvas.enabled = false;

        if (targetModel != null)
        {
            // Calculate the bounds of the entire model
            Bounds bounds = CalculateBounds(targetModel.gameObject);

            // Position the canvas just above the top of the object
            Vector3 topPosition = bounds.center + Vector3.up * (bounds.extents.y + verticalPadding);
            transform.position = topPosition;
        }
    }

    void LateUpdate()
    {
        if (targetModel == null || mainCamera == null) return;

        Bounds bounds = CalculateBounds(targetModel.gameObject);

        // Position the canvas just above the top of the object
        Vector3 topPosition = bounds.center + Vector3.up * (bounds.extents.y + verticalPadding);
        transform.position = topPosition;

        // Always face the camera
        transform.LookAt(mainCamera.transform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (!canvas.enabled)
            canvas.enabled = true;
    }

    // Calculate combined renderer bounds of the object and its children
    Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    public void PlusButton()
    {
        for (int i = 0; i < UIManagerAR.instance.spawner.objectsSpawned.Count; i++)
        {
            if (UIManagerAR.instance.spawner.objectsSpawned[i].name == pd.gameObject.name)
            {
                UIManagerAR.instance.objectSelectedIndex = i;
                break;
            }
        }

        UIManagerAR.instance.PlusBtn(pd.product.id, pd.selectedColorIndex);
    }

    string CleanName(string originalName)
    {
        return originalName.Replace("(Clone)", "").Trim();
    }
}
