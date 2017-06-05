using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServiceCriptoSimetrica
    {
        private AesCryptoServiceProvider aesAlgorithm;

        public ServiceCriptoSimetrica(byte[] secretKey, byte[] IV)
        {
            aesAlgorithm = new AesCryptoServiceProvider();
            aesAlgorithm.Key = secretKey;
            aesAlgorithm.IV = IV;
        }

        public byte[] EncryptDados(byte[] dadosBrutos, int tamanhoDados)
        {
            byte[] dadosEncriptados;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(dadosBrutos, 0, tamanhoDados);
                }

                dadosEncriptados = memoryStream.ToArray();
            }

            return dadosEncriptados;
        }

        public byte[] DecryptDados(byte[] dadosEncriptados)
        {
            byte[] dadosDecriptados = new byte[dadosEncriptados.Length];
            int bytesRead;

            using (MemoryStream memoryStream = new MemoryStream(dadosEncriptados))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    bytesRead = cryptoStream.Read(dadosDecriptados, 0, dadosDecriptados.Length);
                }
            }

            byte[] dadosBrutos = new byte[bytesRead];
            Array.Copy(dadosDecriptados, dadosBrutos, dadosBrutos.Length);
            //string stringDecriptada = Encoding.UTF8.GetString(dadosDecriptados, 0, bytesRead);

            //return stringDecriptada;
            return dadosBrutos;
        }
    }
}
