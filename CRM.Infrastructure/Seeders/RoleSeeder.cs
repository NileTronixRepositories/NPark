using BuildingBlock.Application.Repositories;
using CRM.Application.Abstraction.Seeder;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Seeders
{
    internal class RoleSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 2;
        private readonly IGenericRepository<Role> _roleRepo;
        private readonly ILogger<RoleSeeder> _logger;

        public static readonly Guid SuperAdminRoleId = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479");
        public static readonly Guid AccountRoleId = new Guid("f37ac10b-58cc-4372-a567-0e02b2c3d479");

        public RoleSeeder(IGenericRepository<Role> roleRepo, ILogger<RoleSeeder> logger)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            if (!Guid.TryParse(SuperAdminRoleId.ToString(), out var validGuid))
            {
                _logger.LogError("Invalid GUID format for AdminRoleId: {AdminRoleId}", SuperAdminRoleId);
                return;
            }
            // Admin
            var admin = await _roleRepo.GetByIdAsync(SuperAdminRoleId, default);
            if (admin is null)
            {
                var role = Role.CreateWithID(SuperAdminRoleId, "SuperAdmin", "مدير", "For Admins");
                await _roleRepo.AddAsync(role, default);
                _logger.LogInformation("Admin role created.");
            }

            //Account
            var account = await _roleRepo.GetByIdAsync(AccountRoleId, default);
            if (account is null)
            {
                var role = Role.CreateWithID(AccountRoleId, "AccountAdmin", "مدير حساب", "For Account Admins");
                await _roleRepo.AddAsync(role, default);
                _logger.LogInformation("Account role created.");
            }

            await _roleRepo.SaveChangesAsync();
            _logger.LogInformation("Roles Seeded Successfully.");
        }
    }
}