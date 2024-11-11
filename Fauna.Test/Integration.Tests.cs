using System.Diagnostics.CodeAnalysis;
using System.Text;
using Fauna.Core;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;
using Fauna.Util.Extensions;
using NUnit.Framework;
using static Fauna.Query;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test;

[TestFixture]
public class IntegrationTests
{
    [AllowNull]
    private static Client _client = null!;

    private class Person
    {
        [Field("first_name")]
        public string? FirstName { get; set; }
        [Field("last_name")]
        public string? LastName { get; set; }
        [Field("age")]
        public int Age { get; set; }
    }

    private class EmbeddedSets
    {
        [Field("set")] public Page<EmbeddedSet>? Set { get; set; }
    }

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = GetLocalhostClient();
        Fixtures.EmbeddedSetDb(_client);
    }

    [SetUp]
    [Category("EventStream"), Category("EventFeed")]
    public async Task SetUpStreaming()
    {
        await Fixtures.StreamingSandboxSetup(_client);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
    }


    [Test]
    public async Task UserDefinedObjectTest()
    {
        var expected = new Person
        {
            FirstName = "Georgia",
            LastName = "O'Keeffe",
            Age = 136
        };
        var query = FQL($"{expected}");
        var result = await _client.QueryAsync<Person>(query);
        var actual = result.Data;

        Assert.AreNotEqual(expected, actual);
        Assert.AreEqual(expected.FirstName, actual.FirstName);
        Assert.AreEqual(expected.LastName, actual.LastName);
        Assert.AreEqual(expected.Age, actual.Age);
    }

    [Test]
    public async Task Paginate_SinglePageWithSmallCollection()
    {
        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10].toSet();");

        var paginatedResult = _client.PaginateAsync<int>(query);

        int pageCount = 0;
        await foreach (var page in paginatedResult)
        {
            pageCount++;
            var data = page.Data;
            Assert.AreEqual(10, data.Count());
        }

        Assert.AreEqual(1, pageCount);
    }

    [Test]
    public async Task Paginate_MultiplePagesWithCollection()
    {
        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20].toSet()");

        var paginatedResult = _client.PaginateAsync<int>(query);

        int pageCount = 0;
        await foreach (var page in paginatedResult)
        {
            pageCount++;
            var data = page.Data;
            // default page size is 16
            var size = pageCount == 1 ? 16 : 4;
            Assert.AreEqual(size, data.Count());
        }

        Assert.AreEqual(2, pageCount);
    }

    [Test]
    public async Task Paginate_MultiplePagesWithPocoCollection()
    {
        var items = new List<Person>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add(new Person
            {
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Age = i
            });
        }

        var query = FQL($"{items}.toSet()");

        var paginatedResult = _client.PaginateAsync<Person>(query);

        var pageCount = 0;
        var itemCount = 0;
        await foreach (var page in paginatedResult)
        {
            pageCount++;
            var data = page.Data;
            itemCount += data.Count;
        }

        Assert.Greater(pageCount, 1);
        Assert.AreEqual(100, itemCount);
    }

    [Test]
    [Category("serialization")]
    public async Task ValidateQueryArray()
    {
        var q = new List<Query> { FQL($"4 + 2"), FQL($"5 + 2"), FQL($"6 + 2"), };
        var obj = new QueryArr(q);

        var result = await _client.QueryAsync<List<int>>(FQL($"{obj}"));

        Assert.AreEqual(new List<int> { 6, 7, 8 }, result.Data);
    }

    [Test]
    [Category("serialization")]
    public async Task ValidateQueryArrayWithQueryVal()
    {
        var q = new List<Query> { new QueryVal(6), FQL($"5 + 2"), FQL($"6 + 2"), };
        var obj = new QueryArr(q);

        var result = await _client.QueryAsync<List<int>>(FQL($"{obj}"));

        Assert.AreEqual(new List<int> { 6, 7, 8 }, result.Data);
    }

    [Test]
    [Category("serialization")]
    public async Task ValidateQueryObject()
    {
        var q = new Dictionary<string, Query>
        {
            { "six", FQL($"4 + 2") },
            { "seven", FQL($"5 + 2") },
            { "eight", FQL($"6 + 2")},
        };
        var obj = new QueryObj(q);

        var result = await _client.QueryAsync<Dictionary<string, int>>(FQL($"{obj}"));

        Assert.AreEqual(new Dictionary<string, int>
        {
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 }
        }, result.Data);
    }

    [Test]
    public async Task Paginate_IteratorCanBeFlattened()
    {
        var items = new List<Person>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add(new Person
            {
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Age = i
            });
        }

        var query = FQL($"{items}.toSet()");

        var paginatedResult = _client.PaginateAsync<Person>(query);

        int itemCount = 0;
        await foreach (var item in paginatedResult.FlattenAsync())
        {
            itemCount++;
        }

        Assert.AreEqual(100, itemCount);
    }

    [Test]
    public async Task Paginate_EmbeddedSet()
    {
        var q = FQL($"{{ set: EmbeddedSet.all() }}");
        var r = await _client.QueryAsync<EmbeddedSets>(q);

        Assert.NotNull(r.Data.Set);
        var pageCount = 0;
        var itemCount = 0;
        await foreach (var page in _client.PaginateAsync(r.Data.Set!))
        {
            itemCount += page.Data.Count;
            pageCount++;
        }

        Assert.AreEqual(100, itemCount);
        Assert.AreEqual(7, pageCount);
    }

    [Test]
    public async Task Paginate_EmbeddedSetFlattened()
    {
        var q = FQL($"{{ set: EmbeddedSet.all() }}");
        var r = await _client.QueryAsync<EmbeddedSets>(q);

        Assert.NotNull(r.Data.Set);
        var itemCount = 0;
        await foreach (var item in _client.PaginateAsync(r.Data.Set!).FlattenAsync())
        {
            itemCount++;
        }

        Assert.AreEqual(100, itemCount);
    }

    [Test]
    public async Task NullNamedDocument()
    {
        var q = FQL($"Collection.byName('Fake')");
        var r = await _client.QueryAsync<NamedRef<Dictionary<string, object>>>(q);
        var d = r.Data;
        Assert.AreEqual("Fake", d.Name);
        Assert.AreEqual("Collection", d.Collection.Name);
        Assert.AreEqual("not found", d.Cause);

        var e = Assert.Throws<NullDocumentException>(() => d.Get());
        Assert.NotNull(e);
        Assert.AreEqual("Fake", e!.Name);
        Assert.AreEqual("Collection", e.Collection.Name);
        Assert.AreEqual("not found", e.Cause);
    }

    [Test]
    public async Task StatsCollector()
    {
        if (_client.StatsCollector is null)
        {
            Assert.Fail("StatsCollector should not be null");
            return;
        }

        var startingStats = _client.StatsCollector.Read();

        var q = FQL($"{{ set: EmbeddedSet.all() }}");
        var r = await _client.QueryAsync<EmbeddedSets>(q);

        Assert.NotNull(r.Data.Set);

        var queriesMade = startingStats.QueryCount;
        await foreach (var page in _client.PaginateAsync(r.Data.Set!))
        {
            var pageStats = _client.StatsCollector.Read();
            Assert.Greater(pageStats.ReadOps, startingStats.ReadOps);
            Assert.Greater(pageStats.StorageBytesRead, startingStats.StorageBytesRead);
            Assert.Greater(pageStats.QueryCount, queriesMade);
            queriesMade = pageStats.QueryCount;
        }

        _client.StatsCollector.ReadAndReset();
        var resultStats = _client.StatsCollector.Read();
        Assert.Zero(resultStats.ReadOps);
        Assert.Zero(resultStats.ComputeOps);
        Assert.Zero(resultStats.WriteOps);
        Assert.Zero(resultStats.QueryTimeMs);
        Assert.Zero(resultStats.ContentionRetries);
        Assert.Zero(resultStats.StorageBytesRead);
        Assert.Zero(resultStats.StorageBytesWrite);
        Assert.Zero(resultStats.QueryCount);
        Assert.Zero(resultStats.RateLimitedReadQueryCount);
        Assert.Zero(resultStats.RateLimitedComputeQueryCount);
        Assert.Zero(resultStats.RateLimitedWriteQueryCount);
    }

    [Test]
    public async Task NullableStatsCollector()
    {
        var testClient = GetLocalhostClient(hasStatsCollector: false);

        var query = FQL($"4+5");
        var result = await testClient.QueryAsync<int>(query);
        var actual = result.Data;

        Assert.AreEqual(9, actual);
        Assert.Null(testClient.StatsCollector);
    }

    #region EventStreams

    [Test, Category("EventStream")]
    public async Task StreamRequestCancel()
    {
        var cts = new CancellationTokenSource();
        var stream =
            await _client.EventStreamAsync<StreamingSandbox>(FQL($"StreamingSandbox.all().eventSource()"),
                cancellationToken: cts.Token);
        Assert.NotNull(stream);
        var longRunningTask = Task.Run(async () =>
        {
            int count = 0;
            while (count < 10)
            {
                await Task.Delay(250, cts.Token);
                count++;
            }

            Assert.Fail();
        }, cts.Token);

        await Task.Delay(500, cts.Token).ContinueWith(_ => { cts.Cancel(); }, cts.Token);
        Assert.ThrowsAsync<TaskCanceledException>(async () => await longRunningTask);
    }

    [Test, Category("EventStream")]
    public async Task CanReadEventsFomStream()
    {
        var queries = new[]
        {
            FQL($"StreamingSandbox.create({{ foo: 'bar' }})"),
            FQL($"StreamingSandbox.all().forEach(.update({{ foo: 'baz' }}))"),
            FQL($"StreamingSandbox.all().forEach(.delete())")
        };

        int expectedEvents = queries.Length + 1; // add one for the status event
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)); // prevent runaway test

        // create a task to open the stream and process events
        var streamTask = Task.Run(() =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                var stream = await _client.EventStreamAsync<StreamingSandbox>(
                    FQL($"StreamingSandbox.all().eventSource()"),
                    cancellationToken: cts.Token
                );
                Assert.NotNull(stream);

                await foreach (var evt in stream)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.IsNotEmpty(evt.Cursor, "should have a cursor");
                        Assert.NotZero(evt.Stats.ReadOps, "should have consumed ReadOps");
                        if (evt.Type is EventType.Status)
                        {
                            return;
                        }

                        Assert.NotNull(evt.Data, "should have data");
                        Assert.AreEqual(
                            evt.Data!.Foo,
                            (evt.Type == EventType.Add) ? "bar" : "baz",
                            "Foo should be set"
                        );
                    });

                    expectedEvents--;
                    if (expectedEvents > 0)
                    {
                        continue;
                    }

                    TestContext.WriteLine("Seen all events");
                    break;
                }
            });
        }, cts.Token);

        // invoke queries on a delay to simulate streaming events
        var queryTasks = queries.Select(
            (query, index) => Task.Delay((index + 1) * 500, cts.Token)
                .ContinueWith(
                    _ =>
                    {
                        Assert.DoesNotThrowAsync(async () => { await _client.QueryAsync(query, cancel: cts.Token); },
                            "Should successfully invoke query");
                    }, TaskContinuationOptions.ExecuteSynchronously));

        // wait for all tasks
        queryTasks = queryTasks.Append(streamTask);
        Task.WaitAll(queryTasks.ToArray(), cts.Token);

        Assert.Zero(expectedEvents, "stream handler should process all events");

        await Task.CompletedTask;
    }

    [Test, Category("EventStream")]
    public Task StreamThrowsWithBadRequest()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)); // prevent runaway test

        var ex = Assert.ThrowsAsync<FaunaException>(async () =>
        {
            var stream = await _client.EventStreamAsync<StreamingSandbox>(FQL($"StreamingSandbox.all().eventSource()"),
                streamOptions: new StreamOptions("fake", "abc1234=="),
                cancellationToken: cts.Token);

            await foreach (var _ in stream)
            {
                Assert.Fail("Should not process events");
            }
        });

        Assert.AreEqual("BadRequest: Bad Request", ex?.Message);

        return Task.CompletedTask;
    }

    [Test, Category("EventStream")]
    public async Task CanResumeStreamWithStreamOptions()
    {
        string? token = null;
        string? cursor = null;

        var queries = new[]
        {
            FQL($"StreamingSandbox.create({{ foo: 'bar' }})"),
            FQL($"StreamingSandbox.all().forEach(.update({{ foo: 'baz' }}))"),
            FQL($"StreamingSandbox.all().forEach(.delete())")
        };

        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)); // prevent runaway test
        cts.Token.ThrowIfCancellationRequested();

        int expectedEvents = queries.Length + 1;

        // create a task to open the stream and process events
        var streamTask = Task.Run(() =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                var stream = await _client.EventStreamAsync<StreamingSandbox>(
                    FQL($"StreamingSandbox.all().eventSource()"),
                    cancellationToken: cts.Token
                );
                Assert.NotNull(stream);
                token = stream.Token;

                await foreach (var evt in stream)
                {
                    // break after the first received event
                    cursor = evt.Cursor;
                    // ReSharper disable once AccessToModifiedClosure
                    expectedEvents--;
                    break;
                }
            });
        }, cts.Token);

        // invoke queries on a delay to simulate streaming events
        var queryTasks = queries.Select(
            (query, index) => Task.Delay((index + 1) * 500, cts.Token)
                .ContinueWith(
                    _ =>
                    {
                        Assert.DoesNotThrowAsync(async () => { await _client.QueryAsync(query, cancel: cts.Token); },
                            "Should successfully invoke query");
                    }, TaskContinuationOptions.ExecuteSynchronously));

        // wait for all tasks
        queryTasks = queryTasks.Append(streamTask);
        Task.WaitAll(queryTasks.ToArray(), cts.Token);

        Assert.NotNull(token, "should have a token");
        Assert.NotNull(cursor, "should have a cursor from the first event");

        var stream = await _client.EventStreamAsync<StreamingSandbox>(
            FQL($"StreamingSandbox.all().eventSource()"),
            streamOptions: new StreamOptions(token!, cursor!),
            cancellationToken: cts.Token
        );
        Assert.NotNull(stream);

        await foreach (var evt in stream)
        {
            Assert.IsNotEmpty(evt.Cursor, "should have a cursor");
            expectedEvents--;
            if (expectedEvents > 0)
            {
                continue;
            }

            break;
        }

        Assert.Zero(expectedEvents, "stream handler should process all events");
    }

    [Test, Category("EventStream")]
    public async Task CanOpenStreamWithEventSource()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)); // prevent runaway test
        cts.Token.ThrowIfCancellationRequested();

        EventSource eventSource = _client.QueryAsync<EventSource>(
            FQL($"StreamingSandbox.all().eventSource()"),
            queryOptions: null,
            cancel: cts.Token
        ).Result.Data;

        var stream = await _client.EventStreamAsync<StreamingSandbox>(eventSource, cts.Token);
        Assert.IsNotNull(stream);
    }

    #endregion

    #region EventFeeds

    [Test, Category("EventFeed")]
    public async Task CanOpenFeedWithQuery()
    {
        var feed = await _client.EventFeedAsync<StreamingSandbox>(FQL($"StreamingSandbox.all().eventSource()"));
        Assert.IsNotEmpty(feed.Cursor, "should have a cursor");
        Assert.IsNull(feed.CurrentPage, "should not have loaded a page");

        await using IAsyncEnumerator<FeedPage<StreamingSandbox>> asyncEnumerator = feed.GetAsyncEnumerator();
        await asyncEnumerator.MoveNextAsync();

        Assert.NotNull(feed.CurrentPage, "should have loaded a page");
        Assert.IsNotEmpty(feed.Cursor, "should have a cursor");
        Assert.IsEmpty(feed.CurrentPage!.Events, "should not have events");

        await _client.QueryAsync(FQL($"StreamingSandbox.create({{ foo: 'bar' }})"));

        FeedPage<StreamingSandbox>? lastPage = null;
        await foreach (var page in feed)
        {
            Assert.IsNotEmpty(page.Cursor, "should have a cursor");
            Assert.NotZero(page.Stats.ReadOps, "should have read ops");
            Assert.AreEqual(1, page.Events.Count, "should have 1 event");
            Assert.AreEqual(EventType.Add, page.Events[0].Type, "should be an add event");
            lastPage = page;
        }

        await using IAsyncEnumerator<FeedPage<StreamingSandbox>> asyncEnumeratorAgain = feed.GetAsyncEnumerator();
        await asyncEnumeratorAgain.MoveNextAsync();

        Assert.IsEmpty(feed.CurrentPage!.Events, "should not have any events");
        if (lastPage != null)
        {
            Assert.AreNotEqual(feed.Cursor, lastPage.Cursor, "should have a different cursor");
        }
    }

    [Test, Category("EventFeed")]
    public async Task CanOpenFeedWithEventSource()
    {
        EventSource eventSource =
            _client.QueryAsync<EventSource>(FQL($"StreamingSandbox.all().eventSource()")).Result.Data;
        Assert.NotNull(eventSource);

        var feed = await _client.EventFeedAsync<StreamingSandbox>(eventSource);
        Assert.IsNotNull(feed);

        await using IAsyncEnumerator<FeedPage<StreamingSandbox>> asyncEnumerator = feed.GetAsyncEnumerator();
        await asyncEnumerator.MoveNextAsync();

        Assert.IsNotEmpty(feed.Cursor, "should have a cursor");
        Assert.IsEmpty(feed.CurrentPage!.Events, "should not have any events");
    }

    [Test, Category("EventFeed")]
    public async Task CanUseFeedOptionsPageSize()
    {
        EventSource eventSource =
            _client.QueryAsync<EventSource>(FQL($"StreamingSandbox.all().eventSource()")).Result.Data;
        Assert.NotNull(eventSource);

        const int pageSize = 3;
        const int start = 5;
        const int end = 20;

        // Create Events
        await _client.QueryAsync(
            FQL($"Set.sequence({start}, {end}).forEach(n => StreamingSandbox.create({{ n: n }}))"));

        var feed = await _client.EventFeedAsync<StreamingSandbox>(eventSource, new FeedOptions(pageSize: pageSize));
        Assert.IsNotNull(feed);

        int pages = 0;
        await foreach (var page in feed)
        {
            if (page.HasNext)
            {
                Assert.AreEqual(pageSize, page.Events.Count);
            }

            pages++;
        }

        Assert.AreEqual((end - start) / pageSize, pages, "should have the correct number of pages");
    }

    [Test, Category("EventFeed")]
    public async Task CanUseFeedOptionsStartTs()
    {
        const int pageSize = 3;
        const int start = 5;
        const int end = 20;

        // Create Events
        await _client.QueryAsync(
            FQL($"Set.sequence({start}, {end}).forEach(n => StreamingSandbox.create({{ n: n }}))"));

        long fiveMinutesAgo = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds() * 1000;

        EventSource eventSource =
            _client.QueryAsync<EventSource>(FQL($"StreamingSandbox.all().eventSource()")).Result.Data;
        Assert.NotNull(eventSource);


        var feed = await _client.EventFeedAsync<StreamingSandbox>(eventSource, new FeedOptions(fiveMinutesAgo, pageSize: pageSize));
        Assert.IsNotNull(feed);

        int pages = 0;
        await foreach (var page in feed)
        {
            if (page.Events.Count > 0) Assert.NotNull(page.Events[0].Stats.ProcessingTimeMs);
            if (page.HasNext) Assert.AreEqual(pageSize, page.Events.Count);

            pages++;
        }

        Assert.AreEqual((end - start) / pageSize, pages, "should have the correct number of pages");
    }

    [Test, Category("EventFeed")]
    public async Task CanUseCursorWithQuery()
    {
        var feed = await _client.EventFeedAsync<StreamingSandbox>(
            FQL($"StreamingSandbox.all().eventSource()"),
            feedOptions: new FeedOptions("abc1234==")
        );

        Assert.NotNull(feed);
    }

    [Test, Category("EventFeed")]
    public void ThrowsWhenQueryDoesntReturnEventSource()
    {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _client.EventFeedAsync<StreamingSandbox>(FQL($"42"))
        );

        Assert.That(ex!.Message, Does.Contain("Query must return an EventSource."));
    }

    [Test, Category("EventFeed")]
    public void ThrowsWhenStartTsIsTooOld()
    {
        var ex = Assert.ThrowsAsync<EventException>(async () =>
            {
                long aYearAgo = DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeMilliseconds() * 1000;
                var feed = await _client.EventFeedAsync<StreamingSandbox>(
                    FQL($"StreamingSandbox.all().eventSource()"),
                    new FeedOptions(aYearAgo)
                );
                Assert.IsNotNull(feed);

                await foreach (var unused in feed)
                {
                }
            }
        );

        Assert.That(ex!.Message, Does.Contain("is too far in the past"));
    }

    #endregion

    [Test]
    public async Task CollectionAll()
    {
        var query = FQL($"Collection.all()");
        object result = (await _client.QueryAsync(query)).Data!;
        switch (result)
        {
            case Page<object> p:
                Assert.Greater(p.Data.Count, 0);
                break;
            default:
                Assert.Fail($"Expected Page<Ref<Dictionary<string, object>>> but was {result.GetType()}");
                break;
        }
    }

    [Test]
    [Category("serialization")]
    public void ValidateUnwrappedListOfQueriesError()
    {
        var q = new List<Query>
        {
            FQL($"4 + 2"),
            FQL($"5 + 2"),
            FQL($"6 + 2")
        };
        var ex = Assert.ThrowsAsync<SerializationException>(async () => await _client.QueryAsync<List<int>>(FQL($"{q}")));
        Assert.IsTrue(ex!.Message.StartsWith("Unable to deserialize `FaunaType.Object` with `IntSerializer`"));
    }

    [Test]
    [Category("serialization")]
    public void ValidateUnwrappedMapOfQueriesError()
    {
        var q = new Dictionary<string, Query>
        {
            { "six", FQL($"4 + 2") },
            { "seven", FQL($"1 + 6") },
            { "eight", FQL($"3 + 5") }
        };
        var ex = Assert.ThrowsAsync<SerializationException>(async () => await _client.QueryAsync<Dictionary<string, int>>(FQL($"{q}")));
        Assert.IsTrue(ex!.Message.Contains("Unable to deserialize `FaunaType.Object` with `IntSerializer`"));
    }

    [Test]
    [Category("serialization")]
    public async Task ValidateBytesAcrossTheWire()
    {
        // ReSharper disable once UseUtf8StringLiteral
        byte[] byteArray = [70, 97, 117, 110, 97];
        byte[]? nullArray = null;

        var result = await _client.QueryAsync<List<object?>>(FQL($"let x:Bytes = {byteArray}; let y:Bytes|Null = {nullArray}; [x,y]"));

        Assert.AreEqual(new[] { Encoding.UTF8.GetBytes("Fauna"), null }, result.Data);
    }
}
