namespace TreeCompressionPipeline.TreeStructure;

/// <summary>
/// Nejzákladnější rozhraní libovolného uzlu stromu.
/// </summary>
public interface ITreeNode
{
    /// <summary>
    /// Hodnota uzlu stromu, může být cokoliv.
    /// </summary>
    object Value { get; set; }
}
