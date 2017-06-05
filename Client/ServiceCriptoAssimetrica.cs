using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    class ServiceCriptoAssimetrica
    {
        private RSACryptoServiceProvider rsaClient;

        public ServiceCriptoAssimetrica(string publicKey)
        {
            rsaClient = new RSACryptoServiceProvider();
            rsaClient.FromXmlString(publicKey);
        }

        public byte[] EncriptarDados(byte[] dadosBrutos)
        {
            byte[] dadosEncriptados = rsaClient.Encrypt(dadosBrutos, true);

            return dadosEncriptados;
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
            bool result = rsaClient.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA512"), assinaturaServer);

            return result;
        }

        public bool VerAssinaturaDados(byte[] dadosServer, byte[] assinaturaServer)
        {
            bool result;

            using (SHA512 sha512 = SHA512.Create())
            {
                result = rsaClient.VerifyData(dadosServer, sha512, assinaturaServer);
            }

            return result;
        }
    }
}
