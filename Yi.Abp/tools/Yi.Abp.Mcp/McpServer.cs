using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SqlSugar;

namespace Yi.Abp.Mcp;

public sealed class McpServer
{
    private const string ProtocolVersion = "2024-11-05";
    private const int MaxResponseCharacters = 500_000;
    private const string ServerInstructions =
        "YiMini MCP connects to an already running YiMini Web API at YIMINI_API_BASE_URL; it never starts the Web server. " +
        "Use api_openapi before calling an unfamiliar endpoint. api_request only accepts relative /api/... paths. " +
        "Treat every non-GET API request as a write and pass confirmWrite=true only after the operation is intended. " +
        "database_query is read-only unless YIMINI_DB_ALLOW_WRITE=true and confirmWrite=true are both set. " +
        "Never expose JWTs, passwords, or database connection strings.";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly McpOptions _options;
    private readonly HttpClient _httpClient = new();
    private string? _accessToken;

    public McpServer(McpOptions options)
    {
        _options = options;
        _accessToken = options.AccessToken;
        _httpClient.Timeout = TimeSpan.FromSeconds(options.CommandTimeoutSeconds);
    }

    public async Task RunAsync()
    {
        using var stdin = Console.OpenStandardInput();
        using var reader = new StreamReader(stdin, Encoding.UTF8);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var request = JsonNode.Parse(line)?.AsObject()
                    ?? throw new JsonException("MCP 请求必须是 JSON 对象。");
                var response = await HandleRequestAsync(request);
                if (response is not null)
                {
                    await WriteResponseAsync(response);
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
            }
        }
    }

    public static void PrintToolList()
    {
        foreach (var tool in GetToolDefinitions())
        {
            Console.WriteLine($"{tool["name"]}: {tool["description"]}");
        }
    }

    private async Task<JsonObject?> HandleRequestAsync(JsonObject request)
    {
        var method = request["method"]?.GetValue<string>()
            ?? throw new JsonException("MCP 请求缺少 method。");
        var id = request["id"]?.DeepClone();
        var parameters = request["params"] as JsonObject ?? new JsonObject();

        if (method.StartsWith("notifications/", StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            var result = method switch
            {
                "initialize" => new JsonObject
                {
                    ["protocolVersion"] = ProtocolVersion,
                    ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
                    ["serverInfo"] = new JsonObject { ["name"] = "yimini-mcp", ["version"] = "1.0.0" },
                    ["instructions"] = ServerInstructions
                },
                "ping" => new JsonObject(),
                "tools/list" => new JsonObject { ["tools"] = new JsonArray(GetToolDefinitions().ToArray()) },
                "tools/call" => await CallToolAsync(parameters),
                _ => throw new McpException(-32601, $"不支持的 MCP 方法: {method}")
            };

            return JsonRpcResult(id, result);
        }
        catch (McpException exception)
        {
            return JsonRpcError(id, exception.Code, exception.Message);
        }
        catch (Exception exception)
        {
            return JsonRpcError(id, -32000, exception.Message);
        }
    }

    private async Task<JsonObject> CallToolAsync(JsonObject parameters)
    {
        var name = parameters["name"]?.GetValue<string>()
            ?? throw new McpException(-32602, "tools/call 缺少 name。");
        var arguments = parameters["arguments"] as JsonObject ?? new JsonObject();

        try
        {
            var result = name switch
            {
                "auth_login" => await LoginAsync(arguments),
                "api_openapi" => await GetOpenApiAsync(),
                "api_request" => await RequestApiAsync(arguments),
                "jobs_list" => await ListJobsAsync(),
                "database_query" => ExecuteDatabaseQuery(arguments),
                _ => throw new McpException(-32602, $"未知工具: {name}")
            };

            return ToolResult(result);
        }
        catch (Exception exception)
        {
            return ToolError(exception.Message);
        }
    }

    private async Task<JsonNode> LoginAsync(JsonObject arguments)
    {
        var body = new JsonObject
        {
            ["userName"] = RequiredString(arguments, "userName"),
            ["password"] = RequiredString(arguments, "password"),
            ["code"] = arguments["code"]?.GetValue<string>() ?? string.Empty,
            ["uuid"] = arguments["uuid"]?.GetValue<string>() ?? string.Empty
        };
        var response = await SendApiAsync(HttpMethod.Post, "/api/account/login", body);
        if (!response.IsSuccessStatusCode)
        {
            return new JsonObject
            {
                ["statusCode"] = response.StatusCode,
                ["body"] = response.Body ?? JsonValue.Create(response.RawBody)
            };
        }

        var loginData = response.Body?["data"] ?? response.Body;
        var token = loginData?["token"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new McpException(-32001, "登录响应未包含 token。");
        }

        _accessToken = token;
        return new JsonObject
        {
            ["authenticated"] = true,
            ["hasRefreshToken"] = !string.IsNullOrWhiteSpace(loginData?["refreshToken"]?.GetValue<string>())
        };
    }

    private async Task<JsonNode> GetOpenApiAsync()
    {
        var serviceNames = new[] { "rbac", "file-management", "default", "tenant-management", "audit" };
        var documents = new JsonObject();
        foreach (var serviceName in serviceNames)
        {
            var response = await SendRawAsync(HttpMethod.Get, $"/swagger/{serviceName}/swagger.json", null, false);
            if (response.IsSuccessStatusCode && response.Body is not null)
            {
                documents[serviceName] = response.Body;
            }
        }

        if (documents.Count == 0)
        {
            var response = await SendRawAsync(HttpMethod.Get, "/swagger/v1/swagger.json", null, false);
            if (response.Body is not null)
            {
                documents["v1"] = response.Body;
            }
        }

        return documents;
    }

    private async Task<JsonNode> RequestApiAsync(JsonObject arguments)
    {
        var path = RequiredString(arguments, "path");
        var methodName = arguments["method"]?.GetValue<string>() ?? "GET";
        if (!(path.Equals("/api", StringComparison.OrdinalIgnoreCase)
              || path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            || Uri.TryCreate(path, UriKind.Absolute, out _))
        {
            throw new McpException(-32602, "api_request 只允许调用相对路径 /api/...。");
        }

        var method = new HttpMethod(methodName.ToUpperInvariant());
        if (method != HttpMethod.Get && method != HttpMethod.Post && method != HttpMethod.Put
            && method != HttpMethod.Patch && method != HttpMethod.Delete)
        {
            throw new McpException(-32602, $"不允许的 HTTP 方法: {methodName}");
        }

        var confirmWrite = arguments["confirmWrite"]?.GetValue<bool>() ?? false;
        if (method != HttpMethod.Get && !confirmWrite)
        {
            throw new McpException(-32004, "非 GET API 请求会修改数据，必须传入 confirmWrite=true。");
        }

        var query = arguments["query"] as JsonObject;
        if (query is not null)
        {
            path = AddQueryString(path, query);
        }

        var body = arguments["body"]?.DeepClone();
        var headers = arguments["headers"] as JsonObject;
        var response = await SendApiAsync(method, path, body, headers);
        return new JsonObject
        {
            ["statusCode"] = response.StatusCode,
            ["body"] = response.Body ?? JsonValue.Create(response.RawBody ?? string.Empty)
        };
    }

    private async Task<JsonNode> ListJobsAsync()
    {
        var response = await SendApiAsync(HttpMethod.Get, "/api/mcp/jobs", null);
        return new JsonObject
        {
            ["statusCode"] = response.StatusCode,
            ["body"] = response.Body ?? JsonValue.Create(response.RawBody ?? string.Empty)
        };
    }

    private JsonNode ExecuteDatabaseQuery(JsonObject arguments)
    {
        if (string.IsNullOrWhiteSpace(_options.DbConnection))
        {
            throw new McpException(-32002, "未配置数据库连接串，请设置 YIMINI_DB_CONNECTION 或 YIMINI_CONFIG_FILE。");
        }

        var sql = RequiredString(arguments, "sql").Trim();
        var normalizedSql = sql.TrimEnd(';').Trim();
        if (normalizedSql.Contains(';'))
        {
            throw new McpException(-32602, "database_query 只允许单条 SQL。");
        }

        var firstKeyword = Regex.Match(normalizedSql, "^([A-Za-z]+)").Groups[1].Value.ToUpperInvariant();
        var isRead = firstKeyword is "SELECT" or "SHOW" or "DESCRIBE" or "DESC" or "EXPLAIN"
            || (firstKeyword == "WITH" && !Regex.IsMatch(normalizedSql, "\\b(INSERT|UPDATE|DELETE|REPLACE|MERGE)\\b", RegexOptions.IgnoreCase))
            || (firstKeyword == "PRAGMA" && !normalizedSql.Contains('='));
        var confirmWrite = arguments["confirmWrite"]?.GetValue<bool>() ?? false;
        if (!isRead && (!_options.AllowDbWrite || !confirmWrite))
        {
            throw new McpException(-32003, "数据库默认只读；写操作需要 YIMINI_DB_ALLOW_WRITE=true 且 confirmWrite=true。");
        }

        using var db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = _options.DbType,
            ConnectionString = _options.DbConnection,
            IsAutoCloseConnection = true
        });
        db.Ado.CommandTimeOut = _options.CommandTimeoutSeconds;
        var parameters = BuildParameters(arguments["parameters"] as JsonObject);

        if (!isRead)
        {
            return new JsonObject
            {
                ["operation"] = "write",
                ["affectedRows"] = db.Ado.ExecuteCommand(normalizedSql, parameters.ToArray())
            };
        }

        var rows = db.Ado.SqlQuery<dynamic>(normalizedSql, parameters.ToArray());
        var limitedRows = rows
            .Cast<object>()
            .Take(Math.Max(1, _options.MaxRows))
            .Select(row => JsonNode.Parse(JsonSerializer.Serialize(row)) ?? new JsonObject())
            .ToArray();
        return new JsonObject
        {
            ["operation"] = "read",
            ["rowCount"] = rows.Count,
            ["truncated"] = rows.Count > limitedRows.Length,
            ["rows"] = new JsonArray(limitedRows)
        };
    }

    private static List<SugarParameter> BuildParameters(JsonObject? input)
    {
        if (input is null)
        {
            return new List<SugarParameter>();
        }

        return input.Select(pair => new SugarParameter(
                pair.Key.StartsWith('@') ? pair.Key : $"@{pair.Key}",
                ToDbValue(pair.Value)))
            .ToList();
    }

    private static object ToDbValue(JsonNode? value)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        if (value is JsonValue jsonValue && jsonValue.TryGetValue<JsonElement>(out var element))
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => DBNull.Value,
                JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
                JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
                JsonValueKind.Number => element.GetDecimal(),
                _ => element.ToString()
            };
        }

        return value.ToJsonString();
    }

    private async Task<ApiResponse> SendApiAsync(
        HttpMethod method,
        string path,
        JsonNode? body,
        JsonObject? headers = null)
    {
        return await SendRawAsync(method, path, body, true, headers);
    }

    private async Task<ApiResponse> SendRawAsync(
        HttpMethod method,
        string path,
        JsonNode? body,
        bool authenticate,
        JsonObject? headers = null)
    {
        using var request = new HttpRequestMessage(method, new Uri(_options.ApiBaseUrl, path.TrimStart('/')));
        if (body is not null && method != HttpMethod.Get)
        {
            request.Content = new StringContent(body.ToJsonString(JsonOptions), Encoding.UTF8, "application/json");
        }

        if (authenticate && !string.IsNullOrWhiteSpace(_accessToken))
        {
            var accessToken = _accessToken.Trim();
            if (accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = accessToken["Bearer ".Length..].Trim();
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        if (!string.IsNullOrWhiteSpace(_options.TenantId))
        {
            request.Headers.TryAddWithoutValidation("__tenant", _options.TenantId);
        }

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                    || header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                request.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString() ?? string.Empty);
            }
        }

        using var response = await _httpClient.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();
        if (rawBody.Length > MaxResponseCharacters)
        {
            rawBody = rawBody[..MaxResponseCharacters];
        }

        JsonNode? parsedBody = null;
        try
        {
            parsedBody = JsonNode.Parse(rawBody);
        }
        catch (JsonException)
        {
        }

        return new ApiResponse((int)response.StatusCode, response.IsSuccessStatusCode, parsedBody, rawBody);
    }

    private static string AddQueryString(string path, JsonObject query)
    {
        var separator = path.Contains('?') ? '&' : '?';
        var values = query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value?.ToString() ?? string.Empty)}");
        return path + separator + string.Join('&', values);
    }

    private static string RequiredString(JsonObject arguments, string name) =>
        arguments[name]?.GetValue<string>() is { Length: > 0 } value
            ? value
            : throw new McpException(-32602, $"缺少参数: {name}");

    private static JsonObject ToolResult(JsonNode result) => new()
    {
        ["content"] = new JsonArray(new JsonObject
        {
            ["type"] = "text",
            ["text"] = result.ToJsonString(JsonOptions)
        })
    };

    private static JsonObject ToolError(string message) => new()
    {
        ["isError"] = true,
        ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = message })
    };

    private static JsonObject JsonRpcResult(JsonNode? id, JsonNode result) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id,
        ["result"] = result
    };

    private static JsonObject JsonRpcError(JsonNode? id, int code, string message) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id,
        ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
    };

    private static async Task WriteResponseAsync(JsonObject response)
    {
        await Console.Out.WriteLineAsync(response.ToJsonString(JsonOptions));
        await Console.Out.FlushAsync();
    }

    private static IEnumerable<JsonObject> GetToolDefinitions()
    {
        yield return ToolDefinition("auth_login", "调用 YiMini 账号登录，并仅在当前 MCP 进程内保存返回的 JWT。验证码登录失败时，先从 Web 登录流程获取 code 和 uuid。", new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray("userName", "password"),
            ["properties"] = new JsonObject
            {
                ["userName"] = new JsonObject { ["type"] = "string", ["description"] = "YiMini 登录用户名。" },
                ["password"] = new JsonObject { ["type"] = "string", ["description"] = "YiMini 登录密码，不要写入日志。" },
                ["code"] = new JsonObject { ["type"] = "string", ["description"] = "验证码；后端启用验证码时必填。" },
                ["uuid"] = new JsonObject { ["type"] = "string", ["description"] = "与验证码对应的 uuid。" }
            }
        });
        yield return ToolDefinition("api_openapi", "读取 YiMini 已启用模块的 Swagger 文档，用于发现实际 API 路径、HTTP 方法、查询参数和请求体。只读，不修改数据。", new JsonObject { ["type"] = "object" });
        yield return ToolDefinition("api_request", "调用 YiMini 的相对路径 /api/... 动态 API，自动沿用当前 JWT 和租户头。GET 用于查询；POST、PUT、PATCH、DELETE 都是写操作，必须显式传入 confirmWrite=true。", new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray("path"),
            ["properties"] = new JsonObject
            {
                ["path"] = new JsonObject { ["type"] = "string", ["description"] = "只能填写相对路径，例如 /api/user。不能填写绝对 URL。" },
                ["method"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("GET", "POST", "PUT", "PATCH", "DELETE"), ["description"] = "默认为 GET；非 GET 请求会被视为写操作。" },
                ["query"] = new JsonObject { ["type"] = "object", ["description"] = "URL 查询参数。" },
                ["headers"] = new JsonObject { ["type"] = "object", ["description"] = "额外请求头；Authorization 和 Host 不能覆盖 MCP 安全设置。" },
                ["body"] = new JsonObject { ["description"] = "POST、PUT、PATCH、DELETE 的 JSON 请求体。" },
                ["confirmWrite"] = new JsonObject { ["type"] = "boolean", ["description"] = "写操作必须显式设置为 true；仅在用户明确要求执行写操作时传入。" }
            }
        });
        yield return ToolDefinition("jobs_list", "查看 YiMini 当前 Hangfire 循环任务及其执行状态。需要 Web JWT；只读。", new JsonObject { ["type"] = "object" });
        yield return ToolDefinition("database_query", "使用 SqlSugar 执行一条 SQL。默认只读；INSERT、UPDATE、DELETE、DDL 等写操作必须同时开启 YIMINI_DB_ALLOW_WRITE=true 并传入 confirmWrite=true。", new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray("sql"),
            ["properties"] = new JsonObject
            {
                ["sql"] = new JsonObject { ["type"] = "string", ["description"] = "单条 SQL；不要拼接多条语句。优先使用参数化占位符。" },
                ["parameters"] = new JsonObject { ["type"] = "object", ["description"] = "SQL 参数，例如 { \"id\": \"...\" }。" },
                ["confirmWrite"] = new JsonObject { ["type"] = "boolean", ["description"] = "数据库写操作必须显式设置为 true。" }
            }
        });
    }

    private static JsonObject ToolDefinition(string name, string description, JsonObject inputSchema) => new()
    {
        ["name"] = name,
        ["description"] = description,
        ["inputSchema"] = inputSchema
    };

    private sealed record ApiResponse(int StatusCode, bool IsSuccessStatusCode, JsonNode? Body, string RawBody);

    private sealed class McpException(int code, string message) : Exception(message)
    {
        public int Code { get; } = code;
    }
}
