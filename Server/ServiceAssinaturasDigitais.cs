using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceAssinaturasDigitais
    {
        private RSACryptoServiceProvider rsaSign;

        //public string publicKey;

        public ServiceAssinaturasDigitais()
        {
            rsaSign = new RSACryptoServiceProvider();
            //publicKey = rsaSign.ToXmlString(false);
        }

        public string ObterPublicKey()
        {
            return rsaSign.ToXmlString(false);
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
            byte[] signatureBytes = rsaSign.SignHash(hashBytes, CryptoConfig.MapNameToOID("SHA512"));

            return signatureBytes;
        }

        public byte[] AssinarDados(string dadosBrutos)
        {
            byte[] dadosBytes = Encoding.UTF8.GetBytes(dadosBrutos);
            byte[] signatureDados = null;

            using (SHA512 sha512Algorithm = SHA512.Create())
            {
                signatureDados = rsaSign.SignData(dadosBytes, sha512Algorithm);
            }

            return signatureDados;
        }
    }
}
