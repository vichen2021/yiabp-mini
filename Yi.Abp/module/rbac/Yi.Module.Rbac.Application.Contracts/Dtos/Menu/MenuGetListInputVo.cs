using Volo.Abp.Application.Dtos;
using Yi.Module.Rbac.Domain.Shared.Enums;

namespace Yi.Module.Rbac.Application.Contracts.Dtos.Menu
{
    public class MenuGetListInputVo : PagedAndSortedResultRequestDto
    {

        public bool? State { get; set; }
        public string? MenuName { get; set; }
        public MenuSourceEnum MenuSource { get; set; } = MenuSourceEnum.Ruoyi;
    }
}
