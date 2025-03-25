using System.Text;

namespace TreeCompressionPipeline.TreeStructure;

public class CompressedTree
{
    public byte[] Structure { get; set; }
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