namespace TaskManagerMCP.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "todo";   // todo | in-progress | done | blocked
    public string Priority { get; set; } = "medium"; // low | medium | high | urgent
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "general"; // general | bug | idea | decision
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Tags { get; set; } = new();
}

public class StandupEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public string WhatIDid { get; set; } = string.Empty;
    public string WhatIWillDo { get; set; } = string.Empty;
    public string Blockers { get; set; } = "none";
    public string Author { get; set; } = "developer";
}
