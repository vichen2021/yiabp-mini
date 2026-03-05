namespace Yi.Framework.TenantManagement.Application.Contracts.Dtos
{
    public class TenantInitOutputDto
    {
        /// <summary>
        /// 是否需要强制初始化（数据库已存在且有数据时为 true）
        /// </summary>
        public bool NeedForce { get; set; }
    }
}
