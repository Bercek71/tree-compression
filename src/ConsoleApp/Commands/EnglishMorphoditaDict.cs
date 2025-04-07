#if ENVIRONMENT
using System.ComponentModel;
using System.Windows.Input;
using Ufal.MorphoDiTa;
using ICommand = ConsoleApp.Framework.ICommand;

namespace ConsoleApp.Commands;

[Description("English Morphodita dictionary command")]
public class EnglishMorphoditaDict : ICommand
{
    public void Execute()
    {
        var dictPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ,"Resources", "English" ,  "english-morphium-140407.dict");
        var morpho = Morpho.load(dictPath);
        
        const string sentence = "The quick brown fox, known for its remarkable agility and cunning nature, swiftly jumped over the lazy dog, which had been resting peacefully in the warm sunlight, completely unaware of the lively creature’s presence, while a gentle breeze rustled the golden autumn leaves that had fallen from the towering oak tree nearby, creating a mesmerizing dance of colors in the crisp afternoon air, as distant church bells chimed softly, marking the passage of time in a quaint little village where cobblestone streets wound their way through rows of charming cottages adorned with vibrant flowers, their delicate petals swaying rhythmically in harmony with the wind, as children laughed and played in the background, filling the scene with an atmosphere of pure joy and nostalgia, reminiscent of a simpler time when the world seemed to move at a slower, more deliberate pace, allowing people to appreciate the small yet beautiful moments that often go unnoticed in the hustle and bustle of modern life.";

        var taggedLemmas = new TaggedLemmas();
        
        foreach (var word in sentence.Split(' '))
        {
            morpho.analyze(word, Morpho.GUESSER, taggedLemmas);
            foreach (var taggedLemma in taggedLemmas)
            {
                Console.WriteLine(taggedLemma.lemma + " " + taggedLemma.tag);
            }
        }
    }
}
#endif