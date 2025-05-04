namespace TreeCompressionPipeline;

/// <summary>
/// Pipeline pro zpracování dat v rámci Pipes and Filters architektury.
/// Pipeline je tvořena jednotlivými filtry, které zpracovávají data.
/// </summary>
public class Pipeline
{
    private IFilter? _firstFilter;
    private IFilter? _lastFilter;

    /// <summary>
    /// Observer, který sleduje průběh procesu.
    /// </summary>
    public IProcessObserver? ProcessObserver { get; init; } = null;

    /// <summary>
    /// Přidání filtru do pipeline
    /// </summary>
    /// <param name="filter">
    /// Filtr pro zpracování dat.
    /// </param>
    /// <returns>
    /// Vrací odkaz na pipeline pro fluent chaining.
    /// </returns>
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

    /// <summary>
    /// Zpracování dat v rámci pipeline.
    /// </summary>
    /// <param name="input">
    /// Vstupní data pro zpracování v rámci filtrů
    /// </param>
    /// <returns>
    ///  Vrací zpracovaná data ve formě objektu, který může být libovolného typu.
    /// </returns>
    public object Process(object input)
    {
        var result = _firstFilter?.Process(input);
        if (result == null)
        {
            throw new InvalidOperationException("Pipeline has no filters to process data");
        }
        return result;
    }
}