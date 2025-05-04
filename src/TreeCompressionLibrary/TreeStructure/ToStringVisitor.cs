using System.Text;

namespace TreeCompressionPipeline.TreeStructure
{
    public class ToStringVisitor : IOrderedTreeVisitor
    {
        private int _indentationLevel = 0;
        private readonly HashSet<IOrderedTreeNode> _visitedNodes = [];
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        public void Visit(IOrderedTreeNode node)
        {
            // Prevent revisiting the same node (in case of a cyclic structure)
            if (!_visitedNodes.Add(node as IOrderedTreeNode ?? throw new InvalidOperationException()))
                return;

            // Mark the node as visited

            _stringBuilder.AppendLine($"{new string(' ', _indentationLevel * 2)}└─ {node.Value}");

            // Process all child nodes
            if (node.Children.Count == 0) return;
            _indentationLevel++;
            foreach (var child in node.Children)
            {
                child.Accept(this); // Visit the child node
            }
            _indentationLevel--; // Decrease the indentation level after processing all children
        }
    }
}