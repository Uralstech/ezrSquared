using EzrSquared.Runtime;
using EzrSquared.Runtime.Types.Builtins;
using EzrSquared.Runtime.Types.Wrappers;
using EzrSquared.Syntax;
using EzrSquared.Syntax.Errors;
using System;
using System.Collections.Generic;
using System.Reflection;

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
    /// The static <see cref="Runtime.RuntimeResult"/> of <see cref="Interpreter"/>.
    /// </summary>
    public static RuntimeResult RuntimeResult => Interpreter.RuntimeResult;

    /// <summary>
    /// The runtime context, may be <see langword="null"/>.
    /// </summary>
    public static Context? RuntimeContext { get; private set; }

    /// <summary>
    /// Creates the runtime context.
    /// </summary>
    /// <param name="name">The name of the context.</param>
    public static void CreateRuntimeContext(string name)
    {
        RuntimeContext?.Release();
        RuntimeContext = new Context(name, true, new Position(0, 0, name, string.Empty));
    }

    /// <summary>
    /// Populates the runtime context with all built-ins.
    /// </summary>
    /// <exception cref="NullReferenceException">
    /// Thrown if the runtime context is <see langword="null"/>.
    /// Use <see cref="CreateRuntimeContext(string)"/> to initialize the runtime context.
    /// </exception>
    /// <param name="excludeIO">Exclude built-in I/O functions like <see cref="EzrBuiltinFunctions.Show(string, string, List{Runtime.Types.IEzrObject}, Runtime.Types.IEzrObject, RuntimeResult, Context)"/>?</param>
    public static void PopulateRuntimeContext(bool excludeIO = false)
    {
        if (RuntimeContext is null)
            throw new NullReferenceException($"{nameof(RuntimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        EzrBuiltinsUtility.AddBuiltinConstants(RuntimeContext);
        EzrBuiltinsUtility.AddBuiltinFunctions(RuntimeContext);
        EzrBuiltinsUtility.AddBuiltinTypes(RuntimeContext);

        if (!excludeIO)
            EzrBuiltinsUtility.AddBuiltinIOFunctions(RuntimeContext);
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
        if (RuntimeContext is null)
            throw new NullReferenceException($"{nameof(RuntimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        RuntimeContext.Set(null, name, reference);
    }

    /// <summary>
    /// Adds the given wrapped C# object to the runtime context.
    /// </summary>
    /// <exception cref="NullReferenceException">
    /// Thrown if the runtime context is <see langword="null"/>.
    /// Use <see cref="CreateRuntimeContext(string)"/> to initialize the runtime context.
    /// </exception>
    /// <param name="wrapper">The wrapped object to add.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for the reference. Defaults to <see cref="AccessMod.Constant"/>.</param>
    /// <typeparam name="TMemberInfo">See <see cref="EzrWrapper{TMemberInfo}"/>.</typeparam>
    public static void AddToContext<TMemberInfo>(EzrWrapper<TMemberInfo> wrapper, AccessMod accessibilityModifiers = AccessMod.Constant)
        where TMemberInfo : MemberInfo
    {
        if (RuntimeContext is null)
            throw new NullReferenceException($"{nameof(RuntimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        RuntimeContext.Set(null, wrapper.SharpMemberName, ReferencePool.Get(wrapper, accessibilityModifiers));
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
        if (RuntimeContext is null)
            throw new NullReferenceException($"{nameof(RuntimeContext)} is null! Call {nameof(CreateRuntimeContext)} to create it!");

        // Tokenize the script.
        EzrSyntaxError? lexerError = new Lexer(RuntimeContext.StartPosition.File, script).Tokenize(out List<Token> tokens);
        if (lexerError is not null) // Check for errors.
            return new ExecutionResult([.. tokens], lexerError); // Return tokens with lexing error.

        // Parse the tokens.
        ParseResult parseResult = new Parser(tokens).Parse();
        if (parseResult.Error is not null) // Check for errors.
            return new ExecutionResult([.. tokens], parseResult); // Return the tokens, with the parse error.

        // Interpret the Abstract Syntax Tree.
        RuntimeResult result = Interpreter.Execute(parseResult.Node, RuntimeContext);

        // Return result.
        return new ExecutionResult([.. tokens], parseResult.Node, result);
    }
}
