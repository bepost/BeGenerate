using JetBrains.Annotations;

namespace BeGenerate.AutoInterface;

/// <summary> The accessibility of a generated interface. </summary>
[PublicAPI]
public enum InterfaceAccessibility
{
    /// <summary> The interface is generated as public. </summary>
    Public = 0,

    /// <summary> The interface is generated as internal. </summary>
    Internal
}
