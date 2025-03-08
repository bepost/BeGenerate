namespace BeGenerate.AutoInterface;

/// <summary> The accessibility of a generated interface. </summary>
public enum InterfaceAccessibility
{
    /// <summary> The interface is generated as public. </summary>
    Public = 0,

    /// <summary> The interface is generated as internal. </summary>
    Internal,

    /// <summary> The interface is not given an accessibility. You can leverage the partial to assign it externally. </summary>
    None
}
