using System.Runtime.Serialization;
using System.Collections.Generic;

public enum MessageType : Byte
{
    Invalid = 0,
    Propose,
    Proposal,
    Vote
}

[Serializable]
struct Message
{
    public MessageType Type { get; set; }
    public Byte[] Payload { get; set; }

    public Byte[] Serialize()
    {
        ICollection<Byte> bytes = new List<Byte>();
        bytes.Concat(BitConverter.GetBytes((Byte)Type));
        bytes.Concat(Payload);

        return bytes.ToArray();
    }

    public void Deserialize(Byte[] bytes)
    {

    }
}

class MessageProposal
{
    private Int32 proposalId;
    private Int32 from;
    private Nullable<Int32> value;

    public Int32 ProposalId { get => proposalId; set => proposalId = value; }
    public Int32 From { get => from; set => from = value; }
    public Int32? Value { get => value; set => this.value = value; }

    internal Byte[] Serialize()
    {
        ICollection<Byte> bytes = new List<Byte>();
        bytes.Concat(BitConverter.GetBytes(ProposalId));
        bytes.Concat(BitConverter.GetBytes(From));
        if (Value.HasValue)
        {
            bytes.Concat(BitConverter.GetBytes(Value.Value));
        }

        return bytes.ToArray();
    }
}

class MessageVote
{
    private Boolean ok;
    private Int32 proposalId;
    private Nullable<Int32> value;

    public Boolean Ok { get => ok; set => ok = value; }
    public Int32 ProposalId { get => proposalId; set => proposalId = value; }
    public Int32? Value { get => value; set => this.value = value; }

    internal Byte[] Serialize()
    {
        ICollection<Byte> bytes = new List<Byte>();
        bytes.Concat(BitConverter.GetBytes(Ok));
        bytes.Concat(BitConverter.GetBytes(ProposalId));
        if (Value.HasValue)
        {
            bytes.Concat(BitConverter.GetBytes(Value.Value));
        }

        return bytes.ToArray();
    }
}
