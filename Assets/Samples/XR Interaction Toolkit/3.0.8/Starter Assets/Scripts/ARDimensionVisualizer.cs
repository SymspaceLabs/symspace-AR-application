using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.Rendering.DebugUI.Table;

public class ARDimensionVisualizer : MonoBehaviour
{
    [Header("Line Settings")]
    public float lineWidth = 0.02f;
    public float lineOffset = 0.01f;
    public float cornerOffset = 0.1f;

    [Header("X/Y/Z Lines Offset Seperately")]
    public Vector3 axisOffset = new Vector3(0.1f, 0.02f, 0.1f); // x/y/z offsets separately

    public Material dashMat;

    [Header("Text Settings")]
    public GameObject textPrefab; // Assign a prefab with TextMesh or TextMeshPro

    public float textLength;
    public float textHeight;
    public float textDepth;

    public float textOffset = 0.05f;
    public float verticalTextOffset = 0.01f;

    public LineRenderer[] lineRenderers = new LineRenderer[6];
    public GameObject[] lineTexts = new GameObject[6];
    public Vector3[] externalCorners = new Vector3[8];

    private Transform linesContainer;

    void OnDestroy()
    {
        if (linesContainer != null)
            Destroy(linesContainer.gameObject);
    }

    void Start()
    {
        CreateContainer();
        InitializeLines();
    }

    void CreateContainer()
    {
        GameObject container = new GameObject("DimensionLines_" + gameObject.name);
        linesContainer = container.transform;
        linesContainer.position = transform.position;
        linesContainer.rotation = transform.rotation;
        linesContainer.localScale = Vector3.one;
        container.SetActive(false);
    }


    [ContextMenu("Draw Lines")]
    void InitializeLines()
    {
        Invoke(nameof(CalculateExternalCorners), 1f);
        Invoke(nameof(CreateAllBorderLines), 1f);
        Invoke(nameof(UpdateAllBorderLines), 1f);
        Invoke(nameof(SetupAllTextObjects), 1f);
        Invoke(nameof(ApplyGlobalMeasurementState), 1.1f);
    }

    void ApplyGlobalMeasurementState()
    {
        if (UIManagerAR.instance != null && UIManagerAR.instance.isMeasurementOn)
        {
            linesContainer.gameObject.SetActive(true);
            linesContainer.position = transform.position;
            linesContainer.rotation = transform.rotation;
            CalculateExternalCorners();
            UpdateAllBorderLines();
            UpdateDashes();
            SetupTextAlignmentAll();
            DisableSomeLines();
        }
    }

    void CalculateExternalCorners()
    {
        Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3 center = bounds.center;

        Vector3 scale = transform.lossyScale;

        externalCorners[0] = Vector3.Scale(new Vector3(min.x, min.y, min.z), scale);
        externalCorners[1] = Vector3.Scale(new Vector3(max.x, min.y, min.z), scale);
        externalCorners[2] = Vector3.Scale(new Vector3(max.x, min.y, max.z), scale);
        externalCorners[3] = Vector3.Scale(new Vector3(min.x, min.y, max.z), scale);
        externalCorners[4] = Vector3.Scale(new Vector3(min.x, max.y, min.z), scale);
        externalCorners[5] = Vector3.Scale(new Vector3(max.x, max.y, min.z), scale);
        externalCorners[6] = Vector3.Scale(new Vector3(max.x, max.y, max.z), scale);
        externalCorners[7] = Vector3.Scale(new Vector3(min.x, max.y, max.z), scale);

        center = Vector3.Scale(center, scale);

        for (int i = 0; i < externalCorners.Length; i++)
        {
            Vector3 direction = (externalCorners[i] - center).normalized;

            Vector3 offset = new Vector3(
                direction.x * axisOffset.x,
                direction.y * axisOffset.y,
                direction.z * axisOffset.z
            );

            externalCorners[i] += offset;
        }
    }

    void CreateAllBorderLines()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject lineObj = new GameObject("BorderLine_" + i);
            lineObj.transform.SetParent(linesContainer, false);
            lineObj.transform.localPosition = Vector3.zero;
            lineObj.transform.localRotation = Quaternion.identity;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = dashMat;
            lr.loop = false;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.alignment = LineAlignment.TransformZ;
            lineRenderers[i] = lr;

