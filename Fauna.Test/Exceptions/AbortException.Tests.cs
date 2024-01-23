using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions;

[TestFixture]
public class AbortExceptionTests
{
    private class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    private static MappingContext ctx = new();

    [Test]
    public void Ctor_InitializesPropertiesCorrectly()
    {
        var expectedQueryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", $"{{\"@int\":\"123\"}}");
        var expectedMessage = "Test message";
        var expectedInnerException = new Exception("Inner exception");

        var actual1 = new AbortException(ctx, expectedQueryFailure, expectedMessage);

        Assert.AreEqual(expectedQueryFailure.ErrorInfo.Abort, actual1.QueryFailure.ErrorInfo.Abort);
        Assert.AreEqual(expectedMessage, actual1.Message);
    }

    [Test]
    public void GetData_WithIntData_ReturnsDeserializedObject()
    {
        var expected = 123;
        var queryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", $"{{\"@int\":\"{expected}\"}}");
        var exception = new AbortException(ctx, queryFailure, "Test message");

        var actual = exception.GetData();

        Assert.IsNotNull(actual);
        Assert.IsInstanceOf<int>(actual);
        Assert.AreEqual(expected, (int)actual!);
    }


    [Test]
    public void GetDataT_WithIntData_ReturnsDeserializedTypedObject()
    {
        var expected = 123;
        var queryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", $"{{\"@int\":\"{expected}\"}}");
        var exception = new AbortException(ctx, queryFailure, "Test message");

        var actual = exception.GetData<int>();

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetData_WithPocoData_ReturnsDeserializedObject()
    {
        var expected = new TestClass { Name = "John", Age = 105 };
        var queryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", $"{{\"@object\":{{\"Name\":\"{expected.Name}\",\"Age\":{{\"@int\":\"{expected.Age}\"}}}}}}");
        var exception = new AbortException(ctx, queryFailure, "Test message");

        var actual = exception.GetData() as IDictionary<string, object>;

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Name, actual["Name"]);
        Assert.AreEqual(expected.Age, actual["Age"]);
    }

    [Test]
    public void GetDataT_WithPocoData_ReturnsDeserializedTypedObject()
    {
        var expected = new TestClass { Name = "John", Age = 105 };
        var queryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", $"{{\"@object\":{{\"name\":\"{expected.Name}\",\"age\":{{\"@int\":\"{expected.Age}\"}}}}}}");
        var exception = new AbortException(ctx, queryFailure, "Test message");

        var actual = exception.GetData<TestClass>();

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Age, actual.Age);
    }

    [Test]
    public void GetData_WithNullAbortData_ReturnsNull()
    {
        var queryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", null);
        var exception = new AbortException(ctx, queryFailure, "Test message");

        var result = exception.GetData();

        Assert.IsNull(result);
    }

    [Test]
    public void GetData_CachesDataCorrectly()
    {
        var mockData = new TestClass { Name = "John", Age = 105 };
        var queryFailure = ExceptionTestHelper.CreateQueryFailure("abort", "Query aborted.", $"{{\"@object\":{{\"Name\":\"{mockData.Name}\",\"Age\":{{\"@int\":\"{mockData.Age}\"}}}}}}");
        var exception = new AbortException(ctx, queryFailure, "Test message");

        var callResult1 = exception.GetData();
        var typedCallResult1 = exception.GetData<TestClass>();
        var callResult2 = exception.GetData();
        var typedCallResult2 = exception.GetData<TestClass>();

        Assert.AreSame(callResult1, callResult2);
        Assert.AreSame(typedCallResult1, typedCallResult2);
        Assert.AreNotSame(callResult1, typedCallResult1);
        Assert.AreNotSame(callResult2, typedCallResult2);
    }
}
