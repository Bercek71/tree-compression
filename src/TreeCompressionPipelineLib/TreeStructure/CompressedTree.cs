using System.Text;

namespace TreeCompressionPipeline.TreeStructure;

/// <summary>
/// Datová struktura pro uložení komprimované stromové struktury.
/// </summary>
public class CompressedTree
{
    /// <summary>
    /// Binární reprezentace komprimované stromové struktury.
    /// </summary>
    public byte[] Structure { get; set; } = [];
    
    /// <summary>
    /// Typ stromové struktury, která byla komprimována.
    /// </summary>
    public Type TreeType { get; set; } = typeof(ITreeNode);
    
    public Type NodeValueType { get; set; } = typeof(string);
    
    /// <summary>
    /// Metadata komprimované stromové struktury. 
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
    
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("Compressed Tree:");
        sb.AppendLine("Metadata:");
        foreach (var (key, value) in Metadata)
        {
            sb.AppendLine($"  {key}: {value}");
        }
        sb.AppendLine($"Structure: {Structure.Length} bytes");
        return sb.ToString();
    }
}