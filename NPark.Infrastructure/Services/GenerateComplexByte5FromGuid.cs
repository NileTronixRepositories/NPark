using NPark.Application.Abstraction.Security;
using System.Security.Cryptography;

namespace NPark.Infrastructure.Services
{
    public class ByteVerificationService : IByteVerificationService
    {
        // Method to generate Byte 5 (same as the one used to generate Byte 5 during creation)
        public byte GenerateComplexByte5FromGuid(byte[] guidBytes)
        {
            // Ensure we have 4 bytes
            if (guidBytes.Length != 4)
                throw new ArgumentException("GUID should be 4 bytes");

            // Step 1: Sum all bytes
            int sum = guidBytes.Sum(b => b);

            // Step 2: Multiply the sum by a constant factor
            int product = sum * 12345; // Constant factor, you can change this value

            // Step 3: XOR operation with a random byte
            byte randomByte = 0xAA; // You can change this byte or make it dynamic
            product ^= randomByte;

            // Step 4: Use a shift operation on the product
            product = (product << 3) | (product >> (32 - 3)); // Left circular shift by 3 bits

            // Step 5: Hash the resulting value (using SHA256 for example)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(BitConverter.GetBytes(product));

                // Step 6: Return the first byte of the hash as Byte 5
                return hashBytes[0];
            }
        }

        // Method to verify the Byte 5 from the received 5 bytes
        public bool VerifyByte5(byte[] receivedBytes)
        {
            // Ensure the received byte array has exactly 5 bytes
            if (receivedBytes.Length != 5)
            {
                throw new ArgumentException("Received bytes must be 5 bytes.");
            }

            // The first 4 bytes are from the GUID
            byte[] guidBytes = receivedBytes.Take(4).ToArray();

            // The last byte is the received Byte 5
            byte receivedByte5 = receivedBytes[4];

            // Generate the expected Byte 5 from the GUID bytes using the same equation
            byte expectedByte5 = GenerateComplexByte5FromGuid(guidBytes);

            // Compare the expected Byte 5 with the received Byte 5
            return receivedByte5 == expectedByte5;
        }

        public byte[] DecodeBase64ToBytes(string base64String)
        {
            // Decode the Base64 string to byte array
            byte[] decodedBytes = Convert.FromBase64String(base64String);

            // Ensure we are getting exactly 5 bytes
            if (decodedBytes.Length != 5)
            {
                throw new ArgumentException("Decoded byte array must have exactly 5 bytes.");
            }

            return decodedBytes;
        }
    }
}