using BuildingBlock.Domain.EntitiesHelper;

namespace CRM.Domain.Entities
{
    public sealed class SuperAdmin : AggregateRoot<Guid>
    {
        public string Email { get; private set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public Guid RoleId { get; private set; }
        public Role Role { get; private set; } = null!;

        private SuperAdmin()
        { }

        public static SuperAdmin Create(string email, string password, string phoneNumber)
        {
            return new SuperAdmin()
            {
                Email = email,
                Password = password,
                PhoneNumber = phoneNumber,
            };
        }

        public void UpdateEmail(string email) => Email = email;

        public void UpdatePassword(string password) => Password = password;

        public void UpdatePhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;

        public void UpdateRole(Guid roleId) => RoleId = roleId;
    }
}