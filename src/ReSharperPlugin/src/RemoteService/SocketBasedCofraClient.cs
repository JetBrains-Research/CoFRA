using System.Net;
using System.Net.Sockets;

namespace Cofra.ReSharperPlugin.RemoteService
{
    public class SocketBasedCofraClient : CofraClient
    {
        private readonly TcpClient myTcpClient;

        private SocketBasedCofraClient(TcpClient tcpClient)
            : base(tcpClient.GetStream())
        {
            myTcpClient = tcpClient;
        }

        public static SocketBasedCofraClient Connect(IPAddress address, int port)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(address, port);
            //TODO: closing of the socket

            return new SocketBasedCofraClient(tcpClient);
        }
    }
}