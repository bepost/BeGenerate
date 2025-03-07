using System;

namespace BeGenerate.AutoInterface;

/// <summary> Annotate a class member with this attribute to exclude a member from the generated interface. </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class ExcludeFromInterfaceAttribute : Attribute
{
}
