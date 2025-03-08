using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BeGenerate.AutoInterface;

namespace BeGenerateSample;

[AutoInterface(Name = "Test")]
[Implements<IEnumerable>]
internal class SampleService : Test
{
    [DoesNotReturn]
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
