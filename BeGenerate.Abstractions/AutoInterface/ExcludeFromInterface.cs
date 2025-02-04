using System;

namespace BeGenerate.AutoInterface;

[AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class ExcludeFromInterfaceAttribute : Attribute
{
}
