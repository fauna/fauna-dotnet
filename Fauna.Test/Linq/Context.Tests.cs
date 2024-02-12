using NUnit.Framework;
using Fauna.Mapping.Attributes;
using System.Diagnostics.CodeAnalysis;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.Linq;

[TestFixture]
public class ContextTests
{
    [Object]
    class Author
    {
        [Field] public string? Id { get; set; }
        [Field] public string Name { get; set; } = "Alice";
    }

    [Object]
    class Post
    {
        [Field] public string? Id { get; set; }
    }

    class AuthorDb : DataContext
    {
        public class AuthorCol : Collection<Author>
        {
            public Index<Author> ByName(string name) => Index().Call(name);
            public Index<Author> ByName2(string name) => Index("realByName").Call(name);
        }

        [Name("posts")]
        public class PostCol : Collection<Post> { }

        public AuthorCol Author { get => GetCollection<AuthorCol>(); }
        public PostCol Post { get => GetCollection<PostCol>(); }
    }

    [AllowNull]
    private static Client client;

    [OneTimeSetUp]
    public void SetUp()
    {
        client = NewTestClient();
    }

    [Test]
    public void ReturnsADataContext()
    {
        var db = client.DataContext<AuthorDb>();

        Assert.AreEqual(db.Author.Name, "Author");
        Assert.AreEqual(db.Author.DocType, typeof(Author));
        Assert.AreEqual(db.Post.Name, "posts");
        Assert.AreEqual(db.Post.DocType, typeof(Post));

        var byName = db.Author.ByName("Alice");
        var byName2 = db.Author.ByName2("Alice");

        Assert.AreEqual(byName.Name, "byName");
        Assert.AreEqual(byName.DocType, typeof(Author));
        Assert.AreEqual(byName.Args, new object[] { "Alice" });
        Assert.AreEqual(byName2.Name, "realByName");
        Assert.AreEqual(byName2.DocType, typeof(Author));
        Assert.AreEqual(byName2.Args, new object[] { "Alice" });
    }
}
