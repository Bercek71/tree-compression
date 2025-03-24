using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies
{
    public class DictionaryTreeCompressor : ICompressionStrategy<ISyntacticTreeNode>
    {
        private readonly Dictionary<string, string> _dictionary = new();
        private int _idCounter = 1;

        // Compressing the tree
        public CompressedTree Compress(ISyntacticTreeNode? tree)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            var compressedStructure = EncodeNode(tree);
            //TODO: Finish implementing the compression logic for all implemented algorithms
            throw new NotImplementedException();        
        }

        // Decompressing the tree
        public ISyntacticTreeNode Decompress(CompressedTree? compressedTree)
        {
            if (compressedTree == null) throw new ArgumentNullException(nameof(compressedTree));
            throw new NotImplementedException();
        }

        private string EncodeNode(ITreeNode node)
        {
            var structure = NodeToString(node);
            if (_dictionary.TryGetValue(structure, out var value)) return value;
            value = "D" + _idCounter++;
            _dictionary[structure] = value;
            return value;
        }

        private string NodeToString(ITreeNode node)
        {
            throw new     NotImplementedException();       
            // Assuming that the Children property returns a list of child nodes
           // return node.Value?.ToString() + "(" + string.Join(",", node.Children.Select(EncodeNode)) + ")";
        }
    }
}