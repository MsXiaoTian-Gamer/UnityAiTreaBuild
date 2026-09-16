using System.Collections.Generic;

namespace Trea
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> data = new();

        public void Set<T>(string key, T value)
        {
            data[key] = value;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (data.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return defaultValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (data.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public bool Has(string key)
        {
            return data.ContainsKey(key);
        }

        public void Remove(string key)
        {
            data.Remove(key);
        }

        public void Clear()
        {
            data.Clear();
        }

        public IEnumerable<string> Keys => data.Keys;
    }
}
