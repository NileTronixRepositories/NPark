using BuildingBlock.Application.Repositories;
using CRM.Application.Abstraction.Seeder;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Seeders
{
    internal class PermissionSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 1;
        private readonly IGenericRepository<Permission> _permissionRepo;
        private readonly ILogger<PermissionSeeder> _logger;

        public static readonly Guid CreateCenterPermissionId = new("c4f8b057-b37f-4570-bc65-d29d830fb89d");
        public static readonly Guid UpdateCenterPermissionId = new("17a145f1-ef9d-4f74-98ff-bab12c997b8b");
        public static readonly Guid DeleteCenterPermissionId = new("42a3a074-d2c4-459d-bec9-cd6e29b7b38f");
        public static readonly Guid ReadCenterPermissionId = new("55555555-5555-5555-5555-555555555555");

        public PermissionSeeder(IGenericRepository<Permission> permissionRepo, ILogger<PermissionSeeder> logger)
        {
            _permissionRepo = permissionRepo ?? throw new ArgumentNullException(nameof(permissionRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            var toInsert = new List<Permission>();

            async Task AddIfNotExists(Guid id, string name, string nameAr, string desc)
            {
                var existing = await _permissionRepo.GetByIdAsync(id, default);
                if (existing is not null)
                {
                    _logger.LogInformation("Permission {PermissionName} already exists. Skipping.", name);
                    return;
                }

                toInsert.Add(Permission.CreateWithID(id, name, nameAr, desc));
            }

            // CRUD
            await AddIfNotExists(CreateCenterPermissionId, "Platform:Centers:Create", "إضافة", "For Creating");
            await AddIfNotExists(ReadCenterPermissionId, "Platform:Centers:Read", "قراءة", "For Reading");
            await AddIfNotExists(UpdateCenterPermissionId, "Platform:Centers:Update", "تعديل", "For Updating");
            await AddIfNotExists(DeleteCenterPermissionId, "Platform:Centers:Delete", "حذف", "For Deleting");

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