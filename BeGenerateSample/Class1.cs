using BeGenerate;

namespace BeGenerateSample;

[AutoInterface]
internal class SampleService : ISampleService
{
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
