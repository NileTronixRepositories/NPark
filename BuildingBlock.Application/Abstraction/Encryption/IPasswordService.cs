namespace BuildingBlock.Application.Abstraction.Encryption
{
    public interface IPasswordService
    {
        /// <summary>ينتج Hash آمن مع Salt داخلي وإصدارات.</summary>
        string Hash(string password);

        /// <summary>يتحقق من مطابقة كلمة المرور للـ Hash المخزَّن.</summary>
        bool Verify(string password, string passwordHash);

        bool IsStrongPassword(string password);
    }
}