using BeGenerate;

namespace BeGenerateSample;

[AutoInterface]
public class SampleService
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public string GetMessage()
    {
        return "Hello from SampleService!";
    }
}
