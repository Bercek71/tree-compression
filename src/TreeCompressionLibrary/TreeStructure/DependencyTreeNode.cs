using System.Text;

namespace TreeCompressionPipeline.TreeStructure;

public class DependencyTreeNode(string value) : IDependencyTreeNode
{
    // Základní vlastnosti
    public object Value { get; set; } = value;
    public List<IDependencyTreeNode> LeftChildren { get; } = [];
    public List<IDependencyTreeNode> RightChildren { get; } = [];
    public IDependencyTreeNode? Parent { get; set; }

    // Konstruktor

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var leftChild in LeftChildren)
        {
            stringBuilder.Append(leftChild.ToString());
            if (leftChild is DependencyTreeNode node)
            {
                var value = leftChild.Value?.ToString() ?? string.Empty;
                if (value == "." || value == "," || value == "\"")
                {
                    continue;
                }
            }
            stringBuilder.Append(' ');
        }

        var nodeValue = Value?.ToString() ?? string.Empty;
        if (nodeValue != "<DocumentRoot>" && nodeValue != "<root>")
        {
            stringBuilder.Append(nodeValue);
        }

        if (RightChildren.Count > 0)
        {
            stringBuilder.Append(' ');
        }

        foreach (var rightChild in RightChildren)
        {
            stringBuilder.Append(rightChild.ToString());
            stringBuilder.Append(' ');
        }

        return stringBuilder.ToString().TrimEnd();
    }

    public void AddLeftChild(IDependencyTreeNode child)
    {
        child.Parent = this;
        LeftChildren.Add(child);
    }

    public void AddRightChild(IDependencyTreeNode child)
    {
        child.Parent = this;
        RightChildren.Add(child);
    }

    public int GetNodeCount()
    {
        var count = 1 + LeftChildren.Sum(child => child.GetNodeCount()); // Počítáme aktuální uzel

        count += RightChildren.Sum(child => child.GetNodeCount());
        return count;
    }

    public void Accept(ISyntacticTreeVisitor visitor)
    {
        visitor.Visit(this);
    }

    // Optimalizovaná kompaktní serializace do bytového pole
    public static byte[] SerializeToBytes(IDependencyTreeNode node)
    {
        try
        {
            // Krok 1: Vytvořit slovník stringů pro deduplikaci
            var stringTable = new List<string>();
            
            // Krok 2: Serializujeme do memory streamu s deduplikací stringů
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Nejprve serializujeme slovník
            CollectStrings(node, stringTable);
            
            // Zapíšeme velikost slovníku
            writer.Write(stringTable.Count);
            
            // Zapíšeme všechny stringy ze slovníku
            foreach (var str in stringTable)
            {
                writer.Write(str);
            }
            
            // Serializujeme strom použitím indexů do slovníku
            SerializeNodeCompact(node, writer, stringTable);
            
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during serialization: {ex.Message}");
            return [];
        }
    }

    // Sbírá všechny stringy pro deduplikaci
    private static void CollectStrings(IDependencyTreeNode node, List<string> stringTable)
    {
        var value = node.Value?.ToString() ?? string.Empty;
        if (!stringTable.Contains(value))
        {
            stringTable.Add(value);
        }
        
        foreach (var child in node.LeftChildren)
        {
            CollectStrings(child, stringTable);
        }
        
        foreach (var child in node.RightChildren)
        {
            CollectStrings(child, stringTable);
        }
    }

    // Rekurzivní serializace uzlu a jeho potomků s kompaktní reprezentací
    private static void SerializeNodeCompact(IDependencyTreeNode node, BinaryWriter writer, List<string> stringTable)
    {
        // Zapíšeme index hodnoty ve slovníku místo celého stringu
        var value = node.Value?.ToString() ?? string.Empty;
        var valueIndex = stringTable.IndexOf(value);
        writer.Write((short)valueIndex);  // Použijeme short místo int pro větší úsporu
        
        // Optimalizace: Použijeme byte pro počet potomků, což stačí pro většinu stromů
        var leftCount = (byte)Math.Min(node.LeftChildren.Count, 255);
        var rightCount = (byte)Math.Min(node.RightChildren.Count, 255);
        
        writer.Write(leftCount);
        writer.Write(rightCount);
        
        // Pokud máme více než 255 potomků, zapíšeme to jako speciální případ
        if (node.LeftChildren.Count >= 255)
        {
            writer.Write(node.LeftChildren.Count);
        }
        
        if (node.RightChildren.Count >= 255)
        {
            writer.Write(node.RightChildren.Count);
        }
        
        // Serializujeme všechny levé potomky
        foreach (var child in node.LeftChildren)
        {
            SerializeNodeCompact(child, writer, stringTable);
        }
        
        // Serializujeme všechny pravé potomky
        foreach (var child in node.RightChildren)
        {
            SerializeNodeCompact(child, writer, stringTable);
        }
    }

    // Vlastní deserializace z bytového pole
    public static IDependencyTreeNode DeserializeFromBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return new DependencyTreeNode("<empty>");
        }

        try
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            
            // Načteme slovník stringů
            var stringCount = reader.ReadInt32();
            var stringTable = new List<string>(stringCount);
            
            for (var i = 0; i < stringCount; i++)
            {
                stringTable.Add(reader.ReadString());
            }
            
            // Deserializujeme strom použitím slovníku
            return DeserializeNodeCompact(reader, stringTable);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during deserialization: {ex.Message}");
            return new DependencyTreeNode("<e>");
        }
    }

    // Rekurzivní deserializace uzlu a jeho potomků
    private static IDependencyTreeNode DeserializeNodeCompact(BinaryReader reader, List<string> stringTable)
    {
        // Čteme index hodnoty ve slovníku
        int valueIndex = reader.ReadInt16();
        string value = (valueIndex >= 0 && valueIndex < stringTable.Count) 
            ? stringTable[valueIndex] 
            : string.Empty;
        
        var node = new DependencyTreeNode(value);
        
        // Čteme počet levých a pravých potomků
        var leftCount = reader.ReadByte();
        var rightCount = reader.ReadByte();
        
        // Pokud jsme měli více než 255 potomků, čteme skutečný počet
        int actualLeftCount = leftCount;
        if (leftCount == 255)
        {
            actualLeftCount = reader.ReadInt32();
        }
        
        int actualRightCount = rightCount;
        if (rightCount == 255)
        {
            actualRightCount = reader.ReadInt32();
        }
        
        // Deserializujeme levé potomky
        for (var i = 0; i < actualLeftCount; i++)
        {
            var child = DeserializeNodeCompact(reader, stringTable);
            node.AddLeftChild(child);
        }
        
        // Deserializujeme pravé potomky
        for (var i = 0; i < actualRightCount; i++)
        {
            var child = DeserializeNodeCompact(reader, stringTable);
            node.AddRightChild(child);
        }
        
        return node;
    }
}