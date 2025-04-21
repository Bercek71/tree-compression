using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

/// <summary>
/// Implementation of the TreeRePair algorithm for tree compression
/// </summary>
///
/// <summary>
/// Represents a digram - a pair of adjacent nodes in the tree
/// </summary>
public class Digram : IEquatable<Digram>
{
    public IDependencyTreeNode Parent { get; }
    public IDependencyTreeNode Child { get; }
    public bool IsLeftChild { get; }

    public Digram(IDependencyTreeNode parent, IDependencyTreeNode child, bool isLeftChild)
    {
        Parent = parent;
        Child = child;
        IsLeftChild = isLeftChild;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Digram);
    }


    public bool Equals(Digram? other)
    {
        if (other == null) return false;

        // Two digrams are equal if they have equivalent structure and node values
        bool parentsEqual = Parent.Value.Equals(other.Parent.Value);
        bool childrenEqual = Child.Value.Equals(other.Child.Value);
        bool directionEqual = IsLeftChild == other.IsLeftChild;

        return parentsEqual && childrenEqual && directionEqual;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Parent.Value, Child.Value, IsLeftChild);
    }

    public override string ToString()
    {
        return $"({Parent.Value}, {Child.Value}, {(IsLeftChild ? "Left" : "Right")})";
    }


    public static Digram FromString(string str)
    {
        var parts = str.Trim('(', ')').Split(',');
        if (parts.Length != 3)
            throw new FormatException("Invalid digram format");

        var parent = new DependencyTreeNode(parts[0].Trim());
        var child = new DependencyTreeNode(parts[1].Trim());
        bool isLeftChild = parts[2].Trim() == "Left";

        return new Digram(parent, child, isLeftChild);
    }

    public static byte[] SerializeToBytes(Digram digram)
    {
        // Get the string values
        string parentValue = (string) digram.Parent.Value;
        string childValue = (string) digram.Child.Value;
    
        // Convert strings to UTF-8 bytes
        byte[] parentBytes = System.Text.Encoding.UTF8.GetBytes(parentValue);
        byte[] childBytes = System.Text.Encoding.UTF8.GetBytes(childValue);
    
        // Format: 
        // [1 byte] Parent length
        // [n bytes] Parent value
        // [1 byte] Child length
        // [n bytes] Child value
        // [1 byte] IsLeftChild flag (1=left, 0=right)
    
        // Calculate total size
        int totalSize = 1 + parentBytes.Length + 1 + childBytes.Length + 1;
    
        // Create result buffer
        byte[] result = new byte[totalSize];
        int position = 0;
    
        // Write parent length and value
        result[position++] = (byte)parentBytes.Length;
        Array.Copy(parentBytes, 0, result, position, parentBytes.Length);
        position += parentBytes.Length;
    
        // Write child length and value
        result[position++] = (byte)childBytes.Length;
        Array.Copy(childBytes, 0, result, position, childBytes.Length);
        position += childBytes.Length;
    
        // Write direction flag
        result[position] = digram.IsLeftChild ? (byte)1 : (byte)0;
    
        return result;
    }

    public static Digram DeserializeFromBytes(byte[] data)
    {
        int position = 0;
    
        // Read parent
        int parentLength = data[position++];
        string parentValue = System.Text.Encoding.UTF8.GetString(data, position, parentLength);
        position += parentLength;
    
        // Read child
        int childLength = data[position++];
        string childValue = System.Text.Encoding.UTF8.GetString(data, position, childLength);
        position += childLength;
    
        // Read direction flag
        bool isLeftChild = data[position] == 1;
    
        // Create nodes and digram
        var parent = new DependencyTreeNode(parentValue);
        var child = new DependencyTreeNode(childValue);
    
        return new Digram(parent, child, isLeftChild);
    }
}