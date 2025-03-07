using System;
using System.Collections;
using BeGenerate.AutoInterface;

namespace BeGenerateSample;

[AutoInterface]
[Implements<IEnumerable>]
internal class SampleService : ISampleService
{
    [ExcludeFromInterface]
    public int Test { get; set; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int Add(int a, int b)
    {
        return a + b;
    }

    public string GetMessage()
    {
        return "Hello from SampleService!";
    }
}
