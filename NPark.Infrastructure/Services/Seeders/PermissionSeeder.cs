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

        // CRUD Permissions القديمة
        public static readonly Guid CreatePermissionId = new("d6f0b8ae-7b3f-4a32-9f38-306eec4c80ff");

        public static readonly Guid ReadPermissionId = new("c4f8b057-b37f-4570-bc65-d29d830fb89d");
        public static readonly Guid UpdatePermissionId = new("17a145f1-ef9d-4f74-98ff-bab12c997b8b");
        public static readonly Guid DeletePermissionId = new("42a3a074-d2c4-459d-bec9-cd6e29b7b38f");

        // Parking domain Permissions الجديدة
        public static readonly Guid GenerateTicketPermissionId = new("33333333-3333-3333-3333-333333333333");

        public static readonly Guid GetTicketsPermissionId = new("44444444-4444-4444-4444-444444444444");

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
            await AddIfNotExists(CreatePermissionId, "Create", "إضافة", "For Creating");
            await AddIfNotExists(ReadPermissionId, "Read", "قراءة", "For Reading");
            await AddIfNotExists(UpdatePermissionId, "Update", "تعديل", "For Updating");
            await AddIfNotExists(DeletePermissionId, "Delete", "حذف", "For Deleting");

            // Parking domain
            await AddIfNotExists(GenerateTicketPermissionId, "GenerateTicket", "توليد تذكرة", "Generate parking ticket at entrance");
            await AddIfNotExists(GetTicketsPermissionId, "GetTickets", "عرض التذاكر", "Retrieve parking tickets");

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