using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TaskManagerMCP.Data;
using TaskManagerMCP.Models;

namespace TaskManagerMCP.Tools;

/// <summary>
/// MCP Tools for Task Management.
/// Each [McpServerTool] decorated method becomes a callable tool in GitHub Copilot Agent mode.
/// </summary>
[McpServerToolType]
public static class TaskTools
{
    // ─────────────────────────────────────────────
    //  LIST TASKS
    // ─────────────────────────────────────────────
    [McpServerTool, Description(
        "List all tasks. Optionally filter by status (todo/in-progress/done/blocked), " +
        "priority (low/medium/high/urgent), or assignee name.")]
    public static string ListTasks(
        [Description("Filter by status: todo, in-progress, done, blocked. Leave empty for all.")] string? status = null,
        [Description("Filter by priority: low, medium, high, urgent. Leave empty for all.")] string? priority = null,
        [Description("Filter by assignee name. Leave empty for all.")] string? assignedTo = null)
    {
        var tasks = DataStore.Tasks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
            tasks = tasks.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(priority))
            tasks = tasks.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(assignedTo))
            tasks = tasks.Where(t => t.AssignedTo?.Equals(assignedTo, StringComparison.OrdinalIgnoreCase) == true);

        var result = tasks.OrderByDescending(t => t.Priority).ToList();

        if (!result.Any())
            return "No tasks found matching your filters.";

        var lines = result.Select(t =>
            $"[#{t.Id}] [{t.Priority.ToUpper()}] [{t.Status}] {t.Title}" +
            $"{(t.AssignedTo != null ? $" → @{t.AssignedTo}" : "")}" +
            $"{(t.DueDate.HasValue ? $" | Due: {t.DueDate.Value:yyyy-MM-dd}" : "")}" +
            $"\n    {t.Description}");

        return $"Found {result.Count} task(s):\n\n" + string.Join("\n\n", lines);
    }

    // ─────────────────────────────────────────────
    //  ADD TASK
    // ─────────────────────────────────────────────
    [McpServerTool, Description(
        "Create a new task and add it to the task list.")]
    public static string AddTask(
        [Description("Short, clear title for the task.")] string title,
        [Description("Detailed description of what needs to be done.")] string description,
        [Description("Priority level: low, medium, high, urgent.")] string priority = "medium",
        [Description("Who the task is assigned to (e.g. 'alice').")] string? assignedTo = null,
        [Description("Due date in yyyy-MM-dd format (e.g. '2026-05-20').")] string? dueDate = null,
        [Description("Comma-separated tags (e.g. 'bug,auth,urgent').")] string? tags = null)
    {
        var task = new TaskItem
        {
            Id = DataStore.NextTaskId(),
            Title = title,
            Description = description,
            Status = "todo",
            Priority = priority.ToLower(),
            AssignedTo = assignedTo,
            DueDate = dueDate != null ? DateTime.TryParse(dueDate, out var d) ? d : null : null,
            Tags = tags?.Split(',').Select(t => t.Trim()).ToList() ?? new()
        };

        DataStore.Tasks.Add(task);
        return $"✅ Task #{task.Id} created: \"{task.Title}\" | Priority: {task.Priority} | Status: todo";
    }

    // ─────────────────────────────────────────────
    //  UPDATE TASK STATUS
    // ─────────────────────────────────────────────
    [McpServerTool, Description(
        "Update the status of an existing task by its ID.")]
    public static string UpdateTaskStatus(
        [Description("The numeric ID of the task to update.")] int taskId,
        [Description("New status: todo, in-progress, done, blocked.")] string newStatus)
    {
        var allowed = new[] { "todo", "in-progress", "done", "blocked" };
        if (!allowed.Contains(newStatus.ToLower()))
            return $"❌ Invalid status '{newStatus}'. Allowed: {string.Join(", ", allowed)}";

        var task = DataStore.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            return $"❌ No task found with ID #{taskId}";

        var old = task.Status;
        task.Status = newStatus.ToLower();
        return $"✅ Task #{taskId} \"{task.Title}\" status changed: {old} → {task.Status}";
    }

    // ─────────────────────────────────────────────
    //  GET TASK SUMMARY
    // ─────────────────────────────────────────────
    [McpServerTool, Description(
        "Get a summary dashboard of all tasks: total count, breakdown by status and priority, overdue tasks.")]
    public static string GetTaskSummary()
    {
        var tasks = DataStore.Tasks;
        var today = DateTime.Today;

        var byStatus = tasks.GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var byPriority = tasks.GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());

        var overdue = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value < today && t.Status != "done").ToList();

        var summary = $"""
        📊 TASK SUMMARY
        ═══════════════════════
        Total Tasks: {tasks.Count}

        By Status:
          • todo:        {byStatus.GetValueOrDefault("todo", 0)}
          • in-progress: {byStatus.GetValueOrDefault("in-progress", 0)}
          • done:        {byStatus.GetValueOrDefault("done", 0)}
          • blocked:     {byStatus.GetValueOrDefault("blocked", 0)}

        By Priority:
          • urgent: {byPriority.GetValueOrDefault("urgent", 0)}
          • high:   {byPriority.GetValueOrDefault("high", 0)}
          • medium: {byPriority.GetValueOrDefault("medium", 0)}
          • low:    {byPriority.GetValueOrDefault("low", 0)}

        ⚠️ Overdue: {overdue.Count}
        {(overdue.Any() ? string.Join("\n  ", overdue.Select(t => $"• #{t.Id} {t.Title} (due {t.DueDate:yyyy-MM-dd})")) : "")}
        """;

        return summary;
    }

    // ─────────────────────────────────────────────
    //  GET TASKS DUE TODAY
    // ─────────────────────────────────────────────
    [McpServerTool, Description(
        "Return all tasks that are due today or overdue and not yet done.")]
    public static string GetUrgentTasks()
    {
        var today = DateTime.Today;
        var urgent = DataStore.Tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value <= today && t.Status != "done")
            .OrderBy(t => t.DueDate)
            .ToList();

        if (!urgent.Any())
            return "🎉 No urgent or overdue tasks! You're all caught up.";

        var lines = urgent.Select(t =>
            $"{(t.DueDate < today ? "⚠️" : "📅")} #{t.Id} [{t.Priority.ToUpper()}] \"{t.Title}\" → @{t.AssignedTo ?? "unassigned"} | Due: {t.DueDate:yyyy-MM-dd} | Status: {t.Status}");

        return $"🚨 {urgent.Count} urgent/overdue task(s):\n" + string.Join("\n", lines);
    }
}
