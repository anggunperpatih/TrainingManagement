using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace OperationsRequests.Pages;
public class DemoModel(IWebHostEnvironment environment,IConfiguration configuration):PageModel{public string Password=>configuration["Demo:Password"]??"Demo password is not configured.";public IActionResult OnGet(){return environment.IsDevelopment()?Page():NotFound();}}
