using System.Reflection;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

/// <summary>
/// Implementace filtru pro pipeline. Filtr je základní jednotka pro zpracování dat v rámci pipeline.
/// </summary>
/// <seealso cref="IFilter"/>
/// <inheritdoc cref="IFilter"/>
/// <typeparam name="T">
/// Vstupní typ dat, které filtr zpracovává.
/// </typeparam>
/// <typeparam name="TO">
/// Výstupní typ dat, která filtr vrací.
/// </typeparam>
/// 
public abstract class FilterBase<T, TO> : IFilter
{
    private IFilter? _nextFilter = null;
    private readonly List<IProcessObserver> _processObservers = [];
    
    /// <summary>
    /// Obálka pro zpracování dat v rámci filtru pro konkrétní typ dat.
    /// Musí se doimplementovat
    /// </summary>
    /// <param name="data">
    /// Data pro zpracování filtrem
    /// </param>
    /// <returns>
    /// Zpraocovaná data ve formě objektu, který může být libovolného typu.
    /// </returns>
    protected abstract TO ProcessData(T data);

    /// <summary>
    /// Implementace metody pro zpracování dat v rámci filtru.
    /// Přetypuje data na konkrétní typ a zavolá metodu pro zpracování dat.
    /// </summary>
    /// <param name="data">
    /// Data, která jsou zpracována v rámci filtru.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    ///    Pokud filtr obdrží data, která nejsou typu <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   Pokud zpracování dat skončí chybou.
    /// </exception>
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

    /// <inheritdoc cref="IFilter.Chain"/>
    public IFilter Chain(IFilter nextFilter)
    {
        this._nextFilter = nextFilter;
        return this._nextFilter;
    }

    /// <inheritdoc cref="IProcessSubject.AddObserver"/>
    public void AddObserver(IProcessObserver observer)
    {
        _processObservers.Add(observer);
    }

    /// <inheritdoc cref="IProcessSubject.RemoveObserver"/>
    public void RemoveObserver(IProcessObserver observer)
    {
        _processObservers.Remove(observer);
    }

    /// <inheritdoc cref="IProcessSubject.NotifyStart"/>
    public void NotifyStart(string process)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnStart(process);
        }
    }

    /// <inheritdoc cref="IProcessSubject.NotifyProgress"/>
    public void NotifyProgress(string process, double percentComplete)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnProgress(process, percentComplete);
        }
    }

    /// <inheritdoc cref="IProcessSubject.NotifyComplete"/>
    public void NotifyComplete(string process, object result)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnComplete(process, result);
        }
    }

    /// <inheritdoc cref="IProcessSubject.NotifyError"/>
    public void NotifyError(string process, Exception error)
    {
        foreach (var observer in _processObservers)
        {
            observer.OnError(process, error);
        }
    }
}