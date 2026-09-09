using UnityEngine;

public class GradientInkTest : MonoBehaviour
{
    [Range(32, 192)] public int fieldSize = 128;
    [Range(8, 96)] public int segments = 48;
    [Range(1f, 20f)] public float cameraDistance = 8f;
    [Range(1f, 1.2f)] public float margin = 1.06f;
    [Range(0.04f, 0.3f)] public float pushRadius = 0.14f;
    [Range(0.2f, 3f)] public float dragStrength = 1.2f;
    [Range(0.2f, 3f)] public float swirlStrength = 1.4f;
    [Range(0f, 0.2f)] public float vortexConfinement = 0.04f;
    [Range(0f, 0.05f)] public float velocityDiffusion = 0.02f;
    [Range(0.1f, 2f)] public float velocityDecay = 0.5f;
    [Range(0.3f, 3f)] public float maxVelocity = 1.5f;
    [Range(0f, 0.02f)] public float colorDiffuse = 0.0008f;
    [Range(0f, 0.05f)] public float recover = 0.02f;
    [Range(0, 12)] public int pressureIterations = 4;
    [Range(0f, 1f)] public float ambientSwirl = 0.25f;
    public Material gradientMaterial;

    static Color C(float r, float g, float b) => new Color(r, g, b);

    static readonly Color[,] VividGrid =
    {
        { C(0.12f, 0.30f, 0.95f), C(0.20f, 0.65f, 0.95f), C(0.05f, 0.80f, 0.65f), C(0.25f, 0.80f, 0.35f) },
        { C(0.55f, 0.25f, 0.95f), C(0.25f, 0.40f, 0.95f), C(0.10f, 0.75f, 0.85f), C(0.60f, 0.85f, 0.25f) },
        { C(0.95f, 0.30f, 0.70f), C(0.70f, 0.30f, 0.95f), C(0.15f, 0.75f, 0.90f), C(0.95f, 0.85f, 0.15f) },
        { C(1.00f, 0.35f, 0.55f), C(1.00f, 0.55f, 0.15f), C(1.00f, 0.80f, 0.20f), C(0.95f, 0.25f, 0.25f) }
    };

    Color[] fieldA, fieldB;
    Vector2[] velA, velB;
    float[] curl;
    float[] pressure;
    Mesh mesh;
    Vector2[] uvs;
    Color[] vertexColors;
    Camera cam;
    bool initialized;
    bool dragging;
    Vector2 lastMouseUV;
    float quadZ;
    int N;
    float invN;

    void Update()
    {
        if (!initialized) Init();
        FitToCamera();

        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        Step(dt);
        UpdateDisplay();
    }

