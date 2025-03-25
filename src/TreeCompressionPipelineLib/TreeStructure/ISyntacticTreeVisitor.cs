namespace TreeCompressionPipeline.TreeStructure;

public interface ISyntacticTreeVisitor
{
    void Visit(ISyntacticTreeNode node);
}