using Yi.Abp.Mcp;

var options = McpOptions.Load(args);

if (options.ListTools)
{
    McpServer.PrintToolList();
    return;
}

await new McpServer(options).RunAsync();
