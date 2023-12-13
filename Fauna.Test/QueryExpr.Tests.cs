using NUnit.Framework;
using System.Text.RegularExpressions;
using static Fauna.Query;

namespace Fauna.Test;

[TestFixture]
public class QueryExprTests
{
    [Test]
    public void QueryExpr_Serialize()
    {
        int backorderLimit = 15;
        bool isBackordered = false;
        var subQuery = FQL($@"let product = Product.firstWhere(.backorderLimit == {backorderLimit} && .backordered == {isBackordered})!; product.quantity;");
        var mainQuery = FQL($@"Product.where(.quantity == {subQuery}).order(.title) {{ name, description }}");
        string formattedExpected = @"
        {
            ""fql"": [
                ""Product.where(.quantity == "",
                {
                    ""fql"": [
                        ""let product = Product.firstWhere(.backorderLimit == "",
                        { ""value"": { ""@int"": ""15""} },
                        "" && .backordered == "",
                        { ""value"": false },
                        "")!; product.quantity;""
                    ]
                },
                "").order(.title) { name, description }""
            ]
        }";
        string expected = Regex.Replace(formattedExpected, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

        var actual = mainQuery.Serialize();

        Assert.AreEqual(expected, actual);
    }
}
