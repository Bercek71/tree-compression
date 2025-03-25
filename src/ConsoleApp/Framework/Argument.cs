namespace ConsoleApp.Framework;

public class Argument
{
    public string Shortcut { get; }
    public string Description { get; init; }
    public bool IsRequired { get; init; }
    public string? Value { get; set; }
    public string Name { get; init; }
}