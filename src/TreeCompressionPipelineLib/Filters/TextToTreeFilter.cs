using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

/// <summary>
/// Filtr pro převod textu na stromovou strukturu.
/// </summary>
/// <param name="creationStrategy">
/// Strategie pro vytvoření stromové struktury z textu.
/// </param>
/// <typeparam name="T">
/// Typ stromové struktury, která bude vytvořena.
/// </typeparam>
public class TextToTreeFilter<T>(ITreeCreationStrategy<T> creationStrategy) : FilterBase<string, T> where T : ITreeNode
{
    protected override T ProcessData(string text)
    {
        return creationStrategy.CreateTree(text);
    }

}