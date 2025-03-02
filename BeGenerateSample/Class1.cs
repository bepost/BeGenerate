using BeGenerate.AutoInterface;

namespace BeGenerateSample;

[AutoInterface]
[Implements<IMiddleware>]
internal class SampleService : ISampleService
{
    [ExcludeFromInterface]
    public int Test { get; set; }

    public int Add(int a, int b)
    {
        return a + b;
    }

    public string GetMessage()
    {
        return "Hello from SampleService!";
    }
}

internal interface IMiddleware
{
}
