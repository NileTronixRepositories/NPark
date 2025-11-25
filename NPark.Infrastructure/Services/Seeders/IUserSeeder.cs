using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Repositories;
using Microsoft.Extensions.Logging;
using NPark.Application.Abstraction;
using NPark.Domain.Entities;

namespace NPark.Infrastructure.Services.Seeders
{
    internal class IUserSeeder : ISeeder
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly ILogger<IUserSeeder> _logger;
        private readonly IPasswordService _passwordService;

        public int ExecutionOrder { get; set; } = 4;

        // Usernames / Emails ثابتة علشان نقدر نتحقق منهم
        private const string AdminUserName = "Admin";

        private const string AdminEmail = "Admin@gmail.com";

        private const string EntranceCashierUserName = "EntranceCashier";
        private const string EntranceCashierEmail = "entrance.cashier@npak.local";

        private const string ExitCashierUserName = "ExitCashier";
        private const string ExitCashierEmail = "exit.cashier@npak.local";
        private const string SupervisorUserName = "Supervisor";
        private const string SupervisorEmail = "supervisor@npak.local";

        public IUserSeeder(
            IGenericRepository<User> userRepo,
            ILogger<IUserSeeder> logger,
            IPasswordService passwordService)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        public async Task SeedAsync()
        {
            // Admin
            await CreateUserIfNotExistsAsync(
                userName: AdminUserName,
                email: AdminEmail,
                fullName: "System Admin",
                rawPassword: "Admin123",
                phone: "01004117696",
                nationalId: "2650221627841",
                roleId: RoleSeeder.AdminRoleId);

            // EntranceCashier
            await CreateUserIfNotExistsAsync(
                userName: EntranceCashierUserName,
                email: EntranceCashierEmail,
                fullName: "Entrance Cashier",
                rawPassword: "Entrance123",
                phone: "01000000001",
                nationalId: "11111111111111",
                roleId: RoleSeeder.EntranceCashierRoleId);

            // ExitCashier
            await CreateUserIfNotExistsAsync(
                userName: ExitCashierUserName,
                email: ExitCashierEmail,
                fullName: "Exit Cashier",
                rawPassword: "Exit123",
                phone: "01000000002",
                nationalId: "22222222222222",
                roleId: RoleSeeder.ExitCashierRoleId);

            // Supervisor User
            await CreateUserIfNotExistsAsync(
                userName: SupervisorUserName,
                email: SupervisorEmail,
                fullName: "Supervisor",
                rawPassword: "Supervisor123",
                phone: "01000000003",
                nationalId: "33333333333333",
                roleId: RoleSeeder.SupervisorRoleId);

            await _userRepo.SaveChangesAsync();
            _logger.LogInformation("Users Seeded Successfully (Admin, EntranceCashier, ExitCashier).");
        }

        private async Task CreateUserIfNotExistsAsync(
            string userName,
            string email,
            string fullName,
            string rawPassword,
            string phone,
            string nationalId,
            Guid roleId)
        {
            var exists = await _userRepo.IsExistAsync(
                u => u.Username == userName || u.Email == email,
                default);

            if (exists)
            {
                _logger.LogInformation("User {UserName} already exists. Skipping.", userName);
                return;
            }

            var hashedPassword = _passwordService.Hash(rawPassword);

            var user = User.Create(
                username: userName,
                email: email,
                name: fullName,
                passwordHash: hashedPassword,
                phoneNumber: phone,
                nationalId: nationalId);

            user.SetRole(roleId);

            await _userRepo.AddAsync(user, default);
            _logger.LogInformation("User {UserName} created with role {RoleId}.", userName, roleId);
        }
    }
}