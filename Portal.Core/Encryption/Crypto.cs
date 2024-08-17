using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.Encryption
{
    public class Crypto
    {
        private readonly int _saltSize = 32; // 256 bits
        private readonly int _keySize = 32; // 256 bits
        private readonly int _iterations;

        public static readonly byte[] MagicNumber = { 0x41, 0x45, 0x53, 0x5f }; // 'AES_'

        /// <summary>
        /// Create a new instance of Crypto
        /// </summary>
        /// <param name="iterations">Number of iterations for PBKDF2</param>
        public Crypto(int iterations = 1000)
        {
            _iterations = iterations;
        }

        /// <summary>
        /// Encrypt data using AES with a password
        /// </summary>
        /// <param name="data"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] data, string password)
        {
            byte[] salt = GenerateSalt(_saltSize);

            using (var keyDerivationFunction = new Rfc2898DeriveBytes(password, salt, _iterations))
            {
                byte[] key = keyDerivationFunction.GetBytes(_keySize);
                byte[] iv = keyDerivationFunction.GetBytes(16); // AES block size is 128 bits

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new System.IO.MemoryStream())
                    {
                        ms.Write(MagicNumber, 0, MagicNumber.Length); // write magic number to the beginning of the stream
                        ms.Write(salt, 0, salt.Length); // write salt after the magic number

                        using (var cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock(); // this will write the final block of data
                        }

                        return ms.ToArray();
                    }
                }
            }
        }

        public byte[] Decrypt(byte[] data, string password)
        {
            // Validate the magic number
            if (!Crypto.IsAesEncrypted(data))
            {
                throw new ArgumentException("Data is not encrypted with AES", nameof(data));
            }

            byte[] salt = new byte[_saltSize];
            Array.Copy(data, MagicNumber.Length, salt, 0, _saltSize); // read salt bytes after magic number

            using (var keyDerivationFunction = new Rfc2898DeriveBytes(password, salt, _iterations))
            {
                byte[] key = keyDerivationFunction.GetBytes(_keySize);
                byte[] iv = keyDerivationFunction.GetBytes(16); // AES block size is 128 bits

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        // Skip the magic number and salt
                        ms.Write(data, MagicNumber.Length + _saltSize, data.Length - (MagicNumber.Length + _saltSize));

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        using (var originalMemoryStream = new MemoryStream())
                        {
                            try
                            {
                                cs.CopyTo(originalMemoryStream);
                            }
                            catch (CryptographicException e)
                            {
                                throw new CryptographicException("Incorrect Password.", e);
                            }
                            
                            return originalMemoryStream.ToArray();
                        }
                    }
                }
            }
        }

        public static bool IsAesEncrypted(byte[] data, int offset = 0)
        {
            if (offset + data.Length < offset + MagicNumber.Length)
            {
                return false;
            }

            for (int i = offset; i < offset + MagicNumber.Length; i++)
            {
                if (data[i] != MagicNumber[i])
                {
                    return false;
                }
            }
            return true;
        }

        private byte[] GenerateSalt(int size)
        {
            using var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] salt = new byte[size];
            rng.GetBytes(salt);
            return salt;
        }
    }
}
