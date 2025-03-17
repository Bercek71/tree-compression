using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

public interface ITreeCompressor
{
    
    protected ICompressionStrategy CompressionStrategy { get; }
    protected Pipeline CompressingPipeline { get; }
    protected Pipeline DecompressingPipeline { get; }
    
    CompressedTree Compress(string text);
    string Decompress(CompressedTree compressedTree);
}