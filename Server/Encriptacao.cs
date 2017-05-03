using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Encriptacao
    {
        //Encriptação simétrica, utilizando chaves.
        //RSA --> Assimétrica
        //AES --> Simétrica
        private byte[] symmetricEncrypt(byte[] data, byte[] key, byte[] iv)
        {
            byte[] encData;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform crypto = aes.CreateEncryptor())
                {
                    encData = crypto.TransformFinalBlock(data, 0, data.Length);
                }
            }

            return encData;
        }

        //Desencriptar simétrica
        private byte[] symmetricDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            byte[] DecryptedData;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform crypto = aes.CreateDecryptor())
                {
                    DecryptedData = crypto.TransformFinalBlock(data, 0, data.Length);
                }
            }

            return DecryptedData;
        }




    }
}
