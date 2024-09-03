using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Fauna.Core;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Test.Helpers;
using NUnit.Framework;
using static Fauna.Query;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ServiceExceptionTests
    {
        [AllowNull] private Client _c;

        private class TestAbortClass
        {
            [AllowNull] public string Name { get; init; }
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            _c = TestClientHelper.NewTestClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _c.Dispose();
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
            using var badClient = TestClientHelper.NewTestClient("invalid");
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
            Assert.AreEqual(
                "BadRequest (invalid_query): The query failed 1 validation check\n---\nerror: Unexpected end of query. Expected statement or expression\nat *query*:1:1\n  |\n1 | \"bad query\n  | ^\n  |",
                e!.Message);
            Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
            Assert.AreEqual(1, e.Stats.ComputeOps);
            Assert.IsEmpty(e.QueryTags);
        }

        [Test]
        [TestCase(400, "invalid_query", typeof(QueryCheckException))]
        [TestCase(400, "unbound_variable", typeof(QueryRuntimeException))]
        [TestCase(400, "index_out_of_bounds", typeof(QueryRuntimeException))]
        [TestCase(400, "type_mismatch", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_argument", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_bounds", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_regex", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_schema", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_document_id", typeof(QueryRuntimeException))]
        [TestCase(400, "document_id_exists", typeof(QueryRuntimeException))]
        [TestCase(400, "document_not_found", typeof(QueryRuntimeException))]
        [TestCase(400, "document_deleted", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_function_invocation", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_index_invocation", typeof(QueryRuntimeException))]
        [TestCase(400, "null_value", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_null_access", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_cursor", typeof(QueryRuntimeException))]
        [TestCase(400, "permission_denied", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_effect", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_write", typeof(QueryRuntimeException))]
        [TestCase(400, "internal_failure", typeof(QueryRuntimeException))]
        [TestCase(400, "divide_by_zero", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_id", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_secret", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_time", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_unit", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_date", typeof(QueryRuntimeException))]
        [TestCase(400, "limit_exceeded", typeof(ThrottlingException))]
        [TestCase(400, "stack_overflow", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_computed_field_access", typeof(QueryRuntimeException))]
        [TestCase(400, "disabled_feature", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_receiver", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_timestamp_field_access", typeof(QueryRuntimeException))]
        [TestCase(400, "invalid_request", typeof(InvalidRequestException))]
        [TestCase(400, "abort", typeof(AbortException))]
        [TestCase(400, "constraint_failure", typeof(QueryRuntimeException))]
        [TestCase(401, "unauthorized", typeof(UnauthorizedException))]
        [TestCase(403, "forbidden", typeof(ForbiddenException))]
        [TestCase(409, "contended_transaction", typeof(ContendedTransactionException))]
        [TestCase(429, "limit_exceeded", typeof(ThrottlingException))]
        [TestCase(440, "time_out", typeof(QueryTimeoutException))]
        [TestCase(500, "internal_error", typeof(ServiceException))]
        [TestCase(503, "time_out", typeof(QueryTimeoutException))]
        [TestCase(504, "gateway_timeout", typeof(NetworkException))]
        [TestCase(400, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(401, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(403, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(409, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(429, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(440, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(500, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(503, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(504, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(999, "some unhandled code", typeof(QueryRuntimeException))]
        [TestCase(null, "some unhandled code", typeof(QueryRuntimeException))]
        public void QueryException_All(HttpStatusCode status, string code, Type exceptionType)
        {
            var jsonDoc =
                JsonDocument.Parse(
                    $"{{\"error\": {{\"code\": \"{code}\", \"message\": \"oops\", \"abort\": \"oops\", \"constraint_failures\": [{{\"message\": \"oops\"}}]}}}}");
            var queryFailure = new QueryFailure(status, jsonDoc.RootElement);

            var ex = ExceptionFactory.FromQueryFailure(new MappingContext(), queryFailure);
            Assert.AreEqual(exceptionType, ex.GetType());
            Assert.AreEqual(status, queryFailure.StatusCode);

            jsonDoc.Dispose();
        }

        // TODO(lucas): Write the remaining real tests
    }
}
