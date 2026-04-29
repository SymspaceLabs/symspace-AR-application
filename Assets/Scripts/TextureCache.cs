using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class TextureCache
{
    private static Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static IEnumerator LoadImage(string url, Image targetImage)
    {
        if (string.IsNullOrEmpty(url)) yield break;

        if (spriteCache.TryGetValue(url, out Sprite cached))
        {
            targetImage.sprite = cached;
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success) yield break;

        Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        texCache[url] = texture;

        Sprite sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        spriteCache[url] = sprite;

        targetImage.sprite = sprite;
    }

    public static void Clear()
    {
        foreach (var sprite in spriteCache.Values)
            if (sprite != null) Object.Destroy(sprite);
        foreach (var tex in texCache.Values)
            if (tex != null) Object.Destroy(tex);
        texCache.Clear();
        spriteCache.Clear();
    }
}