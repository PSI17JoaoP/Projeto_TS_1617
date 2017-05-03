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
            Console.WriteLine("Starting Server ...");
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            tcpListener = new TcpListener(endPoint);
            Console.WriteLine("Server Ready!");
        }

        public static void StartListener()
        {
            tcpListener.Start();
            Console.WriteLine("Waiting for connections ...");
        }

        public static void AcceptConnection()
        {
            tcpClient = tcpListener.AcceptTcpClient();
            networkStream = tcpClient.GetStream();
            Console.WriteLine("Connection Successful!");
            Console.WriteLine("Waiting for message ...");
        }

        public static void GetClientMessage()
        {
            int bytesMensagemBufferSize = tcpClient.ReceiveBufferSize;
            byte[] bytesMensagemBuffer = new byte[bytesMensagemBufferSize];

            bytesRead = networkStream.Read(bytesMensagemBuffer, 0, bytesMensagemBufferSize);
            Console.WriteLine("Message Received!", Encoding.UTF8.GetString(bytesMensagemBuffer, 0, bytesRead));
        }

        public static void SendFeedback()
        {
            string mensagemFeedback = "Server here. How are you Client?";
            byte[] bytesMensagemFeedback = Encoding.UTF8.GetBytes(mensagemFeedback);

            networkStream.Write(bytesMensagemFeedback, 0, bytesMensagemFeedback.Length);
            Console.WriteLine("Feedback Sent!");
        }
    }
}
