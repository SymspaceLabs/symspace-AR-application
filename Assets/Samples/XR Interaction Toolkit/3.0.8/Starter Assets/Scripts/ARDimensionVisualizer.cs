using UnityEngine;
using System;
using TMPro;

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

    public float textWidth;
    public float textHeight;
    public float textDepth;

    public float textOffset = 0.05f;
    public float verticalTextOffset = 0.01f;

    private LineRenderer[] lineRenderers = new LineRenderer[6];
    private GameObject[] lineTexts = new GameObject[6];
    private Vector3[] externalCorners = new Vector3[8];

    void Start()
    {
        Invoke(nameof(CalculateExternalCorners), 1f);
        Invoke(nameof(CreateAllBorderLines), 1f);
        Invoke(nameof(UpdateAllBorderLines), 1f);
        Invoke(nameof(SetupAllTextObjects), 1f);
        if(!FindFirstObjectByType<UIManagerAR>().isMeasurementOn)
            Invoke(nameof(ToggleMeasurement), 1f);

        Invoke(nameof(DisableSomeLines), 1);
    }

    void CalculateExternalCorners()
    {
        Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3 center = bounds.center;

        // Apply lossy scale to each coordinate manually
        Vector3 scale = transform.lossyScale;

        // 8 scaled corners (local space + parent scale applied)
        //for(int i = 0; i < externalCorners.Length; i++)
        //    externalCorners[i] = Vector3.Scale(new Vector3(min.x, min.y, min.z), scale);

        externalCorners[0] = Vector3.Scale(new Vector3(min.x, min.y, min.z), scale);
        externalCorners[1] = Vector3.Scale(new Vector3(max.x, min.y, min.z), scale);
        externalCorners[2] = Vector3.Scale(new Vector3(max.x, min.y, max.z), scale);
        externalCorners[3] = Vector3.Scale(new Vector3(min.x, min.y, max.z), scale);
        externalCorners[4] = Vector3.Scale(new Vector3(min.x, max.y, min.z), scale);
        externalCorners[5] = Vector3.Scale(new Vector3(max.x, max.y, min.z), scale);
        externalCorners[6] = Vector3.Scale(new Vector3(max.x, max.y, max.z), scale);
        externalCorners[7] = Vector3.Scale(new Vector3(min.x, max.y, max.z), scale);

        // Also scale the center
        center = Vector3.Scale(center, scale);

        for (int i = 0; i < externalCorners.Length; i++)
        {
            Vector3 direction = (externalCorners[i] - center).normalized;

            // Apply per-axis offset scaling
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
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;
            lineObj.transform.localRotation = Quaternion.identity;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = dashMat;
            lr.loop = false;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lineRenderers[i] = lr;

            AdjustDashes(lineRenderers[i], dashLength: 0.8f);
        }
    }

    void AdjustDashes(LineRenderer lineRenderer, float dashLength = 1f)
    {
        // Ensure tiling mode is enabled
        lineRenderer.textureMode = LineTextureMode.Tile;

        // Get the two points
        Vector3 start = lineRenderer.GetPosition(0);
        Vector3 end = lineRenderer.GetPosition(1);

        // Calculate length between them
        float length = Vector3.Distance(start, end);

        // Compute how many dashes should fit
        float tileCount = length / dashLength;

        // Set tiling (no need to modify the material)
        lineRenderer.textureScale = new Vector2(tileCount, 1f);
    }

    void UpdateAllBorderLines()
    {
        Vector3 offset = transform.TransformDirection(Vector3.forward) * lineOffset;

        // Bottom face lines
        lineRenderers[0].SetPositions(new Vector3[] { externalCorners[0] /*+ offset*/, externalCorners[1]/* + offset */}); // Width (front)
        lineRenderers[1].SetPositions(new Vector3[] { externalCorners[1] /*+ offset*/, externalCorners[2] }); // Depth (right)
        lineRenderers[2].SetPositions(new Vector3[] { externalCorners[2], externalCorners[3] }); // Width (back)
        lineRenderers[3].SetPositions(new Vector3[] { externalCorners[3], externalCorners[0] }); // Depth (left)

        // Vertical lines (height)
        lineRenderers[4].SetPositions(new Vector3[] { externalCorners[0], externalCorners[4] }); // Height (front-left)
        lineRenderers[5].SetPositions(new Vector3[] { externalCorners[2], externalCorners[6] }); // Height (back-right)
    }

    void SetupAllTextObjects()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            if (textPrefab == null) continue;

            GameObject textObj = Instantiate(textPrefab);
            textObj.name = "LineText_" + i;
            textObj.transform.SetParent(lineRenderers[i].transform, false);
            lineTexts[i] = textObj;

            switch (i)
            {
                case 0: // Width (front)
                    lineTexts[i].GetComponentInChildren<TextMeshPro>().text = textWidth + " Inch";
                    break;
                case 2: // Width (back)
                    lineTexts[i].GetComponentInChildren<TextMeshPro>().text = textWidth + " Inch";
                    break;
                case 1: // Depth (right)
                    lineTexts[i].GetComponentInChildren<TextMeshPro>().text = textHeight + " Inch";
                    break;
                case 3: // Depth (left)
                    lineTexts[i].GetComponentInChildren<TextMeshPro>().text = textHeight + " Inch";
                    break;
                case 4: // Height (front-left)
                    lineTexts[i].GetComponentInChildren<TextMeshPro>().text = textDepth + " Inch";
                    break;
                case 5: // Height (back-right)
                    lineTexts[i].GetComponentInChildren<TextMeshPro>().text = textDepth + " Inch";
                    break;
            }

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

            // rotation code
            // Calculate base rotation to align with the line direction
            Vector3 forward = Vector3.Cross(Vector3.up, lineDirection);
            Quaternion baseRotation = Quaternion.LookRotation(forward, Vector3.up);

            // Apply the tilt -30 degrees around local X axis
            Quaternion tilt = Quaternion.AngleAxis(-30f, Vector3.right);

            // Combine the base rotation and tilt
            Quaternion finalRotation = baseRotation * tilt;

            // Correct the angle discrepancy
            finalRotation = Quaternion.Euler(finalRotation.eulerAngles.x, finalRotation.eulerAngles.y, 0f);

            // Set the final corrected rotation to the text
            lineTexts[i].transform.rotation = finalRotation;
        }
        else // Vertical lines (unchanged)
        {
            Vector3 modelCenter = transform.position;
            Vector3 fromModelCenter = (center - modelCenter).normalized;
            Vector3 labelPosition = center + fromModelCenter * 0.4f;
            lineTexts[i].transform.position = labelPosition;

            if (lineTexts[i].transform.localPosition.x < 0)
            {
                lineTexts[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                lineTexts[i].transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    public void DisableSomeLines()
    {
        lineTexts[0].SetActive(false);
        lineTexts[1].SetActive(false);
        lineTexts[5].SetActive(false);

        lineRenderers[0].enabled = false;
        lineRenderers[1].enabled = false;
        lineRenderers[5].enabled = false;
    }

    public void ToggleMeasurement()
    {
            foreach (var line in lineTexts)
                line.SetActive(!line.activeSelf);

            foreach (var lines in lineRenderers)
                lines.enabled = !lines.enabled;

        DisableSomeLines();
    }
}