using System;
using UnityEngine;

public static class RulerHaptics
{
#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void HapticPrepareLight();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void HapticTickLight();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern void HapticSuccess();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern bool ReduceMotionEnabled();
#endif

    static bool reducedMotion;
    static bool checkedMotion;

    public static void Prepare()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try { HapticPrepareLight(); } catch { }
#elif UNITY_ANDROID && !UNITY_EDITOR
        PrepareAndroid();
#endif
    }

    public static void Tick()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try { HapticTickLight(); } catch { }
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidTick();
#endif
    }

    public static void Success()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try { HapticSuccess(); } catch { }
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidSuccess();
#endif
    }

    public static bool IsReduceMotionEnabled()
    {
        if (checkedMotion) return reducedMotion;
        checkedMotion = true;
#if UNITY_IOS && !UNITY_EDITOR
        try { reducedMotion = ReduceMotionEnabled(); } catch { reducedMotion = false; }
#elif UNITY_ANDROID && !UNITY_EDITOR
        reducedMotion = AndroidReduceMotion();
#else
        reducedMotion = false;
#endif
        return reducedMotion;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static void PrepareAndroid()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator != null) vibrator.Call("cancel");
            }
        }
        catch { }
    }

    static void AndroidTick()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null) return;
                int sdkInt = (int)new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
                if (sdkInt >= 29)
                {
                    using (var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        int clockTick = 4;
                        using (var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createPredefined", clockTick))
                            vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", 15L);
                }
            }
        }
        catch
        {
            try { Handheld.Vibrate(); } catch { }
        }
    }

    static bool AndroidReduceMotion()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var contentResolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            using (var settings = new AndroidJavaClass("android.provider.Settings$Global"))
            {
                float scale = settings.CallStatic<float>("getFloat", contentResolver, "animator_duration_scale", 1f);
                return scale <= 0f;
            }
        }
        catch { return false; }
    }

    static void AndroidSuccess()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null) return;
                int sdkInt = (int)new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
                if (sdkInt >= 29)
                {
                    using (var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        int click = 1; // EFFECT_CLICK
                        using (var effect = vibrationEffect.CallStatic<AndroidJavaObject>("createPredefined", click))
                            vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", 30L);
                }
            }
        }
        catch { }
    }
#endif
}
