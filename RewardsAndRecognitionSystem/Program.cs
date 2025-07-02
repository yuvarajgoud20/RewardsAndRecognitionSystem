using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Dapper;
using RewardsAndRecognitionRepository.Data;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Interfaces.Dapper;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;
using RewardsAndRecognitionRepository.Repos.Dapper;
using RewardsAndRecognitionRepository.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<ISample, DapperNotification>();

builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ITeamRepo, TeamRepo>();
builder.Services.AddScoped<INominationRepo, NominationRepo>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IApprovalRepo, ApprovalRepo>();
builder.Services.AddScoped<IYearQuarterRepo, YearQuarterRepo>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    await DbInitializer.SeedRolesAndUsersAsync(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
