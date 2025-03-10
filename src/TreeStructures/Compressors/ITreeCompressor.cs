namespace TreeStructures.Compressors;

public interface ITreeCompressor<T>
{
    string CompressTree(TreeNode<T> node);
    TreeNode<T> DecompressTree(string compressedTree);
}