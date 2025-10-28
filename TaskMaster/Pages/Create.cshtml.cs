using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskMaster.Data;
using TaskMaster.Models;

namespace TaskMaster.Pages;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(AppDbContext context, ILogger<CreateModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public TaskItem NewTask { get; set; } = new();

    public void OnGet()
    {
        // Initialize form
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        NewTask.CreatedAt = DateTime.UtcNow;
        NewTask.IsCompleted = false;
        NewTask.CompletedAt = null;

        _context.Tasks.Add(NewTask);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task created: {TaskId} - {TaskTitle}", NewTask.Id, NewTask.Title);

        return RedirectToPage("/Index");
    }
}