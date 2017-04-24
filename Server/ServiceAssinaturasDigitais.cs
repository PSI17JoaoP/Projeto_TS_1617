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

        public string HashDados(string dadosBrutos)
        {
            string hashDadosString;

            using (SHA1 sha1Algorithm = SHA1.Create())
            {
                byte[] dadosBytes = Encoding.UTF8.GetBytes(dadosBrutos);
                byte[] hashDados = sha1Algorithm.ComputeHash(dadosBytes);

                hashDadosString = Convert.ToBase64String(hashDados);
            }

            return hashDadosString;
        }

        public string AssinarHash(string hashString)
        {
            string assinaturaHash;

            byte[] hashBytes = Convert.FromBase64String(hashString);
            byte[] signatureBytes = rsaSign.SignHash(hashBytes, CryptoConfig.MapNameToOID("SHA1"));

            assinaturaHash = Convert.ToBase64String(signatureBytes);

            return assinaturaHash;
        }

        public string AssinarDados(string dadosBrutos)
        {
            string assinaturaDados;

            byte[] dadosBytes = Encoding.UTF8.GetBytes(dadosBrutos);

            using (SHA1 sha1Algorithm = SHA1.Create())
            {
                byte[] signatureDados = rsaSign.SignData(dadosBytes, sha1Algorithm);
                assinaturaDados = Convert.ToBase64String(signatureDados);
            }

            return assinaturaDados;
        }
    }
}
