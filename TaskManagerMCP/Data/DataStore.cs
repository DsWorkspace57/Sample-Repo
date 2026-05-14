using TaskManagerMCP.Models;

namespace TaskManagerMCP.Data;

/// <summary>
/// Simple in-memory store — in a real MCP server you'd use SQLite, JSON file, or a real DB.
/// This is seeded with sample data so you can test immediately.
/// </summary>
public static class DataStore
{
    private static int _taskIdCounter = 4;
    private static int _noteIdCounter = 3;
    private static int _standupIdCounter = 2;

    public static List<TaskItem> Tasks { get; } = new()
    {
        new TaskItem { Id=1, Title="Fix login bug", Description="Users getting 401 on valid tokens", Status="in-progress", Priority="urgent", AssignedTo="alice", Tags=["auth","bug"], DueDate=DateTime.Today.AddDays(1) },
        new TaskItem { Id=2, Title="Add dark mode", Description="Implement dark mode toggle in Settings page", Status="todo", Priority="medium", AssignedTo="bob", Tags=["ui","feature"] },
        new TaskItem { Id=3, Title="Write unit tests for OrderService", Description="Cover all public methods and edge cases", Status="todo", Priority="high", AssignedTo="alice", Tags=["tests","quality"], DueDate=DateTime.Today.AddDays(3) },
    };

    public static List<Note> Notes { get; } = new()
    {
        new Note { Id=1, Title="Architecture Decision: Use In-Memory Store", Content="For the MVP we decided to use in-memory storage. When we hit 1k users we'll migrate to PostgreSQL. Owner: team lead.", Category="decision", Tags=["architecture"] },
        new Note { Id=2, Title="Login Bug Root Cause", Content="The JWT token is being validated against the wrong secret key in staging. Prod uses env var AUTH_SECRET, staging hardcodes 'dev-secret'.", Category="bug", Tags=["auth","bug"] },
    };

    public static List<StandupEntry> Standups { get; } = new()
    {
        new StandupEntry { Id=1, Date=DateTime.Today.AddDays(-1), WhatIDid="Fixed staging environment config", WhatIWillDo="Work on login bug fix", Blockers="Need prod credentials from DevOps", Author="alice" },
    };

    public static int NextTaskId() => Interlocked.Increment(ref _taskIdCounter);
    public static int NextNoteId() => Interlocked.Increment(ref _noteIdCounter);
    public static int NextStandupId() => Interlocked.Increment(ref _standupIdCounter);
}
