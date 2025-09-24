using System;
using System.Collections.Generic;
using Entities.Core.Scripts.Utility;

namespace Entities.Core.Scripts.Utils
{
    public static class SaveDictionaryExtensions
    {
       public static TValue GetOrDefault<TKey, TValue>(
        this SaveDictionary<TKey, TValue> dict, TKey key, TValue fallback = default)
    {
        var e = dict?.Entries.Find(x => Equals(x.Key, key));
        return e != null ? e.Value : fallback;
    }

    public static void Set<TKey, TValue>(
        this SaveDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));
        var e = dict.Entries.Find(x => Equals(x.Key, key));
        if (e == null) dict.Entries.Add(new SaveEntry<TKey, TValue> { Key = key, Value = value });
        else e.Value = value;
    }

    public static void PushCapped<TKey, T>(
        this SaveDictionary<TKey, List<T>> dict, TKey key, T item, int cap)
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));
        var e = dict.Entries.Find(x => Equals(x.Key, key));
        if (e == null)
        {
            e = new SaveEntry<TKey, List<T>> { Key = key, Value = new List<T>() };
            dict.Entries.Add(e);
        }

        e.Value ??= new List<T>();
        e.Value.Add(item);

        if (cap > 0 && e.Value.Count > cap)
            e.Value.RemoveAt(0);
    }
    }
}