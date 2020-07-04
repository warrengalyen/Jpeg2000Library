using System;
using System.Collections.Generic;

public class NullableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public new TValue this[TKey key]
    {
        get
        {
            TValue value;
            if (TryGetValue(key, out value))
                return value;
            return default(TValue);
        }
        set
        {
            if (ContainsKey(key))
                base[key] = value;
            else
                Add(key, value);
        }
    }
}