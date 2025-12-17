namespace Yi.Framework.Rbac.Application.Contracts.Dtos.Dept
{
	public class DeptGetTreeListOutputDto
	{
		public Guid Id { get; set; }
		public Guid ParentId { get; set; }
		public int OrderNum { get; set; }

		public string DeptName { get; set; } = string.Empty;
		public string DeptCode { get; set; } = string.Empty;
		public string? Leader { get; set; }
		public string? Remark { get; set; }
		public bool State { get; set; }
		public DateTime CreationTime { get; set; }

		public List<DeptGetTreeListOutputDto>? Children { get; set; }
	}
}
