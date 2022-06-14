using System.Net;
using System.Net.Sockets;

namespace BasicPaxosNet
{
    class NetworkDriver
    {
        private const Int32 serviceNumber = 8888;

        private ICollection<IPAddress> peers;
        private readonly UdpClient self;

        public NetworkDriver(IPAddress localAddress)
        {
            self = new UdpClient(new IPEndPoint(localAddress, ServiceNumber));
            peers = new List<IPAddress>();
        }

        public static Int32 ServiceNumber => serviceNumber;

        public void SendTo(Message message, IPAddress peer)
        {
            self.Send(message.Serialize(), new IPEndPoint(peer, ServiceNumber));
        }

        public Byte[] Receive()
        {
            IPEndPoint? remote = null;
            return self.Receive(ref remote);
        }
    }
}
