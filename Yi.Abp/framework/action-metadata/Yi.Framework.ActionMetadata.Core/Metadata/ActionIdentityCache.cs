using Yi.Framework.ActionMetadata.Abstractions.Metadata;

namespace Yi.Framework.ActionMetadata.Core.Metadata
{
    public class ActionIdentityCache
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, ActionIdentity> _cache
            = new();

        public bool TryGetValue(string key, out ActionIdentity? identity)
        {
            return _cache.TryGetValue(key, out identity);
        }

        public void Set(string key, ActionIdentity identity)
        {
            _cache.TryAdd(key, identity);
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
