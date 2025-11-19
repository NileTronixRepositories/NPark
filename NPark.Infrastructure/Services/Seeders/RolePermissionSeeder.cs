using BuildingBlock.Application.Repositories;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Services.Seeders
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
            await AddIfNotExists(RoleSeeder.AdminRoleId, PermissionSeeder.CreatePermissionId);
            await AddIfNotExists(RoleSeeder.AdminRoleId, PermissionSeeder.ReadPermissionId);
            await AddIfNotExists(RoleSeeder.AdminRoleId, PermissionSeeder.UpdatePermissionId);
            await AddIfNotExists(RoleSeeder.AdminRoleId, PermissionSeeder.DeletePermissionId);
            await AddIfNotExists(RoleSeeder.AdminRoleId, PermissionSeeder.GenerateTicketPermissionId);
            await AddIfNotExists(RoleSeeder.AdminRoleId, PermissionSeeder.GetTicketsPermissionId);

            // EntranceCashier → GenerateTicket + GetTickets
            await AddIfNotExists(RoleSeeder.EntranceCashierRoleId, PermissionSeeder.GenerateTicketPermissionId);
            await AddIfNotExists(RoleSeeder.EntranceCashierRoleId, PermissionSeeder.GetTicketsPermissionId);

            // ExitCashier → GetTickets فقط
            await AddIfNotExists(RoleSeeder.ExitCashierRoleId, PermissionSeeder.GetTicketsPermissionId);

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