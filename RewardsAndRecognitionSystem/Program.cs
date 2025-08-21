//final code
using AspNetCore.Localizer.Json.Localizer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
using RewardsAndRecognitionSystem.CustomMappers;
using RewardsAndRecognitionSystem.Filters;
using RewardsAndRecognitionSystem.FluentValidators;
using RewardsAndRecognitionSystem.Middleware;
using RewardsAndRecognitionSystem.SeedData;
using RewardsAndRecognitionSystem.ViewModels;
using Serilog;
using Serilog.Filters.Expressions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);


            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .Enrich.FromLogContext()
               .Enrich.WithMachineName()
               .Enrich.WithThreadId()

               .WriteTo.Logger(lc => lc
                   .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Information)
                   .WriteTo.File(
                       path: "Logs/info-log-.txt",
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7,
                       outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
                   )
               )

               .WriteTo.Logger(lc => lc
                   .Filter.ByIncludingOnly(le => le.Level == Serilog.Events.LogEventLevel.Warning)
                   .WriteTo.File(
                       path: "Logs/warning-log-.txt",
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7,
                       outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
                   )
               )

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


            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
                cfg.AddProfile<EditUserMappingProfile>();
                cfg.AddProfile<CategoryMappingProfle>();
                cfg.AddProfile<TeamViewMapperProfile>();
                cfg.AddProfile<YearQuarterMappingProfile>();
                cfg.AddProfile<NominationMapperProfile>();
            });

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            builder.Services.AddTransient<IValidator<UserViewModel>, UserViewValidator>();
            builder.Services.AddTransient<IValidator<EditUserViewModel>, EditUserViewValidator>();
            builder.Services.AddTransient<IValidator<TeamViewModel>, TeamViewValidator>();
            builder.Services.AddTransient<IValidator<CategoryViewModel>, CategoryViewValidator>();
            builder.Services.AddTransient<IValidator<YearQuarterViewModel>, YearQuarterViewValidator>();
            builder.Services.AddTransient<IValidator<NominationViewModel>, NominationViewValidator>();

            // Background Service
            builder.Services.AddHostedService<PendingNominationBackgroundService>();

            builder.Services.AddRazorPages();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            builder.Services.AddSingleton<IModelMetadataProvider, EmptyModelMetadataProvider>();

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();
            // Localization configuration
            builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "en", "fr", "ar","hi" };
                options.SetDefaultCulture("en");
                options.AddSupportedCultures(supportedCultures);
                options.AddSupportedUICultures(supportedCultures);

            });

            builder.Services.AddScoped<DapperContext>();
            builder.Services.AddScoped<ISample, DapperNotification>();

            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<ITeamRepo, TeamRepo>();
            builder.Services.AddScoped<INominationRepo, NominationRepo>();
            builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
            builder.Services.AddScoped<IApprovalRepo, ApprovalRepo>();
            builder.Services.AddScoped<IYearQuarterRepo, YearQuarterRepo>();



            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.Configure<PaginationSettings>(builder.Configuration.GetSection("PaginationSettings"));

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
            });

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
       

          

            builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            builder.Services.AddSingleton<IStringLocalizer>(sp =>
            {
                var factory = sp.GetRequiredService<IStringLocalizerFactory>();
                return factory.Create(null);
            });

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                await DbInitializer.SeedRolesAndUsersAsync(roleManager, userManager);
                var services = scope.ServiceProvider;
                await SeedData.InitializeAsync(services);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            // Enable localization middleware
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            app.UseRouting();
            // Use localization middleware
         
          
            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                var user = context.User;

                if (context.Request.Path == "/" && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    if (user.IsInRole("TeamLead") || user.IsInRole("Manager") || user.IsInRole("Director"))
                    {
                        context.Response.Redirect("/Dashboard/Index");
                        return;
                    }
                    else
                    {
                        context.Response.Redirect("/Home/Index");
                        return;
                    }
                }

                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;
                if (response.StatusCode == 400)
                {
                    await context.HttpContext.SignOutAsync();
                    context.HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");
                    response.Redirect("/Login");
                }
            });
            app.UseStaticFiles();

            app.Run();
        }
        catch (Exception ex)
        {
            var statusCode = ex switch
            {
                ArgumentNullException => 400,
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                _ => 500
            };

            Log.Fatal(ex,
                "Startup exception occurred. StatusCode={StatusCode}, Type={Type}, Message={Message}",
                statusCode,
                ex.GetType().FullName,
                ex.Message);

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
            Log.CloseAndFlush();
        }
    }
}
