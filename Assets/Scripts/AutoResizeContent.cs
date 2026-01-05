using UnityEngine;

public class AutoResizeContent : MonoBehaviour
{
    public RectTransform content;

    void LateUpdate()
    {
        if (content == null) return;
        ResizeToFitChildren();
    }

    void ResizeToFitChildren()
    {
        if (content.childCount == 0) return;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (RectTransform child in content)
        {
            if (!child.gameObject.activeInHierarchy) continue;

            // Get world corners of each child
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);

            // Transform to content local space
            for (int i = 0; i < 4; i++)
                corners[i] = content.InverseTransformPoint(corners[i]);

            // Track min/max Y positions
            if (corners[0].y < minY) minY = corners[0].y; // bottom
            if (corners[1].y > maxY) maxY = corners[1].y; // top
        }

        float height = Mathf.Abs(maxY - minY);
        Vector2 size = content.sizeDelta;
        size.y = height;
        content.sizeDelta = size;
    }
}
