using System.Net;
using System.Net.Sockets;

namespace BasicPaxosNet
{
    class NetworkDriver
    {
        private const int serviceNumber = 8888;

        private ICollection<IPAddress> peers;
        private readonly UdpClient self;

        public NetworkDriver(IPAddress localAddress)
        {
            self = new UdpClient(new IPEndPoint(localAddress, ServiceNumber));
        }

        public static int ServiceNumber => serviceNumber;

        void SendTo(Message message, IPAddress peer)
        {
            self.Send(message.Serialize(), new IPEndPoint(peer, ServiceNumber));
        }
    }
}
