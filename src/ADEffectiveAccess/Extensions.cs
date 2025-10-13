using System.Collections.Generic;

namespace ADEffectiveAccess;

internal static class Extensions
{
    internal static bool TryAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
    {
        bool result;
        if (result = !dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
        return !result;
    }
}
