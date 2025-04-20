using TreeCompressionPipeline.TreeStructure;
using System.Collections;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

/// <summary>
/// Most compact possible encoding: Bit-packed Balanced Parentheses + preorder values.
/// </summary>
public class BitPackedBPEncoder : ITreeRePairEncoder
{
    public IEnumerable<string> EncodeTree(IDependencyTreeNode root)
    {
        var bits = new List<bool>();
        var values = new List<string>();
        Encode(root, bits, values);

        // Convert bit list to base64 string to store compactly
        byte[] packedBits = PackBits(bits);
        string structureBase64 = System.Convert.ToBase64String(packedBits);

        // Return base64-encoded structure followed by values
        return new[] { structureBase64 }.Concat(values);
    }

    private void Encode(IDependencyTreeNode node, List<bool> bits, List<string> values)
    {
        bits.Add(true); // '('
        values.Add(node.Value.ToString() ?? "");

        foreach (var child in node.LeftChildren)
            Encode(child, bits, values);

        foreach (var child in node.RightChildren)
            Encode(child, bits, values);

        bits.Add(false); // ')'
    }

    public IDependencyTreeNode DecodeTree(IEnumerable<string> sequence)
    {
        var list = sequence.ToList();
        if (list.Count == 0) return null;

        // Decode structure from base64
        var packedBits = System.Convert.FromBase64String(list[0]);
        var bitArray = new BitArray(packedBits);
        var bits = bitArray.Cast<bool>().ToList(); // convert to list<bool> for indexing

        var bitIndex = 0;
        var valIndex = 1; // starts after structure
        return Decode(bits, ref bitIndex, list, ref valIndex);
    }

    private IDependencyTreeNode Decode(List<bool> bits, ref int bitIndex, List<string> values, ref int valIndex)
    {
        if (!bits[bitIndex++]) throw new InvalidDataException("Expected '('");

        var node = new DependencyTreeNode(values[valIndex++]);

        // Decode children until we hit ')'
        while (bitIndex < bits.Count && bits[bitIndex])
        {
            var child = Decode(bits, ref bitIndex, values, ref valIndex);
            node.LeftChildren.Add(child); // default to left; you can adapt this
        }

        bitIndex++; // skip ')'
        return node;
    }

    // Packs list of bits into byte array
    private byte[] PackBits(List<bool> bits)
    {
        int byteCount = (bits.Count + 7) / 8;
        byte[] bytes = new byte[byteCount];
        for (int i = 0; i < bits.Count; i++)
        {
            if (bits[i])
                bytes[i / 8] |= (byte)(1 << (7 - (i % 8)));
        }
        return bytes;
    }
}