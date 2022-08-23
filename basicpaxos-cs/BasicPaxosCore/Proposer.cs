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

    async Task<Int32?> Propose(Int32 value)
    {
        Round++;
        ProposalId = Round << 16 | Id;

        var messagePrepare = new MessagePrepare(ProposalId, Id);
        var message = new Message(MessageType.Prepare, messagePrepare.Serialize());
        Int32 promisedCount = 0;
        foreach (var peerAddress in PeerAddresses)
        {
            await NetworkDriver.SendToAsync(message, peerAddress).AsTask().ContinueWith(_ => NetworkDriver.ReceiveAsync(), TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap().ContinueWith(
                replyBytes =>
                {
                    var buffer = replyBytes.Result.Buffer;
                    try
                    {
                        var reply = MessagePromise.Deserialize(buffer);
                        Debug.Assert(reply != null);

                        {
                            promisedCount++;
                            Debug.Assert(reply.Proposal != null);

                            if (reply.Proposal.Value.Id > ProposalId)
                            {
                                ProposalId = reply.Proposal.Value.Id;
                                Value = reply.Proposal.Value.Value;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        // continue;
                    }

                    if (promisedCount >= Majority())
                    {
                        // break;
                    }
                });
        }

        Int32 acceptedCount = 0;
        if (promisedCount >= Majority())
        {
            Debug.Assert(Value != null, nameof(Value) + " != null");
            var messagePropose = new MessagePropose(ProposalId, Id, Value.Value);
            message = new Message(MessageType.Propose, messagePropose.Serialize());

            foreach (var peerAddress in PeerAddresses)
            {
                NetworkDriver.SendTo(message, peerAddress);
                var replyBytes = NetworkDriver.Receive();

                try
                {
                    var reply = MessageAccepted.Deserialize(replyBytes);
                    Debug.Assert(reply != null);

                    if (reply.Ok)
                    {
                        acceptedCount++;
                    }
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

            if (acceptedCount >= Majority())
            {
                return Value;
            }
        }

        return new Int32?();
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