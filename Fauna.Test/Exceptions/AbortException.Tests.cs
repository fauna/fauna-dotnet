using Fauna.Exceptions;
using NUnit.Framework;

namespace Fauna.Tests;

[TestFixture]
public class AbortExceptionTests
{
    private class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }

    }

    [Test]
    public void Ctor_InitializesPropertiesCorrectly()
    {
        var expectedQueryFailure = CreateQueryFailure(@"{{\""@int\"":\""123\""}}");
        var expectedMessage = "Test message";
        var expectedInnerException = new Exception("Inner exception");

        var actual1 = new AbortException(expectedQueryFailure, expectedMessage);
        var actual2 = new AbortException(expectedQueryFailure, expectedMessage, expectedInnerException);

        Assert.AreEqual(expectedQueryFailure.ErrorInfo.Abort, actual1.QueryFailure.ErrorInfo.Abort);
        Assert.AreEqual(expectedMessage, actual1.Message);

        Assert.AreEqual(expectedQueryFailure.ErrorInfo.Abort, actual2.QueryFailure.ErrorInfo.Abort);
        Assert.AreEqual(expectedMessage, actual2.Message);
        Assert.AreEqual(expectedInnerException, actual2.InnerException);
    }

    [Test]
    public void GetData_WithIntData_ReturnsDeserializedObject()
    {
        var expected = 123;
        var queryFailure = CreateQueryFailure(@$"{{\""@int\"":\""{expected}\""}}");
        var exception = new AbortException(queryFailure, "Test message");

        var actual = exception.GetData();

        Assert.IsNotNull(actual);
        Assert.IsInstanceOf<int>(actual);
        Assert.AreEqual(expected, (int)actual!);
    }


    [Test]
    public void GetDataT_WithIntData_ReturnsDeserializedTypedObject()
    {
        var expected = 123;
        var queryFailure = CreateQueryFailure(@$"{{\""@int\"":\""{expected}\""}}");
        var exception = new AbortException(queryFailure, "Test message");

        var actual = exception.GetData<int>();

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetData_WithPocoData_ReturnsDeserializedObject()
    {
        var expected = new TestClass { Name = "John", Age = 105 };
        var queryFailure = CreateQueryFailure(@$"{{\""@object\"":{{\""Name\"":\""{expected.Name}\"",\""Age\"":{{\""@int\"":\""{expected.Age}\""}}}}}}");
        var exception = new AbortException(queryFailure, "Test message");

        var actual = exception.GetData() as IDictionary<string, object>;

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Name, actual["Name"]);
        Assert.AreEqual(expected.Age, actual["Age"]);
    }

    [Test]
    public void GetDataT_WithPocoData_ReturnsDeserializedTypedObject()
    {
        var expected = new TestClass { Name = "John", Age = 105 };
        var queryFailure = CreateQueryFailure(@$"{{\""@object\"":{{\""Name\"":\""{expected.Name}\"",\""Age\"":{{\""@int\"":\""{expected.Age}\""}}}}}}");
        var exception = new AbortException(queryFailure, "Test message");

        var actual = exception.GetData<TestClass>();

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Age, actual.Age);
    }

    [Test]
    public void GetData_WithNullAbortData_ReturnsNull()
    {
        var queryFailure = CreateQueryFailure(null);
        var exception = new AbortException(queryFailure, "Test message");

        var result = exception.GetData();

        Assert.IsNull(result);
    }

    [Test]
    public void GetData_CachesDataCorrectly()
    {
        var mockData = new TestClass { Name = "John", Age = 105 };
        var queryFailure = CreateQueryFailure(@$"{{\""@object\"":{{\""Name\"":\""{mockData.Name}\"",\""Age\"":{{\""@int\"":\""{mockData.Age}\""}}}}}}");
        var exception = new AbortException(queryFailure, "Test message");

        var callResult1 = exception.GetData<TestClass>();
        var typedCallResult1 = exception.GetData();
        var callResult2 = exception.GetData<TestClass>();
        var typedCallResult2 = exception.GetData();

        Assert.AreSame(callResult1, callResult2);
        Assert.AreSame(typedCallResult1, typedCallResult2);
        Assert.AreNotSame(callResult1, typedCallResult1);
        Assert.AreNotSame(callResult2, typedCallResult2);
    }

    private QueryFailure CreateQueryFailure(string abortData)
    {
        var rawResponseText = $@"{{
            ""error"": {{
                ""code"": ""abort"",
                ""message"": ""Query aborted."",
                ""abort"": ""{abortData}""
            }},
            ""summary"": ""error: Query aborted."",
            ""txn_ts"": 1702346199930000,
            ""stats"": {{
                ""compute_ops"": 1,
                ""read_ops"": 0,
                ""write_ops"": 0,
                ""query_time_ms"": 105,
                ""contention_retries"": 0,
                ""storage_bytes_read"": 261,
                ""storage_bytes_write"": 0,
                ""rate_limits_hit"": []
            }},
            ""schema_version"": 0
        }}";
        return new QueryFailure(rawResponseText);
    }
}