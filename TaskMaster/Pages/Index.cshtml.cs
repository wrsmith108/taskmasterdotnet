using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AppDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<TaskItem> Tasks { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "all";

    public async Task OnGetAsync()
    {
        IQueryable<TaskItem> query = _context.Tasks;

        switch (Filter.ToLower())
        {
            case "active":
                query = query.Where(t => !t.IsCompleted);
                break;
            case "done":
                query = query.Where(t => t.IsCompleted);
                break;
            default: // "all"
                // No filter
                break;
        }

        Tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} deleted", id);
        }

        return RedirectToPage(new { Filter });
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task {TaskId} toggled to {Status}", id, task.IsCompleted);
        }

        return RedirectToPage(new { Filter });
    }
}
