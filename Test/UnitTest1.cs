using Xunit.Abstractions;

namespace Test;

public class UnitTest1 : IClassFixture<UnitSetup>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly UnitSetup _unitSetup;

    public UnitTest1(UnitSetup unitSetup, ITestOutputHelper testOutputHelper)
    {
        _unitSetup = unitSetup;
        _testOutputHelper = testOutputHelper;

        Console.WriteLine("Creating Deck Service");
        _testOutputHelper.WriteLine($"Message from test constructor: {_unitSetup.Message}");
    }

    [Fact]
    public void Test1()
    {
        _testOutputHelper.WriteLine($"Message from test 1: {_unitSetup.Message}");
        Assert.Equal(1, 1);
    }

    [Fact]
    public void Test2()
    {
        _testOutputHelper.WriteLine($"Message from test 2: {_unitSetup.Message}");
        Assert.Equal(1, 1);
    }
}