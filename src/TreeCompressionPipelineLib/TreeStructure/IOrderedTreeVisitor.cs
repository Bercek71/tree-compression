namespace TreeCompressionPipeline.TreeStructure;

/// <summary>
/// Visitor pro zpracování uzlu stromu.
/// </summary>
public interface IOrderedTreeVisitor
{
    
    void Visit(IOrderedTreeNode node);
}
