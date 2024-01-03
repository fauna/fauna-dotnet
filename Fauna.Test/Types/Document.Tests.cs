using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Types;

[TestFixture]
public class DocumentTests
{

    [Test]
    public void Ctor_DocumentNoData()
    {
        const string id = "id";
        var coll = new Module("Foo");
        var ts = DateTime.Parse("2024-01-01");
        var doc = new Document(id, coll, ts);
        Assert.AreEqual(id, doc.Id);
        Assert.AreEqual(coll, doc.Collection);
        Assert.AreEqual(ts, doc.Ts);
        Assert.IsEmpty(doc.Keys);
    }

    [Test]
    public void Ctor_DocumentWithData()
    {
        const string id = "id";
        var coll = new Module("Foo");
        var ts = DateTime.Parse("2024-01-01");
        var d = new Dictionary<string, object?>()
        {
            {"foo", "bar"}
        };
        var doc = new Document(id, coll, ts, d);

        Assert.AreEqual(id, doc.Id);
        Assert.AreEqual(coll, doc.Collection);
        Assert.AreEqual(ts, doc.Ts);
        Assert.AreEqual("bar", doc["foo"]);
    }

    [Test]
    public void DocumentClonesDictionary()
    {
        var d = new Dictionary<string, object?>()
        {
            {"foo", "bar"}
        };
        var doc = new Document("id", new Module("Foo"), DateTime.Parse("2024-01-01"), d);

        d["foo"] = "baz";

        Assert.AreEqual("bar", doc["foo"]);
    }
}
