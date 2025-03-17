using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;
using Ufal.UDPipe;

namespace TreeCompressionAlgorithms.TreeCreationalStrategies;

public class UdPipeCreateTreeStrategy : ITreeCreationStrategy
{
    public ITreeNode CreateTree(string data)
    {
             var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "English",
                "english-ewt-ud-2.5.udpipe"
            );

            var model = Model.load(modelPath);
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
            tokenizer.nextSentence(sentence);
            model.tag(sentence, Model.DEFAULT);
            model.parse(sentence, Model.DEFAULT);
            var rootWord = sentence.words[0];
            var tree = new TreeNode(rootWord.form);

            var words = sentence.words;
            
            BuildTree(tree, rootWord, words.ToList());
            
            
            return tree;
                
            

    }
    
    private static void BuildTree(ITreeNode parentNode, Word parentWord , List<Word> words)
    {
        var children = words.Where(x => x.head == parentWord.id).ToList();

        foreach (var word in children)
        {
            var newNode = new TreeNode(word.form);
            parentNode.AddChild(newNode);
            // Recursively add children of this node
            BuildTree(newNode, word,  words);
        }
    }


}