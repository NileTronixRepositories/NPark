using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using CRM.Application.Abstraction.Seeder;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Seeders
{
    internal class SuperAdminSeeder : ISeeder
    {
        private readonly IGenericRepository<SuperAdmin> _adminRepo;
        private readonly ILogger<SuperAdminSeeder> _logger;
        private readonly IPasswordService _passwordService;

        public int ExecutionOrder { get; set; } = 4;
        private const string AdminUserName = "SuperAdmin";
        private const string AdminEmail = "Louayabulnoor@gmail.com";

        public SuperAdminSeeder(
            IGenericRepository<SuperAdmin> userRepo,
            ILogger<SuperAdminSeeder> logger,
            IPasswordService passwordService)
        {
            _adminRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        public async Task SeedAsync()
        {
            // Admin
            await CreateUserIfNotExistsAsync(
                email: AdminEmail,
                rawPassword: "Admin123",
                phone: "01004117696",
                roleId: RoleSeeder.SuperAdminRoleId);

            await _adminRepo.SaveChangesAsync();
            _logger.LogInformation("Users Seeded Successfully (Admin, EntranceCashier, ExitCashier).");
        }

        private async Task CreateUserIfNotExistsAsync(
            string email,
            string rawPassword,
            string phone,
            Guid roleId)
        {
            var exists = await _adminRepo.IsExistAsync(
                u => u.Email == email,
                default);

            if (exists)
            {
                _logger.LogInformation("User {UserName} already exists. Skipping.", email);
                return;
            }

            var hashedPassword = _passwordService.Hash(rawPassword);

            var user = SuperAdmin.Create(
                email: email,
                password: hashedPassword,
                phoneNumber: phone
               );

            user.UpdateRole(roleId);

            await _adminRepo.AddAsync(user, default);
            _logger.LogInformation("User {UserName} created with role {RoleId}.", email, roleId);
        }
    }
}