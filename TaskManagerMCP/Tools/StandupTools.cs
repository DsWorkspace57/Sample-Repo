using ModelContextProtocol.Server;
using System.ComponentModel;
using TaskManagerMCP.Data;
using TaskManagerMCP.Models;

namespace TaskManagerMCP.Tools;

/// <summary>
/// MCP Tools for Daily Standup tracking.
/// Ask Copilot to generate, store, or retrieve standup entries.
/// </summary>
[McpServerToolType]
public static class StandupTools
{
    [McpServerTool, Description(
        "Post today's standup update: what you did, what you'll do, and any blockers.")]
    public static string PostStandup(
        [Description("What did you accomplish yesterday/today so far?")] string whatIDid,
        [Description("What will you work on next?")] string whatIWillDo,
        [Description("Any blockers or impediments? Write 'none' if clear.")] string blockers = "none",
        [Description("Your name or username.")] string author = "developer")
    {
        var entry = new StandupEntry
        {
            Id = DataStore.NextStandupId(),
            Date = DateTime.UtcNow.Date,
            WhatIDid = whatIDid,
            WhatIWillDo = whatIWillDo,
            Blockers = blockers,
            Author = author
        };

        DataStore.Standups.Add(entry);

        return $"""
        ✅ Standup posted for {entry.Date:yyyy-MM-dd} by @{entry.Author}
        
        ✔️ Did:      {entry.WhatIDid}
        🔜 Will do: {entry.WhatIWillDo}
        🚧 Blockers: {entry.Blockers}
        """;
    }

    [McpServerTool, Description(
        "Get all standup entries from the last N days. Default is last 7 days.")]
    public static string GetRecentStandups(
        [Description("Number of past days to retrieve standups for. Default 7.")] int days = 7)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var entries = DataStore.Standups
            .Where(s => s.Date >= since)
            .OrderByDescending(s => s.Date)
            .ToList();

        if (!entries.Any())
            return $"No standup entries in the last {days} days.";

        var lines = entries.Select(s => $"""
        📅 {s.Date:yyyy-MM-dd} — @{s.Author}
           ✔ Did:      {s.WhatIDid}
           🔜 Will do: {s.WhatIWillDo}
           🚧 Blockers: {s.Blockers}
        """);

        return $"Standups from last {days} days:\n\n" + string.Join("\n\n", lines);
    }

    [McpServerTool, Description(
        "Generate a weekly summary report from all standup entries this week. " +
        "Useful for sprint reviews or weekly status emails.")]
    public static string GenerateWeeklyReport()
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var entries = DataStore.Standups.Where(s => s.Date >= weekStart).ToList();

        if (!entries.Any())
            return "No standup entries this week yet.";

        var blockers = entries.Where(s => s.Blockers != "none").ToList();
        var authors = entries.Select(s => s.Author).Distinct().ToList();

        var accomplishments = string.Join("\n", entries.Select(s => $"  • [{s.Date:ddd}] @{s.Author}: {s.WhatIDid}"));
        var blockerSection = blockers.Any()
            ? string.Join("\n", blockers.Select(s => $"  ⚠️ @{s.Author}: {s.Blockers}"))
            : "  ✅ No blockers this week";

        return $"""
        📋 WEEKLY SUMMARY — Week of {weekStart:MMM dd, yyyy}
        ═══════════════════════════════════════════
        Team Members Active: {string.Join(", ", authors.Select(a => "@" + a))}
        Total Standup Entries: {entries.Count}

        ACCOMPLISHMENTS THIS WEEK:
        {accomplishments}

        BLOCKERS:
        {blockerSection}

        UPCOMING (from latest entries):
        {string.Join("\n", entries.TakeLast(3).Select(s => $"  • @{s.Author}: {s.WhatIWillDo}"))}
        """;
    }
}
