using System;
using System.Collections.Generic;

namespace Entities.Core.Scripts.Utility
{
    [Serializable]
    public class SaveEntry<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }

    [Serializable]
    public class SaveDictionary<TKey, TValue>
    {
        public List<SaveEntry<TKey, TValue>> Entries = new();

        public SaveDictionary() { }
        public SaveDictionary(Dictionary<TKey, TValue> dict)
        {
            foreach (var kv in dict)
            {
                Entries.Add(new SaveEntry<TKey, TValue> { Key = kv.Key, Value = kv.Value });
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var kv in Entries)
            {
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }
    }
}