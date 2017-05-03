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

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start server.");
            Console.ReadKey();

            ServiceTCPSockets.StartServer(portTCP);

            Console.WriteLine("Press any key to start listening for connections.");
            Console.ReadKey();

            ServiceTCPSockets.StartConnection();

            while(true)
            {
                ServiceTCPSockets.AcceptConnection();

                string requestClient = ServiceTCPSockets.GetClientMessage();

                switch (requestClient)
                {
                    case "Login":

                        string userCliente = ServiceTCPSockets.GetClientMessage();
                        string passHashCliente = ServiceTCPSockets.GetClientMessage();

                        if (userCliente == userTemp && passHashCliente == passTemp)
                        {
                            ServiceTCPSockets.SendFeedback("OK");
                        }

                        else
                        {
                            ServiceTCPSockets.SendFeedback("KO");
                        }
                            
                        break;

                    case "GetFileList":



                        break;

                    case "GetSelectedFile":



                        break;

                    default:

                        ServiceTCPSockets.SendFeedback("O servidor não reconheceu o pedido.");

                        break;
                }


            }
        }
    }
}
