using static Fauna.Query;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ServiceExceptionTests
    {
        private readonly Client _c = TestClientHelper.NewTestClient();

        private class TestAbortClass
        {
            [AllowNull] public string Name { get; init; }
        }

        [Test]
        public void AbortException_WithPOCO()
        {

            var q = FQL($"abort({{name: 'special'}})");
            var e = Assert.ThrowsAsync<AbortException>(async () => await _c.QueryAsync(q));
            Assert.NotNull(e);
            Assert.AreEqual(
                "BadRequest (abort): Query aborted.\n---\nerror: Query aborted.\nat *query*:1:6\n  |\n1 | abort({name: 'special'})\n  |      ^^^^^^^^^^^^^^^^^^^\n  |",
                e!.Message);
            Assert.AreEqual("abort", e.ErrorCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
            Assert.AreEqual(1, e.Stats.ComputeOps);
            Assert.IsEmpty(e.QueryTags);
            var d = e.GetData<TestAbortClass>();
            Assert.NotNull(d);
            Assert.AreEqual("special", d!.Name);
        }

        [Test]
        public void AbortException_Default()
        {
            var q = FQL($"abort({{name: 'special'}})");
            var e = Assert.ThrowsAsync<AbortException>(async () => await _c.QueryAsync(q));
            var d = (Dictionary<string, object>)e!.GetData()!;
            Assert.AreEqual("special", d["name"]);
        }

        [Test]
        public void AbortException_Null()
        {
            var q = FQL($"abort(null)");
            var e = Assert.ThrowsAsync<AbortException>(async () => await _c.QueryAsync(q));
            var d = e!.GetData()!;
            Assert.IsNull(d);
        }

        [Test]
        public void AbortException_EmptyString()
        {
            var q = FQL($"abort('')");
            var e = Assert.ThrowsAsync<AbortException>(async () => await _c.QueryAsync(q));
            var d = e!.GetData()!;
            Assert.IsNull(d);
        }

        [Test]
        public void UnauthorizedException_Basic()
        {
            var badClient = TestClientHelper.NewTestClient("invalid");
            var q = FQL($"42");
            var e = Assert.ThrowsAsync<UnauthorizedException>(async () => await badClient.QueryAsync(q));
            Assert.NotNull(e);
            Assert.AreEqual("Unauthorized (unauthorized): Access token required", e!.Message);
            Assert.AreEqual(HttpStatusCode.Unauthorized, e.StatusCode);
            Assert.AreEqual(0, e.Stats.ComputeOps);
            Assert.IsEmpty(e.QueryTags);
        }

        [Test]
        public void QueryCheckException_Basic()
        {
            var q = FQL($"\"bad query");
            var e = Assert.ThrowsAsync<QueryCheckException>(async () => await _c.QueryAsync(q));
            Assert.NotNull(e);
            Assert.AreEqual("BadRequest (invalid_query): The query failed 1 validation check\n---\nerror: Unexpected end of query. Expected statement or expression\nat *query*:1:1\n  |\n1 | \"bad query\n  | ^\n  |", e!.Message);
            Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
            Assert.AreEqual(1, e.Stats.ComputeOps);
            Assert.IsEmpty(e.QueryTags);
        }


        // TODO(lucas): Write the remaining real tests
    }
}
