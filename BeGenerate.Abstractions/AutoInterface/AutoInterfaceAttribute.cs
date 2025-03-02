using System;

namespace BeGenerate.AutoInterface;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AutoInterfaceAttribute : Attribute
{
    public Type[] Implements { get; set; } = [];
}
