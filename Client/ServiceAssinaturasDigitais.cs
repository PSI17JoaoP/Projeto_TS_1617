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

        public byte[] ObterHash(string texto)
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

        public byte[] HashImagem(byte[] file)
        {
            byte[] hashDados = null;

            using (SHA512 sha512Algorithm = SHA512.Create())
            {
                hashDados = sha512Algorithm.ComputeHash(file);
            }

            return hashDados;
        }

        public bool VerAssinaturaHash(byte[] hash, byte[] assinaturaServer)
        {
            bool result = rsaVerify.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA512"), assinaturaServer);

            return result;
        }

        public bool VerAssinaturaDados(byte[] dadosServer, byte[] assinaturaServer)
        {
            bool result;

            using (SHA512 sha512 = SHA512.Create())
            {
                result = rsaVerify.VerifyData(dadosServer, sha512, assinaturaServer);
            }

            return result;
        }
    }
}
