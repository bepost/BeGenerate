using System;

namespace BeGenerate.AutoInterface;

/// <summary>
///     Annotate a class with as <see cref="AutoInterfaceAttribute" />  to add a specific interface to make the
///     generated interface derive from.
/// </summary>
/// <typeparam name="T"> The interface to derive from. </typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ImplementsAttribute<T> : Attribute
{
}
