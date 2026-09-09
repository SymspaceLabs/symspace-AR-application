using UnityEngine;

public class FlowingGradientBackground : MonoBehaviour
{
    public enum GradientPalette { Symspace, Pastel }

    public static bool ReduceMotionEnabled;

    [Range(4, 64)] public int segments = 32;
    [Range(0f, 0.25f)] public float amplitude = 0.18f;
    [Range(4f, 16f)] public float cycleSeconds = 8f;
    [Range(1f, 20f)] public float cameraDistance = 8f;
    [Range(1f, 1.2f)] public float margin = 1.06f;
    public Material gradientMaterial;
    public GradientPalette palette = GradientPalette.Symspace;
    [Range(0.5f, 1.5f)] public float blobScale = 1f;
    [Range(0f, 3f)] public float blobIntensity = 1f;

    [Header("Pastel Blob Placement")]
    [Tooltip("UV space: x 0=left 1=right, y 0=bottom 1=top.")]
    public Vector2 blueTopRight = new Vector2(0.69f, 0.90f);
    public Vector2 cyanCenterRight = new Vector2(0.80f, 0.50f);
    public Vector2 lavenderBottomLeft = new Vector2(0.08f, 0.12f);
    public Vector2 mintBottomRight = new Vector2(0.90f, 0.20f);
    public bool animate = false;

    [Tooltip("Circular size of each blob in UV units (0.02 small ... 0.30 large).")]
    public float blueSize = 0.13f;
    public float cyanSize = 0.11f;
    public float lavenderSize = 0.10f;
    public float mintSize = 0.12f;

    struct ControlPoint
    {
        public Vector2 basePos;
        public Color color;
        public float fx;
        public float fy;
        public float px;
        public float py;
    }

    static readonly Color[] RampStops =
    {
        new Color(0.0157f, 0.0275f, 0.0549f),
        new Color(0.0000f, 0.2510f, 0.5020f),
        new Color(0.0118f, 0.4000f, 0.9961f),
        new Color(0.1529f, 0.7255f, 0.9608f),
        new Color(0.7020f, 0.8510f, 1.0000f)
    };

    static readonly Color[,] PaletteGrid =
    {
        { RampStops[0], RampStops[1], RampStops[2], RampStops[3] },
        { RampStops[1], RampStops[2], RampStops[3], RampStops[3] },
        { RampStops[2], RampStops[2], RampStops[3], RampStops[4] },
        { RampStops[1], RampStops[3], RampStops[3], RampStops[4] }
    };

    static readonly Color[] PastelStops =
    {
        new Color(0.6470f, 0.7804f, 1.0000f),
        new Color(0.6039f, 0.8941f, 0.9725f),
        new Color(0.7569f, 0.7020f, 1.0000f),
        new Color(0.6627f, 0.9294f, 0.8235f)
    };

    struct FlowBlob
    {
        public Vector2 center;
        public Color color;
        public float sigma;
        public float fx;
        public float fy;
        public float px;
        public float py;
    }

    static readonly FlowBlob[] PastelBlobs =
    {
        new FlowBlob { center = new Vector2(0.69f, 0.90f), color = PastelStops[0], sigma = 0.13f, fx = 0.60f, fy = 0.90f, px = 0.0f,  py = 1.2f },
        new FlowBlob { center = new Vector2(0.80f, 0.50f), color = PastelStops[1], sigma = 0.11f, fx = 0.80f, fy = 0.50f, px = 2.0f,  py = 0.5f },
        new FlowBlob { center = new Vector2(0.08f, 0.12f), color = PastelStops[2], sigma = 0.10f, fx = 0.50f, fy = 0.70f, px = 3.5f,  py = 1.8f },
        new FlowBlob { center = new Vector2(0.90f, 0.20f), color = PastelStops[3], sigma = 0.12f, fx = 0.90f, fy = 0.60f, px = 4.2f,  py = 0.8f }
    };

    ControlPoint[] controlPoints;
    Mesh mesh;
    Vector2[] uvs;
    Color[] vertexColors;
    Camera cam;
    bool initialized;
    bool staticDrawn;

    void Awake()
    {
        Init();
    }

    void Update()
    {
        if (!initialized) Init();
        FitToCamera();
        if (ReduceMotionEnabled)
        {
            if (!staticDrawn)
            {
                DrawFrame(0f);
                staticDrawn = true;
            }
            return;
        }
        DrawFrame(Time.time);
    }

    void OnDisable()
    {
        staticDrawn = false;
    }

    void OnValidate()
    {
        if (!initialized) Init();
        DrawFrame(0f);
    }

