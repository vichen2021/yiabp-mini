# YiMini MCP Agent Instructions

## Scope

This project is the stdio adapter between an AI client and the running YiMini Web API. Keep business rules and CRUD implementation in the Web modules; the MCP project should only provide transport, authentication forwarding, API discovery, job inspection, and controlled database access.

## Runtime contract

- MCP uses JSON-RPC over stdio. stdout is reserved for protocol responses; diagnostics belong on stderr.
- The MCP process must not start, stop, or proxy the Web server.
- `YIMINI_API_BASE_URL` points to an already running Web server, normally `http://localhost:19002`.
- `YIMINI_ACCESS_TOKEN` accepts either a raw JWT or `Bearer <JWT>` and must be normalized before sending requests.
- `api_request` may only call relative `/api/...` paths.

## Safety contract

- GET requests are read-only. Every non-GET API request requires `confirmWrite=true`.
- `database_query` accepts one SQL statement and is read-only unless both `YIMINI_DB_ALLOW_WRITE=true` and `confirmWrite=true` are present.
- Never log or return passwords, JWTs, or complete database connection strings.
- Preserve tenant forwarding through `YIMINI_TENANT_ID`.

## Change and verification rules

- Prefer the existing MCP protocol and project patterns; avoid adding a second server transport or business-specific CRUD wrapper.
- Keep tool descriptions and input schemas synchronized with runtime validation.
- After code changes, build `Yi.Abp/tools/Yi.Abp.Mcp/Yi.Abp.Mcp.csproj` and verify `initialize`, `tools/list`, one read API call, and the write-confirmation rejection path.
