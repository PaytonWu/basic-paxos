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
}