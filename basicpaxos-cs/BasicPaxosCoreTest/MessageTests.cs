using BasicPaxosCore;

namespace BasicPaxosCoreTest;

[TestClass]
public class MessageTests
{
    [TestMethod]
    public void MessagePrepareSerializationTest()
    {
        var origin = new MessagePrepare(3, 4);
        var bytes = origin.Serialize();
        Assert.IsNotNull(bytes);
        var deserialized = MessagePrepare.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.ProposalId, deserialized.ProposalId);
        Assert.AreEqual(origin.From, deserialized.From);
    }

    [TestMethod]
    public void MessageProposeSerializationTest()
    {
        var origin = new MessagePropose(1, 2, 3);
        var bytes = origin.Serialize();
        Assert.IsNotNull(bytes);
        var deserialized = MessagePropose.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.From, deserialized.From);
        Assert.AreEqual(origin.ProposalId, deserialized.ProposalId);
        Assert.AreEqual(origin.Type, deserialized.Type);
        Assert.AreEqual(origin.Value, deserialized.Value);
    }

    [TestMethod]
    public void MessagePromiseSerializationTest()
    {
        var origin = new MessagePromise(1);
        var bytes = origin.Serialize();
        Assert.IsNotNull(bytes);
        var deserialized = MessagePromise.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.PrepareId, deserialized.PrepareId);

        origin = new MessagePromise(1, new AcceptedProposal(2, 3));
        bytes = origin.Serialize();
        Assert.IsNotNull(bytes);
        deserialized = MessagePromise.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.PrepareId, deserialized.PrepareId);
        Assert.IsTrue(origin.Proposal.HasValue);
        Assert.IsTrue(deserialized.Proposal.HasValue);
        Assert.AreEqual(origin.Proposal.Value.Value, deserialized.Proposal.Value.Value);
        Assert.AreEqual(origin.Proposal.Value.Id, deserialized.Proposal.Value.Id);
    }

    [TestMethod]
    public void MessageSerializationTest()
    {
        var origin = new Message(MessageType.Prepare);
        var bytes = origin.Serialize();
        Assert.IsNotNull(bytes);
        var deserialized = Message.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.Type, deserialized.Type);
        Assert.AreEqual(origin.Ok, deserialized.Ok);
        Assert.IsNull(origin.Payload);
        Assert.IsNull(deserialized.Payload);

        origin = new Message(MessageType.Prepare);
        bytes = origin.Serialize();
        Assert.IsNotNull(bytes);
        deserialized = Message.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.Type, deserialized.Type);
        Assert.AreEqual(origin.Ok, deserialized.Ok);
        Assert.IsNull(origin.Payload);
        Assert.IsNull(deserialized.Payload);

        origin = new Message(MessageType.Accepted, null!);
        bytes = origin.Serialize();
        deserialized = Message.Deserialize(bytes);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(origin.Type, deserialized.Type);
        Assert.AreEqual(origin.Ok, deserialized.Ok);
        Assert.IsNull(origin.Payload);
        Assert.IsNull(deserialized.Payload);
    }
}
