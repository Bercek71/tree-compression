using System;
using System.IO;
using Ufal.UDPipe;

namespace TreeCompressionMain.Commands
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

            var testingSent = "This is a super long sentence that is going to be parsed by UDPipe, which is going to be totally awesome.";

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
        }
    }
}