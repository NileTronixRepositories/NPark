using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Infrastracture.Bootstrap;
using BuildingBlock.Infrastracture.Interceptors;
using BuildingBlock.Infrastracture.Options;
using BuildingBlock.Infrastracture.Repositories;
using BuildingBlock.Infrastracture.Service;
using CRM.Application.Abstraction;
using CRM.Application.Abstraction.Dashboard;
using CRM.Application.Abstraction.Security;
using CRM.Application.Abstraction.Seeder;
using CRM.Infrastructure.Autorization;
using CRM.Infrastructure.Option;
using CRM.Infrastructure.Seeders;
using CRM.Infrastructure.Service;
using CRM.Infrastructure.Service.Dashboard;
using CRM.Infrastructure.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CRM.Infrastructure.Bootstrap
{
    public static class Bootstrap
    {
        public static IServiceCollection InfrastructureInjection(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EncryptionOptions>(configuration.GetSection("EncryptionOptions"));
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.AddScoped<IEmailSender, MailKitEmailSender>();
            services.AddDbConfig(configuration);
            services.AddScoped<IDbContextProvider, DbContextProvider<CRMDBContext>>();
            services.AddSecurity();
            services.AddEfInfrastructure();
            services.AddBuildingBlockAuditingAndSoftDelete();
            services.EnsureDatabaseInitializationAndUpToDate();
            services.AddHttpClient();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddScoped<IAdminDashboard, AdminDashboardService>();
            services.AddMediaService();
            services.AddQrCodeService();
            services.AddSeeding();
            services.AddAuth(configuration);

            //services.AddHttpClient("device-http", c =>
            //{
            //}).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            //services.AddScoped<ISendProtocol, SendProtocol>();
            services.AddScoped<ITokenReader, JwtReader>();

            return services;
        }

        private static IServiceCollection AddDbConfig(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("Database")!;

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A valid database connection string must be provided.");

            services.AddDbContext<CRMDBContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
                options.AddInterceptors(sp.GetRequiredService<SoftDeleteEntitiesInterceptor>());
                options.AddInterceptors(sp.GetRequiredService<AuditableEntitiesInterceptor>());
                options.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            return services;
        }

        #region Database ifnitialization

        private static void EnsureDatabaseInitializationAndUpToDate(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<CRMDBContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<CRMDBContext>>();

            try
            {
                if (!dbContext.Database.CanConnect())
                {
                    dbContext.Database.EnsureCreated();
                    EnsureMigrationHistoryTableExists(dbContext, logger);
                    MarkMigrationsAsApplied(dbContext, logger);
                    logger.LogInformation("Database created, and migrations marked as applied.");
                    return;
                }

                ApplyOrMarkPendingMigrations(dbContext, logger);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred during database initialization: {ex.Message}");
            }
        }

        private static void EnsureMigrationHistoryTableExists(CRMDBContext dbContext, ILogger<CRMDBContext> logger)
        {
            const string checkTableQuery = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
                               WHERE TABLE_NAME = '__EFMigrationsHistory')
                BEGIN
                    CREATE TABLE __EFMigrationsHistory (
                        MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
                        ProductVersion NVARCHAR(32) NOT NULL
                    );
                END";

            dbContext.Database.ExecuteSqlRaw(checkTableQuery);
            logger.LogInformation("Verified or created the __EFMigrationsHistory table.");
        }

        private static void MarkMigrationsAsApplied(CRMDBContext dbContext, ILogger<CRMDBContext> logger)
        {
            var pendingMigrations = dbContext.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Marking migrations as applied: {string.Join(", ", pendingMigrations)}");
                var efCurrenVersion = GetCurrentEfVersion();
                foreach (var migrationId in pendingMigrations)
                {
                    dbContext.Database.ExecuteSqlInterpolated($@"
                            INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                            VALUES ({migrationId}, {efCurrenVersion})");
                }

                logger.LogInformation("Migrations marked as applied successfully.");
            }
            else
            {
                logger.LogInformation("No migrations to mark as applied.");
            }
        }

        private static string GetCurrentEfVersion()
        {
            var efAssembly = typeof(DbContext).Assembly;
            var version = efAssembly.GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "Unknown";
        }

        private static void ApplyOrMarkPendingMigrations(CRMDBContext dbContext, ILogger<CRMDBContext> logger)
        {
            var pendingMigrations = dbContext.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Pending migrations detected: {string.Join(", ", pendingMigrations)}");

                dbContext.Database.Migrate();
                logger.LogInformation($"Applied migrations: {string.Join(", ", pendingMigrations)}");
            }
            else
            {
                logger.LogInformation("No pending migrations.");
            }
        }

        #endregion Database ifnitialization

        #region Seeding Database

        private static IServiceCollection AddSeeding(this IServiceCollection services)
        {
            services.AddScoped<ISeeder, PermissionSeeder>();
            services.AddScoped<ISeeder, RoleSeeder>();
            services.AddScoped<ISeeder, RolePermissionSeeder>();
            services.AddScoped<ISeeder, SuperAdminSeeder>();
            services.AddScoped<IEnsureSeeding, EnsureSeeding>();
            services.AddHostedService<SeedingHostedService>();

            return services;
        }

        private sealed class SeedingHostedService : IHostedService
        {
            private readonly IServiceScopeFactory _scopeFactory;
            private readonly ILogger<SeedingHostedService> _logger;

            public SeedingHostedService(IServiceScopeFactory scopeFactory, ILogger<SeedingHostedService> logger)
            {
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CRMDBContext>();

                //await db.Database.MigrateAsync(cancellationToken); if want to ensure!

                var seeder = scope.ServiceProvider.GetRequiredService<IEnsureSeeding>();
                _logger.LogInformation("Starting database seeding (HostedService)...");
                await seeder.SeedDatabaseAsync();
                _logger.LogInformation("Database seeding completed (HostedService).");
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }

        #endregion Seeding Database

        #region Authentication & Authorization

        private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtProvider, JwtProvider>();
            var jwtSettings = new JwtOption();
            configuration.GetSection(JwtOption.SectionName).Bind(jwtSettings);

            services.Configure<JwtOption>(
                configuration.GetSection(JwtOption.SectionName));
            services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = jwtSettings.Issuer,
                                ValidAudience = jwtSettings.Audience,
                                IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtSettings.Secret))
                            };
                        });

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }

        #endregion Authentication & Authorization
    }
}