# YiMini MCP

YiMini MCP 是一个独立的 stdio MCP 服务。它把 AI 客户端连接到已经运行的 YiMini Web 后端，复用现有认证、权限和动态 API，不复制业务 CRUD 代码。

MCP 进程不会启动 Web 后端。使用前必须先确认 YiMini Web 是否可以正常访问，并确认 `YIMINI_API_BASE_URL` 指向正确地址。

## 能力

- `auth_login`：调用 YiMini 登录接口，并在当前 MCP 进程内保存 JWT。
- `api_openapi`：读取模块 Swagger，发现实际动态 API 路径和参数。
- `api_request`：调用相对路径 `/api/...` API，自动携带 JWT 和租户请求头。
- `jobs_list`：查看 Hangfire 循环任务和执行状态。
- `database_query`：通过 SqlSugar 执行单条 SQL，默认只读。

## 启动

在仓库根目录执行：

```powershell
dotnet run --project Yi.Abp\tools\Yi.Abp.Mcp\Yi.Abp.Mcp.csproj --no-build
```

stdio 模式下，标准输出只用于 MCP JSON-RPC 消息；诊断信息写入标准错误。不要在 MCP 进程中启动 Web 后端，也不要向 stdout 输出普通日志。

## 配置

配置 MCP 客户端时，将 `command` 指向 `dotnet`，将项目路径放到 `args` 中。以下是通用 JSON 示例：

```json
{
  "mcpServers": {
    "yimini": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "D:/Workspace/Develop/code/yiabp-mini/Yi.Abp/tools/Yi.Abp.Mcp/Yi.Abp.Mcp.csproj",
        "--no-build"
      ],
      "env": {
        "YIMINI_API_BASE_URL": "http://localhost:19002",
        "YIMINI_ACCESS_TOKEN": "Bearer <JWT>",
        "YIMINI_CONFIG_FILE": "D:/Workspace/Develop/code/yiabp-mini/Yi.Abp/src/Yi.Abp.Web/appsettings.json",
        "YIMINI_DB_TYPE": "Sqlite",
        "YIMINI_DB_ALLOW_WRITE": "false"
      }
    }
  }
}
```

`YIMINI_ACCESS_TOKEN` 可以填写原始 JWT，也可以填写带 `Bearer ` 前缀的 JWT。服务发送请求时会统一生成一个 `Authorization: Bearer <JWT>` 请求头。

| 环境变量 | 默认值 | 说明 |
|---|---|---|
| `YIMINI_API_BASE_URL` | `http://localhost:19002` | 已运行的 YiMini Web 地址 |
| `YIMINI_ACCESS_TOKEN` | 空 | JWT；也可先调用 `auth_login` |
| `YIMINI_CONFIG_FILE` | `Yi.Abp/src/Yi.Abp.Web/appsettings.json` | 用于读取默认数据库连接串的配置文件 |
| `YIMINI_TENANT_ID` | 空 | 多租户请求使用的 `__tenant` 值 |
| `YIMINI_DB_CONNECTION` | 从配置文件读取 | 数据库连接串；优先级高于配置文件 |
| `YIMINI_DB_TYPE` | `Sqlite` | SqlSugar 数据库类型 |
| `YIMINI_DB_ALLOW_WRITE` | `false` | 是否允许数据库写操作 |
| `YIMINI_DB_MAX_ROWS` | `200` | 数据库查询最多返回的行数 |
| `YIMINI_DB_COMMAND_TIMEOUT` | `30` | HTTP 和 SQL 命令超时时间（秒） |

## 调用约定

1. 不确定 API 路径时，先调用 `api_openapi`，再使用文档中的相对路径。
2. 查询使用 `api_request` 的 `GET` 方法，例如 `/api/user`、`/api/dept`、`/api/role`。
3. 新增、修改、删除 API 必须显式传入 `confirmWrite=true`；MCP 会拒绝未确认的非 GET 请求。
4. `api_request` 只允许相对 `/api/...` 路径，不能调用绝对 URL、Hangfire 页面或任意外部地址。
5. `database_query` 默认只读，并且只允许单条 SQL。数据库写操作必须同时满足 `YIMINI_DB_ALLOW_WRITE=true` 和 `confirmWrite=true`。
6. 不要在对话、日志或配置示例中回显真实 JWT、密码和完整数据库连接串。

## 示例

查询用户列表：

```json
{
  "method": "GET",
  "path": "/api/user",
  "query": { "skipCount": 0, "maxResultCount": 10 }
}
```

新增部门时，先确认上级部门 ID，再显式确认写操作：

```json
{
  "method": "POST",
  "path": "/api/dept",
  "confirmWrite": true,
  "body": {
    "state": true,
    "deptName": "运营部门",
    "deptCode": "YY",
    "parentId": "<parent-dept-id>",
    "remark": "运营部门"
  }
}
```

查询数据库：

```json
{
  "sql": "SELECT name FROM sqlite_master WHERE type = @type",
  "parameters": { "type": "table" }
}
```

## 故障排查

- `连接被拒绝`：Web 后端没有监听 `YIMINI_API_BASE_URL` 指定的端口；先启动 Web，不要启动第二个 MCP 进程。
- `401`：JWT 缺失、过期或配置未被 MCP 客户端重新加载。
- `403`：账号没有目标 API 的权限。
- `database_query` 提示只读：检查是否确实需要写入，并同时开启环境变量和 `confirmWrite=true`。

构建 MCP：

```powershell
dotnet build Yi.Abp\tools\Yi.Abp.Mcp\Yi.Abp.Mcp.csproj
```
