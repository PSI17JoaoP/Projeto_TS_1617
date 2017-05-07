using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    class ServiceCriptoSimetrica
    {
        private byte[] EncryptDados(byte[] dadosBrutos, byte[] secretKey, byte[] iv)
        {
            byte[] dadosEncriptados;

            using (AesCryptoServiceProvider aesAlgorithm = new AesCryptoServiceProvider())
            {
                aesAlgorithm.Key = secretKey;
                aesAlgorithm.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(dadosBrutos, 0, dadosBrutos.Length);
                    }

                    dadosEncriptados = memoryStream.ToArray();
                }
            }

            return dadosEncriptados;
        }

        private byte[] DecryptDados(byte[] dadosEncriptados, byte[] secretKey, byte[] iv)
        {
            byte[] dadosDecriptados = new byte[dadosEncriptados.Length];

            using (AesCryptoServiceProvider aesAlgorithm = new AesCryptoServiceProvider())
            {
                aesAlgorithm.Key = secretKey;
                aesAlgorithm.IV = iv;

                int bytesRead;

                using (MemoryStream memoryStream = new MemoryStream(dadosEncriptados))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        bytesRead = cryptoStream.Read(dadosDecriptados, 0, dadosDecriptados.Length);
                    }
                }
            }

            return dadosDecriptados;
        }
    }
}
