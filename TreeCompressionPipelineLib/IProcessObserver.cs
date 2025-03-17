namespace TreeCompressionPipeline;

// Observer Pattern
public interface IProcessObserver
{
   protected internal void OnStart(string process);
   protected internal void OnProgress(string process, double percentComplete);
   protected internal void OnComplete(string process, object result);
   protected internal void OnError(string process, Exception error);
}
