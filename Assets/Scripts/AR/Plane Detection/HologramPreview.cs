using System.Collections.Generic;
using UnityEngine;

public class HologramPreview : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float minAlpha = 0.15f;
    [SerializeField] private float maxAlpha = 0.5f;
    [SerializeField] private Color hologramColor = new Color(0f, 1f, 0.8f, 0.4f); // Teal/cyan

    //private Material[] previewMaterials;

    public Material transparentMat;

    void Start()
    {
        //var renderers = GetComponentsInChildren<MeshRenderer>();
        //var mats = new List<Material>();
        //foreach (var r in renderers)
        //{
        //    var newMats = new Material(r.material);
        //    for (int i = 0; i < r.materials.Length; i++)
        //    {
        //        //newMats[i] = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        //        //newMats[i].SetFloat("_Surface", 1); // Transparent
        //        //newMats[i].SetFloat("_Blend", 0);   // Alpha
        //        //newMats[i].SetColor("_BaseColor", hologramColor);
        //        //newMats[i].renderQueue = 3000;
        //        //SetupTransparent(newMats[i]);
        //        //mats.Add(newMats[i]);
        //        //newMats[i] = transparentMat;
        //    }
        //    newMats.EnableKeyword("_EMISSION");
        //    newMats.SetColor("_EmissionColor", Color.yellow * 3f);
        //    r.material = newMats;
        //}
        //previewMaterials = mats.ToArray();

        // Disable colliders and scripts on the preview
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void SetupTransparent(Material mat)
    {
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
    }

    //void Update()
    //{
    //    float alpha = Mathf.Lerp(minAlpha, maxAlpha,
    //        (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

    //    foreach (var mat in previewMaterials)
    //    {
    //        Color c = mat.color;
    //        c.a = alpha;
    //        mat.color = c;
    //    }
    //}

    //public void OnDestroy()
    //{
    //    if (previewMaterials == null) return;
    //    foreach (var mat in previewMaterials)
    //        if (mat != null) Destroy(mat);
    //}
}