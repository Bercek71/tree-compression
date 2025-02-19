
using MorphoDita;
using Ufal.MorphoDiTa;

namespace TreeCompressionMain;

public class Program
{
    public static void Main(string[] args)
    {
        MorphoDitaLoader.LoadNativeLibrary();
        //get tagger from resources
        

        
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), 
            "Resources",
            "English",
            "english-morphium-wsj-140407.tagger");
        var tagger = Tagger.load(filePath);
        var sentence = "This is a test sentence.";
        var words = sentence.Split(' ');
        var forms = new Forms(words);
        var tags = new TaggedLemmas();
        tagger.tag(forms, tags);

        foreach (var tag in tags)
        {
            Console.WriteLine(tag.lemma + " " + tag.tag);
        }

    }
    
}