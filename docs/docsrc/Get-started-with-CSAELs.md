# Get Started with CSAELs

This documentation only applies to the *bleeding-edge latest version* of ezr² RE, which you may
have to compile from the `ezrSquared-re` branch of the repository.

CSAELs, or **CS**harp **A**ssisted **E**zr² **L**ibraries, are class libraries written in C# meant to
be used by the ezr² runtime. With the latest versions of ezr² RE supporting automatic C# wrappers which
can wrap existing C# code not specifically meant for use with ezr², CSAELs are very easy to make if you
know C#.

Although ezr² RE does not yet support importing CSAELs at runtime, it can be added through custom
runtime implementations. See the ["Embedding ezr² page"](Embedding-ezrSquared.md) for more details.

Currently, the only version recommended for making CSAELs with is the latest version of ezr² RE,
available on [*NuGet*](https://www.nuget.org/packages/ezrSquared). The package is targeted for
.NET 9 and .NET Standard 2.1 and is compatible with Unity.

## High Level Usage

The [wrapper infrastructure](~/api/EzrSquared.Runtime.Types.Wrappers.yml) for ezr² already supports
a lot of C# features, so the attributes provided by the NuGet package are only for providing additional
metadata to the runtime or for using specific features like extra keyword arguments or extra positional
arguments (as [*params-modified*](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters#params-modifier)
arguments are not support).

### WrapMember Attribute

When a type or object is being wrapped, by default, all public members are wrapped. Their names as
defined in C# are also converted to snake_case. If you want to wrap private members, or provide
additional metadata like a unique name, you can decorate the member with [`WrapMemberAttribute`](~/api/EzrSquared.Runtime.Types.Wrappers.WrapMemberAttribute.yml).

`WrapMemberAttribute` can be used on the following member types:
- Classes
- Structs
- Constructors
- Methods
- Properties
- Fields

For properties and fields, you can also decorate them as read-only or write-only, without
changing their signature in C#.

### DontWrapMember Attribute

To exclude a public member from being wrapped, you can decorate it with [`DontWrapMemberAttribute`](~/api/EzrSquared.Runtime.Types.Wrappers.DontWrapMemberAttribute.yml).

## Wrapping Methods and Constructors

### FeatureParameter Attribute

When wrapping methods and constructors, you may want additional information from the ezr² runtime.
For example, if you want to create a new object, you will need a context and start and end positions.

To do so, you can use [`FeatureParameterAttribute`](~/api/EzrSquared.Runtime.Types.Wrappers.Members.Methods.FeatureParameterAttribute.yml).
You can define a two parameters, one for the context reference, and one for a reference to the caller
of your method. You can then take the caller's start and end positions for the new object.

```csharp
/// <summary>
/// Wraps the given ezr² object so that the runtime has access to its raw C# object.
/// </summary>
/// <param name="toGet">The object to wrap.</param>
/// <param name="wrapper">Reference to the caller.</param>
/// <param name="executionContext">Reference to the current execution context.</param>
/// <returns>The wrapped object.</returns>
[WrapMember]
public static EzrObjectWrapper GetRaw(IEzrObject toGet,
    [FeatureParameter(Feature.CallerRef)] IEzrObject wrapper,
    [FeatureParameter(Feature.ExecutionRef)] Context executionContext)
{
    return new EzrObjectWrapper(toGet, toGet.GetType(), executionContext, wrapper.StartPosition, wrapper.EndPosition);
}
```

The attribute can also be used to get references to the interpreter, the current `RuntimeResult` and
access to extra positional/keyword arguments. See [`Feature`](~/api/EzrSquared.Runtime.Types.Wrappers.Members.Methods.Feature.yml)
for more details.

### Parameter Attribute

If you want to provide a custom name for your wrapped method's parameters, you can use [ParameterAttribute](~/api/EzrSquared.Runtime.Types.Wrappers.Members.Methods.ParameterAttribute.yml).
You can also declare optional parameters this way. Optional parameters are required to be nullable.

### PrimaryConstructor Attribute

All constructors in a type to be wrapped are, by default, named either by the name provided in their
`WrapMemberAttribute`, or given the predefined name `make` or `make_[n]` where `[n]` starts from 1 and
increases every time another constructor without a given name is found. They are then stored in the context
of the `EzrTypeWrapper`. A single constructor, for each type, can be declared a "primary constructor", which
will be used when the `Execute` method is called on the type wrapper. The constructor must be declared with
a [PrimaryConstructorAttribute](~/api/EzrSquared.Runtime.Types.Wrappers.Members.Methods.PrimaryConstructorAttribute.yml).

## Example References

All runtime error types in ezr² are wrapped! Although the objects themselves are `IEzrObject`s, the type is
wrapped to provide the constructor at runtime. You can check out the source code for all runtime error types
[*here*](https://github.com/Uralstech/ezrSquared/tree/ezrSquared-re/src/Runtime/Types/Core/RuntimeErrors).

All built-in methods are also wrapped! You can check out their source code [*here*](https://github.com/Uralstech/ezrSquared/blob/ezrSquared-re/src/Runtime/Types/Builtins/EzrBuiltinFunctions.cs).