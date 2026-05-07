using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// Action 身份解析缓存
    /// 避免重复解析同一 Action 的身份信息
    /// </summary>
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
            _cache[key] = identity;
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}