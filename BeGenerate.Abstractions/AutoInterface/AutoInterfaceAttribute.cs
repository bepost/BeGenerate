using System;

namespace BeGenerate.AutoInterface;

/// <summary> Annotate a class with this attribute to generate an interface with the same name. </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AutoInterfaceAttribute : Attribute
{
    /// <summary> Initializes a new instance of the <see cref="AutoInterfaceAttribute" /> class. </summary>
    public AutoInterfaceAttribute()
    {
    }

    /// <summary> The accessibility of the generated interface. </summary>
    public InterfaceAccessibility Accessibility { get; set; }

    /// <summary> The name of the generated interface. Default is the same name of the class with an `I` as prefix. </summary>
    public string? Name { get; set; }
}
