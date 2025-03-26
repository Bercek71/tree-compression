using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.CompressionStrategies;

/// <summary>
/// Kompresní strategie pro kompresi a dekompresi stromové struktury.
/// </summary>
/// <typeparam name="T">
/// Typ stromové struktury, která je komprimována a dekomprimována.
/// </typeparam>
public interface ICompressionStrategy<T>
{
    /// <summary>
    /// Komprimuje stromovou strukturu.
    /// </summary>
    /// <param name="tree">
    /// Strom pro kompresi.
    /// </param>
    /// <returns>
    /// Komprimovaný strom.
    /// </returns>
    CompressedTree Compress(T tree);
    
    /// <summary>
    /// Dekomprimuje stromovou strukturu.
    /// </summary>
    /// <param name="compressedTree">
    /// Dekomprimovaný strom.
    /// </param>
    /// <returns>
    /// Původní stromová struktura.
    /// </returns>
    T Decompress(CompressedTree compressedTree);
}
