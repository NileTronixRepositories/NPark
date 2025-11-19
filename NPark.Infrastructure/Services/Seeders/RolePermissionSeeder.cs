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

        private static readonly Guid AdminRoleId = new("f47ac10b-58cc-4372-a567-0e02b2c3d479");

        private static readonly Guid[] PermissionIds =
        {
            new("d6f0b8ae-7b3f-4a32-9f38-306eec4c80ff"), // Create
            new("c4f8b057-b37f-4570-bc65-d29d830fb89d"), // Read
            new("17a145f1-ef9d-4f74-98ff-bab12c997b8b"), // Update
            new("42a3a074-d2c4-459d-bec9-cd6e29b7b38f"), // Delete
        };

        public RolePermissionSeeder(IGenericRepository<RolePermission> rolePermissionRepo, ILogger<RolePermissionSeeder> logger)
        {
            _rolePermissionRepo = rolePermissionRepo ?? throw new ArgumentNullException(nameof(rolePermissionRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            var toInsert = new List<RolePermission>();

            foreach (var permId in PermissionIds)
            {
                // نفترض إن الـ RolePermission له Composite Key (RoleId + PermissionId)
                var exists = await _rolePermissionRepo.IsExistAsync(
                    rp => rp.RoleId == AdminRoleId && rp.PermissionId == permId,
                    default);

                if (exists)
                {
                    _logger.LogInformation(
                        "RolePermission already exists for Role {RoleId} and Permission {PermissionId}. Skipping.",
                        AdminRoleId, permId);
                    continue;
                }

                toInsert.Add(RolePermission.Create(AdminRoleId, permId));
            }

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