using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.Filters;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

/// <summary>
/// Továrna pro vytvoření filtrů v rámci pipeline.
/// </summary>
/// <typeparam name="T">
/// Typ uzlu stromu, který je zpracováván v rámci filtrů.
/// </typeparam>
public abstract class FilterFactory<T> where T : ITreeNode
{
    /// <summary>
    /// Vytoření filtru pro převod textu na stromovou strukturu.
    /// </summary>
    /// <param name="strategy">
    /// Algoritmus pro vytvoření stromové struktury z textu.
    /// </param>
    /// <returns>
    /// Filtr pro převod textu na stromovou strukturu.
    /// </returns>
    public static IFilter CreateTextToTreeFilter(ITreeCreationStrategy<T> strategy) => new TextToTreeFilter<T>(strategy);
    
    /// <summary>
    /// Vytvoření filtru pro kompresi stromové struktury.
    /// </summary>
    /// <param name="strategy">
    /// Algoritmus pro kompresi a dekompresi stromové struktury. Musí být shodný se strategií pro dekompresi
    /// </param>
    /// <returns>
    ///  Filtr pro kompresi stromové struktury.
    /// </returns>
    public static IFilter CreateCompressionFilter(ICompressionStrategy<T> strategy) => new CompressionFilter<T>(strategy);
    
    /// <summary>
    /// Vytvoření filtru pro dekompresi stromové struktury.
    /// </summary>
    /// <param name="strategy">
    ///  Algoritmus pro kompresi a dekompresi stromové struktury. Musí být shodný se strategií pro kompresi.
    /// </param>
    /// <returns>
    /// Filtr pro dekompresi stromové struktury.
    /// </returns>
    public static IFilter CreateDecompressionFilter(ICompressionStrategy<T> strategy) => new DecompressionFilter<T>(strategy);
}
