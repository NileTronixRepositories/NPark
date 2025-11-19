using BuildingBlock.Application.Repositories;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Services.Seeders
{
    internal class PermissionSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 1;
        private readonly IGenericRepository<Permission> _permissionRepo;
        private readonly ILogger<PermissionSeeder> _logger;

        // ثبات الـ IDs علشان نربط عليها بعدين في RolePermission
        private static readonly (Guid Id, string Name, string NameAr, string Desc)[] DefaultPermissions =
        {
            (new Guid("d6f0b8ae-7b3f-4a32-9f38-306eec4c80ff"), "Create", "أضافة", "For Creating"),
            (new Guid("c4f8b057-b37f-4570-bc65-d29d830fb89d"), "Read",   "قراءة", "For Reading"),
            (new Guid("17a145f1-ef9d-4f74-98ff-bab12c997b8b"), "Update", "تعديل", "For Updating"),
            (new Guid("42a3a074-d2c4-459d-bec9-cd6e29b7b38f"), "Delete", "حذف",   "For Deleting"),
        };

        public PermissionSeeder(IGenericRepository<Permission> permissionRepo, ILogger<PermissionSeeder> logger)
        {
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            var toInsert = new List<Permission>();

            foreach (var (id, name, nameAr, desc) in DefaultPermissions)
            {
                var exists = await _permissionRepo.GetByIdAsync(id, default);

                if (exists is not null)
                {
                    _logger.LogInformation("Permission {PermissionName} already exists. Skipping.", name);
                    continue;
                }

                toInsert.Add(Permission.CreateWithID(id, name, nameAr, desc));
            }

            if (toInsert.Count == 0)
            {
                _logger.LogInformation("All permissions already exist. Skipping PermissionSeeder.");
                return;
            }

            await _permissionRepo.AddRangeAsync(toInsert, default);
            await _permissionRepo.SaveChangesAsync();

            _logger.LogInformation("Permissions Seeded Successfully.");
        }
    }
}