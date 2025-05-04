using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

/// <summary>
/// Rozhraní, které definuje metody pro kompresi a dekompresi stromu.
/// </summary>
/// <typeparam name="T">
/// Typ stromu, který je komprimován a dekomprimován.
/// </typeparam>
public interface ITreeCompressor<T> where T : ITreeNode
{
    /// <summary>
    /// Kompresní strategie, která je použita pro kompresi stromu.
    /// </summary>
    protected ICompressionStrategy<T> CompressionStrategy { get; }
    
    /// <summary>
    /// Pipeline pro kompresi stromu.
    /// </summary>
    protected Pipeline CompressingPipeline { get; }
    
    /// <summary>
    /// Pipeline pro dekompresi stromu.
    /// </summary>
    protected Pipeline DecompressingPipeline { get; }
    
    /// <summary>
    /// Metoda pro kompresi stromu.
    /// </summary>
    /// <param name="text">
    /// Text, který by se měl převést do stromové struktury a komprimovat.
    /// </param>
    /// <returns></returns>
    CompressedTree Compress(string text);
    
    /// <summary>
    /// Metoda pro dekompresi stromu.
    /// </summary>
    /// <param name="compressedTree">
    /// Komprimovaný strom, který by se měl dekomprimovat.
    /// </param>
    /// <returns>
    /// Text, který byl dekomprimován ze stromové struktury.
    /// </returns>
    string Decompress(CompressedTree compressedTree);
}