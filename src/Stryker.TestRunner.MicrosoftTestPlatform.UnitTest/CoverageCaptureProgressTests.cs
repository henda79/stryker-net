using Microsoft.Extensions.Logging;
using Shouldly;

namespace Stryker.TestRunner.MicrosoftTestPlatform.UnitTest;

[TestClass]
public class CoverageCaptureProgressTests
{
    private sealed class Lines : ILogger
    {
        public List<string> Logged { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Logged.Add(formatter(state, exception));
    }

    [TestMethod]
    public void TestDone_ShouldLogEveryFivePercent_AndTheLastTest()
    {
        var logger = new Lines();
        var now = new DateTime(2026, 11, 15, 12, 0, 0);
        var progress = new CoverageCaptureProgress(40, logger, () => now);

        for (var i = 0; i < 40; i++)
        {
            progress.TestDone();
        }

        logger.Logged.Count.ShouldBe(20, "every 2 tests of 40 is 5%");
        logger.Logged[0].ShouldBe("Captured coverage for 2 of 40 tests");
        logger.Logged[^1].ShouldBe("Captured coverage for 40 of 40 tests");
    }

    [TestMethod]
    public void TestDone_ShouldLogAfterThirtySeconds_WhenTestsAreSlowerThanAStep()
    {
        // SiteManager: 607 tests at about 10 s each, three at a time - 5% was 10 minutes of silence
        var logger = new Lines();
        var now = new DateTime(2026, 11, 15, 12, 0, 0);
        var progress = new CoverageCaptureProgress(607, logger, () => now);

        progress.TestDone();
        logger.Logged.ShouldBeEmpty("one test in, and no time has passed");

        now = now.AddSeconds(31);
        progress.TestDone();

        logger.Logged.ShouldBe(["Captured coverage for 2 of 607 tests"]);
    }

    [TestMethod]
    public void TestDone_ShouldCountEveryTest_WhenCalledFromManyThreads()
    {
        var logger = new Lines();
        var progress = new CoverageCaptureProgress(1000, logger);

        Parallel.For(0, 1000, _ => progress.TestDone());

        logger.Logged[^1].ShouldBe("Captured coverage for 1000 of 1000 tests");
        logger.Logged.ShouldBe(logger.Logged.OrderBy(l => int.Parse(l.Split(' ')[3])).ToList(), "the counts are logged in order");
    }

    [TestMethod]
    public void TestDone_ShouldLogNothing_WhenThereAreNoTests()
    {
        var logger = new Lines();

        new CoverageCaptureProgress(0, logger).TestDone();

        logger.Logged.ShouldBeEmpty();
    }
}
