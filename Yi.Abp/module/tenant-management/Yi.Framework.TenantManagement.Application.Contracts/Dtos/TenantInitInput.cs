namespace Yi.Framework.TenantManagement.Application.Contracts.Dtos
{
    public class TenantInitInput
    {
        /// <summary>
        /// 是否强制初始化（清空已有数据）
        /// </summary>
        public bool IsForce { get; set; }

        /// <summary>
        /// 管理员账号
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 管理员密码
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
