using System;
using System.Collections.Generic;
using System.Text;

namespace PDASimulator.Utils
{
    public static class UsefulExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, 
            TKey key,
            Func<TValue> valueProvider)
        {
            var exists = dictionary.TryGetValue(key, out var value);

            if (!exists)
            {
                value = valueProvider();
                dictionary.Add(key, value);
            }

            return value;
        }

        public static TValue GetOrCreate<TKey, TValue>(
                this IDictionary<TKey, TValue> dictionary,
                TKey key)
            where TValue : new()
        {
            var exists = dictionary.TryGetValue(key, out var value);

            if (!exists)
            {
                value = new TValue();
                dictionary.Add(key, value);
            }

            return value;
        }
    }
}
