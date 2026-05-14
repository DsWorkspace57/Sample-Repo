using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagerMCP.Tools;

// ─────────────────────────────────────────────────────────────
//  TaskManager MCP Server — stdio transport
//  This server exposes tools for Tasks, Notes, and Standups
//  to GitHub Copilot Agent mode in Visual Studio 2026.
// ─────────────────────────────────────────────────────────────

var builder = Host.CreateApplicationBuilder(args);

// Silence console logs so they don't interfere with stdio MCP protocol.
// MCP over stdio uses stdout for JSON-RPC, so logs must go to stderr only.
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Warning;
});

// Register the MCP Server with stdio transport.
// All classes decorated with [McpServerToolType] are auto-discovered.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();   // Auto-registers TaskTools, NoteTools, StandupTools

var app = builder.Build();

Console.Error.WriteLine("[TaskManagerMCP] Server starting on stdio transport...");
Console.Error.WriteLine("[TaskManagerMCP] Tools: list_tasks, add_task, update_task_status, get_task_summary,");
Console.Error.WriteLine("[TaskManagerMCP]        get_urgent_tasks, list_notes, add_note, search_notes,");
Console.Error.WriteLine("[TaskManagerMCP]        post_standup, get_recent_standups, generate_weekly_report");

await app.RunAsync();
