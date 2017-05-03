using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Projeto
{
    class ServiceTCPSockets
    {
        private const int port = 9090;

        private static TcpClient tcpClient = null;
        private static NetworkStream networkStream = null;
        private static IPEndPoint endPoint = null;

        public static void StartClient()
        {
            try
            {
                tcpClient = new TcpClient();
                endPoint = new IPEndPoint(IPAddress.Loopback, port);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void StartConnection()
        {
            try
            {
                tcpClient.Connect(endPoint);

                networkStream = tcpClient.GetStream();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static Boolean SendMessage(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            bool result = false;    

            try
            {
                networkStream.Write(msg, 0, msg.Length);
                result = true;
            }
            catch (Exception)
            {

                throw;
            }

            return result;
        }

        public static string GetMessage()
        {
            int bufferResponse = tcpClient.ReceiveBufferSize;
            int bytesRead = 0;
            byte[] msg = new byte[bufferResponse];
            bytesRead = networkStream.Read(msg, 0, bufferResponse);

            string message = Encoding.UTF8.GetString(msg, 0, bytesRead);

            return message;
        }
    }
}
