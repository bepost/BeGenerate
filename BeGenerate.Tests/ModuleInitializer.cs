using System.Runtime.CompilerServices;
using VerifyTests;

namespace BeGenerate.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
