using EzrSquared.Runtime;
using EzrSquared.Runtime.Types.CSharpWrappers.Builtins;
using EzrSquared.Syntax;
using EzrSquared.Syntax.Errors;
using System;
using System.Collections.Generic;

namespace EzrSquared.Executor;

/// <summary>
/// Utility to execute ezr² code under one static interpreter.
/// </summary>
public static class CodeExecutor
{
    /// <summary>
    /// The static interpreter.
    /// </summary>
    public static readonly Interpreter Interpreter = new();

    /// <summary>
    /// The runtime context, may be null.
    /// </summary>
    private static Context? s_runtimeContext;

    /// <summary>
    /// Creates the runtime context.
    /// </summary>
    /// <param name="filePath">The file path to the code file this context will be used to execute.</param>
    public static void CreateRuntimeContext(string filePath)
    {
        s_runtimeContext = new Context($"<\"{filePath}\" context>", true, new Position(0, 0, filePath, string.Empty));
    }

    /// <summary>
    /// Populates the runtime context with all built-ins.
    /// </summary>
    /// <exception cref="NullReferenceException">
    /// Thrown if the runtime context is <see langword="null"/>.
    /// Use <see cref="CreateRuntimeContext(string)"/> to initialize the runtime context.
    /// </exception>
    public static void PopulateRuntimeContext()
    {
        if (s_runtimeContext is null)
            throw new NullReferenceException($"{nameof(s_runtimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        EzrBuiltinsUtility.AddBuiltinConstants(s_runtimeContext);
        EzrBuiltinsUtility.AddBuiltinFunctions(s_runtimeContext);
        EzrBuiltinsUtility.AddBuiltinTypes(s_runtimeContext);
    }

    /// <summary>
    /// Adds the given reference to the runtime context.
    /// </summary>
    /// <exception cref="NullReferenceException">
    /// Thrown if the runtime context is <see langword="null"/>.
    /// Use <see cref="CreateRuntimeContext(string)"/> to initialize the runtime context.
    /// </exception>
    /// <param name="reference">The reference to add.</param>
    /// <param name="name">The name of the new symbol.</param>
    public static void AddToContext(Reference reference, string name)
    {
        if (s_runtimeContext is null)
            throw new NullReferenceException($"{nameof(s_runtimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        s_runtimeContext.Set(null, name, reference);
    }

    /// <summary>
    /// Executes the given script.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">
    /// Thrown if the runtime context is <see langword="null"/>.
    /// Use <see cref="CreateRuntimeContext(string)"/> to initialize the runtime context.
    /// </exception>
    public static ExecutionResult Execute(string script)
    {
        if (s_runtimeContext is null)
            throw new NullReferenceException($"{nameof(s_runtimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        // Tokenize the script.
        SyntaxError? lexerError = new Lexer(s_runtimeContext.StartPosition.File, script).Tokenize(out List<Token> tokens);
        if (lexerError is not null) // Check for errors.
            return new ExecutionResult([.. tokens], lexerError); // Return tokens with lexing error.

        // Parse the tokens.
        ParseResult parseResult = new Parser(tokens).Parse();
        if (parseResult.Error is not null) // Check for errors.
            return new ExecutionResult([.. tokens], parseResult); // Return the tokens, with the parse error.

        // Interpret the Abstract Syntax Tree.
        RuntimeResult result = Interpreter.Execute(parseResult.Node, s_runtimeContext);

        // Return result.
        return new ExecutionResult([.. tokens], parseResult.Node, result);
    }
}
