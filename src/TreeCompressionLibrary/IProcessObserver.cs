namespace TreeCompressionPipeline;

/// <summary>
/// Definuje rozhraní pro observera, který sleduje průběh procesu.
/// </summary>
public interface IProcessObserver
{
   /// <summary>
   ///  Metoda, která je volána při spuštění procesu.
   /// </summary>
   /// <param name="process">
   ///  Název procesu, který je spuštěn.
   /// </param>
   public void OnStart(string process);
   
   /// <summary>
   /// Metoda, která je volána při průběhu procesu.
   /// </summary>
   /// <param name="process">
   /// Název procesu, který je sledován.
   /// </param>
   /// <param name="percentComplete">
   /// Procenta dokončení procesu.
   /// </param>
   [Obsolete("Metoda by měla sloužit pouze pro debugging a testování.")]
   public void OnProgress(string process, double percentComplete);
   
   /// <summary>
   /// Metoda, která je volána při dokončení procesu.
   /// </summary>
   /// <param name="process">
   ///  Název procesu, který byl dokončen.
   /// </param>
   /// <param name="result">
   ///   Výsledek procesu, který může být libovolného typu.
   /// </param>
   public void OnComplete(string process, object result);
   
   /// <summary>
   /// Metoda, která je volána při chybě v průběhu procesu.
   /// </summary>
   /// <param name="process">
   ///    Název procesu, ve kterém došlo k chybě.
   /// </param>
   /// <param name="error">
   ///   Vyjímka, která popisuje chybu v průběhu procesu.
   /// </param>
   public void OnError(string process, Exception error);
}
