namespace TreeCompressionPipeline;

public interface IFilter : IProcessSubject
{
    object Process(object data);
    IFilter Chain(IFilter nextFilter);
}
