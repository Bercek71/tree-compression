namespace TreeCompressionPipeline.TreeStructure;

/// <summary>
/// Rozhraní stromové struktury, která definuje základní metody pro klasický jakýsi uspořádaný strom.
/// </summary>
public interface IOrderedTreeNode : ITreeNode
{
    /// <summary>
    /// Potomci uzlu.
    /// </summary>
    List<IOrderedTreeNode> Children { get; }
    
    /// <summary>
    /// Rodič uzlu.
    /// </summary>
    IOrderedTreeNode? Parent { get; set; }
    
    /// <summary>
    /// Přidání potomka do uzlu.
    /// </summary>
    /// <param name="child">
    /// Potomek, který bude přidán do uzlu.
    /// </param>
    void AddChild(IOrderedTreeNode child);
    
    /// <summary>
    /// Přijímání visitora pro zpracování uzlu.
    /// </summary>
    /// <param name="visitor">
    ///  Visitor, který bude zpracovávat uzel.
    /// </param>
    /// <seealso cref="IOrderedTreeVisitor"/>
    void Accept(IOrderedTreeVisitor visitor);
}