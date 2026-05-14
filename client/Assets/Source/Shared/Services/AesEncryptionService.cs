using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Source.Shared.Services
{
    public interface IEncryptionService
    {
        Result<string> Encrypt(string plainText, string userSecret);
        Result<string> Decrypt(string cipherText, string userSecret);
    }

    public class AesEncryptionService : IEncryptionService
    {
        private readonly ILoggingService _loggingService;

        private static readonly byte[] InternalAppSalt = Encoding.UTF8.GetBytes("Source_App_Internal_Salt_2026");

        private const int KeyIterations = 10000;

        public AesEncryptionService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public Result<string> Encrypt(string plainText, string userSecret)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(userSecret))
            {
                return Error.Local(ErrorCodes.EncryptionFailed);
            }

            try
            {
                using Aes aes = Aes.Create();

                aes.Key = DeriveKey(userSecret);

                aes.GenerateIV();

                byte[] iv = aes.IV;

                using var ms = new MemoryStream();

                ms.Write(iv, 0, iv.Length);

                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))

                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Zero-Trust Encryption failed.");

                return Error.Local(ErrorCodes.EncryptionFailed);
            }
        }

        public Result<string> Decrypt(string cipherText, string userSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(userSecret))
            {
                return Error.Local(ErrorCodes.DecryptionFailed);
            }

            try
            {
                byte[] fullCipher = Convert.FromBase64String(cipherText);

                using Aes aes = Aes.Create();
                aes.Key = DeriveKey(userSecret);

                int ivSize = aes.BlockSize / 8;
                byte[] iv = new byte[ivSize];
                byte[] encryptedData = new byte[fullCipher.Length - ivSize];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, ivSize);
                Buffer.BlockCopy(fullCipher, ivSize, encryptedData, 0, encryptedData.Length);

                aes.IV = iv;

                using var ms = new MemoryStream(encryptedData);
                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Decryption failed. UserSecret might be wrong or data tampered.");

                return Error.Local(ErrorCodes.DecryptionFailed);
            }
        }

        private static byte[] DeriveKey(string userSecret)
        {
            using var rfc = new Rfc2898DeriveBytes(userSecret, InternalAppSalt, KeyIterations, HashAlgorithmName.SHA256);

            return rfc.GetBytes(32);
        }
    }
}