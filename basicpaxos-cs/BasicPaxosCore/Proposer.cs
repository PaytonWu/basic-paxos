using System.Net;
using BasicPaxosNet;
class Proposer
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

    public IPAddress[] PeerAddresses { get; private set; }

    private NetworkDriver NetworkDriver { get; set; }

    void Propose(Int32 value)
    {
        Round++;
        ProposalId = Round << 16 | Id;

        var messageProposal = new MessageProposal(ProposalId, Id, value);
        var message = new Message(MessageType.Propose, messageProposal.Serialize());
        foreach (var peerAddress in PeerAddresses)
        {
            NetworkDriver.SendTo(message, peerAddress);
            var reply = NetworkDriver.Receive();


        }
    }
}
