#if EXPERIMENTAL
using System;
using System.IO;
using TreeStructures;
using TreeStructures.Compressors;
using Ufal.UDPipe;

namespace ConsoleApp.Commands
{
    public class UDPipeTest : BaseCommand
    {

        public override void Execute(object? parameter)
        {
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "English",
                "english-ewt-ud-2.5.udpipe"
            );

            var model = Model.load(modelPath);
            if (model == null)
            {
                Console.WriteLine("Error loading UDPipe model.");
                return;
            }

            var testingSent = "Despite the fact that the sun had long since dipped below the horizon, casting a blanket of darkness over the sleepy town, the air still hummed with the sound of distant conversations, the soft rustling of leaves caught in the gentle evening breeze, and the occasional bark of a dog from a nearby yard, as if the very essence of life, with all its complexities and intricacies, still managed to linger in the cool night, refusing to fade into the quiet that normally accompanied the setting of the day, and as I walked down the cobblestone street, each step echoing in the stillness, I couldn’t help but wonder about the endless number of stories that had unfolded in this very spot, each one adding to the rich tapestry of history that had shaped the town, its people, and the world beyond, where the intertwining of human experience, from the grand to the mundane, had created a vast web of connections, connections that, at times, seemed as fragile as a spider’s thread, yet were incredibly resilient, stretching across time and space, weaving through generations of families, cultures, and beliefs, leaving behind traces that would one day be discovered, analyzed, and perhaps even forgotten by those who came after us, but for now, in this fleeting moment, everything seemed so tangible, so alive, so full of potential, as if the universe itself had paused, holding its breath, waiting for the next chapter to begin.";

            // Step 1: Create tokenizer
            var tokenizer = model.newTokenizer(Model.DEFAULT);
            
            if (tokenizer == null)
            {
                Console.WriteLine("Error initializing tokenizer.");
                return;
            }

            tokenizer.setText(testingSent);

            var sentence = new Sentence();
            while (tokenizer.nextSentence(sentence))
            {
                model.tag(sentence, Model.DEFAULT);
                model.parse(sentence, Model.DEFAULT);

                PrintSentence(sentence);
            }
        }

        private void PrintSentence(Sentence sentence)
        {
            Console.WriteLine("Words: {0}", sentence.words.Count);

            foreach (var word in sentence.words)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                    word.id,
                    word.form,
                    word.lemma,
                    word.deprel,
                    word.head,
                    word.deps
                );
            }

            foreach (var sentenceComment in sentence.comments)
            {
                Console.WriteLine("# {0}", sentenceComment);
            }
            
            var rootWord = sentence.words[0];
            var tree = new Tree<string>(rootWord.form);

            var words = sentence.words;
            BuildTree(tree.Root, rootWord, words.ToList());

            tree.PrintTree();

            var compressor = new RePairTreeCompressor<string>();
            var compressedTree = compressor.CompressTree(tree.Root);
            
            Console.WriteLine("Compressed Tree: {0}", compressedTree);

            var treeNode = compressor.DecompressTree(compressedTree);

            var decompressedTree = new Tree<string>(rootWord.form)
            {
                Root = treeNode
            };
            decompressedTree.PrintTree();


//            compresor.ToBinaryFile("Test.bin");
        }

        private static void BuildTree(TreeNode<string> parentNode, Word parentWord , List<Word> words)
        {
            var children = words.Where(x => x.head == parentWord.id).ToList();

            foreach (var word in children)
            {
                var newNode = new TreeNode<string>(word.form);
                parentNode.Add(newNode, word.id > parentWord.id ? Direction.Right : Direction.Left);
                // Recursively add children of this node
                BuildTree(newNode, word,  words);
            }
            
        }
    }
}
#endif
