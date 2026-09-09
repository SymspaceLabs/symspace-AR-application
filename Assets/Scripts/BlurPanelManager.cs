using UnityEngine;
using UnityEngine.SceneManagement;

public static class BlurPanelManager
{
    private static int covers;
    private static bool sceneHooked;

    public static bool IsCovered => covers > 0;

    public static void Cover()
    {
        HookSceneEvents();

        if (covers == 0)
            HideAll();
        covers++;
    }

    public static void Uncover()
    {
        if (covers <= 0)
        {
            covers = 0;
            return;
        }

        covers--;
        if (covers == 0)
            RestoreAll();
    }

    private static BlogsUI FindBlogsUI()
    {
        var blogs = Object.FindObjectsByType<BlogsUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        return blogs != null && blogs.Length > 0 ? blogs[0] : null;
    }

    private static void HideAll()
    {
        var blogs = FindBlogsUI();
        if (blogs != null) blogs.HideBlurPanels();
    }

    private static void RestoreAll()
    {
        var blogs = FindBlogsUI();
        if (blogs != null) blogs.RestoreBlurPanels();
    }

    private static void HookSceneEvents()
    {
        if (sceneHooked)
            return;

        sceneHooked = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        covers = 0;
    }
}
