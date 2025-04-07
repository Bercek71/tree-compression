#if EXPERIMENTAL
using System.ComponentModel;
using ConsoleApp.Framework;
using Ufal.MorphoDiTa;

namespace ConsoleApp.Commands;

[Description("English Morphodita Tagger")]
public class EnglishMorphoditaTagger : ICommand
{
    public void Execute()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), 
            "Resources",
            "English",
            "english-morphium-wsj-140407.tagger");
        var tagger = Tagger.load(filePath);
        const string sentence = "The quick brown fox, known for its remarkable agility and cunning nature, swiftly jumped over the lazy dog, which had been resting peacefully in the warm sunlight, completely unaware of the lively creature’s presence, while a gentle breeze rustled the golden autumn leaves that had fallen from the towering oak tree nearby, creating a mesmerizing dance of colors in the crisp afternoon air, as distant church bells chimed softly, marking the passage of time in a quaint little village where cobblestone streets wound their way through rows of charming cottages adorned with vibrant flowers, their delicate petals swaying rhythmically in harmony with the wind, as children laughed and played in the background, filling the scene with an atmosphere of pure joy and nostalgia, reminiscent of a simpler time when the world seemed to move at a slower, more deliberate pace, allowing people to appreciate the small yet beautiful moments that often go unnoticed in the hustle and bustle of modern life.";
        var words = sentence.Split(' ');
        var forms = new Forms(words);
        var tags = new TaggedLemmas();
        tagger.tag(forms, tags);

        //tagger.tagAnalyzed(forms ,analyses, new Indices(tags.Select(x => x.tag)));
        
        foreach (var tag in tags)
        {
            Console.WriteLine(tag.lemma + " " + tag.tag);
        }

    }
}
#endif