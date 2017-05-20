using EI.SI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        private static void ServerLogin()
        {
            string usernameClient = null;
            string passwordHash = null;

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
                        passwordHash = protocolSI.GetStringFromData();

                        Console.WriteLine("Client Password Hash Received -> " + passwordHash);

                        if (usernameClient == userTemp && passwordHash == passTemp)
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
                    }
                    while (mensagemFeedback != "SUCCESSFUL");

                }
                while (mensagemFeedback != "SUCCESSFUL");
            }

            catch (Exception)
            {
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
                string assinatura = null;
                byte[] assinaturaBytes = null;
                //-----------------------

                Console.WriteLine("Sending {0} Files:", fileListArray.Count());
                fileList = ConcatFileNames(fileListArray);

                //----------------------
                assinatura = servicoAssinaturas.AssinarDados(fileList);
                assinaturaBytes = Convert.FromBase64String(assinatura);
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

                //---------------
                /*string assinatura = null;
                byte[] assinaturaBytes = null;*/
                //--------------- 

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
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

                    /*int fileBufferSize = 30720;
                    byte[] fileBuffer = new byte[fileBufferSize];

                    int bytesRead;

                    while((bytesRead = fileStream.Read(fileBuffer, 0, fileBufferSize)) > 0)
                    {
                        networkStream.Write(fileBuffer, 0, bytesRead);
                    }*/



                    //----------------------------------------
                    /*byte[] file = new byte[fileStream.Length];
                    fileStream.Read(file, 0, file.Length);
                    byte[] hashFile = servicoAssinaturas.HashImagem(file);
                    string hashString = Convert.ToBase64String(hashFile);
                    byte[] hash = Convert.FromBase64String(hashString);

                    assinatura = servicoAssinaturas.AssinarHash(hash);
                    assinaturaBytes = Convert.FromBase64String(assinatura);
                        
                    networkStream.Write(hash, 0, hashFile.Length); //1
                    networkStream.Write(assinaturaBytes, 0, assinaturaBytes.Length); //2

                    fileStream.Position = 0;
                    */
                    //----------------------------------------


                    // Isto funciona
                    //fileStream.CopyTo(networkStream);
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
