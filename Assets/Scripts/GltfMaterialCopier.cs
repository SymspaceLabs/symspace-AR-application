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
}