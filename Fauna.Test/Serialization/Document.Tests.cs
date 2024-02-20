using Fauna.Types;
using NUnit.Framework;
using static Fauna.Test.Helpers.TestClientHelper;
using static Fauna.Query;

namespace Fauna.Test.Serialization;

public class DocumentTests
{

    private static readonly Client Client = NewTestClient();

    [OneTimeSetUp]
    public void SetUp()
    {
        Fixtures.SingleDocumentDb(Client);
    }

    [Test]
    public async Task DeserializeDocumentDynamicTest()
    {
        var q = FQL($"SingleDocument.all().first()");
        var r = await Client.QueryAsync(q);
        var doc = (Document)r.Data!;
        Assert.AreEqual(doc["isValid"], true);
    }

    [Test]
    public async Task DeserializeDocumentCheckedTest()
    {
        var q = FQL($"SingleDocument.all().first()");
        var r = await Client.QueryAsync<Document>(q);
        Assert.AreEqual(r.Data["isValid"], true);
    }

    [Test]
    public async Task DeserializeNullDocumentDynamicTest()
    {
        var q = FQL($"SingleDocument.byId('123')");
        var r = await Client.QueryAsync(q);
        var doc = (Document)r.Data!;
        Assert.AreEqual("not found", doc.Cause);
    }

    [Test]
    public async Task DeserializeNullDocumentCheckedTest()
    {
        var q = FQL($"SingleDocument.byId('123')");
        var r = await Client.QueryAsync<Document>(q);
        var doc = r.Data;
        Assert.AreEqual("not found", doc.Cause);
    }

    [Test]
    public async Task DeserializeDocumentAsClassTest()
    {
        var q = FQL($"SingleDocument.all().first()");
        var r = await Client.QueryAsync<SingleDocument>(q);
        var doc = r.Data;
        Assert.IsTrue(doc.IsValid);
    }

    [Test]
    public async Task DeserializeNullDocumentAsClassTest()
    {
        var q = FQL($"SingleDocument.byId('123')");
        var r = await Client.QueryAsync<SingleDocument>(q);
        var doc = r.Data;
        Assert.AreEqual("not found", doc.Cause);
        Assert.AreEqual("123", doc.Id);
    }

}