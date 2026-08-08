using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OperationsRequests.Domain;

namespace OperationsRequests.Pages;

public class DemoModel(IWebHostEnvironment environment, IConfiguration configuration, UserManager<ApplicationUser> users) : PageModel
{
    public string Password => configuration["Demo:Password"] ?? "Demo password is not configured.";
    public List<(string Role, string Email)> Accounts { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (!environment.IsDevelopment()) return NotFound();

        var demoUsers = await users.Users
            .Where(x => x.Email != null && x.Email.EndsWith("@demo.local"))
            .OrderBy(x => x.Email)
            .ToListAsync();

        foreach (var u in demoUsers)
        {
            var roles = await users.GetRolesAsync(u);
            Accounts.Add((roles.FirstOrDefault() ?? "", u.Email!));
        }

        return Page();
    }
}
