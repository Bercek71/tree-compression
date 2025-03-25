namespace TreeCompressionPipeline.TreeStructure;

public interface IOrderedTreeVisitor
{
    void Visit(IOrderedTreeNode node);
}
