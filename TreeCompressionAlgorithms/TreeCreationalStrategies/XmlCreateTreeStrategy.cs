using System.Xml;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.TreeCreationalStrategies;

public class XmlCreateTreeStrategy : ITreeCreationStrategy
{
    public ITreeNode CreateTree(string text)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(text);  // Load the XML string into an XmlDocument

        // Call a method to process the root element and create the tree structure
        if (xmlDoc.DocumentElement != null) return CreateTreeNodeFromXml(xmlDoc.DocumentElement);
        throw new ArgumentNullException(nameof(xmlDoc.DocumentElement));
    }

    private static ITreeNode CreateTreeNodeFromXml(XmlNode xmlNode)
    {
        // Create a new tree node for the current XML element
        ITreeNode node = new XmlTreeNode(xmlNode.Name, xmlNode.InnerText);
        if (node == null) throw new ArgumentNullException(nameof(node));


        // Recursively add child nodes for each child of the current XML element
        foreach (XmlNode childNode in xmlNode.ChildNodes)
        {
            var childTreeNode = CreateTreeNodeFromXml(childNode);
            node.AddChild(childTreeNode);  // Assuming ITreeNode has a method to add children
        }

        return node;
    }
}