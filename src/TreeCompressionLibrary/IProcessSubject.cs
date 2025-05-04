namespace TreeCompressionPipeline;

/// <summary>
/// Rozhraní pro definici subjektu, díky kterému mohou observeři sledovat průběh procesu.
/// </summary>
public interface IProcessSubject
{
    /// <summary>
    /// Přidání observeru, který bude sledovat průběh procesu.
    /// </summary>
    /// <param name="observer">
    /// Observer, který bude sledovat průběh procesu.
    /// </param>
    public void AddObserver(IProcessObserver observer);
    
    /// <summary>
    /// Odebrání observeru, který sledoval průběh procesu.
    /// </summary>
    /// <param name="observer">
    /// Observer, který sledoval průběh procesu.
    /// </param>
    public void RemoveObserver(IProcessObserver observer);
    
    /// <summary>
    /// Metoda pro oznámení observerům o začátku procesu.
    /// </summary>
    /// <param name="process">
    /// Název procesu, který je spuštěn.
    /// </param>
    protected void NotifyStart(string process);

    /// <summary>
    /// Metoda by měla sloužit pouze pro debugging a testování průběhu procesu.
    /// </summary>
    /// <param name="process">
    /// Název procesu, který je sledován.
    /// </param>
    /// <param name="percentComplete">
    /// Procenta dokončení procesu.
    /// </param>
    [Obsolete("Metoda by měla sloužit pouze pro debugging a testování.")]
    protected void NotifyProgress(string process, double percentComplete);
    
    /// <summary>
    /// Metoda pro oznámení observerům o dokončení procesu.
    /// </summary>
    /// <param name="process">
    /// Název procesu, který byl dokončen.
    /// </param>
    /// <param name="result">
    /// Výsledek procesu, který může být libovolného typu.
    /// </param>
    protected void NotifyComplete(string process, object result);
    
    /// <summary>
    /// Metoda pro oznámení observerům o chybě v průběhu procesu.
    /// </summary>
    /// <param name="process">
    /// Název procesu, ve kterém došlo k chybě.
    /// </param>
    /// <param name="error">
    /// Vyjímka, která nastala v průběhu procesu.
    /// </param>
    protected void NotifyError(string process, Exception error);
}