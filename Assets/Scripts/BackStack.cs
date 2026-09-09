using System;
using System.Collections.Generic;
using UnityEngine;

public static class BackStack
{
    private static readonly List<Action> backActions = new List<Action>();

    public static void Push(Action backAction)
    {
        if (backAction == null) return;
        backActions.Add(backAction);
    }

    public static void Remove(Action backAction)
    {
        if (backAction == null) return;
        backActions.Remove(backAction);
    }

    public static void Clear()
    {
        backActions.Clear();
    }

    public static bool GoBack()
    {
        for (int i = backActions.Count - 1; i >= 0; i--)
        {
            Action action = backActions[i];
            backActions.RemoveAt(i);

            if (action == null) continue;

            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogWarning("BackStack action failed: " + e.Message);
            }
            return true;
        }
        return false;
    }
}
