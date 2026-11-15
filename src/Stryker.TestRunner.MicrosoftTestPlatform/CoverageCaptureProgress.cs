using Microsoft.Extensions.Logging;

namespace Stryker.TestRunner.MicrosoftTestPlatform;

/// <summary>
/// Reports how far a test-by-test coverage capture has got. Capture runs every test on its own, so on
/// a test project whose tests are expensive to start (a server and a database per run, say) it can
/// take most of an hour, and with nothing logged between its first and last line it reads as a hang.
/// Logs every 5% of the tests, and at least every 30 seconds while any are done, whichever is first.
/// </summary>
internal sealed class CoverageCaptureProgress
{
    private static readonly TimeSpan MaxQuiet = TimeSpan.FromSeconds(30);

    private readonly int _total;
    private readonly ILogger _logger;
    private readonly Func<DateTime> _clock;
    private readonly object _lock = new();
    private int _done;
    private int _lastStep;
    private DateTime _lastLog;

    public CoverageCaptureProgress(int total, ILogger logger, Func<DateTime>? clock = null)
    {
        _total = total;
        _logger = logger;
        _clock = clock ?? (() => DateTime.UtcNow);
        _lastLog = _clock();
    }

    /// <summary>Counts one more test captured, from any thread, and logs when a step or the quiet limit is reached.</summary>
    public void TestDone()
    {
        if (_total <= 0)
        {
            return;
        }

        // logged under the lock too, so the counts always appear in order
        lock (_lock)
        {
            var done = ++_done;
            var step = done * 20 / _total;
            var now = _clock();
            if (step <= _lastStep && now - _lastLog < MaxQuiet && done < _total)
            {
                return;
            }

            _lastStep = Math.Max(step, _lastStep);
            _lastLog = now;
            _logger.LogInformation("Captured coverage for {Done} of {Total} tests", done, _total);
        }
    }
}
