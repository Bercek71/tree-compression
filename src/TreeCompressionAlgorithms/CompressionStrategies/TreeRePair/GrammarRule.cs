namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

/// <summary>
/// Represents a grammar rule in the TreeRePair algorithm
/// </summary>
public class GrammarRule
{
    public string Nonterminal { get; }
    public Digram Digram { get; }

    public GrammarRule(string nonterminal, Digram digram)
    {
        Nonterminal = nonterminal;
        Digram = digram;
    }

    public override string ToString()
    {
        return $"{Nonterminal} → {Digram}";
    }

    public static GrammarRule FromString(string str)
    {
        var parts = str.Split(new[] { "→" }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new FormatException("Invalid grammar rule format");

        string nonterminal = parts[0].Trim();
        Digram digram = Digram.FromString(parts[1].Trim());

        return new GrammarRule(nonterminal, digram);
    }

    public static byte[] SerializeToBytes(GrammarRule rule)
    {
        // Convert nonterminal to UTF-8 bytes
        byte[] nonterminalBytes = System.Text.Encoding.UTF8.GetBytes(rule.Nonterminal);
    
        // Get the serialized digram bytes
        byte[] digramBytes = Digram.SerializeToBytes(rule.Digram);
    
        // Format:
        // [1 byte] Nonterminal length
        // [n bytes] Nonterminal value
        // [remaining bytes] Serialized digram
    
        // Create result buffer
        byte[] result = new byte[1 + nonterminalBytes.Length + digramBytes.Length];
        int position = 0;
    
        // Write nonterminal length and value
        result[position++] = (byte)nonterminalBytes.Length;
        Array.Copy(nonterminalBytes, 0, result, position, nonterminalBytes.Length);
        position += nonterminalBytes.Length;
    
        // Write digram data
        Array.Copy(digramBytes, 0, result, position, digramBytes.Length);
    
        return result;
    }

    public static GrammarRule DeserializeFromBytes(byte[] data)
    {
        int position = 0;
    
        // Read nonterminal
        int nonterminalLength = data[position++];
        string nonterminal = System.Text.Encoding.UTF8.GetString(data, position, nonterminalLength);
        position += nonterminalLength;
    
        // Extract digram bytes
        byte[] digramBytes = new byte[data.Length - position];
        Array.Copy(data, position, digramBytes, 0, digramBytes.Length);
    
        // Deserialize digram
        Digram digram = Digram.DeserializeFromBytes(digramBytes);
    
        return new GrammarRule(nonterminal, digram);
    }
}