    void Init()
    {
        if (initialized) return;

        cam = GetComponentInParent<Camera>();
        if (cam == null) cam = Camera.main;

        int count = (segments + 1) * (segments + 1);
        Vector3[] verts = new Vector3[count];
        uvs = new Vector2[count];
        vertexColors = new Color[count];
        int[] tris = new int[segments * segments * 6];

        for (int j = 0; j <= segments; j++)
        {
            for (int i = 0; i <= segments; i++)
            {
                float u = (float)i / segments;
                float v = (float)j / segments;
                int idx = j * (segments + 1) + i;
                verts[idx] = new Vector3(u - 0.5f, v - 0.5f, 0f);
                uvs[idx] = new Vector2(u, v);
            }
        }

        int tri = 0;
        for (int j = 0; j < segments; j++)
        {
            for (int i = 0; i < segments; i++)
            {
                int a = j * (segments + 1) + i;
                int b = a + 1;
                int c = a + (segments + 1);
                int d = c + 1;
                tris[tri++] = a; tris[tri++] = c; tris[tri++] = b;
                tris[tri++] = b; tris[tri++] = c; tris[tri++] = d;
            }
        }

        mesh = new Mesh();
        mesh.name = "FlowingGradientMesh";
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 0.1f));
        mesh.MarkDynamic();

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
        if (mr.sharedMaterial == null)
        {
            mr.sharedMaterial = gradientMaterial != null
                ? gradientMaterial
                : new Material(Shader.Find("Symspace/FlowingGradient"));
        }

        if (palette != GradientPalette.Pastel)
        {
            System.Random rng = new System.Random(20260806);
            controlPoints = new ControlPoint[16];
            float w = (2f * Mathf.PI) / cycleSeconds;
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float u = i / 3f;
                    float v = j / 3f;
                    ControlPoint cp;
                    cp.basePos = new Vector2(u, v);
                    cp.color = PaletteGrid[j, i] * (0.9f + (float)rng.NextDouble() * 0.2f);
                    cp.fx = w * (0.6f + (float)rng.NextDouble() * 0.9f);
                    cp.fy = w * (0.6f + (float)rng.NextDouble() * 0.9f);
                    cp.px = (float)rng.NextDouble() * Mathf.PI * 2f;
                    cp.py = (float)rng.NextDouble() * Mathf.PI * 2f;
                    controlPoints[j * 4 + i] = cp;
                }
            }
        }

        initialized = true;
        DrawFrame(0f);
    }

    void FitToCamera()
    {
        if (cam == null) return;
        if (cam.orthographic)
        {
            float halfH = cam.orthographicSize * margin;
            float halfW = halfH * cam.aspect;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(halfW * 2f, halfH * 2f, 1f);
        }
        else
        {
            float halfH = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraDistance * margin;
            float halfW = halfH * cam.aspect;
            transform.localPosition = new Vector3(0f, 0f, cameraDistance);
            transform.localScale = new Vector3(halfW * 2f, halfH * 2f, 1f);
        }
    }

    void DrawFrame(float t)
    {
        if (palette == GradientPalette.Pastel)
        {
            DrawPastelFrame(t);
            return;
        }

        Vector2[] drift = new Vector2[16];
        for (int k = 0; k < 16; k++)
        {
            ControlPoint cp = controlPoints[k];
            drift[k] = new Vector2(
                amplitude * Mathf.Cos(t * cp.fx + cp.px),
                amplitude * Mathf.Cos(t * cp.fy + cp.py));
        }

        for (int idx = 0; idx < uvs.Length; idx++)
        {
            Vector2 uv = uvs[idx];
            Vector2 d = SampleDrift(uv.x, uv.y, drift);
            vertexColors[idx] = SampleField(uv.x + d.x, uv.y + d.y);
        }

        mesh.colors = vertexColors;
    }

    void DrawPastelFrame(float t)
    {
        float aspect = cam != null ? cam.aspect : 1f;
        float w = (2f * Mathf.PI) / cycleSeconds;
        float amp = amplitude * 0.4f;

        Vector2[] baseCenters = { blueTopRight, cyanCenterRight, lavenderBottomLeft, mintBottomRight };
        Vector2[] centers = new Vector2[4];
        for (int b = 0; b < 4; b++)
        {
            centers[b] = baseCenters[b];
            if (animate)
            {
                FlowBlob blob = PastelBlobs[b];
                centers[b] += new Vector2(
                    amp * Mathf.Cos(t * w * blob.fx + blob.px),
                    amp * Mathf.Cos(t * w * blob.fy + blob.py));
            }
        }

        float[] sizes = { blueSize, cyanSize, lavenderSize, mintSize };
        for (int idx = 0; idx < uvs.Length; idx++)
        {
            Vector2 uv = uvs[idx];
            float r = 1f, g = 1f, b = 1f;
            for (int k = 0; k < 4; k++)
            {
                float dx = (uv.x - centers[k].x) * aspect;
                float dy = uv.y - centers[k].y;
                float d2 = dx * dx + dy * dy;
                float s = sizes[k] * blobScale;
                float fall = Mathf.Exp(-d2 / (2f * s * s));
                Color col = PastelBlobs[k].color;
                r += (col.r - 1f) * fall * blobIntensity;
                g += (col.g - 1f) * fall * blobIntensity;
                b += (col.b - 1f) * fall * blobIntensity;
            }
            vertexColors[idx] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }

        mesh.colors = vertexColors;
    }

    Vector2 SampleDrift(float u, float v, Vector2[] drift)
    {
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        float fu = u * 3f;
        float fv = v * 3f;
        int i = Mathf.Min(Mathf.FloorToInt(fu), 2);
        int j = Mathf.Min(Mathf.FloorToInt(fv), 2);
        float s = fu - i;
        float t = fv - j;
        Vector2 d00 = drift[j * 4 + i];
        Vector2 d10 = drift[j * 4 + i + 1];
        Vector2 d01 = drift[(j + 1) * 4 + i];
        Vector2 d11 = drift[(j + 1) * 4 + i + 1];
        return (1f - s) * (1f - t) * d00 + s * (1f - t) * d10 + (1f - s) * t * d01 + s * t * d11;
    }

    Color SampleField(float u, float v)
    {
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        float fu = u * 3f;
        float fv = v * 3f;
        int i = Mathf.Min(Mathf.FloorToInt(fu), 2);
        int j = Mathf.Min(Mathf.FloorToInt(fv), 2);
        float s = fu - i;
        float t = fv - j;
        Color c00 = controlPoints[j * 4 + i].color;
        Color c10 = controlPoints[j * 4 + i + 1].color;
        Color c01 = controlPoints[(j + 1) * 4 + i].color;
        Color c11 = controlPoints[(j + 1) * 4 + i + 1].color;
        return (1f - s) * (1f - t) * c00 + s * (1f - t) * c10 + (1f - s) * t * c01 + s * t * c11;
    }
}
