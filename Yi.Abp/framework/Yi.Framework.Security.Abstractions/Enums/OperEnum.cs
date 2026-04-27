using System.ComponentModel;

namespace Yi.Framework.Security.Abstractions.Enums
{
    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public enum OperEnum
    {
        [Description("新增")] Insert = 0,
        [Description("修改")] Update = 1,
        [Description("删除")] Delete = 2,
        [Description("授权")] Auth = 3,
        [Description("导出")] Export = 4,
        [Description("导入")] Import = 5,
        [Description("强制退出")] ForceLogout = 6,
        [Description("生成代码")] GenerateCode = 7,
        [Description("清空")] Clear = 8,
        [Description("其他")] Other = 9,
    }
}