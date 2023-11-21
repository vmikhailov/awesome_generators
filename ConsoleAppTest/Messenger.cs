namespace ConsoleAppTest;

internal static partial class Messenger
{
    public static void Say(string message)
    {
        HelloFrom($"Generated Code Says: {message}");
    }

    static partial void HelloFrom(string name);
}

internal partial class TestClass
{
    public void Say(string msg)
    {
        SayIt("You", msg);    
    }
    
    partial void SayIt(string from, string msg);
}


public interface ITestInterface
{
    
}

public interface ITestInterface2
{
    
}