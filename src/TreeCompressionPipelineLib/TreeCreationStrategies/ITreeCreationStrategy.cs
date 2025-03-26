using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.TreeCreationStrategies;

/// <summary>
/// Strategie pro vytvoření stromové struktury z textu.
/// </summary>
/// <typeparam name="T">
/// Typ stromové struktury, která bude vytvořena.
/// </typeparam>
public interface ITreeCreationStrategy<out T> where T : ITreeNode
{
    /// <summary>
    /// Metoda pro vytvoření stromové struktury z textu.
    /// </summary>
    /// <param name="text">
    /// Text, který by se měl převést do stromové struktury.
    /// </param>
    /// <returns>
    /// Vrací vytvořenou stromovou strukturu daného typu.
    /// </returns>
    public T CreateTree(string text);
}