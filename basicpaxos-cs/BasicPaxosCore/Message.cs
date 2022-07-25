using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BasicPaxosCore;

public enum MessageType : Byte
{
    Invalid = 0,
    Prepare,
    Promise,
    Propose,
    Accepted
}

internal class Message
{
    public Message(MessageType type, [DisallowNull] Byte[] payload)
    {
        Type = type;
        Ok = true;
        Payload = payload;
    }

    public Message(MessageType type, Boolean ok)
    {
        Type = type;
        Ok = ok;
        Payload = Array.Empty<Byte>();
    }

    public Message(MessageType type)
    {
        Type = type;
        Payload = Array.Empty<Byte>();
        Ok = false;
    }

    public MessageType Type { get; }
    public Boolean Ok { get; }
    public Byte[] Payload { get; }

    public Byte[] Serialize()
    {
        var bytes = Array.Empty<Byte>();

        bytes.Concat(BitConverter.GetBytes((Byte)Type));
        bytes.Concat(BitConverter.GetBytes(Ok));
        if (Ok)
        {
            bytes.Concat(Payload);
        }

        return bytes.ToArray();
    }

    public static Message Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length < 2)
        {
            throw new ArgumentException("Input bytes too short");
        }

        var type = (MessageType)bytes[0];
        var ok = bytes[1] != 0;
        if (ok)
        {
            Debug.Assert(bytes.Length > 2);

            var payload = bytes.Skip(1).Take(bytes.Length).ToArray();
            return new Message(type, payload);
        }

        return new Message(type);
    }
}

class MessagePrepare
{
    public Int32 ProposalId { get; internal set; }
    public Int32 From { get; internal set; }

    internal MessagePrepare(Int32 proposalId, Int32 from)
    {
        ProposalId = proposalId;
        From = from;
    }

    internal Byte[] Serialize()
    {
        var bytes = Array.Empty<Byte>();
        bytes.Concat(BitConverter.GetBytes(ProposalId));
        bytes.Concat(BitConverter.GetBytes(From));

        return bytes.ToArray();
    }

    internal static MessagePrepare Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length == 8)
        {
            return new MessagePrepare(BitConverter.ToInt32(bytes, 0), BitConverter.ToInt32(bytes, 4));
        }

        Debug.Assert(false);
        throw new ArgumentException("Input bytes length invalid");
    }
}

class MessagePropose
{
    public Int32 ProposalId { get; internal set; }
    public Int32 From { get; private set; }
    public Int32 Value { get; private set; }

    public MessageType Type => MessageType.Propose;

    internal MessagePropose(Int32 proposalId, Int32 from, Int32 value)
    {
        ProposalId = proposalId;
        From = from;
        Value = value;
    }

    internal Byte[] Serialize()
    {
        var bytes = Array.Empty<Byte>();
        bytes.Concat(BitConverter.GetBytes(ProposalId));
        bytes.Concat(BitConverter.GetBytes(From));
        bytes.Concat(BitConverter.GetBytes(Value));

        return bytes.ToArray();
    }

    internal static MessagePropose Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length == 12)
        {
            return new MessagePropose(BitConverter.ToInt32(bytes, 0), BitConverter.ToInt32(bytes, 4),
                BitConverter.ToInt32(bytes, 8));
        }

        Debug.Assert(false);
        throw new ArgumentException("Input bytes length invalid");
    }
}

class MessagePromise
{
    public AcceptedProposal? Proposal { get; private set; }

    public Boolean Ok => Proposal.HasValue;

    public MessagePromise(AcceptedProposal proposal)
    {
        Proposal = proposal;
    }

    public MessagePromise()
    {
    }

    internal Byte[] Serialize()
    {
        ICollection<Byte> bytes = new List<Byte>();
        if (Proposal.HasValue)
        {
            bytes.Concat(BitConverter.GetBytes(Proposal.Value.Id));
            bytes.Concat(BitConverter.GetBytes(Proposal.Value.Value));
        }

        return bytes.ToArray();
    }

    internal static MessagePromise Deserialize([DisallowNull] Byte[] bytes)
    {
        switch (bytes.Length)
        {
            case 0:
                return new MessagePromise();

            case 8:
                return new MessagePromise(new AcceptedProposal(BitConverter.ToInt32(bytes, 0), BitConverter.ToInt32(bytes, 4)));

            default:
                Debug.Assert(false);
                throw new ArgumentException("Input bytes length invalid");
        }
    }
}

class MessageAccepted
{
    public Boolean Ok { get; private set; }

    public MessageAccepted(Boolean ok)
    {
        Ok = ok;
    }

    internal Byte[] Serialize()
    {
        ICollection<Byte> bytes = new List<Byte>();
        bytes.Concat(BitConverter.GetBytes(Ok));

        return bytes.ToArray();
    }

    internal static MessageAccepted Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length != 1)
        {
            throw new ArgumentException("Input bytes incorrect");
        }

        return new MessageAccepted(BitConverter.ToBoolean(bytes, 0));
    }
}