    void Init()
    {
        if (initialized) return;

        cam = GetComponentInParent<Camera>();
        if (cam == null) cam = Camera.main;

        N = fieldSize;
        invN = 1f / (N - 1);
        fieldA = new Color[N * N];
        fieldB = new Color[N * N];
        velA = new Vector2[N * N];
        velB = new Vector2[N * N];
        curl = new float[N * N];
        pressure = new float[N * N];

        System.Random rng = new System.Random(987654);
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Vector2 uv = new Vector2(x * invN, y * invN);
                Color c = BaseAt(uv.x, uv.y);
                c.r += (float)(rng.NextDouble() - 0.5) * 0.06f;
                c.g += (float)(rng.NextDouble() - 0.5) * 0.06f;
                c.b += (float)(rng.NextDouble() - 0.5) * 0.06f;
                fieldA[y * N + x] = c;
            }
        }

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
        mesh.name = "GradientInkTestMesh";
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

        initialized = true;
        lastMouseUV = MouseUV();
        UpdateDisplay();
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
            quadZ = 0f;
        }
        else
        {
            float halfH = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraDistance * margin;
            float halfW = halfH * cam.aspect;
            transform.localPosition = new Vector3(0f, 0f, cameraDistance);
            transform.localScale = new Vector3(halfW * 2f, halfH * 2f, 1f);
            quadZ = cameraDistance;
        }
    }

    Vector2 MouseUV()
    {
        if (cam == null) return Vector2.one * 0.5f;
        Plane plane = new Plane(cam.transform.forward, cam.transform.position + cam.transform.forward * quadZ);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float t))
        {
            Vector3 world = ray.GetPoint(t);
            Vector3 local = transform.InverseTransformPoint(world);
            return new Vector2(Mathf.Clamp01(local.x + 0.5f), Mathf.Clamp01(local.y + 0.5f));
        }
        return Vector2.one * 0.5f;
    }

    Vector2 CellUV(int x, int y) => new Vector2(x * invN, y * invN);

    Color BaseAt(float u, float v)
    {
        u = Mathf.Clamp01(u) * 3f;
        v = Mathf.Clamp01(v) * 3f;
        int i = Mathf.Min(Mathf.FloorToInt(u), 2);
        int j = Mathf.Min(Mathf.FloorToInt(v), 2);
        float s = u - i;
        float t = v - j;
        Color c00 = VividGrid[j, i];
        Color c10 = VividGrid[j, i + 1];
        Color c01 = VividGrid[j + 1, i];
        Color c11 = VividGrid[j + 1, i + 1];
        return Color.Lerp(Color.Lerp(c00, c10, s), Color.Lerp(c01, c11, s), t);
    }

    void Step(float dt)
    {
        Vector2 mouseUV = MouseUV();
        bool down = Input.GetMouseButton(0);
        Vector2 deltaUV = down && dragging ? mouseUV - lastMouseUV : Vector2.zero;
        dragging = down;
        lastMouseUV = mouseUV;

        AddVelocity(mouseUV, deltaUV, dt);

        AdvectVel(dt);
        SwapVel();

        VorticityConfinement(dt);

        if (pressureIterations > 0) Project();

        DiffuseVelocity();

        float decay = Mathf.Exp(-velocityDecay * dt);
        for (int i = 0; i < velA.Length; i++)
        {
            velA[i] *= decay;
            Vector2 v = velA[i];
            float m = v.magnitude;
            if (m > maxVelocity) velA[i] = v * (maxVelocity / m);
        }

        AdvectColor(dt);
        SwapField();

        DiffuseAndRecover(dt);
    }

    void AddVelocity(Vector2 mouseUV, Vector2 deltaUV, float dt)
    {
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Vector2 uv = CellUV(x, y);
                int i = y * N + x;

                Vector2 d = uv - new Vector2(0.5f, 0.5f);
                float fall = Mathf.Exp(-d.sqrMagnitude * 3f);
                velA[i] += new Vector2(-d.y, d.x) * (ambientSwirl * 0.5f) * fall * dt;
            }
        }

        if (deltaUV.sqrMagnitude < 1e-10f) return;

        float speed = deltaUV.magnitude / Mathf.Max(dt, 1e-4f);
        Vector2 dir = deltaUV / deltaUV.magnitude;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        float inject = Mathf.Min(speed * 0.5f, 1.5f);
        float R = pushRadius;
        float R2 = R * R;

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Vector2 uv = CellUV(x, y);
                Vector2 to = uv - mouseUV;
                float r2 = to.x * to.x + to.y * to.y;
                if (r2 > R2 * 9f) continue;
                float r = Mathf.Sqrt(r2);
                float fall = Mathf.Exp(-r2 / R2);
                Vector2 radialTangent = new Vector2(-to.y, to.x) / (r + 0.2f);
                Vector2 add = (dir * dragStrength + radialTangent * swirlStrength + perp * swirlStrength * 0.4f) * fall * inject;
                velA[y * N + x] += add;
            }
        }
    }

    Vector2 SampleVel(float u, float v)
    {
        u = Mathf.Clamp01(u) * (N - 1);
        v = Mathf.Clamp01(v) * (N - 1);
        int x = Mathf.FloorToInt(u);
        int y = Mathf.FloorToInt(v);
        int x1 = Mathf.Min(x + 1, N - 1);
        int y1 = Mathf.Min(y + 1, N - 1);
        float fx = u - x;
        float fy = v - y;
        Vector2 v00 = velA[y * N + x];
        Vector2 v10 = velA[y * N + x1];
        Vector2 v01 = velA[y1 * N + x];
        Vector2 v11 = velA[y1 * N + x1];
        return Vector2.Lerp(Vector2.Lerp(v00, v10, fx), Vector2.Lerp(v01, v11, fx), fy);
    }

    Color SampleField(float u, float v)
    {
        u = Mathf.Clamp01(u) * (N - 1);
        v = Mathf.Clamp01(v) * (N - 1);
        int x = Mathf.FloorToInt(u);
        int y = Mathf.FloorToInt(v);
        int x1 = Mathf.Min(x + 1, N - 1);
        int y1 = Mathf.Min(y + 1, N - 1);
        float fx = u - x;
        float fy = v - y;
        Color c00 = fieldA[y * N + x];
        Color c10 = fieldA[y * N + x1];
        Color c01 = fieldA[y1 * N + x];
        Color c11 = fieldA[y1 * N + x1];
        return Color.Lerp(Color.Lerp(c00, c10, fx), Color.Lerp(c01, c11, fx), fy);
    }

    void AdvectVel(float dt)
    {
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Vector2 uv = CellUV(x, y);
                Vector2 v = velA[y * N + x];
                Vector2 src = uv - v * dt;
                velB[y * N + x] = SampleVel(src.x, src.y);
            }
        }
    }

    void AdvectColor(float dt)
    {
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Vector2 uv = CellUV(x, y);
                Vector2 v = velA[y * N + x];
                Vector2 src = uv - v * dt;
                fieldB[y * N + x] = SampleField(src.x, src.y);
            }
        }
    }

    void SwapVel()
    {
        Vector2[] tmp = velA;
        velA = velB;
        velB = tmp;
    }

    void SwapField()
    {
        Color[] tmp = fieldA;
        fieldA = fieldB;
        fieldB = tmp;
    }

    void VorticityConfinement(float dt)
    {
        for (int y = 1; y < N - 1; y++)
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x;
                float dwdx = (velA[i + 1].y - velA[i - 1].y) * 0.5f;
                float dwdy = (velA[i + N].x - velA[i - N].x) * 0.5f;
                curl[i] = dwdx - dwdy;
            }
        }

        for (int y = 1; y < N - 1; y++)
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x;
                float gx = (curl[i + 1] - curl[i - 1]) * 0.5f;
                float gy = (curl[i + N] - curl[i - N]) * 0.5f;
                float len = Mathf.Sqrt(gx * gx + gy * gy) + 1e-5f;
                float f = vortexConfinement * curl[i];
                velA[i].x += f * (gy / len);
                velA[i].y += f * (-gx / len);
            }
        }
    }

    void DiffuseVelocity()
    {
        if (velocityDiffusion <= 0f) return;
        for (int y = 1; y < N - 1; y++)
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x;
                Vector2 avg = (velA[i - 1] + velA[i + 1] + velA[i - N] + velA[i + N]) * 0.25f;
                velA[i] = Vector2.Lerp(velA[i], avg, velocityDiffusion);
            }
        }
    }

    void Project()
    {
        for (int y = 1; y < N - 1; y++)
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x;
                pressure[i] = 0f;
            }
        }

        for (int iter = 0; iter < pressureIterations; iter++)
        {
            for (int y = 1; y < N - 1; y++)
            {
                for (int x = 1; x < N - 1; x++)
                {
                    int i = y * N + x;
                    float div = ((velA[i + 1].x - velA[i - 1].x) + (velA[i + N].y - velA[i - N].y)) * 0.5f;
                    pressure[i] = (pressure[i - 1] + pressure[i + 1] + pressure[i - N] + pressure[i + N] - div) * 0.25f;
                }
            }
            for (int y = 0; y < N; y++)
            {
                int iTop = y;
                int iBot = (N - 1) * N + y;
                pressure[iTop] = pressure[iTop + N];
                pressure[iBot] = pressure[iBot - N];
            }
            for (int x = 0; x < N; x++)
            {
                int iL = x * N;
                int iR = x * N + (N - 1);
                pressure[iL] = pressure[iL + 1];
                pressure[iR] = pressure[iR - 1];
            }
        }

        for (int y = 1; y < N - 1; y++)
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x;
                velA[i].x -= (pressure[i + 1] - pressure[i - 1]) * 0.5f;
                velA[i].y -= (pressure[i + N] - pressure[i - N]) * 0.5f;
            }
        }

        for (int i = 0; i < N; i++)
        {
            velA[i] = Vector2.zero;
            velA[(N - 1) * N + i] = Vector2.zero;
            velA[i * N] = Vector2.zero;
            velA[i * N + (N - 1)] = Vector2.zero;
        }
    }

    void DiffuseAndRecover(float dt)
    {
        float k = colorDiffuse * dt * 60f;
        float rk = recover * dt * 60f;
        for (int y = 1; y < N - 1; y++)
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x;
                Vector2 uv = CellUV(x, y);
                Color c = fieldA[i];
                Color lap = (fieldA[i - 1] + fieldA[i + 1] + fieldA[i - N] + fieldA[i + N] - c * 4f) * k;
                c += lap;
                c = Color.Lerp(c, BaseAt(uv.x, uv.y), Mathf.Min(rk, 1f));
                fieldA[i] = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b));
            }
        }
    }

    void UpdateDisplay()
    {
        for (int idx = 0; idx < uvs.Length; idx++)
        {
            Vector2 uv = uvs[idx];
            vertexColors[idx] = SampleField(uv.x, uv.y);
        }
        mesh.colors = vertexColors;
    }
}
