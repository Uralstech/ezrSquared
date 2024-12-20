# Embedding ezr²

This documentation only applies to the *bleeding-edge latest version* of ezr² RE, which you may
have to compile from the `ezrSquared-re` branch of the repository.

Currently, the only version recommended for embedding in your C# apps is
the latest version of ezr² RE, available on [*NuGet*](https://www.nuget.org/packages/ezrSquared).

The NuGet package is targeted for .NET 9 and .NET Standard 2.1 and is compatible with Unity.
You may need to use third-party packages like [*NuGetForUnity*](https://github.com/GlitchEnzo/NuGetForUnity)
to import it into Unity.

## CodeExecutor.cs

The high-level class to interact with the ezr² runtime is the [`CodeExecutor`](~/api/EzrSquared.Executor.CodeExecutor.yml) class.

### Setup

All you have to do is call `CodeExecutor.CreateRuntimeContext("main");`. "main" is just the name for the runtime context.

If you want to include built-in constants (like `true`, `false`, `nothing`),
functions (like `throw_error`, `assert`) or types (like `runtime_error`, `math_error`)
in the context, just call `CodeExecutor.PopulateRuntimeContext()`.

If you are not making a console app, or want control of the IO, you can choose to not include the built-in IO
functions (`show`, `get` and `clear`), you can set `excludeIO` to `true`: `CodeExecutor.PopulateRuntimeContext(true)`.

`CodeExecutor` also provides methods to add objects to the context. For example, you can add
a reference to a custom `show` function, which uses Unity's UI instead of `Console.WriteLine`.
You can check out the [*"Get Started with CSAELS" page*](Get-started-with-CSAELs.md) to know
more about wrappers.

```csharp
public TMP_Text OutputText;

private void Start()
{
    CodeExecutor.CreateRuntimeContext("main");
    CodeExecutor.PopulateRuntimeContext(true);

    CodeExecutor.AddToContext(new EzrMethodWrapper((Action<string>)ShowOnCanvas, Context.Empty, Position.None, Position.None));
}

[WrapMember("show")]
private async void ShowOnCanvas(string text)
{
    await Awaitable.MainThreadAsync(); // Switch into Unity's main thread to modify the UI.
    OutputText.text += $"\n{text}";

    await Awaitable.BackgroundThreadAsync(); // Switch back to the background thread, so that ezr² does not block Unity.
}
```

### Interpreting Code

Just call `CodeExecutor.Execute` with the code you want to interpret! It returns a
[`ExecutionResult`](~/api/EzrSquared.Executor.ExecutionResult.yml) object, which contains info
about the execution, like the AST, parse errors, runtime errors, and the actual result.

So, for example, you can run a script asynchronously in Unity like so:

```csharp
private async Awaitable RunScript(string code)
{
    ShowOnCanvas($">>> {code}");

    await Awaitable.BackgroundThreadAsync();
    ExecutionResult executionResult = CodeExecutor.Execute(code);

    if (executionResult.Success)
        ShowOnCanvas(executionResult.Result.ToString(CodeExecutor.RuntimeResult));
    else if (executionResult.Result is EzrRuntimeError)
        ShowOnCanvas($"<color=#FF0000>{executionResult.Result.ToPureString(CodeExecutor.RuntimeResult)}</color>");
    else 
        ShowOnCanvas($"<color=#FF0000>{executionResult.LexerError ?? executionResult.ParseError}</color>");
}
```

### Unity Shell Example

This is an example script for making a simple interpreter shell in Unity.

```csharp
using EzrSquared;
using EzrSquared.Executor;
using EzrSquared.Runtime;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Runtime.Types.Wrappers.Members.Methods;
using System;
using TMPro;
using UnityEngine;

public class Runner : MonoBehaviour
{
    [Tooltip("Text to show interpreter output.")]
    public TMP_Text OutputText;

    [Tooltip("Input field for getting the user's code.")]
    public TMP_InputField InputField;

    private void Start()
    {
        // Initialize the runtime context.
        CodeExecutor.CreateRuntimeContext("main");

        // Add built-ins, excluding IO functions.
        CodeExecutor.PopulateRuntimeContext(true);

        // Add wrapper for "show" function which uses Unity UI. 
        CodeExecutor.AddToContext(new EzrMethodWrapper((Action<string>)ShowOnCanvas, Context.Empty, Position.None, Position.None));
    }

    // This function will be called by a "Submit" or "Run" button.
    public async void Run()
    {
        await RunScript(InputField.text);
    }

    // Function to show text on the screen.
    [WrapMember("show")]
    private async void ShowOnCanvas(string text)
    {
        await Awaitable.MainThreadAsync(); // Switch into Unity's main thread to modify the UI.
        OutputText.text += $"\n{text}";

        await Awaitable.BackgroundThreadAsync(); // Switch back to the background thread, so that ezr² does not block Unity.
    }

    // Function to run ezr² code in a background thread.
    private async Awaitable RunScript(string code)
    {
        ShowOnCanvas($">>> {code}");

        await Awaitable.BackgroundThreadAsync();
        ExecutionResult executionResult = CodeExecutor.Execute(code);

        if (executionResult.Success)
            ShowOnCanvas(executionResult.Result.ToString(CodeExecutor.RuntimeResult));
        else if (executionResult.Result is EzrRuntimeError)
            ShowOnCanvas($"<color=#FF0000>{executionResult.Result.ToPureString(CodeExecutor.RuntimeResult)}</color>");
        else 
            ShowOnCanvas($"<color=#FF0000>{executionResult.LexerError ?? executionResult.ParseError}</color>");
    }
}
```

## Lower-Level Access

You can directly use the `Lexer`, `Parser` and `Interpreter` classes directly to execute code. You can even inherit from them to
customize the tokenization, parsing and interpretation of the ezr² language. This requires knowledge of the ezr² language
infrastructure, but provides much more customizability. You can start by cloning the ezr² repository and exploring the API!
