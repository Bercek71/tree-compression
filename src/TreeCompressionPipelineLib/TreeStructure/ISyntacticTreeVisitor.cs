namespace TreeCompressionPipeline.TreeStructure;

public interface ISyntacticTreeVisitor
{
    void Visit(IDependencyTreeNode node);
}