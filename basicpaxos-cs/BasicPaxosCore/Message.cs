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

public class Message
{
    public Message(MessageType type, Byte[] payload)
    {
        // Debug.Assert(payload != null);
        Type = type;
        Payload = payload;
    }

    public Message(MessageType type)
    {
        Type = type;
    }

    public MessageType Type { get; }
    [AllowNull]
    public Byte[] Payload { get; }
    public Boolean Ok => Payload != null;

    public Byte[] Serialize()
    {
        IEnumerable<Byte> bytes = Array.Empty<Byte>();
        Debug.Assert(!bytes.Any());

        bytes = bytes.Concat(BitConverter.GetBytes((Byte)Type));
        bytes = bytes.Concat(BitConverter.GetBytes(Ok));
        if (Ok)
        {
            bytes = bytes.Concat(Payload);
        }

        return bytes.ToArray();
    }

    public static Message Deserialize(Byte[] bytes)
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

            var payload = bytes.Skip(2).Take(bytes.Length).ToArray();
            return new Message(type, payload);
        }

        return new Message(type);
    }
}

public class MessagePrepare
{
    public Int32 ProposalId { get; internal set; }
    public Int32 From { get; internal set; }

    public MessagePrepare(Int32 proposalId, Int32 from)
    {
        ProposalId = proposalId;
        From = from;
    }

    public Byte[] Serialize()
    {
        IEnumerable<Byte> bytes = Array.Empty<Byte>();
        bytes = bytes.Concat(BitConverter.GetBytes(ProposalId));
        bytes = bytes.Concat(BitConverter.GetBytes(From));

        return bytes.ToArray();
    }

    public static MessagePrepare Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length == 8)
        {
            return new MessagePrepare(BitConverter.ToInt32(bytes, 0), BitConverter.ToInt32(bytes, 4));
        }

        Debug.Assert(false);
        throw new ArgumentException("Input bytes length invalid");
    }
}

public class MessagePropose
{
    public Int32 ProposalId { get; internal set; }
    public Int32 From { get; private set; }
    public Int32 Value { get; private set; }

    public MessageType Type => MessageType.Propose;

    public MessagePropose(Int32 proposalId, Int32 from, Int32 value)
    {
        ProposalId = proposalId;
        From = from;
        Value = value;
    }

    public Byte[] Serialize()
    {
        IEnumerable<Byte> bytes = Array.Empty<Byte>();
        bytes = bytes.Concat(BitConverter.GetBytes(ProposalId));
        bytes = bytes.Concat(BitConverter.GetBytes(From));
        bytes = bytes.Concat(BitConverter.GetBytes(Value));

        return bytes.ToArray();
    }

    public static MessagePropose Deserialize([DisallowNull] Byte[] bytes)
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

public class MessagePromise
{
    public AcceptedProposal? Proposal { get; private set; }

    public Int32 PrepareId { get; private set; }

    public MessagePromise(Int32 prepareId, AcceptedProposal proposal)
    {
        PrepareId = prepareId;
        Proposal = proposal;
    }

    public MessagePromise(Int32 prepareId)
    {
        this.PrepareId = prepareId;
    }

    public Byte[] Serialize()
    {
        IEnumerable<Byte> bytes = Array.Empty<Byte>();
        bytes = bytes.Concat(BitConverter.GetBytes(PrepareId));
        if (Proposal.HasValue)
        {
            bytes = bytes.Concat(BitConverter.GetBytes(Proposal.Value.Id));
            bytes = bytes.Concat(BitConverter.GetBytes(Proposal.Value.Value));
        }

        return bytes.ToArray();
    }

    public static MessagePromise Deserialize(Byte[] bytes)
    {
        switch (bytes.Length)
        {
            case 4:
                return new MessagePromise(BitConverter.ToInt32(bytes, 0));

            case 12:
                return new MessagePromise(BitConverter.ToInt32(bytes, 0),
                    new AcceptedProposal(BitConverter.ToInt32(bytes, 4), BitConverter.ToInt32(bytes, 8)));

            default:
                Debug.Assert(false);
                throw new ArgumentException("Input bytes length invalid");
        }
    }
}

public class MessageAccepted
{
    public Byte[] Serialize()
    {
        IEnumerable<Byte> bytes = Array.Empty<Byte>();
        return bytes.ToArray();
    }

    public static MessageAccepted Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length != 0)
        {
            throw new ArgumentException("Input bytes incorrect");
        }

        return new MessageAccepted();
    }
}
