using TreeCompressionPipeline;

namespace ConsoleApp.Utils;

public class ProcessTimer : IProcessObserver
{
    private readonly StopWatch _stopWatch = new StopWatch();
    private readonly Stack<string> _processStack = new Stack<string>();
    private readonly Dictionary<string, TimeSpan> _processTimes = new Dictionary<string, TimeSpan>();

    public void OnStart(string process)
    {
        _processStack.Push(process);
        _stopWatch.Start();
    }

    public void OnProgress(string process, double percentComplete)
    {
        throw new NotImplementedException();
    }

    public void OnComplete(string process, object result)
    {
        if (process != _processStack.Peek()) return;
        var elapsed = _stopWatch.Stop();
        _processStack.Pop();
        Console.WriteLine($"{process} completed in {elapsed.TotalMilliseconds} ms");
        if (!_processTimes.TryAdd(process, elapsed))
        {
            _processTimes[process] = elapsed;
        }
    }

    public void OnError(string process, Exception error)
    {
        OnComplete(process, error.Message);
        Console.WriteLine($"{process} failed with error: {error.Message}");
    }

    public TimeSpan this[string compressionFilter] => _processTimes[compressionFilter];
}