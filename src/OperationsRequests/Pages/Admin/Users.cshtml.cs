using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OperationsRequests.Data;
using OperationsRequests.Domain;

namespace OperationsRequests.Pages.Admin;

[Authorize(Roles = "PlatformAdmin")]
public class UsersModel(ApplicationDbContext db, UserManager<ApplicationUser> users) : PageModel
{
    public List<ApplicationUser> Users { get; set; } = [];
    public List<Country> Countries { get; set; } = [];
    public List<Territory> Territories { get; set; } = [];
    public List<Site> Sites { get; set; } = [];
    public Dictionary<string, string> UserRoles { get; set; } = [];

    [BindProperty]
    public UserInput Input { get; set; } = new();

    public sealed class UserInput
    {
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = Roles.StaffMember;
        public int? CountryId { get; set; }
        public int? TerritoryId { get; set; }
        public int? SiteId { get; set; }
    }

    public async Task OnGetAsync() => await Load();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await Load();

        if (!Roles.All.Contains(Input.Role) || string.IsNullOrWhiteSpace(Input.Email) || string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.AddModelError("", "Email, password and a valid role are required.");
            return Page();
        }

        var ok = IsValidAssignment(Input.Role, Input.CountryId, Input.TerritoryId, Input.SiteId,
            await ResolveTerritory(Input.TerritoryId), await ResolveSite(Input.SiteId));

        if (!ok)
        {
            ModelState.AddModelError("", "Invalid organisation assignment.");
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            DisplayName = Input.DisplayName,
            EmailConfirmed = true,
            CountryId = Input.CountryId,
            TerritoryId = Input.TerritoryId,
            SiteId = Input.SiteId
        };

        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }

        await users.AddToRoleAsync(user, Input.Role);
        db.AuditEvents.Add(new AuditEvent
        {
            Action = "UserCreated",
            ActorId = (await users.GetUserAsync(User))!.Id,
            ActorName = "Platform Admin",
            Note = $"{user.Email}: {Input.Role}"
        });
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignAsync(string id, string role, int? countryId, int? territoryId, int? siteId)
    {
        var u = await users.FindByIdAsync(id);
        if (u is null || !Roles.All.Contains(role)) return NotFound();

        var old = string.Join(',', await users.GetRolesAsync(u));
        var terr = await ResolveTerritory(territoryId);
        var site = await ResolveSite(siteId);
        var ok = IsValidAssignment(role, countryId, territoryId, siteId, terr, site);
        if (!ok) return BadRequest("Invalid organisation assignment.");

        await users.RemoveFromRolesAsync(u, await users.GetRolesAsync(u));
        await users.AddToRoleAsync(u, role);
        u.CountryId = countryId;
        u.TerritoryId = territoryId;
        u.SiteId = siteId;
        await users.UpdateAsync(u);
        db.AuditEvents.Add(new AuditEvent
        {
            Action = "OrgAssignmentChanged",
            ActorId = (await users.GetUserAsync(User))!.Id,
            ActorName = "Platform Admin",
            Note = $"{u.Email}: {old} -> {role}"
        });
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(string id)
    {
        var u = await users.FindByIdAsync(id);
        if (u is null) return NotFound();
        u.IsActive = !u.IsActive;
        await users.UpdateSecurityStampAsync(u);
        await users.UpdateAsync(u);
        db.AuditEvents.Add(new AuditEvent
        {
            Action = u.IsActive ? "UserActivated" : "UserDeactivated",
            ActorName = "Platform Admin",
            Note = u.Email
        });
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    static bool IsValidAssignment(string role, int? countryId, int? territoryId, int? siteId, Territory? terr, Site? site) =>
        (role == Roles.PlatformAdmin && countryId is null && territoryId is null && siteId is null) ||
        (role == Roles.CountryOpsManager && countryId is not null && territoryId is null && siteId is null) ||
        (role == Roles.TerritoryManager && terr?.CountryId == countryId && siteId is null) ||
        ((role == Roles.SiteManager || role == Roles.StaffMember) && site?.TerritoryId == territoryId && site?.Territory.CountryId == countryId);

    async Task<Territory?> ResolveTerritory(int? territoryId) =>
        territoryId is null ? null : await db.Territories.FindAsync(territoryId);

    async Task<Site?> ResolveSite(int? siteId) =>
        siteId is null ? null : await db.Sites.Include(x => x.Territory).SingleOrDefaultAsync(x => x.Id == siteId);

    async Task Load()
    {
        Users = await users.Users.OrderBy(x => x.Email).ToListAsync();
        Countries = await db.Countries.ToListAsync();
        Territories = await db.Territories.ToListAsync();
        Sites = await db.Sites.Include(x => x.Territory).ThenInclude(x => x.Country).ToListAsync();
        UserRoles = [];
        foreach (var u in Users)
        {
            var roles = await users.GetRolesAsync(u);
            UserRoles[u.Id] = roles.FirstOrDefault() ?? "";
        }
    }
}
