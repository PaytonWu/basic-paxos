﻿using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BasicPaxosCore;

internal class NetworkDriver
{
    private const Int32 ServiceNumber = 8888;

    private ICollection<IPAddress> _peers;
    private readonly UdpClient _self;

    public NetworkDriver(IPAddress localAddress)
    {
        _self = new UdpClient(new IPEndPoint(localAddress, ServiceNumber));
        _peers = new List<IPAddress>();
    }

    public void SendTo(Message message, IPAddress peer)
    {
        _self.Send(message.Serialize(), new IPEndPoint(peer, ServiceNumber));
    }

    public async ValueTask<Int32> SendToAsync(Message message, IPAddress peer)
    {
        return await _self.SendAsync(message.Serialize(), new IPEndPoint(peer, ServiceNumber));
    }

    public Byte[] Receive()
    {
        IPEndPoint? remote = null;
        return _self.Receive(ref remote);
    }

    public Task<UdpReceiveResult> ReceiveAsync()
    {
        return _self.ReceiveAsync();
    }
}