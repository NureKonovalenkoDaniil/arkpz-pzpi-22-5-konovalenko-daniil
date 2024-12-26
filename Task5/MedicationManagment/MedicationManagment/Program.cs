using MedicationManagement.DBContext;
using MedicationManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics;
using MedicationManagement.BackgroundServices;

namespace MedicationManagement
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Register services
            RegisterServices(builder);

            // Configure authentication and authorization
            ConfigureAuthentication(builder);

            // Configure Swagger for API documentation
            ConfigureSwagger(builder);

            // Add hosted services
            builder.Services.AddHostedService<ExpiryNotificationService>();
            builder.Services.AddHostedService<StorageConditionMonitoringService>();

            var app = builder.Build();

            // Автоматичне застосування міграцій
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var contextMS = services.GetRequiredService<MedicineStorageContext>();
                    var contextUC = services.GetRequiredService<UserContext>();
                    contextMS.Database.Migrate(); // Застосування міграцій
                    contextUC.Database.Migrate(); // Застосування міграцій
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while applying migrations.");
                }
            }

            // Ensure roles are created
            await EnsureRolesCreated(app);

            // Configure middleware
            ConfigureMiddleware(app);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/index.html")
                {
                    context.Response.Redirect("/", true);
                    return;
                }
                await next();
            });


            app.Run();
        }

        private static void RegisterServices(WebApplicationBuilder builder)
        {
            // Register application services
            builder.Services.AddScoped<IServiceMedicine, ServiceMedicine>();
            builder.Services.AddScoped<IServiceStorageCondition, ServiceStorageCondition>();
            builder.Services.AddScoped<IServiceIoTDevice, ServiceIoTDevice>();
            builder.Services.AddScoped<IServiceAuditLog, ServiceAuditLog>();

            // Register database contexts
            builder.Services.AddDbContext<MedicineStorageContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDbContext<UserContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<UserContext>()
            .AddDefaultTokenProviders();

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
        }

        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(30); // Set cookie expiration to 30 days
                options.SlidingExpiration = true; // Enable sliding expiration
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError("Authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Token validated successfully.");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();
        }

        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API Documentation",
                    Description = "This is the Swagger documentation for your API."
                });
            });
        }

        private static async Task EnsureRolesCreated(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Administrator", "User", "Sensor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                    c.RoutePrefix = "swagger"; // To make Swagger available at the root URL
                });
            }

            app.UseExceptionHandler("/error");
            app.Map("/error", (HttpContext context) =>
            {
                var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                return Results.Problem(error?.Message);
            });

            app.UseRouting();
            app.UseCors("AllowReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
