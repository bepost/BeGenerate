using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BeGenerate.AutoInterface;

namespace BeGenerateSample;

[AutoInterface(Name = "Test")]
[Implements<IEnumerable>]
internal class SampleService : Test
{
    [Obsolete]
    public string Message { [Obsolete("get")] get; [Obsolete("get")] set; }

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

    [DoesNotReturn]
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
