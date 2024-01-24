using NUnit.Framework;
using Fauna.Mapping.Attributes;
using System.Diagnostics.CodeAnalysis;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.Linq;

[TestFixture]
public class ContextValidationTests
{
    [Collection]
    class Foo
    {
        [Field] public string? Id { get; set; }
    }

    [Collection]
    class Bar
    {
        [Field] public string? Id { get; set; }
    }

    class FooDb : DatabaseContext
    {
        public interface FooCol : Collection<Foo>
        {
            public Index<Foo> ByName(string name);
        }

        public FooCol Foo { get => GetCollection<FooCol>(); }
    }


    [AllowNull]
    private static Client _client;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = NewTestClient();
    }

    class InvalidPrivateDb : DatabaseContext
    {
        private interface FooCol : Collection<Foo> { }
    }

    [Test]
    public void DisallowsPrivateCollections()
    {
        try
        {
            _client.DatabaseContext<InvalidPrivateDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection type: Must be public.");
        }
    }

    class InvalidGenericDb : DatabaseContext
    {
        public interface FooCol<D> : Collection<D> { }
    }

    [Test]
    public void DisallowsGenericCollections()
    {
        try
        {
            _client.DatabaseContext<InvalidGenericDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection type: Cannot be generic.");
        }
    }

    class InvalidDoubleDb : DatabaseContext
    {
        public interface FooCol : Collection<Foo>, Collection<Bar> { }
    }

    [Test]
    public void DisallowsMultipleCollInheritance()
    {
        try
        {
            _client.DatabaseContext<InvalidDoubleDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection type: Cannot implement Collection<> multiple times.");
        }
    }

    class InvalidCollInheritanceDb : DatabaseContext
    {
        public interface FooCol : Collection { }
    }

    [Test]
    public void MustInheritGenericColl()
    {
        try
        {
            _client.DatabaseContext<InvalidCollInheritanceDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection type: Must implement Collection<>.");
        }
    }

    class InvalidCrossedDb : DatabaseContext
    {
        public FooDb.FooCol Foo { get => GetCollection<FooDb.FooCol>(); }
    }

    [Test]
    public void MemberMustReturnOwnCollection()
    {
        try
        {
            var db = _client.DatabaseContext<InvalidCrossedDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection property: Must return a nested collection type.");
        }
    }

    class InvalidNullableDb : DatabaseContext
    {
        public interface FooCol : Collection<Foo> { }

        public FooCol? Foo { get; }
    }

    [Test]
    public void MemberCannotBeNullable()
    {
        try
        {
            var db = _client.DatabaseContext<InvalidNullableDb>();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual(ex.Message, "Invalid collection property: Cannot be nullable.");
        }
    }
}
