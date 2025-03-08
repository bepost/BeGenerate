# BeGenerate

[![GitHub License](https://img.shields.io/github/license/bepost/BeGenerate)](LICENSE)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/bepost/BeGenerate/build.yaml)](https://github.com/bepost/BeGenerate/actions)
[![NuGet Version](https://img.shields.io/nuget/v/BeGenerate)](https://www.nuget.org/packages/BeGenerate)

This repo is the home for BeGenerate, a .NET Code Generator that simplifies the process of creating boilerplate code.
The current feature set is still limited as we're at a pre-release stage, but we're working hard to add more features
and
improve the existing ones.

## Usage

Add the BeGenerate package to your project:

```bash
dotnet add package BeGenerate
```

This will add the BeGenerate code generator to your project, and will add the required abstractions to your project.

### AutoInterface

The AutoInterface feature allows you to automatically generate interfaces for your classes. This is useful when you have
a class that you want to extract an interface from, but you don't want to do it manually.

To use the AutoInterface feature, you need to add the `AutoInterface` attribute to your class:

```csharp
using BeGenerate.AutoInterface;

[AutoInterface]
internal sealed class MyService : IMyService
{
    public void MyMethod()
    {
        // Do something
    }
    
    void IMyService.MyExplicitMethod()
    {
        // Do something
    }
}
```

When you build your project, the code generator will automatically generate an interface for your class:

```csharp
public partial interface IMyService
{
    void MyMethod();
    void MyExplicitMethod();
}
```

You can leave out methods and properties by adding the `ExcludeFromInterface` attribute to them:

```csharp
using BeGenerate.AutoInterface;

[AutoInterface]
internal sealed class MyService : IMyService
{
    public void MyMethod()
    {
        // Do something
    }
    
    [ExcludeFromInterface]
    public void MyMethodToExclude()
    {
        // Do something
    }
}
```

If the generated interface should implement another interface, you can specify it using the `Implements<T>` attribute:

```csharp
[AutoInterface]
[Implements<IOtherInterface>]
internal sealed class MyService : IMyService
...
```

When you build your project, the code generator will automatically generate an interface for your class:

```csharp
public partial interface IMyService : IOtherInterface 
...
```

You can give your interface a custom name by specifying it in the `Name` argument of the `AutoInterface` attribute:

```csharp
[AutoInterface(Name = "IMyCustomService")]
internal sealed class MyService : IMyCustomService
...
```

If you don't want your interface to be public, you can also alter the visibility of the generated interface:

```csharp
[AutoInterface(Accessibility = InterfaceAccessibility.Internal)]
internal sealed class MyService : IMyCustomService
...
```

Or even remove th access modifier entirely, so you can define it yourself using the partial keyword:

```csharp
[AutoInterface(Accessibility = InterfaceAccessibility.None)]
internal sealed class MyService : IMyCustomService;

public partial interface IMyCustomService;
...
```

## Contributing

BeGenerate is a community project. We invite your participation through issues and pull requests!

## License

All assets and code are under the MIT LICENSE and in the public domain unless specified otherwise.