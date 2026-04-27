using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Yi.Framework.Security.Abstractions.Enums;
using Yi.Framework.Security.Abstractions.Logging;
using Yi.Framework.Security.Abstractions.Metadata;

namespace Yi.Framework.Security.Core.Filters
{
    /// <summary>
    /// 操作日志 Action Filter
    /// </summary>
    public class OperLogActionFilter : IAsyncActionFilter
    {
        private readonly IActionMetadataResolver _metadataResolver;
        private readonly IOperLogStore _operLogStore;
        private readonly OperLogOptions _options;

        public OperLogActionFilter(
            IActionMetadataResolver metadataResolver,
            IOperLogStore operLogStore,
            OperLogOptions options)
        {
            _metadataResolver = metadataResolver;
            _operLogStore = operLogStore;
            _options = options;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null)
            {
                await next();
                return;
            }

            // 解析元数据
            var metadata = _metadataResolver.Resolve(descriptor);

            // 忽略日志记录
            if (!_options.IsEnabled)
            {
                await next();
                return;
            }

            // 查询操作默认不记录（除非开启审计模式或有显式特性）
            if (!metadata.IsWriteOperation && !_options.LogReadOperations && metadata.ExplicitLogInfo == null)
            {
                await next();
                return;
            }

            // 获取日志信息（显式优先）
            var (title, operType) = metadata.ExplicitLogInfo ?? (metadata.LogTitle, metadata.OperType ?? OperEnum.Other);

            if (title == null)
            {
                await next();
                return;
            }

            // 执行业务逻辑
            var result = await next();

            // 保存日志
            await _operLogStore.SaveAsync(
                metadata,
                context.HttpContext,
                result.Result,
                result.Exception
            );
        }
    }
}