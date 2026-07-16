using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace Yi.Abp.Mcp;

public sealed class McpOptions
{
    public Uri ApiBaseUrl { get; init; } = new("http://localhost:19002");
    public string? AccessToken { get; init; }
    public string? TenantId { get; init; }
    public string? DbConnection { get; init; }
    public DbType DbType { get; init; } = DbType.Sqlite;
    public bool AllowDbWrite { get; init; }
    public int MaxRows { get; init; } = 200;
    public int CommandTimeoutSeconds { get; init; } = 30;
    public bool ListTools { get; init; }

    public static McpOptions Load(string[] args)
    {
        var configFile = GetValue(args, "--config")
            ?? Environment.GetEnvironmentVariable("YIMINI_CONFIG_FILE")
            ?? Path.Combine("Yi.Abp", "src", "Yi.Abp.Web", "appsettings.json");

        var configurationBuilder = new ConfigurationBuilder();
        var configPath = Path.GetFullPath(configFile);
        if (File.Exists(configPath))
        {
            configurationBuilder
                .SetBasePath(Path.GetDirectoryName(configPath)!)
                .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: false);
        }

        var configuration = configurationBuilder.Build();
        var apiBaseUrl = GetValue(args, "--api-base-url")
            ?? Environment.GetEnvironmentVariable("YIMINI_API_BASE_URL")
            ?? "http://localhost:19002";
        var dbConnection = GetValue(args, "--db-connection")
            ?? Environment.GetEnvironmentVariable("YIMINI_DB_CONNECTION")
            ?? configuration["DbConnOptions:Url"];
        var dbTypeText = GetValue(args, "--db-type")
            ?? Environment.GetEnvironmentVariable("YIMINI_DB_TYPE")
            ?? configuration["DbConnOptions:DbType"]
            ?? nameof(DbType.Sqlite);

        if (!Uri.TryCreate(apiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var apiUri)
            || apiUri.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException("YIMINI_API_BASE_URL 必须是 http 或 https 地址。");
        }

        if (!Enum.TryParse<DbType>(dbTypeText, true, out var dbType))
        {
            throw new ArgumentException($"不支持的数据库类型: {dbTypeText}");
        }

        return new McpOptions
        {
            ApiBaseUrl = apiUri,
            AccessToken = GetValue(args, "--access-token")
                ?? Environment.GetEnvironmentVariable("YIMINI_ACCESS_TOKEN"),
            TenantId = GetValue(args, "--tenant-id")
                ?? Environment.GetEnvironmentVariable("YIMINI_TENANT_ID"),
            DbConnection = dbConnection,
            DbType = dbType,
            AllowDbWrite = GetBoolean(args, "--allow-db-write")
                ?? GetBooleanEnvironment("YIMINI_DB_ALLOW_WRITE"),
            MaxRows = GetInt(args, "--max-rows")
                ?? GetIntEnvironment("YIMINI_DB_MAX_ROWS")
                ?? 200,
            CommandTimeoutSeconds = GetInt(args, "--command-timeout")
                ?? GetIntEnvironment("YIMINI_DB_COMMAND_TIMEOUT")
                ?? 30,
            ListTools = args.Contains("--list-tools", StringComparer.OrdinalIgnoreCase)
        };
    }

    private static string? GetValue(string[] args, string name)
    {
        var index = Array.FindIndex(args, x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static bool? GetBoolean(string[] args, string name)
    {
        var value = GetValue(args, name);
        return value is null ? null : bool.Parse(value);
    }

    private static bool GetBooleanEnvironment(string name) =>
        bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value;

    private static int? GetInt(string[] args, string name)
    {
        var value = GetValue(args, name);
        return value is null ? null : int.Parse(value);
    }

    private static int? GetIntEnvironment(string name) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : null;
}
