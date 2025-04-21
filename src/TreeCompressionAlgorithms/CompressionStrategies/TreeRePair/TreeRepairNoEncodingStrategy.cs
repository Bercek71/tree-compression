using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair
{

    /// <summary>
    /// Occurrence of a digram in the tree
    /// </summary>
    public class DigramOccurrence
    {
        public IDependencyTreeNode Parent { get; }
        public IDependencyTreeNode Child { get; }
        public bool IsLeftChild { get; }

        public DigramOccurrence(IDependencyTreeNode parent, IDependencyTreeNode child, bool isLeftChild)
        {
            Parent = parent;
            Child = child;
            IsLeftChild = isLeftChild;
        }
    }

    /// <summary>
    /// Implementation of the TreeRePair algorithm for tree compression with protection against empty stack errors
    /// </summary>
    public class TreeRepairNoEncodingStrategy : ICompressionStrategy<IDependencyTreeNode>
    {
        private int _ruleCounter = 0;
        private readonly List<GrammarRule> _rules = new();
        private readonly Dictionary<Digram, List<DigramOccurrence>> _digramIndex = new();
        private readonly Dictionary<string, GrammarRule> _rulesByNonterminal = new();
        private IDependencyTreeNode _rootNode; // Uchováváme referenci na kořen stromu
        private const int MAX_ITERATIONS = 100; // Snížený bezpečnostní limit proti zacyklení
        private const int MAX_DECOMPRESS_DEPTH = 50; // Snížený limit hloubky rekurze při dekompresi

        // Sledujeme historii kompresních kroků
        private HashSet<string> _processedDigrams = new();

        public CompressedTree Compress(IDependencyTreeNode root)
        {
            try
            {
                // Reset internal state for new compression
                _ruleCounter = 0;
                _rules.Clear();
                _rulesByNonterminal.Clear();
                _processedDigrams.Clear();

                if (root == null)
                {
                    // Ochrana proti null vstupu
                    throw new ArgumentNullException(nameof(root), "Root node cannot be null");
                }

                // Create a deep copy of the tree to avoid modifying the original
                _rootNode = DeepCopyTree(root);

                // Build the initial digram index
                BuildDigramIndex(_rootNode);

                // Main compression loop with iteration counter
                bool compressionPerformed;
                int iterations = 0;
                int noProgressCounter = 0; // Počítadlo kroků bez pokroku

                do
                {
                    compressionPerformed = CompressionStep();
                    iterations++;

                    // Zastavit kompresi, pokud nedochází k pokroku
                    if (!compressionPerformed)
                    {
                        noProgressCounter++;
                        // Pokud jsme 3 kroky po sobě neudělali pokrok, končíme
                        if (noProgressCounter >= 3)
                        {
                            Console.WriteLine("Stopping compression - no progress made for 3 consecutive steps.");
                            break;
                        }
                    }
                    else
                    {
                        // Resetujeme počítadlo, pokud došlo k pokroku
                        noProgressCounter = 0;
                    }

                    // Bezpečnostní pojistka proti nekonečné smyčce
                    if (iterations > MAX_ITERATIONS)
                    {
                        Console.WriteLine(
                            $"Warning: Reached maximum iterations ({MAX_ITERATIONS}). Stopping compression.");
                        break;
                    }
                } while (compressionPerformed);

                // Očistíme pravidla, která nebyla použita při kompresi (případné neefektivní pravidla)
                CleanupUnusedRules();

                // Update the rule dictionary for potential future decompression
                _rulesByNonterminal.Clear();
                foreach (var rule in _rules)
                {
                    _rulesByNonterminal[rule.Nonterminal] = rule;
                }

                return new CompressedTree
                {
                    Structure = DependencyTreeNode.SerializeToBytes(_rootNode),
                    Metadata = _rules.ToDictionary(r => r.Nonterminal, r => r.Digram.ToString())
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Compression failed", ex);
            }
        }

        /// <summary>
        /// Cleans up unused rules that don't appear in the compressed tree
        /// </summary>
        private void CleanupUnusedRules()
        {
            // Find all nonterminals that are actually used in the tree
            var usedNonterminals = new HashSet<string>();
            CollectUsedNonterminals(_rootNode, usedNonterminals);

            // Filter out rules that aren't used
            _rules.RemoveAll(rule => !usedNonterminals.Contains(rule.Nonterminal));
        }

        /// <summary>
        /// Collects all nonterminals that are used in the tree
        /// </summary>
        private void CollectUsedNonterminals(IDependencyTreeNode node, HashSet<string> usedNonterminals)
        {
            if (node == null) return;

            // Check if this node is a nonterminal
            if (node.Value is string value && value.StartsWith("N") && int.TryParse(value.Substring(1), out _))
            {
                usedNonterminals.Add(value);
            }

            // Check left children
            foreach (var child in node.LeftChildren)
            {
                CollectUsedNonterminals(child, usedNonterminals);
            }

            // Check right children
            foreach (var child in node.RightChildren)
            {
                CollectUsedNonterminals(child, usedNonterminals);
            }
        }

        /// <summary>
        /// Performs a single compression step
        /// </summary>
        /// <returns>True if compression was performed, false otherwise</returns>
        private bool CompressionStep()
        {
            // Find the most frequent digram
            var mostFrequentDigram = FindMostFrequentDigram();

            // If no digram has frequency >= 2, we're done
            if (mostFrequentDigram == null || _digramIndex[mostFrequentDigram].Count < 2)
            {
                return false;
            }

            // Kontrola, zda jsme tento digram už nezpracovávali
            string digramKey = mostFrequentDigram.ToString();
            if (_processedDigrams.Contains(digramKey))
            {
                Console.WriteLine($"Warning: Digram {digramKey} was already processed. Skipping to avoid cycles.");
                return false;
            }

            // Přidáme digram do historie zpracovaných
            _processedDigrams.Add(digramKey);

            // Ukládáme stav před kompresí pro porovnání
            int currentDigramCount = CountTotalDigrams();

            // Create a new nonterminal
            string nonterminal = $"N{_ruleCounter++}";

            // Create a grammar rule
            var rule = new GrammarRule(nonterminal, mostFrequentDigram);
            _rules.Add(rule);

            // Replace all occurrences of the digram with the new nonterminal
            ReplaceDigramOccurrences(mostFrequentDigram, nonterminal);

            // Rebuild the digram index from the root node
            _digramIndex.Clear();
            BuildDigramIndex(_rootNode);

            // Kontrola, zda došlo ke skutečné komprimaci
            int newDigramCount = CountTotalDigrams();
            if (newDigramCount >= currentDigramCount)
            {
                Console.WriteLine(
                    $"Warning: Compression step did not reduce digram count: {currentDigramCount} -> {newDigramCount}");
                // Vrátíme false, abychom ukončili kompresi, pokud není efektivní
                return false;
            }

            return true;
        }

        /// <summary>
        /// Counts the total number of digrams in the index
        /// </summary>
        private int CountTotalDigrams()
        {
            return _digramIndex.Values.Sum(list => list.Count);
        }

        /// <summary>
        /// Creates a deep copy of the tree
        /// </summary>
        private IDependencyTreeNode DeepCopyTree(IDependencyTreeNode node)
        {
            if (node == null) return null;

            // Create a new node with the same value
            var newNode = new DependencyTreeNode((string)node.Value);

            // Copy left children
            foreach (var child in node.LeftChildren)
            {
                var childCopy = DeepCopyTree(child);
                newNode.AddLeftChild(childCopy);
            }

            // Copy right children
            foreach (var child in node.RightChildren)
            {
                var childCopy = DeepCopyTree(child);
                newNode.AddRightChild(childCopy);
            }

            return newNode;
        }

        /// <summary>
        /// Builds the initial digram index for the tree
        /// </summary>
        private void BuildDigramIndex(IDependencyTreeNode root)
        {
            if (root == null) return;

            _digramIndex.Clear();
            CollectDigrams(root);
        }

        /// <summary>
        /// Collects all digrams in the tree and adds them to the index
        /// </summary>
        private void CollectDigrams(IDependencyTreeNode node)
        {
            if (node == null) return;

            // Process left children
            foreach (var child in node.LeftChildren)
            {
                // Add the parent-child digram to the index
                AddDigramToIndex(node, child, true);

                // Recursively process the child's digrams
                CollectDigrams(child);
            }

            // Process right children
            foreach (var child in node.RightChildren)
            {
                // Add the parent-child digram to the index
                AddDigramToIndex(node, child, false);

                // Recursively process the child's digrams
                CollectDigrams(child);
            }
        }

        /// <summary>
        /// Adds a digram to the index
        /// </summary>
        private void AddDigramToIndex(IDependencyTreeNode parent, IDependencyTreeNode child, bool isLeftChild)
        {
            if (parent == null || child == null) return;

            var digram = new Digram(parent, child, isLeftChild);
            var occurrence = new DigramOccurrence(parent, child, isLeftChild);

            if (!_digramIndex.ContainsKey(digram))
            {
                _digramIndex[digram] = new List<DigramOccurrence>();
            }

            _digramIndex[digram].Add(occurrence);
        }

        /// <summary>
        /// Finds the most frequent digram in the tree
        /// </summary>
        private Digram? FindMostFrequentDigram()
        {
            Digram? mostFrequentDigram = null;
            int maxFrequency = 1; // We need at least 2 occurrences

            foreach (var entry in _digramIndex)
            {
                if (entry.Value.Count > maxFrequency)
                {
                    maxFrequency = entry.Value.Count;
                    mostFrequentDigram = entry.Key;
                }
            }

            return mostFrequentDigram;
        }

        /// <summary>
        /// Replaces all occurrences of a digram with a new nonterminal
        /// </summary>
        private void ReplaceDigramOccurrences(Digram digram, string nonterminal)
        {
            if (!_digramIndex.ContainsKey(digram)) return;

            var occurrences = _digramIndex[digram].ToList(); // Create a copy to avoid modification issues
            int replacedCount = 0;

            foreach (var occurrence in occurrences)
            {
                if (ReplaceDigram(occurrence, nonterminal))
                {
                    replacedCount++;
                }
            }

            if (replacedCount < 2)
            {
                Console.WriteLine(
                    $"Warning: Only replaced {replacedCount} occurrences of digram {digram}. Expected at least 2.");
            }
        }

        /// <summary>
        /// Replaces a single occurrence of a digram with a new nonterminal
        /// </summary>
        /// <returns>True if replacement was successful</returns>
        private bool ReplaceDigram(DigramOccurrence occurrence, string nonterminal)
        {
            if (occurrence == null) return false;

            var parent = occurrence.Parent;
            var child = occurrence.Child;
            bool isLeftChild = occurrence.IsLeftChild;

            if (parent == null || child == null) return false;

            // Kontrola, zda uzly stále existují ve stromu
            if (parent.Parent == null && parent != _rootNode)
            {
                Console.WriteLine(
                    "Warning: Trying to replace a digram with a parent that is no longer in the tree");
                return false;
            }

            // Create a new node for the nonterminal
            var newNode = new DependencyTreeNode(nonterminal);

            try
            {
                if (isLeftChild)
                {
                    // Kontrola, zda dítě stále existuje v seznamu levých dětí
                    if (!parent.LeftChildren.Contains(child))
                    {
                        Console.WriteLine("Warning: Child node is no longer in parent's left children list");
                        return false;
                    }

                    // Remove the child from the parent's left children
                    parent.LeftChildren.Remove(child);

                    // Přesunutí všech dětí původního dítěte do nového neterminálu
                    foreach (var grandChild in child.LeftChildren.ToList())
                    {
                        child.LeftChildren.Remove(grandChild);
                        newNode.AddLeftChild(grandChild);
                    }

                    foreach (var grandChild in child.RightChildren.ToList())
                    {
                        child.RightChildren.Remove(grandChild);
                        newNode.AddRightChild(grandChild);
                    }

                    // Add the new nonterminal node as a left child of the parent
                    parent.AddLeftChild(newNode);
                }
                else
                {
                    // Kontrola, zda dítě stále existuje v seznamu pravých dětí
                    if (!parent.RightChildren.Contains(child))
                    {
                        Console.WriteLine("Warning: Child node is no longer in parent's right children list");
                        return false;
                    }

                    // Remove the child from the parent's right children
                    parent.RightChildren.Remove(child);

                    // Přesunutí všech dětí původního dítěte do nového neterminálu
                    foreach (var grandChild in child.LeftChildren.ToList())
                    {
                        child.LeftChildren.Remove(grandChild);
                        newNode.AddLeftChild(grandChild);
                    }

                    foreach (var grandChild in child.RightChildren.ToList())
                    {
                        child.RightChildren.Remove(grandChild);
                        newNode.AddRightChild(grandChild);
                    }

                    // Add the new nonterminal node as a right child of the parent
                    parent.AddRightChild(newNode);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error replacing digram: {ex.Message}");
                return false;
            }
        }

        public IDependencyTreeNode Decompress(CompressedTree compressedTree)
        {
            try
            {
                if (compressedTree == null)
                {
                    throw new ArgumentNullException(nameof(compressedTree), "Compressed tree cannot be null");
                }

                if (compressedTree.Structure == null || compressedTree.Structure.Length == 0)
                {
                    throw new ArgumentException("Compressed tree structure is empty", nameof(compressedTree));
                }

                // Kontrola, zda vůbec byla provedena komprese - pokud nejsou metadata, vrátíme původní strom
                if (compressedTree.Metadata == null || compressedTree.Metadata.Count == 0)
                {
                    Console.WriteLine("No compression metadata found - returning original tree");
                    return DependencyTreeNode.DeserializeFromBytes(compressedTree.Structure);
                }

                _rulesByNonterminal.Clear();

                // Bezpečné parsování pravidel
                foreach (var rule in compressedTree.Metadata)
                {
                    try
                    {
                        _rulesByNonterminal[rule.Key] = new GrammarRule(rule.Key, Digram.FromString(rule.Value));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing rule {rule.Key}: {ex.Message}. Skipping.");
                    }
                }

                // Kontrola cyklických závislostí v pravidlech
                if (HasCyclicDependencies())
                {
                    Console.WriteLine(
                        "Warning: Detected cyclic dependencies in grammar rules. This may cause issues during decompression.");
                }

                // Create a deep copy of the compressed tree to avoid modifying it
                IDependencyTreeNode treeCopy =
                    DeepCopyTree(DependencyTreeNode.DeserializeFromBytes(compressedTree.Structure));

                if (treeCopy == null)
                {
                    throw new InvalidOperationException("Failed to deserialize compressed tree");
                }

                // Decompress the tree by expanding nonterminals
                DecompressNode(treeCopy, new HashSet<string>(), 0);

                return treeCopy;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during decompression: {ex.Message}");
                // V případě selhání dekomprese se pokusíme vrátit alespoň původní komprimovaný strom
                try
                {
                    return DependencyTreeNode.DeserializeFromBytes(compressedTree.Structure);
                }
                catch
                {
                    // Pokud selže i deserializace, vytvoříme prázdný kořenový uzel
                    Console.WriteLine("Failed to deserialize original tree. Creating empty root node.");
                    return new DependencyTreeNode("<root>");
                }
            }
        }

        /// <summary>
        /// Checks if grammar rules have cyclic dependencies
        /// </summary>
        private bool HasCyclicDependencies()
        {
            // Pro každý neterminál vytvoříme seznam neterminálů, které používá
            Dictionary<string, HashSet<string>> dependencies = new Dictionary<string, HashSet<string>>();

            foreach (var rule in _rulesByNonterminal.Values)
            {
                dependencies[rule.Nonterminal] = new HashSet<string>();

                // Zjistíme, zda pravidlo odkazuje na nějaký neterminál v digramu
                CheckForNonterminals(rule.Digram.Parent, dependencies[rule.Nonterminal]);
                CheckForNonterminals(rule.Digram.Child, dependencies[rule.Nonterminal]);
            }

            // Hledání cyklů pomocí algoritmu hledání cyklů v grafu
            foreach (var nonterm in dependencies.Keys)
            {
                HashSet<string> visited = new HashSet<string>();
                HashSet<string> recStack = new HashSet<string>();

                if (IsCyclicUtil(nonterm, visited, recStack, dependencies))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Utility method for cycle detection using DFS
        /// </summary>
        private bool IsCyclicUtil(string current, HashSet<string> visited, HashSet<string> recStack,
            Dictionary<string, HashSet<string>> dependencies)
        {
            // Ochrana proti null
            if (current == null) return false;

            // Označíme jako navštívený a přidáme do rekurzního zásobníku
            visited.Add(current);
            recStack.Add(current);

            // Zkontrolujeme všechny závislosti
            if (dependencies.ContainsKey(current))
            {
                foreach (var dep in dependencies[current])
                {
                    // Ochrana proti null
                    if (dep == null) continue;

                    // Pokud jsme ještě nenavštívili tento neterminál
                    if (!visited.Contains(dep))
                    {
                        if (IsCyclicUtil(dep, visited, recStack, dependencies))
                        {
                            return true;
                        }
                    }
                    // Pokud je neterminál v rekurzním zásobníku, našli jsme cyklus
                    else if (recStack.Contains(dep))
                    {
                        return true;
                    }
                }
            }

            // Odebereme ze zásobníku, abychom mohli najít další cykly
            recStack.Remove(current);
            return false;
        }

        /// <summary>
        /// Checks if a node contains nonterminal values
        /// </summary>
        private void CheckForNonterminals(IDependencyTreeNode node, HashSet<string> dependencies)
        {
            if (node == null) return;

            if (node.Value is string value && value.StartsWith("N") && int.TryParse(value.Substring(1), out _))
            {
                dependencies.Add(value);
            }
        }

        /// <summary>
        /// Decompresses a node and all its children by expanding nonterminals
        /// </summary>
        private void DecompressNode(IDependencyTreeNode node, HashSet<string> expandingNonterminals, int depth)
        {
            if (node == null) return;

            // Ochrana proti příliš hluboké rekurzi
            if (depth > MAX_DECOMPRESS_DEPTH)
            {
                Console.WriteLine(
                    $"Warning: Reached maximum decompression depth ({MAX_DECOMPRESS_DEPTH}). Stopping decompression.");
                return;
            }

            // First, check if the current node is a nonterminal
            string nodeValue = node.Value as string;
            if (nodeValue != null && nodeValue.StartsWith("N") && int.TryParse(nodeValue.Substring(1), out _))
            {
                // Kontrola cyklické závislosti
                if (expandingNonterminals.Contains(nodeValue))
                {
                    Console.WriteLine(
                        $"Warning: Detected cyclic dependency for nonterminal {nodeValue}. Skipping expansion.");
                    return;
                }

                // Přidání neterminálu do sady expandovaných
                expandingNonterminals.Add(nodeValue);

                try
                {
                    // This is a nonterminal, expand it
                    ExpandNonterminal(node);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error expanding nonterminal {nodeValue}: {ex.Message}");
                }
                finally
                {
                    // Odstranění neterminálu ze sady po expanzi (i v případě chyby)
                    expandingNonterminals.Remove(nodeValue);
                }
            }

            // Process left children (in a copy to avoid modification issues)
            var leftChildren = node.LeftChildren?.ToList() ?? new List<IDependencyTreeNode>();
            foreach (var child in leftChildren)
            {
                if (child != null)
                {
                    DecompressNode(child, new HashSet<string>(expandingNonterminals), depth + 1);
                }
            }

            // Process right children (in a copy to avoid modification issues)
            var rightChildren = node.RightChildren?.ToList() ?? new List<IDependencyTreeNode>();
            foreach (var child in rightChildren)
            {
                if (child != null)
                {
                    DecompressNode(child, new HashSet<string>(expandingNonterminals), depth + 1);
                }
            }
        }

        /// <summary>
        /// Expands a nonterminal node according to its grammar rule
        /// </summary>
        private void ExpandNonterminal(IDependencyTreeNode node)
        {
            if (node == null) return;

            string nonterminal = (string)node.Value;

            // If this is not a nonterminal or we don't have a rule for it, return
            if (!_rulesByNonterminal.TryGetValue(nonterminal, out var rule))
            {
                Console.WriteLine($"Warning: No rule found for nonterminal {nonterminal}");
                return;
            }

            // Get the digram from the rule
            var digram = rule.Digram;
            if (digram == null || digram.Parent == null || digram.Child == null)
            {
                Console.WriteLine($"Warning: Invalid digram for nonterminal {nonterminal}");
                return;
            }

            // Create nodes representing the parent and child from the digram
            var parentNode = new DependencyTreeNode((string)digram.Parent.Value);
            var childNode = new DependencyTreeNode((string)digram.Child.Value);

            // Connect them according to the digram structure
            if (digram.IsLeftChild)
            {
                parentNode.AddLeftChild(childNode);
            }
            else
            {
                parentNode.AddRightChild(childNode);
            }

            // Replace the nonterminal node with the expanded structure
            ReplaceNodeWithExpanded(node, parentNode);
        }

        /// <summary>
        /// Replaces a nonterminal node with its expanded structure in the tree
        /// </summary>
        private void ReplaceNodeWithExpanded(IDependencyTreeNode nonterminalNode, IDependencyTreeNode expandedNode)
        {
            if (nonterminalNode == null || expandedNode == null) return;

            // Get the parent of the nonterminal node
            var parent = nonterminalNode.Parent;
            if (parent == null)
            {
                // This is the root node, speciální případ
                if (nonterminalNode == _rootNode)
                {
                    // Kopírování dětí expandedNode do _rootNode
                    _rootNode.Value = expandedNode.Value;

                    // Přesunout děti z expandedNode do _rootNode
                    foreach (var child in expandedNode.LeftChildren.ToList())
                    {
                        expandedNode.LeftChildren.Remove(child);
                        _rootNode.AddLeftChild(child);
                    }

                    foreach (var child in expandedNode.RightChildren.ToList())
                    {
                        expandedNode.RightChildren.Remove(child);
                        _rootNode.AddRightChild(child);
                    }

                    return;
                }

                // This is the root node, can't replace
                Console.WriteLine("Warning: Cannot replace root node during expansion");
                return;
            }

            try
            {
                // Determine if the nonterminal is a left or right child
                bool isLeftChild = parent.LeftChildren.Contains(nonterminalNode);

                // Nejprve přesuneme všechny děti z neterminálního uzlu do listového uzlu expandované struktury
                var leafNode = FindLeafNode(expandedNode);

                if (leafNode != null)
                {
                    // Přesunout levé děti
                    foreach (var child in nonterminalNode.LeftChildren.ToList())
                    {
                        nonterminalNode.LeftChildren.Remove(child);
                        leafNode.AddLeftChild(child);
                    }

                    // Přesunout pravé děti
                    foreach (var child in nonterminalNode.RightChildren.ToList())
                    {
                        nonterminalNode.RightChildren.Remove(child);
                        leafNode.AddRightChild(child);
                    }
                }

                // Remove the nonterminal from the parent
                if (isLeftChild)
                {
                    parent.LeftChildren.Remove(nonterminalNode);
                    parent.AddLeftChild(expandedNode);
                }
                else
                {
                    parent.RightChildren.Remove(nonterminalNode);
                    parent.AddRightChild(expandedNode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error replacing node: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds a leaf node in the expanded structure
        /// </summary>
        private IDependencyTreeNode FindLeafNode(IDependencyTreeNode node)
        {
            if (node == null) return null;

            // Prohledáme strom do hloubky a najdeme list
            if (node.LeftChildren.Count == 0 && node.RightChildren.Count == 0)
            {
                return node;
            }

            // Preferujeme nejprve pravé potomky
            if (node.RightChildren.Count > 0)
            {
                return FindLeafNode(node.RightChildren.Last());
            }

            // Pokud neexistují pravé děti, použijeme levé
            if (node.LeftChildren.Count > 0)
            {
                return FindLeafNode(node.LeftChildren.Last());
            }

            // Fallback - teoreticky bychom sem neměli dojít, ale pro jistotu vrátíme původní uzel
            return node;
        }
    }
}