using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BasicPaxosCore;

public enum MessageType : Byte
{
    Invalid = 0,
    Prepare,
    Propose,
    Promise,
    Accepted
}

// [Serializable]
internal class Message
{
    public Message(MessageType type, [DisallowNull] Byte[] payload)
    {
        Type = type;
        Payload = payload;
    }

    public MessageType Type { get; }
    public Byte[] Payload { get; }

    public Byte[] Serialize()
    {
        var bytes = Array.Empty<Byte>();

        bytes.Concat(BitConverter.GetBytes((Byte)Type));
        bytes.Concat(Payload);

        return bytes.ToArray();
    }

    public static Message Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length < 2)
        {
            throw new ArgumentException("Input bytes too short");
        }

        var type = (MessageType)bytes[0];
        var payload = bytes.Skip(0).Take(bytes.Length).ToArray();

        return new Message(type, payload);
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
    public Boolean Ok { get; private set; }
    public Int32 ProposalId { get; private set; }
    public Int32? Value { get; private set; }

    public MessagePromise(Boolean ok, Int32 proposalId, Int32 value)
    {
        Ok = ok;
        ProposalId = proposalId;
        Value = value;
    }

    public MessagePromise(Boolean ok, Int32 proposalId)
    {
        Ok = ok;
        ProposalId = proposalId;
    }

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

    internal static MessagePromise Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length < 5)
        {
            throw new ArgumentException("Input bytes too short");
        }

        switch (bytes.Length)
        {
            case 5:
                return new MessagePromise(BitConverter.ToBoolean(bytes, 0), BitConverter.ToInt32(bytes, 1));

            case 9:
                return new MessagePromise(BitConverter.ToBoolean(bytes, 0), BitConverter.ToInt32(bytes, 1), BitConverter.ToInt32(bytes, 5));

            default:
                Debug.Assert(false);
                throw new ArgumentException("Input bytes length invalid");
        }
    }
}

class MessageAccepted
{
    public Boolean Ok { get; private set; }
    public Int32 ProposalId { get; private set; }
    public Int32? Value { get; private set; }

    public MessageAccepted(Boolean ok, Int32 proposalId, Int32 value)
    {
        Ok = ok;
        ProposalId = proposalId;
        Value = value;
    }

    public MessageAccepted(Boolean ok, Int32 proposalId)
    {
        Ok = ok;
        ProposalId = proposalId;
    }

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

    internal static MessageAccepted Deserialize([DisallowNull] Byte[] bytes)
    {
        if (bytes.Length < 5)
        {
            throw new ArgumentException("Input bytes too short");
        }

        switch (bytes.Length)
        {
            case 5:
                return new MessageAccepted(BitConverter.ToBoolean(bytes, 0), BitConverter.ToInt32(bytes, 1));

            case 9:
                return new MessageAccepted(BitConverter.ToBoolean(bytes, 0), BitConverter.ToInt32(bytes, 1), BitConverter.ToInt32(bytes, 5));

            default:
                Debug.Assert(false);
                throw new ArgumentException("Input bytes length invalid");
        }
    }
}
