using System;
using System.Collections.Generic;
using UnityEngine;

public static class RecentSearches
{
    private const string PrefsKey = "RecentSearches_v1";
    private const int MaxCount = 8;

    [Serializable]
    private class Data
    {
        public List<string> items = new List<string>();
    }

    public static List<string> Get()
    {
        return Load().items;
    }

    public static void Add(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return;

        term = term.Trim();

        Data data = Load();

        data.items.RemoveAll(s =>
            string.Equals(s, term, StringComparison.OrdinalIgnoreCase));

        data.items.Insert(0, term);

        if (data.items.Count > MaxCount)
            data.items.RemoveRange(MaxCount, data.items.Count - MaxCount);

        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void Remove(string term)
    {
        Data data = Load();

        int removed = data.items.RemoveAll(s =>
            string.Equals(s, term, StringComparison.OrdinalIgnoreCase));

        if (removed == 0)
            return;

        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }

    private static Data Load()
    {
        if (!PlayerPrefs.HasKey(PrefsKey))
            return new Data();

        try
        {
            Data data = JsonUtility.FromJson<Data>(PlayerPrefs.GetString(PrefsKey));
            if (data == null)
                data = new Data();
            if (data.items == null)
                data.items = new List<string>();
            return data;
        }
        catch (Exception)
        {
            return new Data();
        }
    }
}