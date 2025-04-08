namespace TreeCompressionPipeline.TreeStructure;

/// <summary>
/// Uzel syntaktického stromu.
/// </summary>
public interface IDependencyTreeNode : ITreeNode
{
    
    /// <summary>
    /// Leví potomci uzlu, uspořádaní zleva doprava.
    /// </summary>
    List<IDependencyTreeNode> LeftChildren { get; }
    
    /// <summary>
    /// Praví potomci uzlu, uspořádaní zleva doprava.
    /// </summary>
    List<IDependencyTreeNode> RightChildren { get; }
    
    /// <summary>
    /// Přidání potomka do levého podstromu.
    /// </summary>
    /// <param name="child">
    /// potomek, který bude přidán do levého podstromu.
    /// </param>
    void AddLeftChild(IDependencyTreeNode child);
    
    /// <summary>
    /// Přidání potomka do pravého podstromu.
    /// </summary>
    /// <param name="child">
    /// Potomek, který bude přidán do pravého podstromu.
    /// </param>
    void AddRightChild(IDependencyTreeNode child);
    
    void Accept(ISyntacticTreeVisitor visitor);
    
    /// <summary>
    /// Rodič uzlu.
    /// </summary>
    IDependencyTreeNode? Parent { get; set; }
    
}