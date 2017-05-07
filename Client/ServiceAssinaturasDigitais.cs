using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    class ServiceAssinaturasDigitais
    {
        private RSACryptoServiceProvider rsaVerify;

        public ServiceAssinaturasDigitais(string publicKey)
        {
            rsaVerify = new RSACryptoServiceProvider();
            rsaVerify.FromXmlString(publicKey);
        }

        private byte[] ObterHash(string texto)
        {
            byte[] dados;
            byte[] hash;

            using (SHA512 sha512 = SHA512.Create())
            {
                dados = Encoding.UTF8.GetBytes(texto);
                hash = sha512.ComputeHash(dados);
            }

            return hash;
        }

        private bool VerAssinaturaHash(string hashServer, string assinaturaServer)
        {
            byte[] hash = Convert.FromBase64String(hashServer);
            byte[] assinatura = Convert.FromBase64String(assinaturaServer);
            bool result;

            result = rsaVerify.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA512"), assinatura);

            return result;
        }

        private bool VerAssinaturaDados(string dadosServer, string assinaturaServer)
        {
            byte[] dados = Encoding.UTF8.GetBytes(dadosServer);
            byte[] assinatura = Convert.FromBase64String(assinaturaServer);
            bool result;

            using (SHA512 sha512 = SHA512.Create())
            {
                result = rsaVerify.VerifyData(dados, sha512, assinatura);
            }

            return result;
        }
    }
}
