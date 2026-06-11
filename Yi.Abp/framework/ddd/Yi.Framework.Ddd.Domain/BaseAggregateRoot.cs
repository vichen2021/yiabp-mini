using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ObjectExtending;

namespace Yi.Framework.Ddd.Domain;

/// <summary>
/// 聚合根基类，包含扩展属性，不包含基于 ConcurrencyStamp 的乐观锁。
/// 如需启用请使用 ABP 原生 <see cref="AggregateRoot{TKey}" />。
/// </summary>
public abstract class BaseAggregateRoot<TKey> : BasicAggregateRoot<TKey>, IHasExtraProperties
{
    public virtual ExtraPropertyDictionary ExtraProperties { get; protected set; }

    protected BaseAggregateRoot()
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
    }

    protected BaseAggregateRoot(TKey id)
        : base(id)
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
    }
}
