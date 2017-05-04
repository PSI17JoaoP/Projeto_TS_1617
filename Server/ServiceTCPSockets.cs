using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceTCPSockets
    {
        private static int bytesRead;

        private static TcpListener tcpListener = null;
        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;

        public static void StartServer(int port)
        {
            try
            {
                Console.WriteLine("Starting Server ...");
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);
                tcpListener = new TcpListener(endPoint);
                Console.WriteLine("Server Ready!");
            }
            catch (Exception)
            {
                StopServer();
            }

        }

        public static void StartConnection()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("Waiting for connections ...");
            }
            catch (Exception)
            {
                StopServer();
            }
        }

        public static void AcceptConnection()
        {
            try
            {
                tcpClient = tcpListener.AcceptTcpClient();
                networkStream = tcpClient.GetStream();
                Console.WriteLine("Connection Successful!");
                Console.WriteLine("Waiting for message ...");
            }
            catch (Exception)
            {
                StopServer();
            }
        }

        public static string GetClientMessage()
        {
            try
            {
                int bytesMensagemBufferSize = tcpClient.ReceiveBufferSize;
                byte[] bytesMensagemBuffer = new byte[bytesMensagemBufferSize];

                bytesRead = networkStream.Read(bytesMensagemBuffer, 0, bytesMensagemBufferSize);
                string mensagemCliente = Encoding.UTF8.GetString(bytesMensagemBuffer, 0, bytesRead);
                Console.WriteLine("Client: ", mensagemCliente);

                return mensagemCliente;
            }

            catch (Exception)
            {
                StopServer();

                return "ERROR";
            }
        }

        public static void SendFeedback(string mensagemFeedback)
        {
            try
            {
                byte[] bytesMensagemFeedback = Encoding.UTF8.GetBytes(mensagemFeedback);

                networkStream.Write(bytesMensagemFeedback, 0, bytesMensagemFeedback.Length);
                Console.WriteLine("Feedback Sent!");
            }
            catch (Exception)
            {
                StopServer();
            }
        }

        public static void StopServer()
        {
            if(networkStream == null)
            {
                networkStream.Close();
            }

            if (tcpClient == null)
            {
                tcpClient.Close();
            }

            if (tcpListener == null)
            {
                tcpListener.Stop();
            }
        }
    }
}
