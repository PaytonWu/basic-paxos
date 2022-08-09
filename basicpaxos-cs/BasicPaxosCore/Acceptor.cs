using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace BasicPaxosCore;

public struct AcceptedProposal
{
    public AcceptedProposal(Int32 id, Int32 value)
    {
        Id = id;
        Value = value;
    }

    public Int32 Id { get; }
    public Int32 Value { get; }
}

internal class Acceptor
{
    private NetworkDriver NetworkDriver { get; set; }
    public Int32 Id { get; private set; }
    public Int32 MaxProposalId { get; set; }
    public AcceptedProposal? AcceptedProposal { get; set; }

    public Acceptor(Int32 id, IPAddress ipAddress)
    {
        Id = id;
        NetworkDriver = new NetworkDriver(ipAddress);
    }

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
        var from = task.Result.RemoteEndPoint;

        var message = Message.Deserialize(bytes);

        switch (message.Type)
        {
            case MessageType.Prepare:
            {
                Debug.Assert(message.Ok);

                var prepare = MessagePrepare.Deserialize(message.Payload);
                OnPrepare(from.Address, prepare);
                break;
            }
            case MessageType.Propose:
            {
                var propose = MessagePropose.Deserialize(message.Payload);
                OnPropose(from.Address, propose);
                break;
            }
        }
    }

    private void OnPrepare(IPAddress from, MessagePrepare prepare)
    {

        if (prepare.ProposalId > MaxProposalId)
        {
            MaxProposalId = prepare.ProposalId;

            var messagePromise = AcceptedProposal.HasValue
                ? new MessagePromise(prepare.ProposalId, AcceptedProposal.Value)
                : new MessagePromise(prepare.ProposalId);
            var message = new Message(MessageType.Promise, messagePromise.Serialize());
            NetworkDriver.SendTo(message, from);
        }
        else
        {
            var message = new Message(MessageType.Promise);
            NetworkDriver.SendTo(message, from);
        }
        //}
        //else
        //{
        //    MaxProposalId = prepare.ProposalId;

        //    Debug.Assert(!AcceptedProposal.HasValue);

        //    var message = new Message(MessageType.Promise);
        //    NetworkDriver.SendTo(message, from);
        //}
    }

    private void OnPropose(IPAddress from, MessagePropose propose)
    {
        if (propose.ProposalId >= MaxProposalId)
        {
            MaxProposalId = propose.ProposalId;
            AcceptedProposal = new AcceptedProposal(propose.ProposalId, propose.Value);

            var messageAccepted = new MessageAccepted(true);
            var message = new Message(MessageType.Accepted, messageAccepted.Serialize());
            NetworkDriver.SendTo(message, from);
        }
        else
        {
            var message = new Message(MessageType.Accepted);
            NetworkDriver.SendTo(message, from);
        }
    }
}