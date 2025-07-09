﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Dapper;
using RewardsAndRecognitionRepository.Data;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Interfaces.Dapper;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;
using RewardsAndRecognitionRepository.Repos.Dapper;
using RewardsAndRecognitionRepository.Repositories;
using RewardsAndRecognitionRepository.Service;
using RewardsAndRecognitionSystem.Common;
using RewardsAndRecognitionSystem.Filters;
using RewardsAndRecognitionSystem.Middleware;
using Serilog;
using Serilog.Filters.Expressions;

try
{
    // Enable Serilog internal debugging (optional but useful)
    Serilog.Debugging.SelfLog.Enable(Console.Error);

    var builder = WebApplication.CreateBuilder(args);

    // Enable internal Serilog diagnostics (optional, helps debug filters)
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    // Configure Serilog programmatically for fully separated logs
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()

        // Info only
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Information)
            .WriteTo.File(
                path: "Logs/info-log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
            )
        )

        // Warning only
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Warning)
            .WriteTo.File(
                path: "Logs/warning-log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
            )
        )

        // Error and Fatal only
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(le =>
                le.Level == Serilog.Events.LogEventLevel.Error ||
                le.Level == Serilog.Events.LogEventLevel.Fatal)
            .WriteTo.File(
                path: "Logs/error-log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
            )
        )
        .CreateLogger();
    builder.Host.UseSerilog();

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddRazorPages();
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

    builder.Services.AddSingleton<IModelMetadataProvider, EmptyModelMetadataProvider>();

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentity<User, IdentityRole>()
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

    // Adding Services
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
    });

    // Custom Exception Filter
    /*builder.Services.AddControllersWithViews(config =>
    {
        config.Filters.Add(typeof(CustomExceptionFilter));
    });*/

    var app = builder.Build();
    
    app.UseMiddleware<ExceptionHandlingMiddleware>();
   

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
}
catch(Exception ex)
{
    var statusCode = ex switch
    {
        ArgumentNullException => 400,
        UnauthorizedAccessException => 401,
        KeyNotFoundException => 404,
        _ => 500
    };

    // ✅ Log error using Serilog with extra info
    Log.Fatal(ex,
        "Startup exception occurred. StatusCode={StatusCode}, Type={Type}, Message={Message}",
        statusCode,
        ex.GetType().FullName,
        ex.Message);
    // Now start a minimal host that returns a user-friendly error page
    var errorApp = WebApplication.Create();

    errorApp.Run(async context =>
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(@"
            <html>
                <head><title>Error</title></head>
                <body>
                    <h1>Oops! Something went wrong.</h1>
                    <p>Status Code: " + statusCode + @"</p>
                    <p>Please try again later or contact support.</p>
                </body>
            </html>");
    });
    errorApp.Run();

}
finally
{
    Log.CloseAndFlush(); // 🚨 Always flush logs on app exit
}