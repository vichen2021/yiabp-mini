using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yi.Framework.Authorization.Abstractions.Attributes;

namespace Yi.Abp.Web.Controllers;

[ApiController]
[Route("api/mcp")]
[Authorize]
[IgnorePermission]
public sealed class McpController : ControllerBase
{
    [HttpGet("jobs")]
    public ActionResult<IReadOnlyList<McpRecurringJobDto>> GetJobs()
    {
        var storage = JobStorage.Current;
        if (storage is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Hangfire 尚未初始化。");
        }

        var jobs = storage.GetConnection()
            .GetRecurringJobs()
            .Select(job => new McpRecurringJobDto
            {
                Id = job.Id,
                Cron = job.Cron,
                Queue = job.Queue,
                TimeZoneId = job.TimeZoneId,
                NextExecution = job.NextExecution,
                LastExecution = job.LastExecution,
                LastJobId = job.LastJobId,
                LastJobState = job.LastJobState,
                Error = job.Error,
                CreatedAt = job.CreatedAt
            })
            .OrderBy(job => job.Id, StringComparer.Ordinal)
            .ToList();

        return Ok(jobs);
    }
}

public sealed class McpRecurringJobDto
{
    public string Id { get; init; } = string.Empty;
    public string Cron { get; init; } = string.Empty;
    public string? Queue { get; init; }
    public string? TimeZoneId { get; init; }
    public DateTime? NextExecution { get; init; }
    public DateTime? LastExecution { get; init; }
    public string? LastJobId { get; init; }
    public string? LastJobState { get; init; }
    public string? Error { get; init; }
    public DateTime? CreatedAt { get; init; }
}
