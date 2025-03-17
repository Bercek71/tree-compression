namespace TreeCompressionPipeline;

public class Pipeline
{
    private IFilter? _firstFilter;
    private IFilter? _lastFilter;

    public IProcessObserver? ProcessObserver { get; init; } = null;

    public Pipeline AddFilter(IFilter filter)
    {
        if (ProcessObserver != null)
        {
            filter.AddObserver(ProcessObserver);
        }
        if (_firstFilter == null)
        {
            _firstFilter = _lastFilter = filter;
        }
        else
        {
            _lastFilter = _lastFilter?.Chain(filter);
        }
        return this;
    }


    public object Process(object input)
    {
        return _firstFilter?.Process(input);
    }
}