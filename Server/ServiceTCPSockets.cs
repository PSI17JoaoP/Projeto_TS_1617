using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ServiceTCPSockets
    {
        private const int PORT = 9090;

        private static int bytesRead;

        private static TcpListener tcpListener = null;
        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;

        public static void StartServer()
        {
            try
            {
                Console.WriteLine("Starting Server ...");
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
                tcpListener = new TcpListener(endPoint);
                Console.WriteLine("Server Ready!");
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static void StartListener()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("Waiting for connections ...");
            }
            catch (Exception)
            {

                throw;
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

                throw;
            }
        }

        public static void GetClientMessage()
        {
            try
            {
                int bytesMensagemBufferSize = tcpClient.ReceiveBufferSize;
                byte[] bytesMensagemBuffer = new byte[bytesMensagemBufferSize];

                bytesRead = networkStream.Read(bytesMensagemBuffer, 0, bytesMensagemBufferSize);
                Console.WriteLine("Message Received!", Encoding.UTF8.GetString(bytesMensagemBuffer, 0, bytesRead));
            }
            catch (Exception)
            {

                throw;
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

                throw;
            }
        }
    }
}
