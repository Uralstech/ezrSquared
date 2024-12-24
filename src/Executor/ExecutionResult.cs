using EzrSquared.Runtime;
using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types;
using EzrSquared.Syntax;
using EzrSquared.Syntax.Errors;

namespace EzrSquared.Executor;

/// <summary>
/// The result of a code execution operation.
/// </summary>
public class ExecutionResult
{
    /// <summary>
    /// The tokens making up the executed script.
    /// </summary>
    public readonly Token[] Tokens;

    /// <summary>
    /// Any error that occurred during lexing.
    /// </summary>
    public readonly EzrSyntaxError? LexerError;

    /// <summary>
    /// The Abstract Syntax Tree of the executed script.
    /// </summary>
    public readonly Node? Ast;

    /// <summary>
    /// Any error that occurred during parsing.
    /// </summary>
    public readonly EzrSyntaxError? ParseError;

    /// <summary>
    /// The result of the execution.
    /// </summary>
    public readonly IEzrObject? Result;

    /// <summary>
    /// Was the execution successful?
    /// </summary>
    public readonly bool Success;

    /// <summary>
    /// Creates a new execution result, which failed at lexing.
    /// </summary>
    /// <param name="tokens">The tokens returned by the lexer.</param>
    /// <param name="error">The error.</param>
    public ExecutionResult(Token[] tokens, EzrSyntaxError error)
    {
        Tokens = tokens;
        LexerError = error;

        Success = false;
    }

    /// <summary>
    /// Creates a new execution result, which failed at parsing.
    /// </summary>
    /// <param name="tokens">The tokens returned by the lexer.</param>
    /// <param name="result">The parse result carrying the error.</param>
    public ExecutionResult(Token[] tokens, ParseResult result)
    {
        Tokens = tokens;
        ParseError = result.Error;

        Success = false;
    }

    /// <summary>
    /// Creates a new execution result.
    /// </summary>
    /// <param name="tokens">The tokens returned by the lexer.</param>
    /// <param name="ast">The executed AST node.</param>
    /// <param name="result">The runtime result carrying the result or error.</param>
    public ExecutionResult(Token[] tokens, Node ast, RuntimeResult result)
    {
        Tokens = tokens;
        Ast = ast;

        Success = result.Error is null;
        Result = Success ? result.Reference.Object : result.Error;
    }

    /// <summary>
    /// Returns the error message of the execution, be it a lexing, parsing or interpretation error.
    /// </summary>
    /// <returns>The error message.</returns>
    public string GetErrorMessage()
    {
        return (Success, LexerError is not null, ParseError is not null) switch
        {
            (true, _, _) => string.Empty,
            (false, true, _) => LexerError!.ToString(),
            (false, false, true) => ParseError!.ToString(),
            _ => Result!.ToPureString(CodeExecutor.Interpreter.RuntimeResult),
        };
    }
}
