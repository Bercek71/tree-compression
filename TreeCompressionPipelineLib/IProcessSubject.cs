namespace TreeCompressionPipeline;

public interface IProcessSubject
{
    public void AddObserver(IProcessObserver observer);
    public void RemoveObserver(IProcessObserver observer);
    protected void NotifyStart(string process);
    protected void NotifyProgress(string process, double percentComplete);
    protected void NotifyComplete(string process, object result);
    protected void NotifyError(string process, Exception error);
}