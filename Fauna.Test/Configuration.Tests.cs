using Fauna.Core;
using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class ConfigurationTests
{
    [Test]
    public void ConstructorWorksFine()
    {
        var b = new Configuration("secret");

        Assert.AreEqual("secret", b.Secret);
        Assert.AreEqual(Endpoints.Default, b.Endpoint);
        Assert.IsTrue(b.DisposeHttpClient);
    }

    [Test]
    public void ConstructorUsesEnvVar()
    {
        Environment.SetEnvironmentVariable("FAUNA_SECRET", "secret");
        var b = new Configuration();

        Assert.AreEqual("secret", b.Secret);
        Assert.AreEqual(Endpoints.Default, b.Endpoint);
        Assert.IsTrue(b.DisposeHttpClient);
    }

    [Test]
    public void ConstructorThrowsWithNullSecret()
    {
        string? currentVal = Environment.GetEnvironmentVariable("FAUNA_SECRET");
        Assert.Throws<ArgumentNullException>(() =>
        {
            Environment.SetEnvironmentVariable("FAUNA_SECRET", null);
            var b = new Configuration();

        });
        Environment.SetEnvironmentVariable("FAUNA_SECRET", currentVal);
    }

    [Test]
    public void ConstructorWithHttpClient()
    {
        var b = new Configuration("secret", new HttpClient());

        Assert.AreEqual("secret", b.Secret);
        Assert.AreEqual(Endpoints.Default, b.Endpoint);
        Assert.IsFalse(b.DisposeHttpClient);
    }

    [Test]
    public void FinalQueryOptions()
    {
        var defaults = new QueryOptions
        {
            TypeCheck = true,
            QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } },
            QueryTimeout = TimeSpan.FromSeconds(30)
        };
        var overrides = new QueryOptions
        {
            Linearized = true,
            QueryTags = new Dictionary<string, string> { { "foo", "yep" } }
        };

        var finalOptions = QueryOptions.GetFinalQueryOptions(defaults, overrides);

        Assert.IsNotNull(finalOptions);
        Assert.IsTrue(finalOptions!.Linearized);
        Assert.IsTrue(finalOptions!.TypeCheck);
        Assert.IsNull(finalOptions!.TraceParent);
        Assert.IsNotNull(finalOptions!.QueryTags);
        Assert.AreEqual("yep", finalOptions!.QueryTags!["foo"]);
        Assert.AreEqual("luhrmann", finalOptions!.QueryTags!["baz"]);
    }
}
