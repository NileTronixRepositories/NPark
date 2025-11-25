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

        // ثابتين علشان نستخدمهم فى الـ Permissions
        public static readonly Guid AdminRoleId = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479");

        public static readonly Guid EntranceCashierRoleId = new Guid("11111111-1111-1111-1111-111111111111");
        public static readonly Guid ExitCashierRoleId = new Guid("22222222-2222-2222-2222-222222222222");
        public static readonly Guid SupervisorRoleId = new Guid("3f47ac10-b58c-4372-a567-0e02b2c3d480");

        public RoleSeeder(IGenericRepository<Role> roleRepo, ILogger<RoleSeeder> logger)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            if (!Guid.TryParse(AdminRoleId.ToString(), out var validGuid))
            {
                _logger.LogError("Invalid GUID format for AdminRoleId: {AdminRoleId}", AdminRoleId);
                return;
            }
            // Admin
            var admin = await _roleRepo.GetByIdAsync(AdminRoleId, default);
            if (admin is null)
            {
                var role = Role.CreateWithID(AdminRoleId, "Admin", "مدير", "For Admins");
                await _roleRepo.AddAsync(role, default);
                _logger.LogInformation("Admin role created.");
            }

            // EntranceCashier
            var entranceCashier = await _roleRepo.GetByIdAsync(EntranceCashierRoleId, default);
            if (entranceCashier is null)
            {
                var role = Role.CreateWithID(
                    EntranceCashierRoleId,
                    "EntranceCashier",
                    "كاﺷير الدخول",
                    "Parking entrance cashier"
                );
                await _roleRepo.AddAsync(role, default);
                _logger.LogInformation("EntranceCashier role created.");
            }

            // ExitCashier
            var exitCashier = await _roleRepo.GetByIdAsync(ExitCashierRoleId, default);
            if (exitCashier is null)
            {
                var role = Role.CreateWithID(
                    ExitCashierRoleId,
                    "ExitCashier",
                    "كاﺷير الخروج",
                    "Parking exit cashier"
                );
                await _roleRepo.AddAsync(role, default);
                _logger.LogInformation("ExitCashier role created.");
            }

            // Supervisor Role
            var supervisor = await _roleRepo.GetByIdAsync(SupervisorRoleId, default);
            if (supervisor is null)
            {
                var role = Role.CreateWithID(SupervisorRoleId, "Supervisor", "مشرف", "For Supervisors");
                await _roleRepo.AddAsync(role, default);
                _logger.LogInformation("Supervisor role created.");
            }

            await _roleRepo.SaveChangesAsync();
            _logger.LogInformation("Roles Seeded Successfully.");
        }
    }
}