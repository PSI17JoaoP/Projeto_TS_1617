using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceCriptoAssimetrica
    {
        private RSACryptoServiceProvider rsaServer;

        public ServiceCriptoAssimetrica()
        {
            rsaServer = new RSACryptoServiceProvider();
            File.WriteAllText("Public.txt", rsaServer.ToXmlString(false));
            File.WriteAllText("Private.txt", rsaServer.ToXmlString(true));
        }

        public string ObterPublicKey()
        {
            return rsaServer.ToXmlString(false);
        }

        private byte[] EncriptarDados(byte[] dadosBrutos)
        {
            byte[] dadosEncriptados = rsaServer.Encrypt(dadosBrutos, true);

            return dadosEncriptados;
        }

        private byte[] DecriptarDados(byte[] dadosEncriptados)
        {
            byte[] dadosBrutos = rsaServer.Decrypt(dadosEncriptados, true);

            return dadosBrutos;
        }

        public byte[] HashDados(byte[] dadosBrutos)
        {
            byte[] hashDados = null;

            using (SHA512 sha512Algorithm = SHA512.Create())
            {
                hashDados = sha512Algorithm.ComputeHash(dadosBrutos);
            }

            return hashDados;
        }

        public byte[] HashImagem(byte[] file)
        {
            byte[] hashDados = null;

            using (SHA512 sha512Algorithm = SHA512.Create())
            {
                hashDados = sha512Algorithm.ComputeHash(file);
            }

            return hashDados;
        }

        public byte[] AssinarHash(byte[] hashBytes)
        {
            byte[] signatureBytes = rsaServer.SignHash(hashBytes, CryptoConfig.MapNameToOID("SHA512"));

            return signatureBytes;
        }

        public byte[] AssinarDados(string dadosBrutos)
        {
            byte[] dadosBytes = Encoding.UTF8.GetBytes(dadosBrutos);
            byte[] signatureDados = null;

            using (SHA512 sha512Algorithm = SHA512.Create())
            {
                signatureDados = rsaServer.SignData(dadosBytes, sha512Algorithm);
            }

            return signatureDados;
        }
    }
}
