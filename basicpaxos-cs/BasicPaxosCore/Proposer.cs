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

    void Prepare(Int32 value)
    {
        Round++;
        ProposalId = Round << 16 | Id;

        var messageProposal = new MessageProposal(ProposalId, Id, value);
        var message = new Message(MessageType.Propose, messageProposal.Serialize());
        Int32 prepareCount = 0;
        foreach (var peerAddress in PeerAddresses)
        {
            NetworkDriver.SendTo(message, peerAddress);
            var replyBytes = NetworkDriver.Receive();

            try
            {
                var reply = MessageVote.Deserialize(replyBytes);
                Debug.Assert(reply != null);

                if (reply.Ok)
                {
                    prepareCount++;
                    if (reply.ProposalId > ProposalId)
                    {
                        ProposalId = reply.ProposalId;
                        Value = reply.Value;
                    }
                }
            }
            catch (ArgumentException)
            {
                continue;
            }

            if (prepareCount >= Majority())
            {
                break;
            }
        }
    }
}