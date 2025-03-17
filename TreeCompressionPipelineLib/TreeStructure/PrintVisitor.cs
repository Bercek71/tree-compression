namespace TreeCompressionPipeline.TreeStructure
{
    public class PrintVisitor : ITreeVisitor
    {
        private int _indentationLevel = 0;
        private readonly HashSet<ITreeNode> _visitedNodes = [];

        public void Visit(ITreeNode node)
        {
            // Prevent revisiting the same node (in case of a cyclic structure)
            if (!_visitedNodes.Add(node))
                return;

            // Mark the node as visited

            // Print the node value with indentation
            Console.WriteLine($"{new string(' ', _indentationLevel * 2)}└─ {node.Value}");

            // Process all child nodes
            if (!node.Children.Any()) return;
            _indentationLevel++;
            foreach (var child in node.Children)
            {
                child.Accept(this); // Visit the child node
            }
            _indentationLevel--; // Decrease the indentation level after processing all children
        }
    }
}