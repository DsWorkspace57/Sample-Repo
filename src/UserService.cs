// ⚠️ This file has TODOs and intentional issues.
// Use GitHub Copilot Agent mode + your TaskManager MCP server to:
// 1. Create tasks for each TODO
// 2. Add a note documenting the design decision
// 3. Post a standup update after you fix something

namespace SampleProject;

/// <summary>
/// UserService — manages user accounts.
/// Several methods are incomplete — see the TODO comments.
/// </summary>
public class UserService
{
    private readonly Dictionary<int, User> _users = new();
    private int _nextId = 1;

    // TODO: Add validation — email must not be null/empty and must contain '@'
    // TODO: Check for duplicate emails before registering
    public User Register(string name, string email, string role = "viewer")
    {
        var user = new User
        {
            Id = _nextId++,
            Name = name,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        _users[user.Id] = user;
        return user;
    }

    // TODO: Return null instead of throwing if user not found
    public User GetUser(int id)
    {
        return _users[id]; // KeyNotFoundException if missing!
    }

    // TODO: Add role validation — allowed roles: viewer, editor, admin
    // TODO: Only admins should be able to promote other users to admin
    public void ChangeRole(int userId, string newRole)
    {
        _users[userId].Role = newRole;
    }

    // TODO: Implement soft delete (set IsActive = false) instead of hard delete
    public void DeleteUser(int id)
    {
        _users.Remove(id);
    }

    // TODO: Add pagination — return page number + page size
    public List<User> GetAllUsers()
    {
        return _users.Values.ToList();
    }

    // TODO: Add password hashing — NEVER store plain text passwords
    public bool ValidatePassword(int userId, string plainPassword)
    {
        var user = _users.GetValueOrDefault(userId);
        return user?.Password == plainPassword; // ← SECURITY BUG: plain text comparison!
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "viewer";
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
