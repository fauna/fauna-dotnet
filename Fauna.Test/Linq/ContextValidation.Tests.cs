using NUnit.Framework;
using Fauna.Mapping.Attributes;
using System.Diagnostics.CodeAnalysis;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.Linq;

[TestFixture]
public class ContextValidationTests
{
    [Object]
    class Foo
    {
        [Field] public string? Id { get; set; }
    }

    [Object]
    class Bar
    {
        [Field] public string? Id { get; set; }
    }

    [AllowNull]
    private static Client _client;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = NewTestClient();
    }

    class InvalidGenericDb : DataContext
    {
        public class FooCol<D> : Collection<D> { }
    }

    [Test]
    public void DisallowsGenericCollections()
    {
        try
        {
            _client.DataContext<InvalidGenericDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection type: Cannot be generic.");
        }
    }

    class FooDb : DataContext
    {
        public class FooCol : Collection<Foo> { }
    }

    class InvalidCrossedDb : DataContext
    {
        public FooDb.FooCol Foo { get => GetCollection<FooDb.FooCol>(); }
    }

    [Test]
    public void MemberMustReturnOwnCollection()
    {
        try
        {
            var db = _client.DataContext<InvalidCrossedDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection property: Must return a nested collection type.");
        }
    }
}
