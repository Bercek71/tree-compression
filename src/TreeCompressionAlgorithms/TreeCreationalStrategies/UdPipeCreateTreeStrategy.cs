using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;
using Ufal.UDPipe;

namespace TreeCompressionAlgorithms.TreeCreationalStrategies;

/// <summary>
/// Využití UDPipe pro vytvoření stromové struktury.
/// </summary>
public class UdPipeCreateTreeStrategy : ITreeCreationStrategy<ISyntacticTreeNode>
{
    
    private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "Resources",
        "English",
        "english-ewt-ud-2.5.udpipe"
    );
    
    public ISyntacticTreeNode CreateTree(string data)
    {
            var model = Model.load(_modelPath);
            if (model == null)
            {
                throw new Exception("Error loading UDPipe model.");
            }

            var tokenizer = model.newTokenizer(Model.DEFAULT);
            
            if (tokenizer == null)
            {
                throw new Exception("Error initializing tokenizer.");
            }

            tokenizer.setText(data);

            var sentence = new Sentence();
            var docTree = new SyntacticTreeNode("<DocumentRoot>");
            while (tokenizer.nextSentence(sentence))
            {
                model.tag(sentence, Model.DEFAULT);
                model.parse(sentence, Model.DEFAULT);
                var rootWord = sentence.words[0];
                var tree = new SyntacticTreeNode(rootWord.form);

                var words = sentence.words;

                BuildTree(tree, rootWord, words.ToList());
                
                docTree.AddRightChild(tree);
            }
            return docTree;
    }

    private static void BuildTree(ISyntacticTreeNode parentNode, Word parentWord , List<Word> words)
    {
        var children = words.Where(x => x.head == parentWord.id).ToList();

        foreach (var word in children)
        {
            var newNode = new SyntacticTreeNode(word.form);
            if(word.id > parentWord.id)
                parentNode.AddRightChild(newNode);
            else
                parentNode.AddLeftChild(newNode);
            BuildTree(newNode, word,  words);
        }
            
    }


}