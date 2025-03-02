using System;

namespace BeGenerate.AutoInterface;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ImplementsAttribute<T> : Attribute
{
}