            AdjustDashes(lineRenderers[i], dashLength: 0.8f);
            lineObj.layer = LayerMask.NameToLayer("top");
        }
    }

    void LateUpdate()
    {
        if (linesContainer == null || !linesContainer.gameObject.activeSelf) return;
        if (lineRenderers[0] == null) return;

        linesContainer.position = transform.position;
        linesContainer.rotation = transform.rotation;
    }

    void AdjustDashes(LineRenderer lineRenderer, float dashLength = 1f)
    {
        lineRenderer.textureMode = LineTextureMode.Tile;

        Vector3 start = lineRenderer.GetPosition(0);
        Vector3 end = lineRenderer.GetPosition(1);

        float length = Vector3.Distance(start, end);

        float tileCount = length / dashLength;

        lineRenderer.textureScale = new Vector2(tileCount, 1f);
    }

    void UpdateDashes()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            if (lineRenderers[i] != null)
                AdjustDashes(lineRenderers[i], 0.8f);
        }
    }

    void UpdateAllBorderLines()
    {
        // Bottom face lines
        lineRenderers[0].SetPositions(new Vector3[] { externalCorners[0], externalCorners[1] });
        lineRenderers[1].SetPositions(new Vector3[] { externalCorners[1], externalCorners[2] });
        lineRenderers[2].SetPositions(new Vector3[] { externalCorners[2], externalCorners[3] });
        lineRenderers[3].SetPositions(new Vector3[] { externalCorners[3], externalCorners[0] });

        // Vertical lines (height)
        lineRenderers[4].SetPositions(new Vector3[] { externalCorners[0], externalCorners[4] });
        lineRenderers[5].SetPositions(new Vector3[] { externalCorners[2], externalCorners[6] });
    }

    public void UpdateTexts()
    {
        for(int i = 0; i < lineTexts.Length; i ++)
        {
            if (lineTexts[i] == null)
                continue;

            var tmp = lineTexts[i].GetComponentInChildren<TextMeshPro>();
            if (tmp == null)
            {
                Debug.LogError("TextMeshPro missing in prefab!");
                continue;
            }

            switch (i)
            {
                case 0:
                case 2:
                    tmp.text = textLength + " Inch";
                    break;

                case 1:
                    tmp.text = textHeight + " Inch";
                    break;

                case 3:
                    tmp.text = textDepth + " Inch";
                    break;

                case 4:
                    tmp.text = textHeight + " Inch";
                    break;

                case 5:
                    tmp.text = textDepth + " Inch";
                    break;
            }
        }
    }

    public void SetupAllTextObjects()
    {
        if (textPrefab == null) return;
        if (lineRenderers == null) return;

        for (int i = 0; i < lineRenderers.Length; i++)
        {
            if (lineRenderers[i] == null)
                continue;

            GameObject textObj = Instantiate(textPrefab);
            textObj.name = "LineText_" + i;
            textObj.transform.SetParent(lineRenderers[i].transform, false);

            lineTexts[i] = textObj;

            var tmp = textObj.GetComponentInChildren<TextMeshPro>();
            if (tmp == null)
            {
                Debug.LogError("TextMeshPro missing in prefab!");
                continue;
            }

            switch (i)
            {
                case 0:
                case 2:
                    tmp.text = textLength + " Inch";
                    break;

                case 1:
                    tmp.text = textHeight + " Inch";
                    break;

                case 3:
                    tmp.text = textDepth + " Inch";
                    break;

                case 4:
                    tmp.text = textHeight + " Inch";
                    break;

                case 5:
                    tmp.text = textDepth + " Inch";
                    break;
            }

                SetupTextAlignment(i);
        }
    }

    void SetupTextAlignmentAll()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            if (lineTexts[i] != null)
                SetupTextAlignment(i);
        }
    }

    void SetupTextAlignment(int i)
    {
        if (i >= lineRenderers.Length || lineTexts[i] == null) return;

        Vector3 localP0 = lineRenderers[i].GetPosition(0);
        Vector3 localP1 = lineRenderers[i].GetPosition(1);

        Vector3 worldP0 = lineRenderers[i].transform.TransformPoint(localP0);
        Vector3 worldP1 = lineRenderers[i].transform.TransformPoint(localP1);

        // Midpoint of the line
        Vector3 center = (worldP0 + worldP1) * 0.5f;
        Vector3 lineDirection = (worldP1 - worldP0).normalized;

        if (Mathf.Abs(lineDirection.x) > Mathf.Abs(lineDirection.y)) // Horizontal lines
        {
            Vector3 modelCenter = transform.position;

            // Calculate outward direction (horizontal only)
            Vector3 fromModelCenter = center - modelCenter;
            fromModelCenter.y = 0; // Remove vertical component
            fromModelCenter = fromModelCenter.normalized;

            // Apply both outward offset AND lower the text
            Vector3 labelPosition = center + (fromModelCenter * 0.05f) + (Vector3.up * -0.12f);

            lineTexts[i].transform.position = labelPosition;

            Vector3 forward = Vector3.Cross(Vector3.up, lineDirection);
            Quaternion baseRotation = Quaternion.LookRotation(forward, Vector3.up);
            Quaternion tilt = Quaternion.AngleAxis(-30f, Vector3.right);
            lineTexts[i].transform.rotation = baseRotation * tilt;
        }
        else // Vertical lines
        {
            Vector3 modelCenter = transform.position;
            Vector3 fromModelCenter = (center - modelCenter).normalized;
            Vector3 labelPosition = center + fromModelCenter * 0.3f;
            lineTexts[i].transform.position = labelPosition;

            Vector3 modelLocalDir = transform.InverseTransformDirection(fromModelCenter);
            if (modelLocalDir.x < 0)
            {
                lineTexts[i].transform.rotation = transform.rotation;
            }
            else
            {
                lineTexts[i].transform.rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
            }
        }
    }

    public void DisableSomeLines()
    {
        if (lineTexts[0] != null)
        {
            lineTexts[0].SetActive(false);
            lineRenderers[0].enabled = false;
        }
        if (lineTexts[1] != null)
        {
            lineTexts[1].SetActive(false);
            lineRenderers[1].enabled = false;

        }
        if (lineTexts[5] != null)
        {
            lineTexts[5].SetActive(false);
            lineRenderers[5].enabled = false;

        }

    }

    public void ToggleMeasurement()
    {
        if (linesContainer == null) return;
        if (lineRenderers[0] == null) return;

        bool isActive = !linesContainer.gameObject.activeSelf;
        linesContainer.gameObject.SetActive(isActive);

        if (isActive)
        {
            StartCoroutine(SetMeasurement());
        }
    }

    public IEnumerator SetMeasurement()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Setting measurement for " + gameObject.name);
        linesContainer.position = transform.position;
        linesContainer.rotation = transform.rotation;
        CalculateExternalCorners();
        UpdateAllBorderLines();
        UpdateDashes();
        SetupTextAlignmentAll();
        DisableSomeLines();
        
    }
}