namespace ConsoleApp.Utils;

public class StopWatch
{
    private DateTime _start = DateTime.Now;
    private bool _isRunning = false;


    public void Start()
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Stopwatch is already running.");
        }
        
        _start = DateTime.Now;
        _isRunning = true;
    }
    
    public TimeSpan Stop()
    {
        if (!_isRunning)
        {
            throw new InvalidOperationException("Stopwatch is not running.");
        }
        
        var elapsed = DateTime.Now.Subtract(_start);
        _isRunning = false;
        return elapsed;
    }
    
}