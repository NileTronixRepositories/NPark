using BuildingBlock.Application.Repositories;
using CRM.Application.Abstraction.Seeder;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Seeders
{
    internal class RolePermissionSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 3;
        private readonly IGenericRepository<RolePermission> _rolePermissionRepo;
        private readonly ILogger<RolePermissionSeeder> _logger;

        public RolePermissionSeeder(
            IGenericRepository<RolePermission> rolePermissionRepo,
            ILogger<RolePermissionSeeder> logger)
        {
            _rolePermissionRepo = rolePermissionRepo ?? throw new ArgumentNullException(nameof(rolePermissionRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            var toInsert = new List<RolePermission>();

            async Task AddIfNotExists(Guid roleId, Guid permissionId)
            {
                var exists = await _rolePermissionRepo.IsExistAsync(
                    rp => rp.RoleId == roleId && rp.PermissionId == permissionId,
                    default);

                if (exists)
                {
                    _logger.LogInformation(
                        "RolePermission already exists for Role {RoleId} and Permission {PermissionId}. Skipping.",
                        roleId, permissionId);
                    return;
                }

                toInsert.Add(RolePermission.Create(roleId, permissionId));
            }

            // Admin → كل الـ Permissions
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.CreateCenterPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.ReadCenterPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.UpdateCenterPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.DeleteCenterPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.CreateProductPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.ReadProductPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.UpdateProductPermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.DeleteProductPermissionId);
            await AddIfNotExists(RoleSeeder.AccountRoleId, PermissionSeeder.CreateTicketPermissionId);
            await AddIfNotExists(RoleSeeder.AccountRoleId, PermissionSeeder.ReadTicketPermissionId);
            await AddIfNotExists(RoleSeeder.AccountRoleId, PermissionSeeder.UpdateTicketPermissionId);
            await AddIfNotExists(RoleSeeder.AccountRoleId, PermissionSeeder.DeleteTicketPermissionId);
            await AddIfNotExists(RoleSeeder.AccountRoleId, PermissionSeeder.ReadProductPermissionId);
            await AddIfNotExists(RoleSeeder.AccountRoleId, PermissionSeeder.ReadAccountSitePermissionId);
            await AddIfNotExists(RoleSeeder.SuperAdminRoleId, PermissionSeeder.ReadPlatformSitePermissionId);

            if (toInsert.Count == 0)
            {
                _logger.LogInformation("All RolePermission mappings already exist. Skipping RolePermissionSeeder.");
                return;
            }

            await _rolePermissionRepo.AddRangeAsync(toInsert, default);
            await _rolePermissionRepo.SaveChangesAsync();

            _logger.LogInformation("RolePermission Seeded Successfully.");
        }
    }
}