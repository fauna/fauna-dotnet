using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Types;

[TestFixture]
public class NamedDocumentTests
{

    [Test]
    public void Ctor_NamedDocumentNoData()
    {
        const string name = "name";
        var coll = new Module("Foo");
        var ts = DateTime.Parse("2024-01-01");
        var doc = new NamedDocument(name, coll, ts);
        Assert.AreEqual(name, doc.Name);
        Assert.AreEqual(coll, doc.Collection);
        Assert.AreEqual(ts, doc.Ts);
        Assert.IsEmpty(doc.Keys);
    }

    [Test]
    public void Ctor_NamedDocumentWithData()
    {
        const string name = "name";
        var coll = new Module("Foo");
        var ts = DateTime.Parse("2024-01-01");
        var d = new Dictionary<string, object?>()
        {
            {"foo", "bar"}
        };
        var doc = new NamedDocument(name, coll, ts, d);

        Assert.AreEqual(name, doc.Name);
        Assert.AreEqual(coll, doc.Collection);
        Assert.AreEqual(ts, doc.Ts);
        Assert.AreEqual("bar", doc["foo"]);
    }


    [Test]
    public void NamedDocumentClonesDictionary()
    {
        var d = new Dictionary<string, object?>()
        {
            {"foo", "bar"}
        };
        var doc = new NamedDocument("name", new Module("Foo"), DateTime.Parse("2024-01-01"), d);

        d["foo"] = "baz";

        Assert.AreEqual("bar", doc["foo"]);
    }
}
