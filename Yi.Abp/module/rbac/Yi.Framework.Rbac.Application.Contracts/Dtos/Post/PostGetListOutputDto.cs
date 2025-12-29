using Volo.Abp.Application.Dtos;

namespace Yi.Framework.Rbac.Application.Contracts.Dtos.Post
{
    public class PostGetListOutputDto : EntityDto<Guid>
    {
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public bool State { get; set; }
        public string PostCode { get; set; } = string.Empty;
        public string PostName { get; set; } = string.Empty;
        
        /// <summary>
        /// 所属部门ID
        /// </summary>
        public Guid DeptId { get; set; }
        
        public string? Remark { get; set; }

        public int OrderNum { get; set; }
    }
}
