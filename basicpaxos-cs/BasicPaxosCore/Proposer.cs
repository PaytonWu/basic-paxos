using System.Diagnostics;
using System.Net;

namespace BasicPaxosCore;

internal class Proposer
{
    public Proposer(IPAddress ipAddress, Int32 id, IPAddress[] peerAddresses)
    {
        Id = id;
        PeerAddresses = peerAddresses;
        NetworkDriver = new NetworkDriver(ipAddress);
    }

    public Int32 Id { get; private set; }
    public Int32 Round { get; set; }
    public Int32 ProposalId { get; set; }

    public Int32? Value { get; private set; }

    public IPAddress[] PeerAddresses { get; private set; }

    private NetworkDriver NetworkDriver { get; set; }

    private Int32 Majority()
    {
        return PeerAddresses.Length / 2 + 1;
    }

    Int32 Propose(Int32 value)
    {
        Round++;
        ProposalId = Round << 16 | Id;

        var messagePrepare = new MessagePrepare(ProposalId, Id);
        var message = new Message(MessageType.Prepare, messagePrepare.Serialize());
        var promisedCount = 0;
        foreach (var peerAddress in PeerAddresses)
        {
            try
            {
                NetworkDriver.SendTo(message, peerAddress);
                var bytes = NetworkDriver.Receive();
                var replyMessage = Message.Deserialize(bytes);
                Debug.Assert(replyMessage != null);
                if (replyMessage.Type != MessageType.Promise || !replyMessage.Ok)
                {
                    continue;
                }

                var reply = MessagePromise.Deserialize(replyMessage.Payload);

                {
                    promisedCount++;
                    Debug.Assert(reply.Proposal != null);
                    var repliedProposal = reply.Proposal.Value;

                    if (repliedProposal.Id > ProposalId)
                    {
                        ProposalId = repliedProposal.Id;
                        Value = repliedProposal.Value;
                    }
                }
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (promisedCount >= Majority())
            {
                break;
            }
        }

        var acceptedCount = 0;
        if (promisedCount < Majority())
        {
            return 0;
        }

        Debug.Assert(Value != null);
        var proposedValue = Value.Value;

        var messagePropose = new MessagePropose(ProposalId, Id, proposedValue);
        message = new Message(MessageType.Propose, messagePropose.Serialize());

        foreach (var peerAddress in PeerAddresses)
        {
            NetworkDriver.SendTo(message, peerAddress);
            var bytes = NetworkDriver.Receive();

            try
            {
                var replyMessage = Message.Deserialize(bytes);
                if (replyMessage.Type != MessageType.Accepted || !replyMessage.Ok)
                {
                    continue;
                }

                acceptedCount++;
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (acceptedCount >= Majority())
            {
                break;
            }
        }

        return acceptedCount >= Majority() ? proposedValue : 0;
    }

    private void StartProposer()
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
            case MessageType.Promise:
            {
                var prepare = MessagePromise.Deserialize(bytes);
                break;
            }
            case MessageType.Accepted:
            {
                var proposal = MessageAccepted.Deserialize(bytes);
                break;
            }
        }
    }
}