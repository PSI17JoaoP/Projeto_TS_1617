using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private const int portTCP = 9090;

        private const string userTemp = "admin";

        private const string passTemp = "admin";

        private static ServiceTCPSockets serverTCP = new ServiceTCPSockets(portTCP);

        static void Main(string[] args)
        {
            serverTCP.StartConnection();

            bool loopServer = true;
            bool loopCliente = true;

            while(loopServer)
            {
                serverTCP.AcceptConnection();

                while (loopCliente)
                {
                    string requestClient = serverTCP.GetClientMessage();                

                    switch (requestClient)
                    {
                        case "Login":

                            string userCliente = serverTCP.GetClientMessage();
                            string passHashCliente = serverTCP.GetClientMessage();

                            if(userCliente != "ERROR" && passHashCliente != "ERROR")
                            {
                                if (userCliente == userTemp && passHashCliente == passTemp)
                                {
                                    serverTCP.SendFeedback("OK");
                                }

                                else
                                {
                                    serverTCP.SendFeedback("KO");
                                }
                            }

                            else
                            {
                                serverTCP.StopServer();
                                loopCliente = false;
                                loopServer = false;
                            }

                            break;

                        case "GetFileList":



                            break;

                        case "GetSelectedFile":



                            break;

                        default:

                            serverTCP.SendFeedback("O servidor não reconheceu o pedido.");

                            break;
                    }
                }
            }
        }
    }
}
