using System.Net;
using Fauna.Exceptions;
using NUnit.Framework;
using static Fauna.Query;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.E2E;

public class E2EErrorHandlingTest
{
    private readonly Client _client = GetLocalhostClient();

    [OneTimeSetUp]
    public void SetUp()
    {
        _client.QueryAsync(FQL($"Collection.byName('E2EErrorHandling')?.delete()")).Wait();
        _client.QueryAsync(FQL(
                $"Collection.create({{name: 'E2EErrorHandling', fields: {{'name': {{signature: 'String'}},'quantity': {{signature: 'Int', default: '0'}}}}, constraints: [{{unique: ['name']}},{{check:{{name: 'posQuantity', body: '(doc) => doc.quantity >= 0' }}}}]}})"))
            .Wait();
    }

    [Test]
    public async Task FailsOnUniqueConstraintTest()
    {
        await _client.QueryAsync<object>(FQL($"E2EErrorHandling.create({{name: 'cheese', quantity: 1}})"));

        var e = Assert.ThrowsAsync<ConstraintFailureException>(async () =>
            await _client.QueryAsync<object>(FQL($"E2EErrorHandling.create({{name: 'cheese', quantity: 1}})")));
        Assert.AreEqual("BadRequest (constraint_failure): Failed unique constraint.\n---\nerror: Failed unique constraint.\nconstraint failures:\n  name: Failed unique constraint\nat *query*:1:24\n  |\n1 | E2EErrorHandling.create({name: 'cheese', quantity: 1})\n  |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\n  |", e!.Message);
        Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
        Assert.NotNull(e.ConstraintFailures);
        Assert.AreEqual(1, e.ConstraintFailures!.Length);
        Assert.AreEqual("Failed unique constraint", e.ConstraintFailures![0].Message);
        Assert.Null(e.ConstraintFailures![0].Name);

        Assert.AreEqual(1, e.ConstraintFailures[0].Paths!.Length);
        Assert.AreEqual(1, e.ConstraintFailures![0].Paths![0].Length);

    }

    [Test]
    public void FailsWithIntegerConstraintFailurePath()
    {
        var e = Assert.ThrowsAsync<ConstraintFailureException>(async () =>
            await _client.QueryAsync<object>(FQL($"Collection.create({{name: \"Foo\", constraints: [{{unique: [\"$$$\"] }}]}})")));

        Assert.AreEqual("BadRequest (constraint_failure): Failed to create Collection.\n---\nerror: Failed to create Collection.\nconstraint failures:\n  constraints[0].unique: Value `$$$` is not a valid FQL path expression.\nat *query*:1:18\n  |\n1 | Collection.create({name: \"Foo\", constraints: [{unique: [\"$$$\"] }]})\n  |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^\n  |", e!.Message);
        Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
        Assert.NotNull(e.ConstraintFailures);
        Assert.AreEqual(1, e.ConstraintFailures!.Length);
        Assert.AreEqual("Value `$$$` is not a valid FQL path expression.", e.ConstraintFailures![0].Message);
        Assert.Null(e.ConstraintFailures![0].Name);

        Assert.AreEqual(1, e.ConstraintFailures[0].Paths!.Length);
        Assert.AreEqual(3, e.ConstraintFailures![0].Paths![0].Length);
    }
}
