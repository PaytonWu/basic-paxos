using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace BasicPaxosCore;

internal class Acceptor
{
    public Acceptor(Int32 id, IPAddress ipAddress)
    {
        Id = id;
        NetworkDriver = new NetworkDriver(ipAddress);
    }

    private NetworkDriver NetworkDriver { get; set; }
    public Int32 Id { get; private set; }
    public Int32? MinProposalId { get; set; }
    public Int32? AcceptedProposalId { get; set; }
    public Int32? AcceptedValue { get; set; }

    private void StartReceiver()
    {
        var recverThread = new Thread(OnReceive)
        {
            IsBackground = true
        };
        recverThread.Start();
    }

    private void OnReceive()
    {
        var task = NetworkDriver.ReceiveAsync();
        task.Wait();
        var bytes = task.Result.Buffer;
        var message = Message.Deserialize(bytes);

        switch (message.Type)
        {
            case MessageType.Proposal:
            {
                var proposal = MessageProposal.Deserialize(bytes);
                break;
            }

        }
    }

    private void OnPrepare(MessageProposal prepare)
    {

    }
}