using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.CompressionStrategies;

public interface ICompressionStrategy<T>
{
    CompressedTree Compress(T? tree);
    T Decompress(CompressedTree compressedTree);
}
