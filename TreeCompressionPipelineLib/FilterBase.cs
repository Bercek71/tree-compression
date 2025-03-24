using System.Reflection;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

public abstract class FilterBase<T, TO> : IFilter
{
    private IFilter? _nextFilter = null;
    private readonly List<IProcessObserver> _processObservers = [];
    
    protected abstract TO ProcessData(T data);

    public object Process(object data)
    {
        NotifyStart(GetType().Name);
        try
        {
            if (data is not T typedData)
            {
                throw new ArgumentException("Filter received invalid data type");
            }
                
            var processedData = ProcessData(typedData);
            NotifyComplete(GetType().Name, processedData ?? throw new InvalidOperationException());
            return _nextFilter != null ? _nextFilter.Process(processedData) : processedData;
        }
        catch (Exception e)
        {
            NotifyError(GetType().Name, e);
            throw;
        }

    }

    
    public IFilter Chain(IFilter nextFilter)
    {
        this._nextFilter = nextFilter;
        return this._nextFilter;
    }

    public void AddObserver(IProcessObserver observer)
    {
        _processObservers.Add(observer);
    }

    public void RemoveObserver(IProcessObserver observer)
    {
        _processObservers.Remove(observer);
    }

    public void NotifyStart(string process)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnStart(process);
        }
    }

    public void NotifyProgress(string process, double percentComplete)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnProgress(process, percentComplete);
        }
    }

    public void NotifyComplete(string process, object result)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnComplete(process, result);
        }
    }

    public void NotifyError(string process, Exception error)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnError(process, error);
        }
    }
}