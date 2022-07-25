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
            case MessageType.Prepare:
            {
                var prepare = MessagePrepare.Deserialize(bytes);
                OnPrepare(prepare);
                break;
            }
            case MessageType.Propose:
            {
                var propose = MessagePropose.Deserialize(bytes);
                OnPropose(propose);
                break;
            }
        }
    }

    private void OnPrepare(MessagePrepare prepare)
    {
        if (AcceptedProposalId.HasValue)
        {
            if (prepare.ProposalId <= AcceptedProposalId)
            {
                var messagePromise = AcceptedValue.HasValue
                    ? new MessagePromise(true, AcceptedProposalId.Value, AcceptedValue.Value)
                    : new MessagePromise(true, AcceptedProposalId.Value);
                // NetworkDriver.SendTo();
            }
        }
        else
        {
            MinProposalId = prepare.ProposalId;
        }
    }

    private void OnPropose(MessagePropose propose)
    {

    }
}