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

        private static ServiceTCPSockets tcpServer = new ServiceTCPSockets(portTCP);

        static void Main(string[] args)
        {
            if(tcpServer.StartConnection())
            {
                if(tcpServer.AcceptConnection())
                {
                    tcpServer.GetClientMessage();
                }
            }
        }
    }
}
