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

        //private static ServiceTCPSockets serverTCP = new ServiceTCPSockets(portTCP);

        private static TcpListener tcpListener = null;
        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;

        static void Main(string[] args)
        {
            int requestBufferSize;
            byte[] requestBuffer;
            string requestClient;

            int bytesRead;

            ServerStart();
            ServerLogin();

            while (true)
            {
                requestBufferSize = tcpClient.ReceiveBufferSize;
                requestBuffer = new byte[requestBufferSize];

                bytesRead = networkStream.Read(requestBuffer, 0, requestBufferSize);
                requestClient = Encoding.UTF8.GetString(requestBuffer, 0, bytesRead);
                Console.WriteLine("Client Request ->" + requestClient);

                if (requestClient == "GETLIST")
                {
                    ServerSendList();
                }

                else if (requestClient == "GETFILE")
                {
                    int requestFileSize;
                    byte[] bufferRequestFile;
                    string requestFile;

                    requestFileSize = tcpClient.ReceiveBufferSize;
                    bufferRequestFile = new byte[requestFileSize];

                    bytesRead = networkStream.Read(bufferRequestFile, 0, requestFileSize);
                    requestFile = Encoding.UTF8.GetString(bufferRequestFile, 0, bytesRead);

                    Console.WriteLine("Sending File '" + requestFile + "' ...");
                    ServerSendFile(requestFile);
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
            }

            catch
            {
                StopServer();
            }
        }

        private static void ServerLogin()
        {
            int bytesRead;

            byte[] bytesUsernameBuffer = null;
            int bytesUsernameBufferSize;
            string usernameClient = null;

            byte[] bytesPasswordBuffer = null;
            int bytesPasswordBufferSize;
            string passwordHash = null;

            byte[] bytesFeedback = null;
            string mensagemFeedback = null;

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
                        bytesUsernameBufferSize = tcpClient.ReceiveBufferSize;
                        bytesUsernameBuffer = new byte[bytesUsernameBufferSize];
                        bytesRead = networkStream.Read(bytesUsernameBuffer, 0, bytesUsernameBufferSize);
                        usernameClient = Encoding.UTF8.GetString(bytesUsernameBuffer, 0, bytesRead);
                        Console.WriteLine("Client Username Received -> " + usernameClient);

                        bytesPasswordBufferSize = tcpClient.ReceiveBufferSize;
                        bytesPasswordBuffer = new byte[bytesPasswordBufferSize];
                        bytesRead = networkStream.Read(bytesPasswordBuffer, 0, bytesPasswordBufferSize);
                        passwordHash = Encoding.UTF8.GetString(bytesPasswordBuffer, 0, bytesRead);
                        Console.WriteLine("Client Password Hash Received -> " + passwordHash);

                        if (usernameClient == userTemp && passwordHash == passTemp)
                        {
                            mensagemFeedback = "SUCCESSFUL";
                            bytesFeedback = Encoding.UTF8.GetBytes(mensagemFeedback);
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);
                            Console.WriteLine("Login Attempt -> " + Encoding.UTF8.GetString(bytesFeedback));
                        }

                        else
                        {
                            mensagemFeedback = "FAILED";
                            bytesFeedback = Encoding.UTF8.GetBytes(mensagemFeedback);
                            networkStream.Write(bytesFeedback, 0, bytesFeedback.Length);
                            Console.WriteLine("Login Attempt -> " + Encoding.UTF8.GetString(bytesFeedback));
                        }
                    }
                    while (mensagemFeedback != "SUCCESSFUL");

                }
                while (mensagemFeedback != "SUCCESSFUL");
            }

            catch
            {
                StopServer();
            }
        }

        private static void ServerSendList()
        {
            try
            {
                string[] fileListArray = Directory.GetFiles(fileListPath);
                byte[] fileListBuffer;
                string fileName;

                foreach (string filePath in fileListArray)
                {
                    fileName = Path.GetFileName(filePath) + ";";
                    Console.WriteLine("Sending File Name '" + fileName + "' ...");
                    fileListBuffer = Encoding.UTF8.GetBytes(fileName);
                    networkStream.Write(fileListBuffer, 0, fileListBuffer.Length);
                    Console.WriteLine("File Name '" + fileName + "' SENT");
                }
            }

            catch
            {
                StopServer();
            }
        }

        private static void ServerSendFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(fileListPath, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    int fileBufferSize = 20480;
                    byte[] fileBuffer = new byte[fileBufferSize];

                    int bytesRead;

                    while((bytesRead = fileStream.Read(fileBuffer, 0, fileBufferSize)) > 0)
                    {
                        networkStream.Write(fileBuffer, 0, bytesRead);
                    }
                }

                Console.WriteLine("File '" + fileName + "' SENT");
            }

            catch
            {
                StopServer();
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
