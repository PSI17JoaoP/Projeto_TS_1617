using EI.SI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private const int Port = 9090;

        private const string userTemp = "admin";

        private const string passTemp = "admin";

        private static string fileListPath = Path.Combine(Environment.CurrentDirectory, @"Files\");

        private static TcpListener tcpListener = null;
        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;

        //-------------

        private static SqlConnection connect;

        //-------------

        private static ServiceAssinaturasDigitais servicoAssinaturas;

        //-------------

        private static ProtocolSI protocolSI;

        //------------

        static void Main(string[] args)
        {
            string requestClient = null;

            protocolSI = new ProtocolSI();

            ServerStart();
            ServerLogin();

            while (true)
            {
                if (tcpClient.Connected)
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    requestClient = protocolSI.GetStringFromData();

                    Console.WriteLine("Client Request ->" + requestClient);

                    if (requestClient == "GETLIST")
                    {
                        ServerSendList();
                    }

                    else if (requestClient == "GETFILE")
                    {
                        string requestFile = null;

                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        requestFile = protocolSI.GetStringFromData();

                        Console.WriteLine("Sending File '" + requestFile + "' ...");
                        ServerSendFile(requestFile);
                    }
                    else if (requestClient == "SHUTDOWN")
                    {
                        StopServer();
                        break;
                    }
                }

                else
                {
                    StopServer();
                    break;
                }
            }
        }

        private static void ServerStart()
        {
            try
            {
                Console.WriteLine("Starting Server ...");
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, Port);
                tcpListener = new TcpListener(endPoint);
                Console.WriteLine("Server Ready!");

                tcpListener.Start();
                Console.WriteLine("Waiting for connections ...");

                servicoAssinaturas = new ServiceAssinaturasDigitais();
            }

            catch (Exception)
            {
                StopServer();
            }
        }


        private static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            using (HashAlgorithm hashAlgorithm = SHA512.Create())
            {
                // Declarar e inicializar buffer para o texto e salt
                byte[] plainTextWithSaltBytes =
                              new byte[plainText.Length + salt.Length];

                // Copiar texto para buffer
                for (int i = 0; i < plainText.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainText[i];
                }
                // Copiar salt para buffer a seguir ao texto
                for (int i = 0; i < salt.Length; i++)
                {
                    plainTextWithSaltBytes[plainText.Length + i] = salt[i];
                }

                //Devolver hash do text + salt
                return hashAlgorithm.ComputeHash(plainTextWithSaltBytes);
            }
        }

        private static void ServerLogin()
        {

            string usernameClient = null;
            byte[] passwordHash = null;

            byte[] bytesFeedback = null;
            string mensagemFeedback = null;

            //----------------
            string key = null;
            byte[] keyBytes = null;
            //----------------

            try
            {
                do
                {
                    tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Connection Successful!");

                    networkStream = tcpClient.GetStream();
                    Console.WriteLine("Waiting for message ...");

                    do
                    {
                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        usernameClient = protocolSI.GetStringFromData();

                        Console.WriteLine("Client Username Received -> " + usernameClient);


                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                        passwordHash = protocolSI.GetData();

                        Console.WriteLine("Client Password Hash Received -> " + Encoding.UTF8.GetString(passwordHash));

                        //Database
                        connect = new SqlConnection();
                        connect.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\leona\Source\Repos\Projeto_TS_1617\Server\BD.mdf';Integrated Security=True");

                        connect.Open();

                        String sql = "SELECT * FROM Users WHERE Username = @username";
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = sql;

                        
                        SqlParameter param = new SqlParameter("@username", usernameClient);

                        cmd.Parameters.Add(param);
                        cmd.Connection = connect;

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            throw new Exception("Utilizador não existe");
                        }

                        
                        reader.Read();

                        byte[] saltedPasswordHashStored = (byte[])reader["Password"];

                        byte[] saltStored = (byte[])reader["Salt"];

                        byte[] saltedhash = GenerateSaltedHash(passwordHash, saltStored);

                        connect.Close();

                        if (saltedhash.SequenceEqual(saltedPasswordHashStored))
                        {
                            mensagemFeedback = "SUCCESSFUL";
                            bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, mensagemFeedback);
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);

                            Console.WriteLine("Login Attempt -> " + mensagemFeedback);


                            //-----Envio de chave pública
                            key = servicoAssinaturas.ObterPublicKey();
                            keyBytes = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, key);
                            networkStream.Write(keyBytes, 0, keyBytes.Length);

                            Console.WriteLine("Public Key Shared");
                        }
                        else
                        {
                            mensagemFeedback = "FAILED";
                            bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, mensagemFeedback);
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);

                            Console.WriteLine("Login Attempt -> " + mensagemFeedback);
                        }

                        //--------

                        /*if (usernameClient == userTemp && passwordHash == passTemp)
                        {
                            mensagemFeedback = "SUCCESSFUL";
                            bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, mensagemFeedback);
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);

                            Console.WriteLine("Login Attempt -> " + mensagemFeedback);

                            
                            //-----Envio de chave pública
                            key = servicoAssinaturas.ObterPublicKey();
                            keyBytes = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, key);
                            networkStream.Write(keyBytes, 0, keyBytes.Length);

                            Console.WriteLine("Public Key Shared");

                        }

                        else
                        {
                            mensagemFeedback = "FAILED";
                            bytesFeedback = protocolSI.Make(ProtocolSICmdType.DATA, mensagemFeedback);
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);

                            Console.WriteLine("Login Attempt -> " + mensagemFeedback);
                        }*/
                    }
                    while (mensagemFeedback != "SUCCESSFUL");

                }
                while (mensagemFeedback != "SUCCESSFUL");
            }

            catch (Exception)
            {
                throw;
                StopServer();
            }
        }

        private static void ServerSendList()
        {
            try
            {
                string[] fileListArray = Directory.GetFiles(fileListPath);
                byte[] fileListBuffer = null;
                string fileList = null;

                //-----------------------
                byte[] assinaturaBytes = null;
                //-----------------------

                Console.WriteLine("Sending {0} Files:", fileListArray.Count());
                fileList = ConcatFileNames(fileListArray);

                //----------------------
                assinaturaBytes = servicoAssinaturas.AssinarDados(fileList);
                byte[] assinaturaPacket = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, assinaturaBytes);
                networkStream.Write(assinaturaPacket, 0, assinaturaPacket.Length);
                //----------------------

                fileListBuffer = protocolSI.Make(ProtocolSICmdType.DATA, fileList);
                networkStream.Write(fileListBuffer, 0, fileListBuffer.Length);
                Console.WriteLine("All File Names SENT!");
            }

            catch (Exception)
            {
                //throw;
                StopServer();
            }
        }

        private static string ConcatFileNames(string[] fileListArray)
        {
            string fileName = null;
            string fileList = null;

            foreach (string filePath in fileListArray)
            {
                fileName = Path.GetFileName(filePath) + ";";
                Console.WriteLine("\t" + fileName);
                fileList = String.Concat(fileList, fileName);
            }

            return fileList;
        }

        private static void ServerSendFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(fileListPath, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    //NOVO
                    
                    byte[] assinaturaBytes = null;
                    byte[] fullImageBuffer = new byte[fileStream.Length];
                    byte[] imageHash = null;
                    byte[] packetAssinatura = null;

                    fileStream.Read(fullImageBuffer, 0, fullImageBuffer.Length);
                    fileStream.Seek(0, SeekOrigin.Begin);

                    imageHash = servicoAssinaturas.HashImagem(fullImageBuffer);

                    assinaturaBytes = servicoAssinaturas.AssinarHash(imageHash);

                    packetAssinatura = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, assinaturaBytes);
                    networkStream.Write(packetAssinatura, 0, packetAssinatura.Length); //assinatura
                    
                    //----------


                    byte[] imageBuffer = null;
                    int bytesFileRead;

                    int fileBufferSize = 1024;
                    byte[] fileBuffer = new byte[fileBufferSize];

                    byte[] endOfFile;

                    ProtocolSICmdType protocaolTipoResposta;

                    bytesFileRead = fileStream.Read(fileBuffer, 0, fileBufferSize);
                    imageBuffer = protocolSI.Make(ProtocolSICmdType.DATA, fileBuffer, bytesFileRead);
                    networkStream.Write(imageBuffer, 0, imageBuffer.Length);
                    Console.WriteLine("\t-> Bytes Sent: " + bytesFileRead + " + " + (imageBuffer.Length - bytesFileRead));

                    do
                    {
                        bytesFileRead = fileStream.Read(fileBuffer, 0, fileBufferSize);

                        do
                        {
                            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                            protocaolTipoResposta = protocolSI.GetCmdType();

                            if (protocaolTipoResposta == ProtocolSICmdType.ACK)
                            {
                                if (bytesFileRead > 0)
                                {
                                    imageBuffer = protocolSI.Make(ProtocolSICmdType.DATA, fileBuffer, bytesFileRead);
                                    networkStream.Write(imageBuffer, 0, imageBuffer.Length);
                                    Console.WriteLine("\t-> Bytes Sent: " + bytesFileRead + " + " + (imageBuffer.Length - bytesFileRead));
                                }

                                else
                                {
                                    endOfFile = protocolSI.Make(ProtocolSICmdType.EOF);
                                    networkStream.Write(endOfFile, 0, endOfFile.Length);
                                    Console.WriteLine("\t-> End of File (EOF) Sent");
                                }
                            }
                        }
                        while (protocaolTipoResposta != ProtocolSICmdType.ACK);
                    }
                    while (bytesFileRead > 0);

                }

                Console.WriteLine("File '" + fileName + "' SENT!");
            }

            catch (Exception)
            {
                //StopServer();
                throw;
            }
        }

        private static void StopServer()
        {
            if (networkStream != null)
            {
                networkStream.Close();
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
            }

            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }
    }
}
