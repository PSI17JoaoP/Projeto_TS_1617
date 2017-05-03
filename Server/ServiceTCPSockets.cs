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
        private int bytesRead;

        private TcpListener tcpListener = null;
        private TcpClient tcpClient = null;
        private NetworkStream networkStream = null;

        public ServiceTCPSockets(int port)
        {
            StartServer(port);
        }

        private void StartServer(int port)
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
                throw;
            }

        }

        public bool StartConnection()
        {
            try
            {
                bool encontrouLigacao = false;

                tcpListener.Start();
                Console.WriteLine("Waiting for connections ...");

                if(tcpListener.Pending())
                {
                    encontrouLigacao = true;
                }

                return encontrouLigacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool AcceptConnection()
        {
            try
            {
                bool mensagemRecebida = false;

                tcpClient = tcpListener.AcceptTcpClient();
                networkStream = tcpClient.GetStream();
                Console.WriteLine("Connection Successful!");
                Console.WriteLine("Waiting for message ...");

                if(networkStream.DataAvailable == true)
                {
                    mensagemRecebida = true;
                }

                return mensagemRecebida;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetClientMessage()
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

        public void SendFeedback(string mensagemFeedback)
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
