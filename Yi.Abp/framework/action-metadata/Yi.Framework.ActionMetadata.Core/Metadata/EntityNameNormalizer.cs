namespace Yi.Framework.ActionMetadata.Core.Metadata
{
    /// <summary>
    /// 实体名规范化。
    /// </summary>
    public static class EntityNameNormalizer
    {
        /// <summary>
        /// 规范化实体名：UserAggregateRoot -> user。
        /// </summary>
        public static string Normalize(string name)
        {
            name = name.Replace("AggregateRoot", "")
                .Replace("Entity", "")
                .Replace("Service", "");

            return name.ToLowerInvariant();
        }
    }
}
