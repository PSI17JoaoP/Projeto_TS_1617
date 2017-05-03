using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private RSACryptoServiceProvider rsa;

        private void btnLogin_Click(object sender, EventArgs e)
        {
            gbMenu.Visible = true;
        }

        #region RegionHash

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

        #endregion

        #region RegionRSA

        private void IniciarRSA(string key)
        {
            rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
        }

        private bool VerAssinaturaHash(string hashS, string assinaturaS)
        {
            byte[] hash = Convert.FromBase64String(hashS);
            byte[] assinatura = Convert.FromBase64String(assinaturaS);
            bool result;

            result = rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA512"), assinatura);

            return result;
        }

        private bool VerAssinaturaDados(string dadosS, string assinaturaS)
        {
            byte[] dados = Encoding.UTF8.GetBytes(dadosS);
            byte[] assinatura = Convert.FromBase64String(assinaturaS);
            bool result;

            using (SHA512 sha512 = SHA512.Create())
            {
                result = rsa.VerifyData(dados, sha512, assinatura);
            }

            return result;
        }

        #endregion
    }
}
