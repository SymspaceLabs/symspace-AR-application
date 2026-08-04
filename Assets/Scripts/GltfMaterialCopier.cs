using UnityEngine;

public static class GltfMaterialCopier
{
    public static void CopyAllTextures(Material src, Material dst)
    {
        Shader shader = src.shader;

        int count = shader.GetPropertyCount();

        for (int i = 0; i < count; i++)
        {
            if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
            {
                string name = shader.GetPropertyName(i);
                Texture tex = src.GetTexture(name);

                if (tex != null)
                    dst.SetTexture(name, tex);
            }
        }
    }

    public static void DeleteTextures(Material mat)
    {
        mat.SetTexture("_BaseMap", null);
        mat.SetTexture("_BumpMap", null);
        mat.SetTexture("_MetallicGlossMap", null);
        mat.SetTexture("_OcclusionMap", null);
        mat.SetTexture("_EmissionMap", null);

        mat.DisableKeyword("_NORMALMAP");
        mat.DisableKeyword("_EMISSION");
    }
}