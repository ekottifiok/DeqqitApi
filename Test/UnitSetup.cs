namespace Test;

public class UnitSetup : IAsyncLifetime
{
    public required string Message { get; set; }

    public Task InitializeAsync()
    {
        Console.WriteLine("Initializing...");
        Message = "Hello";
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Console.WriteLine("Disposed.");
        return Task.CompletedTask;
    }
}