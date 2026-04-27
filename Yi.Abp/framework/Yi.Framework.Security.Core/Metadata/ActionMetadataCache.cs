using System.Collections.Concurrent;
using Yi.Framework.Security.Abstractions.Metadata;

namespace Yi.Framework.Security.Core.Metadata
{
    /// <summary>
    /// Action元数据缓存
    /// </summary>
    public class ActionMetadataCache
    {
        private readonly ConcurrentDictionary<string, ActionMetadata> _cache = new();

        public bool TryGetValue(string key, out ActionMetadata? metadata)
        {
            return _cache.TryGetValue(key, out metadata);
        }

        public void Set(string key, ActionMetadata metadata)
        {
            _cache.TryAdd(key, metadata);
        }
    }
}