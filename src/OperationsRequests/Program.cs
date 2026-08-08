using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OperationsRequests.Data;
using OperationsRequests.Domain;
using OperationsRequests.Services;
var builder=WebApplication.CreateBuilder(args);
var cs=builder.Configuration.GetConnectionString("DefaultConnection")??throw new InvalidOperationException("Connection string missing.");
builder.Services.AddDbContext<ApplicationDbContext>(o=>o.UseSqlServer(cs));
builder.Services.AddDefaultIdentity<ApplicationUser>(o=>{o.SignIn.RequireConfirmedAccount=false;o.Password.RequiredLength=12;o.Password.RequireDigit=true;o.Password.RequireUppercase=true;o.Password.RequireNonAlphanumeric=true;}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(o=>o.Events.OnValidatePrincipal=async c=>{var um=c.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();var u=await um.GetUserAsync(c.Principal!);if(u is null||!u.IsActive){c.RejectPrincipal();}}); builder.Services.AddAuthorization(o=>{foreach(var r in Roles.All)o.AddPolicy(r,p=>p.RequireRole(r));}); builder.Services.AddScoped<DemoSeeder>();builder.Services.AddScoped<RequestWorkflowService>();builder.Services.AddScoped<ScopeService>();builder.Services.AddRazorPages();
var app=builder.Build(); if(app.Environment.IsDevelopment()){app.UseMigrationsEndPoint();using var scope=app.Services.CreateScope();await scope.ServiceProvider.GetRequiredService<DemoSeeder>().SeedAsync();}else{app.UseExceptionHandler("/Error");app.UseHsts();} app.UseHttpsRedirection();app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.MapStaticAssets();app.MapRazorPages().WithStaticAssets();app.Run();



public partial class Program { }
