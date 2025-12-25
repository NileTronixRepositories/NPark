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

        // Center
        public static readonly Guid CreateCenterPermissionId = new("c4f8b057-b37f-4570-bc65-d29d830fb89d");

        public static readonly Guid UpdateCenterPermissionId = new("17a145f1-ef9d-4f74-98ff-bab12c997b8b");
        public static readonly Guid DeleteCenterPermissionId = new("42a3a074-d2c4-459d-bec9-cd6e29b7b38f");
        public static readonly Guid ReadCenterPermissionId = new("55555555-5555-5555-5555-555555555555");

        // Product
        public static readonly Guid CreateProductPermissionId = new("7b1e9c2e-5d3b-4c5e-8b0a-2a9dd3f8c0a1");

        public static readonly Guid ReadProductPermissionId = new("a6b0c0c7-2b76-4f21-9a0b-2d1a2d7f8c02");
        public static readonly Guid UpdateProductPermissionId = new("f5a1e18d-8f4b-4e18-9a7f-4f6e9b3a0c03");
        public static readonly Guid DeleteProductPermissionId = new("c3d4a2b1-1e1b-4b3a-8b7a-8c9f0a1b2c04");

        // Ticket (NEW) - CRUD
        public static readonly Guid CreateTicketPermissionId = new("b9f2b7c1-0e42-4a6e-9bb5-5d9c31d3a101");

        public static readonly Guid ReadTicketPermissionId = new("b9f2b7c1-0e42-4a6e-9bb5-5d9c31d3a102");
        public static readonly Guid UpdateTicketPermissionId = new("b9f2b7c1-0e42-4a6e-9bb5-5d9c31d3a103");
        public static readonly Guid DeleteTicketPermissionId = new("b9f2b7c1-0e42-4a6e-9bb5-5d9c31d3a104");
        public static readonly Guid UpdateTicketBySuperAdminPermissionId = new("b8f2b7c1-0e42-4a6e-9bb5-5d9c31d3a104");

        // Site (NEW) - Read Only
        public static readonly Guid ReadAccountSitePermissionId = new("2d1b71bb-707f-4b1e-9df9-7d58b1f2b201");

        public static readonly Guid ReadPlatformSitePermissionId = new("2d1b71bb-707f-4b1e-9df9-7d58b1f2b202");

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

            // Center - CRUD
            await AddIfNotExists(CreateCenterPermissionId, "Platform:Centers:Create", "إضافة", "For Creating");
            await AddIfNotExists(ReadCenterPermissionId, "Platform:Centers:Read", "قراءة", "For Reading");
            await AddIfNotExists(UpdateCenterPermissionId, "Platform:Centers:Update", "تعديل", "For Updating");
            await AddIfNotExists(DeleteCenterPermissionId, "Platform:Centers:Delete", "حذف", "For Deleting");

            // Product - CRUD
            await AddIfNotExists(CreateProductPermissionId, "Platform:Products:Create", "إضافة", "For Creating");
            await AddIfNotExists(ReadProductPermissionId, "Platform:Products:Read", "قراءة", "For Reading");
            await AddIfNotExists(UpdateProductPermissionId, "Platform:Products:Update", "تعديل", "For Updating");
            await AddIfNotExists(DeleteProductPermissionId, "Platform:Products:Delete", "حذف", "For Deleting");

            // Ticket - CRUD (NEW)
            await AddIfNotExists(CreateTicketPermissionId, "Account:Tickets:Create", "إضافة", "For Creating"); // Add/Create
            await AddIfNotExists(ReadTicketPermissionId, "Account:Tickets:Read", "قراءة", "For Reading");
            await AddIfNotExists(UpdateTicketPermissionId, "Account:Tickets:Update", "تعديل", "For Updating");
            await AddIfNotExists(DeleteTicketPermissionId, "Account:Tickets:Delete", "حذف", "For Deleting");
            await AddIfNotExists(UpdateTicketBySuperAdminPermissionId, "Platform:Tickets:Update", "تعديل", "For Update");
            // Site - Read (NEW)
            await AddIfNotExists(ReadAccountSitePermissionId, "Account:Site:Read", "قراءة", "For Reading Sites (Account)");
            await AddIfNotExists(ReadPlatformSitePermissionId, "Platform:Site:Read", "قراءة", "For Reading Sites (Platform)");

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