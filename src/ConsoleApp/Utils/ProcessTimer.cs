using TreeCompressionPipeline;
using TreeCompressionPipeline.TreeStructure;

namespace ConsoleApp.Utils;

public class ProcessTimer : IProcessObserver
{
    private readonly StopWatch _stopWatch = new StopWatch();
    private readonly Stack<string> _processStack = new Stack<string>();
    private readonly Dictionary<string, TimeSpan> _processTimes = new Dictionary<string, TimeSpan>();
    public IDependencyTreeNode? Node { get; private set; }

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
        if (!_processTimes.TryAdd(process, elapsed))
        {
            _processTimes[process] = elapsed;
        }

        if (process == ProcessType.TextToTreeFilter)
        {
            Node = result as IDependencyTreeNode;
        }
    }

    public void OnError(string process, Exception error)
    {
        OnComplete(process, error.Message);
    }

    public TimeSpan this[string compressionFilter] => _processTimes[compressionFilter];
}