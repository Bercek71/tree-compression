using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

/// <summary>
/// Jednoduchá implementace observera pro sledování průběhu procesu. Slouží hlavně pro debugging a testování.
/// </summary>
public class ProcessMonitor : IProcessObserver
{
    public void OnStart(string process) => Console.WriteLine($"Starting: {process}");
    public void OnProgress(string process, double percentComplete) => Console.WriteLine($"Progress: {process} {percentComplete}%");

    public void OnComplete(string process, object result)
    {
        Console.WriteLine($"Process complete: {process}, Result: \n {result.ToString()}");
    }
    public void OnError(string process, Exception error) => Console.WriteLine($"Error in {process}: {error.Message}");
}
