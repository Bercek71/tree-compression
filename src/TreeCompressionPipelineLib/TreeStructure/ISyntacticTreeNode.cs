namespace TreeCompressionPipeline.TreeStructure;

public interface ISyntacticTreeNode : ITreeNode
{
    List<ISyntacticTreeNode> LeftChildren { get; }
    List<ISyntacticTreeNode> RightChildren { get; }
    
    void AddLeftChild(ISyntacticTreeNode child);
    void AddRightChild(ISyntacticTreeNode child);
    
    void Accept(ISyntacticTreeVisitor visitor);
    
    ISyntacticTreeNode? Parent { get; set; }
    
}