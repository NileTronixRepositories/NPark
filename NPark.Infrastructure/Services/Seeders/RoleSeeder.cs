using BuildingBlock.Application.Repositories;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Services.Seeders
{
    internal class RoleSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 2;
        private readonly IGenericRepository<Role> _roleRepo;
        private readonly ILogger<RoleSeeder> _logger;

        private static readonly Guid AdminRoleId = new("f47ac10b-58cc-4372-a567-0e02b2c3d479");

        public RoleSeeder(IGenericRepository<Role> roleRepo, ILogger<RoleSeeder> logger)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            var exists = await _roleRepo.GetByIdAsync(AdminRoleId, default);
            if (exists is not null)
            {
                _logger.LogInformation("Admin role already exists. Skipping RoleSeeder.");
                return;
            }

            var role = Role.CreateWithID(AdminRoleId, "Admin", "مدير", "For Admins");
            await _roleRepo.AddAsync(role, default);
            await _roleRepo.SaveChangesAsync();

            _logger.LogInformation("Roles Seeded Successfully.");
        }
    }
}