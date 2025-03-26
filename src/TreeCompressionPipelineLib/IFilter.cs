namespace TreeCompressionPipeline;

/// <summary>
/// Rozhraní pro vytvoření filtru v rámci pipeline.
/// Projekt využívá architekturu Pipes and Filters.
/// Filtr představuje jednotku zpracování dat.
/// Dědí z rozhraní IProcessSubject, které definuje metody pro přidání a odebrání observerů.
/// </summary>
public interface IFilter : IProcessSubject
{
    /// <summary>
    /// Metoda pro zpracování dat v rámci filtru.
    /// </summary>
    /// <param name="data">
    /// Přijatá data pro zpracování ve formě objektu, který může být libovolného typu.
    /// </param>
    /// <returns>
    /// Vrací zpracovaná data ve formě objektu, který může být libovolného typu.
    /// </returns>
    object Process(object data);
    
    /// <summary>
    ///  Metoda pro přidání dalšího filtru do řetězce filtrů pro zpracování dat.
    /// </summary>
    /// <param name="nextFilter">
    /// Další filtr, který bude následovat po aktuálním filtru.
    /// </param>
    /// <returns>
    /// Vrací odkaz na aktuální filtr, pro fluent chaining.
    /// </returns>
    IFilter Chain(IFilter nextFilter);
}
