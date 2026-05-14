using ModelContextProtocol.Server;
using System.ComponentModel;
using TaskManagerMCP.Data;
using TaskManagerMCP.Models;

namespace TaskManagerMCP.Tools;

/// <summary>
/// MCP Tools for Notes — save architectural decisions, bug findings, ideas.
/// </summary>
[McpServerToolType]
public static class NoteTools
{
    [McpServerTool, Description(
        "List all notes. Optionally filter by category: general, bug, idea, decision.")]
    public static string ListNotes(
        [Description("Filter by category: general, bug, idea, decision. Leave empty for all.")] string? category = null)
    {
        var notes = DataStore.Notes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category))
            notes = notes.Where(n => n.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var result = notes.OrderByDescending(n => n.CreatedAt).ToList();

        if (!result.Any())
            return "No notes found.";

        var lines = result.Select(n =>
            $"📝 [{n.Id}] [{n.Category.ToUpper()}] {n.Title}\n" +
            $"   {n.Content[..Math.Min(200, n.Content.Length)]}{(n.Content.Length > 200 ? "..." : "")}");

        return $"{result.Count} note(s):\n\n" + string.Join("\n\n", lines);
    }

    [McpServerTool, Description(
        "Add a new note. Use category='decision' for architecture decisions, " +
        "'bug' for bug investigations, 'idea' for feature ideas.")]
    public static string AddNote(
        [Description("Title of the note.")] string title,
        [Description("Full content of the note.")] string content,
        [Description("Category: general, bug, idea, decision.")] string category = "general",
        [Description("Comma-separated tags.")] string? tags = null)
    {
        var note = new Note
        {
            Id = DataStore.NextNoteId(),
            Title = title,
            Content = content,
            Category = category.ToLower(),
            Tags = tags?.Split(',').Select(t => t.Trim()).ToList() ?? new()
        };

        DataStore.Notes.Add(note);
        return $"📝 Note #{note.Id} saved: \"{note.Title}\" [{note.Category}]";
    }

    [McpServerTool, Description(
        "Search notes by keyword across title and content.")]
    public static string SearchNotes(
        [Description("Keyword to search for in note titles and content.")] string keyword)
    {
        var matches = DataStore.Notes
            .Where(n => n.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                     || n.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!matches.Any())
            return $"No notes found containing '{keyword}'.";

        var lines = matches.Select(n =>
            $"📝 [{n.Id}] [{n.Category.ToUpper()}] {n.Title}\n   {n.Content}");

        return $"Found {matches.Count} note(s) matching '{keyword}':\n\n" + string.Join("\n\n", lines);
    }
}
