using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RulerTicksGraphic : Graphic
{
    [Range(1f, 200f)] public float pxPerUnit = 30f;
    [Range(1, 8)] public int ticksPerUnit = 2;
    [Range(1f, 20f)] public float majorEvery = 10f;
    public float tickWidth = 2f;
    public float midTickHeight = 16f;
    public float maxTickHeight = 28f;
    public Color midTickColor = new Color(0.55f, 0.58f, 0.65f, 1f);
    public Color maxTickColor = new Color(0.10f, 0.12f, 0.16f, 1f);
    [Range(0.5f, 2f)] public float fadeSpan = 1f;
    public bool fadeFarTicks = true;

    float currentValue;
    float minDraw;
    float maxDraw;

    public void SetValue(float v)
    {
        if (Mathf.Approximately(currentValue, v)) return;
        currentValue = v;
        SetVerticesDirty();
    }

    public void SetRange(float min, float max)
    {
        if (Mathf.Approximately(minDraw, min) && Mathf.Approximately(maxDraw, max)) return;
        minDraw = min;
        maxDraw = max;
        SetVerticesDirty();
    }

    void OnValidate()
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        float halfW = rect.width * 0.5f;

        float minTick = minDraw + Mathf.Epsilon;
        float maxTick = maxDraw - Mathf.Epsilon;

        int first = Mathf.FloorToInt((currentValue - halfW / pxPerUnit) * ticksPerUnit);
        int last = Mathf.CeilToInt((currentValue + halfW / pxPerUnit) * ticksPerUnit);

        int absFirst = Mathf.CeilToInt(minTick * ticksPerUnit - Mathf.Epsilon);
        int absLast = Mathf.FloorToInt(maxTick * ticksPerUnit + Mathf.Epsilon);
        first = Mathf.Max(first, absFirst);
        last = Mathf.Min(last, absLast);

        if (first > last) return;

        for (int i = first; i <= last; i++)
        {
            float x = ((float)i / ticksPerUnit - currentValue) * pxPerUnit;
            if (x < -halfW || x > halfW) continue;

            float height;
            Color tickColor;
            if (i % ticksPerUnit == 0)
            {
                height = maxTickHeight;
                tickColor = maxTickColor;
            }
            else
            {
                height = midTickHeight;
                tickColor = midTickColor;
            }

            float alpha = 1f;
            if (fadeFarTicks)
            {
                float dist = Mathf.Abs(x) / (halfW * fadeSpan);
                alpha = 1f - Mathf.Clamp01(dist);
                if (alpha <= 0.02f) continue;
            }

            Color c = tickColor;
            c.a *= alpha;

            float halfW2 = tickWidth * 0.5f;
            float halfH2 = height * 0.5f;

            int idx = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.position = new Vector3(x - halfW2, -halfH2, 0f);
            v.color = c;
            vh.AddVert(v);
            v.position = new Vector3(x + halfW2, -halfH2, 0f);
            vh.AddVert(v);
            v.position = new Vector3(x + halfW2, halfH2, 0f);
            vh.AddVert(v);
            v.position = new Vector3(x - halfW2, halfH2, 0f);
            vh.AddVert(v);
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }
    }
}
