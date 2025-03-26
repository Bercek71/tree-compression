namespace TreeCompressionPipeline.TreeStructure;

/// <summary>
/// Uzel syntaktického stromu.
/// </summary>
public interface ISyntacticTreeNode : ITreeNode
{
    
    /// <summary>
    /// Leví potomci uzlu, uspořádaní zleva doprava.
    /// </summary>
    List<ISyntacticTreeNode> LeftChildren { get; }
    
    /// <summary>
    /// Praví potomci uzlu, uspořádaní zleva doprava.
    /// </summary>
    List<ISyntacticTreeNode> RightChildren { get; }
    
    /// <summary>
    /// Přidání potomka do levého podstromu.
    /// </summary>
    /// <param name="child">
    /// potomek, který bude přidán do levého podstromu.
    /// </param>
    void AddLeftChild(ISyntacticTreeNode child);
    
    /// <summary>
    /// Přidání potomka do pravého podstromu.
    /// </summary>
    /// <param name="child">
    /// Potomek, který bude přidán do pravého podstromu.
    /// </param>
    void AddRightChild(ISyntacticTreeNode child);
    
    void Accept(ISyntacticTreeVisitor visitor);
    
    /// <summary>
    /// Rodič uzlu.
    /// </summary>
    ISyntacticTreeNode? Parent { get; set; }
    
}