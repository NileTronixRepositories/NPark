namespace NPark.Application.Abstraction.Security
{
    public interface IByteVerificationService
    {
        byte GenerateComplexByte5FromGuid(byte[] guidBytes);

        bool VerifyByte5(byte[] receivedBytes);

        public byte[] DecodeBase64ToBytes(string base64String);
    }
}