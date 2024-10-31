using EzrSquared.Runtime.Collections;
using EzrSquared.Runtime.Nodes;
using EzrSquared.Runtime.Types;
using EzrSquared.Runtime.Types.Collections;
using EzrSquared.Runtime.Types.Core;
using EzrSquared.Runtime.Types.Core.Errors;
using EzrSquared.Runtime.Types.Core.Numerics;
using EzrSquared.Runtime.Types.Core.Text;
using EzrSquared.Runtime.Types.CSharpWrappers.SourceWrappers;
using EzrSquared.Runtime.Types.Executables;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EzrSquared.Runtime;

/// <summary>
/// The ezr² Interpreter. The job of the Interpreter is to execute the Abstract Syntax Tree (AST) received from the <see cref="Syntax.Parser"/> (as a single <see cref="Node"/> object)!
/// </summary>
public class Interpreter
{
    /// <summary>
    /// The result of the current execution.
    /// </summary>
    public readonly RuntimeResult RuntimeResult = new();

    /// <summary>
    /// Executes an AST under the given <see cref="Context"/> and returns the result.
    /// </summary>
    /// <param name="ast">The AST, as a single <see cref="Node"/> object.</param>
    /// <param name="runtimeContext">The run-time <see cref="Context"/> in which the AST will be executed.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <returns>The <see cref="Runtime.RuntimeResult"/> object.</returns>
    public RuntimeResult Execute(Node ast, Context runtimeContext, AccessMod accessibilityModifiers = AccessMod.None)
    {
        RuntimeResult.Reset();

        VisitNode(ast, runtimeContext, null, accessibilityModifiers);
        return RuntimeResult;
    }

    /// <summary>
    /// Vists a <see cref="Node"/> and sends it to the appropriate execution function.
    /// </summary>
    /// <param name="astNode">The <see cref="Node"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the execution will take place.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <param name="ignoreUndefinedVariable">Should the interpreter ignore undefined variables?</param>
    /// <exception cref="ArgumentException">Raised if <paramref name="astNode"/> is of type <see cref="InvalidNode"/>. This should <i><b>never</b></i> happen as this means something has gone <i><b>very wrong</b></i> in the <see cref="Syntax.Parser"/>.</exception>
    /// <exception cref="NotImplementedException">Raised if the method for executing <paramref name="astNode"/> has not been implemented.</exception>
    internal void VisitNode(Node astNode, Context executionContext, Context? callingContext, AccessMod accessibilityModifiers, bool ignoreUndefinedVariable = false)
    {
        callingContext ??= executionContext;
        switch (astNode)
        {
            case ValueNode node:
                VisitValueNode(node, executionContext);
                break;
            case ArrayLikeNode node:
                VisitArrayLikeNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case DictionaryNode node:
                VisitDictionaryNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case VariableAccessNode node:
                VisitVariableAccessNode(node, executionContext, callingContext, accessibilityModifiers, ignoreUndefinedVariable);
                break;
            case VariableAssignmentNode node:
                VisitVariableAssignmentNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case DefineBlockNode node:
                VisitDefineBlockNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case UnaryOperationNode node:
                VisitUnaryOperationNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case BinaryOperationNode node:
                VisitBinaryOperationNode(node, executionContext, callingContext, accessibilityModifiers, ignoreUndefinedVariable);
                break;
            case IfNode node:
                VisitIfNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case CountNode node:
                VisitCountNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case ForEachNode node:
                VisitForEachNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case WhileNode node:
                VisitWhileNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case TryNode node:
                VisitTryNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case FunctionDefinitionNode node:
                VisitFunctionDefinitionNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case ClassDefinitionNode node:
                VisitClassDefinitionNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case CallNode node:
                VisitCallNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case NoValueNode node:
                VisitNoValueNode(node, executionContext);
                break;
            case ReturnNode node:
                VisitReturnNode(node, executionContext, callingContext, accessibilityModifiers);
                break;
            case InvalidNode:
                throw new ArgumentException("Interpreter received an InvalidNode object! Something has gone very wrong in the Parser.", nameof(astNode));
            default:
                throw new NotImplementedException($"Visit{astNode.GetType().Name}() not defined!");
        }
    }

    /// <summary>
    /// Throws errors for <see cref="Context.Set(Context?, string, Reference)"/> returns.
    /// </summary>
    /// <param name="status">The set status.</param>
    /// <param name="referenceName">The name of the set reference.</param>
    /// <param name="referenceNode">The node of the reference object.</param>
    /// <param name="referenceContext">The context of the reference.</param>
    /// <exception cref="NotImplementedException">Thrown if an unknown set status is encountered.</exception>
    private void HandleSetStatus(Context.SetStatus status, string referenceName, Node referenceNode, Context referenceContext)
    {
        switch (status)
        {
            case Context.SetStatus.Ok:
                break;
            case Context.SetStatus.ConstantAssignmentNotAllowed:
                RuntimeResult.Failure(new EzrIllegalOperationError($"Cannot change constant value \"{referenceName}\"!", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            case Context.SetStatus.PrivateSymbolAssignmentNotAllowed:
                RuntimeResult.Failure(new EzrPrivateMemberOperationError($"Cannot change or create private value \"{referenceName}\"!", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            case Context.SetStatus.SymbolAccessibilityChangeInParentNotAllowed:
                RuntimeResult.Failure(new EzrIllegalOperationError($"Cannot change \"{referenceName}\" to private or constant in a parent context!", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            case Context.SetStatus.PrivateSymbolAssignmentInChildNotAllowed:
                RuntimeResult.Failure(new EzrPrivateMemberOperationError($"Cannot change or create private value \"{referenceName}\" in child!", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            case Context.SetStatus.UnregisteredSymbolScopeOrVariabilityChangeNotAllowed:
                RuntimeResult.Failure(new EzrPrivateMemberOperationError("Cannot change reference to private or constant as it is not registered in a context!", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            case Context.SetStatus.StaticAssignmentWithoutDefinedContextNotAllowed:
                RuntimeResult.Failure(new EzrIllegalOperationError("Cannot assign static variable outside a static context!", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            case Context.SetStatus.InvalidGlobalPrivateAssignmentNotAllowed:
                RuntimeResult.Failure(new EzrIllegalOperationError("Cannot assign a variable with both 'global' and 'private' accessibility modifiers! Are you assigning it in a 'global' or 'private' define block?", referenceContext, referenceNode.StartPosition, referenceNode.EndPosition));
                break;
            default:
                throw new NotImplementedException($"Case for SetStatus {status} not implemented in {nameof(AssignValueToVariable)}!");
        }
    }

    /// <summary>
    /// Throws errors for <see cref="Context.Set(Context?, ValueTuple{IEzrObject, string}, Reference, AccessMod)"/> returns.
    /// </summary>
    /// <param name="setResult">The set result.</param>
    /// <param name="referenceName">The name of the set reference.</param>
    /// <param name="referenceNode">The node of the reference object.</param>
    /// <param name="referenceContext">The context of the reference.</param>
    /// <returns>The set reference.</returns>
    private Reference HandleSetStatus((Context.SetStatus Status, Reference Reference) setResult, string referenceName, Node referenceNode, Context referenceContext)
    {
        HandleSetStatus(setResult.Status, referenceName, referenceNode, referenceContext);
        return setResult.Reference;
    }

    /// <summary>
    /// Creates a "value" type object - i.e. <see cref="EzrInteger"/>, <see cref="EzrFloat"/>, <see cref="EzrString"/>, <see cref="EzrCharacter"/> and <see cref="EzrCharacterList"/>, from a <see cref="ValueNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="ValueNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the value will be created.</param>
    /// <exception cref="NotImplementedException">Raised if an unimplemented or unexpected <see cref="TokenType"/> is encountered in <paramref name="node"/>.</exception>
    private void VisitValueNode(ValueNode node, Context executionContext)
    {
        switch (node.Value.Type)
        {
            case TokenType.Integer:
                RuntimeResult.Success(ReferencePool.Get(new EzrInteger(BigInteger.Parse(node.Value.Value), executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
                break;
            case TokenType.FloatingPoint:
                RuntimeResult.Success(ReferencePool.Get(new EzrFloat(double.Parse(node.Value.Value), executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
                break;
            case TokenType.String:
                RuntimeResult.Success(ReferencePool.Get(new EzrString(node.Value.Value, executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
                break;
            case TokenType.Character:
                RuntimeResult.Success(ReferencePool.Get(new EzrCharacter(char.Parse(node.Value.Value), executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
                break;
            case TokenType.CharacterList:
                RuntimeResult.Success(ReferencePool.Get(new EzrCharacterList(node.Value.Value, executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
                break;
            default:
                throw new NotImplementedException($"Invalid TokenType \"{node.Value.Type}\" for {nameof(VisitValueNode)}!");
        }
    }

    /// <summary>
    /// Creates an array-like object, see <see cref="ArrayLikeNode"/>, <see cref="CreateList(ArrayLikeNode, Context, Context, AccessMod)"/> and <see cref="CreateArray(ArrayLikeNode, Context, Context, AccessMod)"/> for more information.
    /// </summary>
    /// <param name="node">The <see cref="ArrayLikeNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the array-like object will be created.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    #region private void VisitArrayLikeNode(ArrayLikeNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    private void VisitArrayLikeNode(ArrayLikeNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        if (node.CreateList)
            CreateList(node, executionContext, callingContext, accessibilityModifiers);
        else
            CreateArray(node, executionContext, callingContext, accessibilityModifiers);
    }

    /// <summary>
    /// Creates a list of references to its elements.
    /// </summary>
    /// <param name="node">The <see cref="ArrayLikeNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the list will be created.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void CreateList(ArrayLikeNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        RuntimeEzrObjectList elementsReferences = new(node.Elements.Count);
        for (int i = 0; i < node.Elements.Count; i++)
        {
            Node elementNode = node.Elements[i];
            VisitNode(elementNode, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            Reference reference = ReferencePool.Get(RuntimeResult.Reference.Object, AccessMod.None, string.Empty);
            reference.UpdateRegister(true);

            elementsReferences.Add(reference);
        }

        RuntimeResult.Success(ReferencePool.Get(new EzrList(elementsReferences, executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Creates an array of objects.
    /// </summary>
    /// <param name="node">The <see cref="ArrayLikeNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the array will be created.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void CreateArray(ArrayLikeNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        IEzrObject[] elements = new IEzrObject[node.Elements.Count];
        for (int i = 0; i < node.Elements.Count; i++)
        {
            Node elementNode = node.Elements[i];
            VisitNode(elementNode, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            elements[i] = RuntimeResult.Reference.Object;
        }

        RuntimeResult.Success(ReferencePool.Get(new EzrArray(elements, executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }
    #endregion

    /// <summary>
    /// Creates a <see cref="EzrDictionary"/> object from a <see cref="DictionaryNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="DictionaryNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the dictionary will be created.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitDictionaryNode(DictionaryNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        RuntimeEzrObjectDictionary dictionary = new();
        List<(Node Key, Node Value)> pairs = node.KeyValuePairs;

        for (int i = 0; i < pairs.Count; i++)
        {
            // Get the key.
            VisitNode(pairs[i].Key, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            IEzrObject key = RuntimeResult.Reference.Object;

            // Get the value.
            VisitNode(pairs[i].Value, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            // Add it to the dictionary!
            dictionary.Update(key, RuntimeResult.Reference.Object, RuntimeResult);
            if (RuntimeResult.ShouldReturn)
                return;
        }

        RuntimeResult.Success(ReferencePool.Get(new EzrDictionary(dictionary, executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="VariableAccessNode"/> and access a variable from the given <see cref="Context"/>.
    /// </summary>
    /// <param name="node">The <see cref="VariableAccessNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> from which the variable will be accessed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <param name="ignoreUndefinedVariable">Should the interpreter ignore undefined variables?</param>
    /// <exception cref="NotImplementedException">Thrown if a case for the <see cref="Context.GetStatus"/> of the operation was not defined.</exception>
    private void VisitVariableAccessNode(VariableAccessNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers, bool ignoreUndefinedVariable)
    {
        string name = node.Name.Value;
        AccessMod operationAccessibilityModifiers = node.AccessibilityModifiers | accessibilityModifiers;

        // Get the reference.
        Context.GetStatus status = executionContext.Get(callingContext, name, out Reference variable, operationAccessibilityModifiers, ignoreUndefinedVariable);
        switch (status)
        {
            case Context.GetStatus.Ok:
            case Context.GetStatus.UndefinedSymbolAccessNotAllowed when ignoreUndefinedVariable:
                // If the reference is empty, break.
                if (variable.IsEmpty)
                {
                    RuntimeResult.Success(variable);
                    break;
                }

                // Else, update the referenced object with the new context and position.
                variable.Object.Update(executionContext, node.StartPosition, node.EndPosition);

                RuntimeResult.Success(variable);
                break;
            case Context.GetStatus.UndefinedSymbolAccessNotAllowed:
                RuntimeResult.Failure(new EzrUndefinedValueError($"Variable \"{name}\" not defined!", executionContext, node.StartPosition, node.EndPosition));
                break;
            case Context.GetStatus.AccessToPrivateSymbolNotAllowed:
                RuntimeResult.Failure(new EzrPrivateMemberOperationError($"Cannot access private value \"{name}\"!", executionContext, node.StartPosition, node.EndPosition));
                break;
            case Context.GetStatus.StaticAccessWithoutDefinedContextNotAllowed:
                RuntimeResult.Failure(new EzrIllegalOperationError("Cannot access static variable outside a static context!", executionContext, node.StartPosition, node.EndPosition));
                break;
            case Context.GetStatus.InvalidGlobalPrivateAccessNotAllowed:
                RuntimeResult.Failure(new EzrIllegalOperationError("Cannot access a variable with both 'global' and 'private' accessibility modifiers! Are you accessing it in a 'global' or 'private' define block?", executionContext, node.StartPosition, node.EndPosition));
                break;
            default:
                throw new NotImplementedException($"Case for GetStatus {status} not implemented in {nameof(VisitVariableAccessNode)}!");
        }
    }

    /// <summary>
    /// Executes a <see cref="VariableAssignmentNode"/> and sets a variable/constant in the given <see cref="Context"/>.
    /// </summary>
    /// <remarks>
    /// A binary operation may also be executed depending on <see cref="VariableAssignmentNode.AssignmentOperator"/>.<br/>
    /// In the case a <see cref="ArrayLikeNode"/> is provided as <see cref="VariableAssignmentNode.Variable"/>, multiple variables/constants will be assigned.
    /// </remarks>
    /// <param name="node">The <see cref="VariableAssignmentNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the variable will be assigned.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    #region VisitVariableAssignmentNode(VariableAssignmentNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    private void VisitVariableAssignmentNode(VariableAssignmentNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        AccessMod operationAccessibilityModifiers = node.AccessibilityModifiers | accessibilityModifiers;

        bool isSingleVariable = false;
        Node[] variableNodes;

        // Check how many references are to be assigned.
        if (node.Variable is ArrayLikeNode arrayLikeNode)
            variableNodes = [.. arrayLikeNode.Elements];
        else
        {
            // Set flag if there is only one.
            isSingleVariable = true;

            variableNodes = [node.Variable];
        }

        Reference[] variables = new Reference[variableNodes.Length];

        // Get the references.
        for (int i = 0; i < variableNodes.Length; i++)
        {
            Node elementNode = variableNodes[i];

            // Set scope to local, if it is not global - to restrict it to the current execution context.
            if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
                operationAccessibilityModifiers |= AccessMod.LocalScope;

            Reference? reference = GetRegisteredReference(elementNode, executionContext, callingContext, operationAccessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            variables[i] = reference!;
        }

        VisitNode(node.Value, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        AssignValuesToVariables(node, variables, isSingleVariable, executionContext, callingContext, accessibilityModifiers);
    }

    /// <summary>
    /// Gets and checks if a node represents a variable reference.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the variable will be assigned.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <returns>The variable reference. <see langword="null"/> if any error occurs.</returns>
    private Reference? GetRegisteredReference(Node node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        VisitNode(node, executionContext, callingContext, accessibilityModifiers, true);
        if (RuntimeResult.ShouldReturn)
            return null;

        Reference reference = RuntimeResult.Reference;
        if (!reference.IsRegistered)
        {
            RuntimeResult.Failure(new EzrUnexpectedTypeError($"Expected reference or identifier for variable name, but got object of type \"{reference.Object.TypeName}\"!", executionContext, node.StartPosition, node.EndPosition));
            return null;
        }

        return reference;
    }

    /// <summary>
    /// Assigns the value(s) to the variable(s) defined in the <see cref="VariableAssignmentNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="VariableAssignmentNode"/> being executed.</param>
    /// <param name="variables">The array of variable references to be assigned to.</param>
    /// <param name="isSingleVariable">Is only one variable being assigned?</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the variable will be assigned.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void AssignValuesToVariables(VariableAssignmentNode node, Reference[] variables, bool isSingleVariable, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        IEzrObject value = RuntimeResult.Reference.Object;
        if (isSingleVariable)
        {
            AssignValueToVariable(variables[0], value, node, executionContext, callingContext, accessibilityModifiers);
            return;
        }

        IEzrObject[] references;
        IEzrObject[]? values;

        switch (value)
        {
            case IEzrIndexedCollection otherCollection when otherCollection.Length != variables.Length:
                RuntimeResult.Failure(new EzrIllegalOperationError("Mismatched number of variables and values in assignment!", executionContext, node.StartPosition, node.EndPosition));
                return;

            case EzrArray otherArray:
                values = otherArray.Value; break;
            case EzrList otherList:
                values = new IEzrObject[variables.Length];
                for (int i = 0; i < otherList.Value.Count; i++)
                    values[i] = otherList.Value[i].Object;

                break;

            default:
                values = null; break;
        }

        references = new IEzrObject[variables.Length];
        for (int i = 0; i < variables.Length; i++)
        {
            AssignValueToVariable(variables[i], values?[i] ?? value, node, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            references[i] = RuntimeResult.Reference.Object;
        }

        RuntimeResult.Success(ReferencePool.Get(new EzrArray(references, executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Assigns one of the values to one of the variables defined in the <see cref="VariableAssignmentNode"/>.
    /// </summary>
    /// <param name="variable">The variable reference to assign to.</param>
    /// <param name="value">The value object to be assigned.</param>
    /// <param name="node">The <see cref="VariableAssignmentNode"/> being executed.</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the variable will be assigned.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void AssignValueToVariable(Reference variable, IEzrObject value, VariableAssignmentNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        Context assignmentContext = variable.RegisteredContext ?? executionContext;
        if (node.AssignmentOperator != TokenType.Colon)
        {
            if (variable.IsEmpty)
            {
                RuntimeResult.Failure(new EzrUndefinedValueError($"Variable \"{variable.Name}\" does not exist for operation!", assignmentContext, node.Variable.StartPosition, node.Variable.EndPosition));
                return;
            }

            ExecuteBinaryOperation(variable.Object, value, node, node.AssignmentOperator, assignmentContext);
            if (RuntimeResult.ShouldReturn)
                return;

            value = RuntimeResult.Reference.Object;
        }

        (IEzrObject Object, string Name) newVariable = (value, variable.Name);
        Reference variableReference =
            HandleSetStatus(
                assignmentContext.Set(callingContext, newVariable, variable, node.AccessibilityModifiers | accessibilityModifiers),
                variable.Name, node.Variable, assignmentContext
            );

        if (RuntimeResult.ShouldReturn)
            return;

        RuntimeResult.Success(variableReference);
    }
    #endregion

    /// <summary>
    /// Executes a <see cref="DefineBlockNode"/> under its accessibility modifiers in the given <see cref="Context"/>.
    /// </summary>
    /// <param name="node">The <see cref="DefineBlockNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the block will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitDefineBlockNode(DefineBlockNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        AccessMod operationAccessibilityModifiers = node.AccessibilityModifiers | accessibilityModifiers;
        VisitNode(node.Body, executionContext, callingContext, operationAccessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;
    }

    /// <summary>
    /// Executes a <see cref="UnaryOperationNode"/> and performs a unary operation.
    /// </summary>
    /// <param name="node">The <see cref="UnaryOperationNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the operation will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <exception cref="NotImplementedException">Thrown if a case for the <see cref="TokenType"/> of the operand was not defined.</exception>
    private void VisitUnaryOperationNode(UnaryOperationNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        VisitNode(node.Operand, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        IEzrObject operand = RuntimeResult.Reference.Object;
        switch (node.Operator)
        {
            case TokenType.KeywordNot:
            case TokenType.KeywordInvert:
                operand.Inversion(RuntimeResult);
                break;

            case TokenType.Tilde:
                operand.BitwiseNegation(RuntimeResult);
                break;

            case TokenType.HyphenMinus:
                operand.Negation(RuntimeResult);
                break;
            case TokenType.Plus:
                operand.Affirmation(RuntimeResult);
                break;

            default:
                throw new NotImplementedException($"Case for TokenType {node.Operator} not implemented in {nameof(VisitUnaryOperationNode)}()!");
        }

        if (RuntimeResult.ShouldReturn)
            return;

        RuntimeResult.Reference.Object.Update(executionContext, node.StartPosition, node.EndPosition);
    }

    /// <summary>
    /// Executes a <see cref="BinaryOperationNode"/> and performs a binary operation.
    /// </summary>
    /// <param name="node">The <see cref="BinaryOperationNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the operation will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <param name="ignoreUndefinedVariable">Should the interpreter ignore undefined variables?</param>
    private void VisitBinaryOperationNode(BinaryOperationNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers, bool ignoreUndefinedVariable)
    {
        switch (node.Operator)
        {
            case TokenType.Period:
                ExecuteObjectAttributeAccess(node, executionContext, callingContext, accessibilityModifiers, ignoreUndefinedVariable); break;

            case TokenType.KeywordOr:
            case TokenType.KeywordAnd:
                ExecuteConjunctiveOperators(node, executionContext, callingContext, accessibilityModifiers); break;

            default:
                VisitNode(node.Left, executionContext, callingContext, accessibilityModifiers);
                if (RuntimeResult.ShouldReturn)
                    return;

                IEzrObject left = RuntimeResult.Reference.Object;

                VisitNode(node.Right, executionContext, callingContext, accessibilityModifiers);
                if (RuntimeResult.ShouldReturn)
                    return;

                ExecuteBinaryOperation(left, RuntimeResult.Reference.Object, node, node.Operator, executionContext); break;
        }
    }

    /// <summary>
    /// Executes an object attribute access operation.
    /// </summary>
    /// <param name="node">The operation's AST node.</param>
    /// <param name="executionContext">The context under which the operation will take place.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    /// <param name="ignoreUndefinedVariable">Should the interpreter ignore undefined variables?</param>
    private void ExecuteObjectAttributeAccess(BinaryOperationNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers, bool ignoreUndefinedVariable)
    {
        AccessMod operationAccessibilityModifiers = (node.Left as BinaryOperationNode)?.Operator != TokenType.Period
            ? accessibilityModifiers & ~AccessMod.LocalScope
            : accessibilityModifiers;

        VisitNode(node.Left, executionContext, callingContext, accessibilityModifiers & ~AccessMod.LocalScope);
        if (RuntimeResult.ShouldReturn)
            return;

        RuntimeResult.Reference.Object.Interpret(node.Right, callingContext, this, RuntimeResult, ignoreUndefinedVariable);
        if (RuntimeResult.ShouldReturn)
            return;

        Reference reference = RuntimeResult.Reference;
        if (!reference.IsEmpty)
            reference.Object.Update(executionContext, node.StartPosition, node.EndPosition);
    }

    /// <summary>
    /// Executes a boolean conjunctive operation.
    /// </summary>
    /// <param name="node">The operation's AST node.</param>
    /// <param name="executionContext">The context under which the operation will take place.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void ExecuteConjunctiveOperators(BinaryOperationNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        VisitNode(node.Left, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        bool firstEvaluation = RuntimeResult.Reference.Object.EvaluateBoolean(RuntimeResult);
        if (RuntimeResult.ShouldReturn)
            return;

        EzrBoolean booleanValue;
        if ((!firstEvaluation && node.Operator == TokenType.KeywordAnd) || (firstEvaluation && node.Operator == TokenType.KeywordOr))
        {
            booleanValue = firstEvaluation ? EzrConstants.True : EzrConstants.False;
            booleanValue.Update(executionContext, node.StartPosition, node.EndPosition);

            RuntimeResult.Success(ReferencePool.Get(booleanValue, AccessMod.PrivateConstant));
            return;
        }

        VisitNode(node.Right, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        bool secondEvaluation = RuntimeResult.Reference.Object.EvaluateBoolean(RuntimeResult);
        if (RuntimeResult.ShouldReturn)
            return;

        booleanValue = secondEvaluation ? EzrConstants.True : EzrConstants.False;
        booleanValue.Update(executionContext, node.StartPosition, node.EndPosition);

        RuntimeResult.Success(ReferencePool.Get(booleanValue, AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a binary operation.
    /// </summary>
    /// <param name="first">The first value in the operation.</param>
    /// <param name="second">The second value in the operation.</param>
    /// <param name="node">The operation's AST node.</param>
    /// <param name="operation">The operation to perform.</param>
    /// <param name="executionContext">The context under which the operation will take place.</param>
    /// <exception cref="NotImplementedException">Thrown if a case for the <see cref="TokenType"/> of the operand was not defined.</exception>
    private void ExecuteBinaryOperation(IEzrObject first, IEzrObject second, Node node, TokenType operation, Context executionContext)
    {
        switch (operation)
        {
            case TokenType.Plus:
            case TokenType.AssignmentAddition:
                first.Addition(second, RuntimeResult);
                break;

            case TokenType.HyphenMinus:
            case TokenType.AssignmentSubtraction:
                first.Subtraction(second, RuntimeResult);
                break;

            case TokenType.Asterisk:
            case TokenType.AssignmentMultiplication:
                first.Multiplication(second, RuntimeResult);
                break;

            case TokenType.Slash:
            case TokenType.AssignmentDivision:
                first.Division(second, RuntimeResult);
                break;

            case TokenType.Caret:
            case TokenType.AssignmentPower:
                first.Power(second, RuntimeResult);
                break;

            case TokenType.PercentSign:
            case TokenType.AssignmentModulo:
                first.Modulo(second, RuntimeResult);
                break;

            case TokenType.Ampersand:
            case TokenType.AssignmentBitwiseAnd:
                first.BitwiseAnd(second, RuntimeResult);
                break;

            case TokenType.VerticalBar:
            case TokenType.AssignmentBitwiseOr:
                first.BitwiseOr(second, RuntimeResult);
                break;

            case TokenType.Backslash:
            case TokenType.AssignmentBitwiseXOr:
                first.BitwiseXOr(second, RuntimeResult);
                break;

            case TokenType.BitwiseLeftShift:
            case TokenType.AssignmentBitwiseLeftShift:
                first.BitwiseLeftShift(second, RuntimeResult);
                break;

            case TokenType.BitwiseRightShift:
            case TokenType.AssignmentBitwiseRightShift:
                first.BitwiseRightShift(second, RuntimeResult);
                break;

            case TokenType.EqualSign:
                first.ComparisonEqual(second, RuntimeResult);
                break;

            case TokenType.ExclamationMark:
                first.ComparisonNotEqual(second, RuntimeResult);
                break;

            case TokenType.LessThanSign:
                first.ComparisonLessThan(second, RuntimeResult);
                break;

            case TokenType.GreaterThanSign:
                first.ComparisonGreaterThan(second, RuntimeResult);
                break;

            case TokenType.LessThanOrEqual:
                first.ComparisonLessThanOrEqual(second, RuntimeResult);
                break;

            case TokenType.GreaterThanOrEqual:
                first.ComparisonGreaterThanOrEqual(second, RuntimeResult);
                break;

            case TokenType.KeywordIn:
                second.HasValueContained(first, RuntimeResult);
                break;

            case TokenType.KeywordNot:
                second.NotHasValueContained(first, RuntimeResult);
                break;

            default:
                throw new NotImplementedException($"Case for TokenType {operation} not implemented in {nameof(ExecuteBinaryOperation)}()!");
        }

        if (RuntimeResult.ShouldReturn)
            return;

        RuntimeResult.Reference.Object.Update(executionContext, node.StartPosition, node.EndPosition);
    }

    /// <summary>
    /// Executes an <see cref="IfNode"/> and performs a boolean expression.
    /// </summary>
    /// <param name="node">The <see cref="IfNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the expression will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitIfNode(IfNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        for (int i = 0; i < node.Cases.Count; i++)
        {
            VisitNode(node.Cases[i].Condition, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            bool condition = RuntimeResult.Reference.Object.EvaluateBoolean(RuntimeResult);
            if (RuntimeResult.ShouldReturn)
                return;

            if (condition)
            {
                VisitNode(node.Cases[i].Body, executionContext, callingContext, accessibilityModifiers);
                return;
            }
        }

        if (node.ElseCase is not null)
        {
            VisitNode(node.ElseCase, executionContext, callingContext, accessibilityModifiers);
            return;
        }

        EzrConstants.Nothing.Update(executionContext, node.StartPosition, node.EndPosition);
        RuntimeResult.Success(ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="CountNode"/> and creates a counting loop.
    /// </summary>
    /// <param name="node">The <see cref="CountNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the loop will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitCountNode(CountNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        VisitNode(node.To, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        List<IEzrObject> returns = [];
        IEzrObject toParameter = RuntimeResult.Reference.Object;

        if (toParameter is not EzrInteger and not EzrFloat)
        {
            RuntimeResult.Failure(new EzrUnexpectedTypeError($"Expected types integer or float, but got object of type \"{toParameter.TypeName}\"!", executionContext, node.To.StartPosition, node.To.EndPosition));
            return;
        }

        IEzrObject? fromParameter = null;
        IEzrObject? stepParameter = null;

        Reference? iterationVariableReference = null;
        string iterationVariableName = string.Empty;
        Context iterationVariableRegisteredContext = executionContext;

        AccessMod operationAccessibilityModifiers = accessibilityModifiers;
        if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
            operationAccessibilityModifiers |= AccessMod.LocalScope;

        if (node.From is not null)
        {
            VisitNode(node.From, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            fromParameter = RuntimeResult.Reference.Object;
            if (fromParameter.HashTag != toParameter.HashTag)
            {
                RuntimeResult.Failure(new EzrUnexpectedTypeError($"All parameters of the count loop must be of the same type! Expected type {toParameter.TypeName}, but got object of type \"{fromParameter.TypeName}\".", executionContext, node.From.StartPosition, node.From.EndPosition));
                return;
            }
        }

        if (node.Step is not null)
        {
            VisitNode(node.Step, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            stepParameter = RuntimeResult.Reference.Object;
            if (stepParameter.HashTag != toParameter.HashTag)
            {
                RuntimeResult.Failure(new EzrUnexpectedTypeError($"All parameters of the count loop must be of the same type! Expected type {toParameter.TypeName}, but got object of type \"{stepParameter.TypeName}\".", executionContext, node.Step.StartPosition, node.Step.EndPosition));
                return;
            }
        }

        if (node.IterationVariable is not null)
        {
            iterationVariableReference = GetRegisteredReference(node.IterationVariable, executionContext, callingContext, operationAccessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            iterationVariableName = iterationVariableReference!.Name;
            iterationVariableRegisteredContext = iterationVariableReference.RegisteredContext ?? executionContext;
        }

        if (toParameter is EzrInteger toParameterInteger)
        {
            BigInteger to = toParameterInteger.Value;
            BigInteger from = (fromParameter as EzrInteger)?.Value ?? 0;

            BigInteger step;
            Func<BigInteger, bool> condition;

            if (from < to)
                (step, condition) = ((stepParameter as EzrInteger)?.Value ?? 1, i => i < to);
            else
                (step, condition) = ((stepParameter as EzrInteger)?.Value ?? -1, i => i > to);

            for (BigInteger i = from; condition(i); i += step)
            {
                int iterationResult = IterateLoop(i, null, returns, node, callingContext,
                    executionContext, accessibilityModifiers, operationAccessibilityModifiers,
                    iterationVariableReference, iterationVariableName, iterationVariableRegisteredContext);

                if (iterationResult == -1)
                    return;
                else if (iterationResult == 1)
                    continue;
                else if (iterationResult == 2)
                    break;
            }
        }
        else
        {
            double to = ((EzrFloat)toParameter).Value;
            double from = (fromParameter as EzrFloat)?.Value ?? 0;

            double step;
            Func<double, bool> condition;

            if (from < to)
                (step, condition) = ((stepParameter as EzrFloat)?.Value ?? 1, i => i <= to);
            else
                (step, condition) = ((stepParameter as EzrFloat)?.Value ?? -1, i => i >= to);

            for (double i = from; condition(i); i += step)
            {
                int iterationResult = IterateLoop(null, i, returns, node, callingContext,
                    executionContext, accessibilityModifiers, operationAccessibilityModifiers,
                    iterationVariableReference, iterationVariableName, iterationVariableRegisteredContext);

                if (iterationResult == -1)
                    return;
                else if (iterationResult == 1)
                    continue;
                else if (iterationResult == 2)
                    break;
            }
        }

        RuntimeResult.Success(ReferencePool.Get(new EzrArray([.. returns], executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Code to run in an iteration of a count expression.
    /// </summary>
    /// <param name="iBigInteger">The current iteration of the loop as a <see cref="BigInteger"/>.</param>
    /// <param name="iDouble">The current iteration of the loop as a <see cref="double"/>.</param>
    /// <param name="returns">The list of the returns of the loop.</param>
    /// <param name="node">The loop node.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the iteration.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the iteration will be executed.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing iteration.</param>
    /// <param name="operationAccessibilityModifiers">The accessibility modifiers for the iteration variable.</param>
    /// <param name="iterationVariableReference">The iteration variable reference.</param>
    /// <param name="iterationVariableName">The name of the iteration variable</param>
    /// <param name="iterationVariableRegisteredContext">The context under which the iteration variable is registered.</param>
    /// <returns>-1 for errors, 1 to skip the iteration, 2 for stopping the loop or 0 for continuation of the loop.</returns>
    private int IterateLoop(
        BigInteger? iBigInteger,
        double? iDouble,
        List<IEzrObject> returns,
        CountNode node,
        Context callingContext,
        Context executionContext,
        AccessMod accessibilityModifiers,
        AccessMod operationAccessibilityModifiers,
        Reference? iterationVariableReference,
        string iterationVariableName,
        Context iterationVariableRegisteredContext)
    {
        if (iterationVariableReference is not null && node.IterationVariable is not null)
        {
            (IEzrObject Object, string Name) newIterationVariable = (
                iBigInteger is not null
                    ? new EzrInteger(iBigInteger.Value, iterationVariableRegisteredContext, node.IterationVariable.StartPosition, node.IterationVariable.EndPosition)
                    : new EzrFloat(iDouble!.Value, iterationVariableRegisteredContext, node.IterationVariable.StartPosition, node.IterationVariable.EndPosition),

                iterationVariableName);

            HandleSetStatus(
                iterationVariableRegisteredContext.Set(callingContext, newIterationVariable, iterationVariableReference, operationAccessibilityModifiers),
                iterationVariableName,
                node.IterationVariable,
                iterationVariableRegisteredContext
            );

            if (RuntimeResult.ShouldReturn)
                return -1;
        }

        VisitNode(node.Body, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturnLoop)
            return -1;
        else if (RuntimeResult.SkipSet)
        {
            RuntimeResult.Reset();
            return 1;
        }
        else if (RuntimeResult.StopSet)
        {
            RuntimeResult.Reset();
            return 2;
        }

        returns.Add(RuntimeResult.Reference.Object);
        return 0;
    }

    /// <summary>
    /// Executes a <see cref="ForEachNode"/> and creates a for-each loop.
    /// </summary>
    /// <param name="node">The <see cref="ForEachNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the loop will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitForEachNode(ForEachNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        AccessMod operationAccessibilityModifiers = accessibilityModifiers;
        if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
            operationAccessibilityModifiers |= AccessMod.LocalScope;

        Reference? iterationVariableReference = GetRegisteredReference(node.Expression.Left, executionContext, callingContext, operationAccessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        string iterationVariableName = iterationVariableReference!.Name;
        Context iterationVariableRegisteredContext = iterationVariableReference.RegisteredContext ?? executionContext;

        VisitNode(node.Expression.Right, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        Reference iterableObjectReference = RuntimeResult.Reference;
        if (iterableObjectReference.Object is not IEzrIndexedCollection iterableObject)
        {
            RuntimeResult.Failure(new EzrUnexpectedTypeError($"Expected an iterable object to iterate, but got object of type \"{iterableObjectReference.Object.TypeName}\"!", executionContext, node.Expression.Right.StartPosition, node.Expression.Right.EndPosition));
            return;
        }

        List<IEzrObject> returns = new(iterableObject.Length);
        foreach (IEzrObject ezrObject in iterableObject)
        {
            ezrObject.Update(executionContext, node.Expression.StartPosition, node.Expression.EndPosition);
            (IEzrObject Object, string Name) newIterationVariable = (ezrObject, iterationVariableName);

            HandleSetStatus(
                iterationVariableRegisteredContext.Set(callingContext, newIterationVariable, iterationVariableReference, operationAccessibilityModifiers),
                iterationVariableName,
                node.Expression.Left,
                iterationVariableRegisteredContext
            );

            if (RuntimeResult.ShouldReturn)
                return;

            VisitNode(node.Body, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturnLoop)
                return;
            else if (RuntimeResult.SkipSet)
            {
                RuntimeResult.Reset();
                continue;
            }
            else if (RuntimeResult.StopSet)
            {
                RuntimeResult.Reset();
                break;
            }

            returns.Add(RuntimeResult.Reference.Object);
        }

        RuntimeResult.Success(ReferencePool.Get(new EzrArray([.. returns], executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="WhileNode"/> and creates a while (condition = true) loop.
    /// </summary>
    /// <param name="node">The <see cref="WhileNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the loop will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitWhileNode(WhileNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        List<IEzrObject> returns = [];

        do
        {
            VisitNode(node.Condition, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            bool condition = RuntimeResult.Reference.Object.EvaluateBoolean(RuntimeResult);
            if (RuntimeResult.ShouldReturn)
                return;

            if (!condition)
                break;

            VisitNode(node.Body, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturnLoop)
                return;

            if (RuntimeResult.SkipSet)
            {
                RuntimeResult.Reset();
                continue;
            }
            if (RuntimeResult.StopSet)
            {
                RuntimeResult.Reset();
                break;
            }

            returns.Add(RuntimeResult.Reference.Object);
        } while (true);

        RuntimeResult.Success(ReferencePool.Get(new EzrArray([.. returns], executionContext, node.StartPosition, node.EndPosition), AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="TryNode"/> and creates a try-catch block.
    /// </summary>
    /// <param name="node">The <see cref="TryNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the block will be executed.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitTryNode(TryNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        VisitNode(node.Block, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturnTryCatch || RuntimeResult.Error is null)
            return;

        IEzrRuntimeError error = RuntimeResult.Error!;
        RuntimeResult.Reset();

        for (int i = 0; i < node.Cases.Count; i++)
        {
            (Node errorType, Node? errorVariableNode, Node body) = node.Cases[i];

            VisitNode(errorType, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            IEzrObject targetObject = RuntimeResult.Reference.Object;
            if (targetObject is not EzrSharpSourceTypeWrapper target || !typeof(IEzrRuntimeError).IsAssignableFrom(target.SharpType))
            {
                RuntimeResult.Failure(new EzrUnexpectedTypeError($"Expected error type, but got object of type \"{targetObject.TypeName}\"!", executionContext, errorType.StartPosition, errorType.EndPosition));
                return;
            }

            if (target.SharpType.IsAssignableFrom(error.GetType()))
            {
                if (errorVariableNode is not null)
                {
                    AccessMod operationAccessibilityModifiers = accessibilityModifiers;
                    if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
                        operationAccessibilityModifiers |= AccessMod.LocalScope;

                    Reference? reference = GetRegisteredReference(errorVariableNode, executionContext, callingContext, operationAccessibilityModifiers);
                    if (RuntimeResult.ShouldReturn)
                        return;

                    Context assignmentContext = reference!.RegisteredContext ?? executionContext;
                    error.Update(assignmentContext, errorVariableNode.StartPosition, errorVariableNode.EndPosition);

                    (IEzrObject Object, string Name) newErrorVariable = (error, reference.Name);
                    HandleSetStatus(assignmentContext.Set(callingContext, newErrorVariable, reference, operationAccessibilityModifiers), reference.Name, errorVariableNode, assignmentContext);
                    if (RuntimeResult.ShouldReturn)
                        return;
                }

                VisitNode(body, executionContext, callingContext, accessibilityModifiers);
                return;
            }
        }

        if (node.EmptyCase.HasValue)
        {
            (Node? emptyCaseVariable, Node emptyCaseBody) = node.EmptyCase.Value;
            if (emptyCaseVariable is not null)
            {
                AccessMod operationAccessibilityModifiers = accessibilityModifiers;
                if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
                    operationAccessibilityModifiers |= AccessMod.LocalScope;

                Reference? reference = GetRegisteredReference(emptyCaseVariable, executionContext, callingContext, operationAccessibilityModifiers);
                if (RuntimeResult.ShouldReturn)
                    return;

                Context assignmentContext = reference!.RegisteredContext ?? executionContext;
                error.Update(assignmentContext, emptyCaseVariable.StartPosition, emptyCaseVariable.EndPosition);

                (IEzrObject Object, string Name) newErrorVariable = (error, reference.Name);
                HandleSetStatus(assignmentContext.Set(callingContext, newErrorVariable, reference, operationAccessibilityModifiers), reference.Name, emptyCaseVariable, assignmentContext);
                if (RuntimeResult.ShouldReturn)
                    return;
            }

            VisitNode(emptyCaseBody, executionContext, callingContext, accessibilityModifiers);
            return;
        }

        EzrConstants.Nothing.Update(executionContext, node.StartPosition, node.EndPosition);
        RuntimeResult.Success(ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="FunctionDefinitionNode"/> and creates a function.
    /// </summary>
    /// <param name="node">The <see cref="FunctionDefinitionNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the function will be created.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitFunctionDefinitionNode(FunctionDefinitionNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        Context newContext = new($"<function parameters>", false, node.StartPosition, executionContext, executionContext.StaticContext);

        (string Name, Node Node)[] parameters = new (string, Node)[node.Parameters.Count];
        for (int i = 0; i < parameters.Length; i++)
        {
            Node parameter = node.Parameters[i];
            VisitNode(parameter, newContext, callingContext, AccessMod.None, true);
            if (RuntimeResult.ShouldReturn)
                return;

            Reference reference = RuntimeResult.Reference;
            if (string.IsNullOrEmpty(reference.Name))
            {
                RuntimeResult.Failure(new EzrUnexpectedTypeError($"Expected an identifier or variable assignment to store arguments in, but received object of type \"{reference.Object.TypeName}\"!", executionContext, parameter.StartPosition, parameter.EndPosition));
                return;
            }

            parameters[i] = (reference.Name, node.Parameters[i]);
        }

        OptionalExtraArguments keywordArguments = null;
        if (node.ExtraKeywordArguments is not null)
        {
            Reference? reference = GetRegisteredReference(node.ExtraKeywordArguments, newContext, callingContext, AccessMod.None);
            if (RuntimeResult.ShouldReturn)
                return;

            keywordArguments = (node.ExtraKeywordArguments.StartPosition, node.ExtraKeywordArguments.EndPosition, reference!.Name);
        }

        OptionalExtraArguments positionalArguments = null;
        if (node.ExtraPositionalArguments is not null)
        {
            Reference? reference = GetRegisteredReference(node.ExtraPositionalArguments, newContext, callingContext, AccessMod.None);
            if (RuntimeResult.ShouldReturn)
                return;

            positionalArguments = (node.ExtraPositionalArguments.StartPosition, node.ExtraPositionalArguments.EndPosition, reference!.Name);
        }

        newContext.Release();

        EzrFunction function;
        if (node.Name is not null)
        {
            AccessMod operationAccessibilityModifiers = node.AccessibilityModifiers | accessibilityModifiers;
            if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
                operationAccessibilityModifiers |= AccessMod.LocalScope;

            Reference? reference = GetRegisteredReference(node.Name, executionContext, callingContext, operationAccessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            Context assignmentContext = reference!.RegisteredContext ?? executionContext;
            function = new EzrFunction(reference.Name, node.Body, parameters, keywordArguments, positionalArguments, node.ReturnLast, assignmentContext, node.StartPosition, node.EndPosition);

            (IEzrObject Object, string Name) newFunctionVariable = (function, reference.Name);
            HandleSetStatus(assignmentContext.Set(callingContext, newFunctionVariable, reference, operationAccessibilityModifiers), reference.Name, node.Name, assignmentContext);
            if (RuntimeResult.ShouldReturn)
                return;
        }
        else
            function = new EzrFunction(null, node.Body, parameters, keywordArguments, positionalArguments, node.ReturnLast, executionContext, node.StartPosition, node.EndPosition);

        RuntimeResult.Success(ReferencePool.Get(function, AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="ClassDefinitionNode"/> and creates a class.
    /// </summary>
    /// <param name="node">The <see cref="ClassDefinitionNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> in which the class will be created.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitClassDefinitionNode(ClassDefinitionNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        EzrClass[] parents = new EzrClass[node.Parents.Count];
        for (int i = 0; i < parents.Length; i++)
        {
            Node parentNode = node.Parents[i];
            VisitNode(parentNode, executionContext, callingContext, accessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            IEzrObject ezrObject = RuntimeResult.Reference.Object;
            if (ezrObject is not EzrClass parent)
            {
                RuntimeResult.Failure(new EzrUnexpectedTypeError($"Expected a class, but received object of type \"{ezrObject.TypeName}\"!", executionContext, parentNode.StartPosition, parentNode.EndPosition));
                return;
            }

            parents[i] = parent;
        }

        EzrClass @class;
        if (node.Name is not null)
        {
            AccessMod operationAccessibilityModifiers = node.AccessibilityModifiers | accessibilityModifiers;
            if ((operationAccessibilityModifiers & AccessMod.Global) != AccessMod.Global)
                operationAccessibilityModifiers |= AccessMod.LocalScope;

            Reference? reference = GetRegisteredReference(node.Name, executionContext, callingContext, operationAccessibilityModifiers);
            if (RuntimeResult.ShouldReturn)
                return;

            Context assignmentContext = reference!.RegisteredContext ?? executionContext;
            @class = new EzrClass(reference.Name, node.Body, parents, node.Readonly,
                ((node.AccessibilityModifiers | accessibilityModifiers) & AccessMod.Static) == AccessMod.Static, this,
                RuntimeResult, assignmentContext, node.StartPosition, node.EndPosition);

            if (RuntimeResult.ShouldReturn)
                return;

            (IEzrObject Object, string Name) newClassVariable = (@class, reference.Name);
            HandleSetStatus(assignmentContext.Set(callingContext, newClassVariable, reference, operationAccessibilityModifiers), reference.Name, node.Name, assignmentContext);
            if (RuntimeResult.ShouldReturn)
                return;
        }
        else
        {
            @class = new EzrClass(null, node.Body, parents, node.Readonly,
                ((node.AccessibilityModifiers | accessibilityModifiers) & AccessMod.Static) == AccessMod.Static, this,
                RuntimeResult, executionContext, node.StartPosition, node.EndPosition);

            if (RuntimeResult.ShouldReturn)
                return;
        }

        RuntimeResult.Success(ReferencePool.Get(@class, AccessMod.PrivateConstant));
    }

    /// <summary>
    /// Executes a <see cref="CallNode"/> and calls the <see cref="IEzrObject.Execute(Reference[], Interpreter, RuntimeResult)"/> function in <see cref="CallNode.Receiver"/>.
    /// </summary>
    /// <param name="node">The <see cref="CallNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the call takes place.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitCallNode(CallNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        VisitNode(node.Receiver, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        IEzrObject receiver = RuntimeResult.Reference.Object;

        Reference[] arguments = [];
        if (node.Arguments.Count > 0)
        {
            Context argumentsContext = new($"<{receiver.TypeName} arguments>", false, receiver.StartPosition, callingContext, callingContext.StaticContext);
            arguments = new Reference[node.Arguments.Count];

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                Node argument = node.Arguments[i];
                VisitNode(argument, argumentsContext, callingContext, accessibilityModifiers & ~AccessMod.LocalScope);
                if (RuntimeResult.ShouldReturn)
                    return;

                Reference reference = RuntimeResult.GetReferenceCopyIfReleasable();
                if (reference.RegisteredContext?.Id == argumentsContext.Id)
                {
                    Reference copy = reference.ShallowCopy();
                    copy.UpdateName(reference.Name);

                    reference = copy;
                }

                arguments[i] = reference;
            }

            argumentsContext.Release();
        }

        receiver.Execute(arguments, this, RuntimeResult);
        if (RuntimeResult.ShouldReturn)
            return;

        RuntimeResult.Reference.Object.Update(executionContext, node.StartPosition, node.EndPosition);
    }

    /// <summary>
    /// Executes a <see cref="NoValueNode"/>.
    /// </summary>
    /// <remarks>
    /// This is (currently) only used for executing loop skip (calling <see cref="RuntimeResult.SetSkipFlag(Reference)"/>) and loop stop (calling <see cref="RuntimeResult.SetStopFlag(Reference)"/>) expressions.
    /// </remarks>
    /// <param name="node">The <see cref="NoValueNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> under which the node is executed.</param>
    /// <exception cref="NotImplementedException">Thrown when an unimplemented or unexpected <see cref="NoValueNode.ValueType"/> is encountered.</exception>
    private void VisitNoValueNode(NoValueNode node, Context executionContext)
    {
        EzrConstants.Nothing.Update(executionContext, node.StartPosition, node.EndPosition);
        Reference toReturn = ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant);

        switch (node.ValueType)
        {
            case TokenType.KeywordSkip:
                RuntimeResult.SetSkipFlag(toReturn); break;

            case TokenType.KeywordStop:
                RuntimeResult.SetStopFlag(toReturn); break;

            default:
                throw new NotImplementedException($"Invalid {nameof(TokenType)} \"{node.ValueType}\" for {nameof(VisitNoValueNode)}()!");
        }
    }

    /// <summary>
    /// Executes a <see cref="ReturnNode"/> and sets the return flag in <see cref="RuntimeResult"/>.
    /// </summary>
    /// <param name="node">The <see cref="ReturnNode"/> to execute.</param>
    /// <param name="executionContext">The <see cref="Context"/> from which the arguments of the <see cref="ReturnNode"/> will be taken, if any.</param>
    /// <param name="callingContext">The <see cref="Context"/> calling on the execution of the <see cref="Node"/>.</param>
    /// <param name="accessibilityModifiers">The accessibility modifiers for objects that will be assigned from the executing <see cref="Node"/>.</param>
    private void VisitReturnNode(ReturnNode node, Context executionContext, Context callingContext, AccessMod accessibilityModifiers)
    {
        if (node.Value is null)
        {
            EzrConstants.Nothing.Update(executionContext, node.StartPosition, node.EndPosition);

            RuntimeResult.SetReturnFlag(ReferencePool.Get(EzrConstants.Nothing, AccessMod.PrivateConstant));
            return;
        }

        VisitNode(node.Value, executionContext, callingContext, accessibilityModifiers);
        if (RuntimeResult.ShouldReturn)
            return;

        Reference toReturn = RuntimeResult.Reference;
        if (node.ReturnLast)
        {
            switch (toReturn.Object)
            {
                case EzrArray otherArray when otherArray.Value.Length == 0:
                case EzrList otherList when otherList.Value.Count == 0:
                    RuntimeResult.Failure(new EzrIllegalOperationError($"Cannot return the last element of an empty {toReturn.Object.TypeName}!", executionContext, node.Value.StartPosition, node.Value.EndPosition));
                    return;

                case EzrArray otherArray:
                    toReturn = ReferencePool.Get(otherArray.Value[^1], AccessMod.PrivateConstant); break;
                case EzrList otherList:
                    toReturn = otherList.Value[^1]; break;

                default:
                    RuntimeResult.Failure(new EzrUnexpectedTypeError($"Can only return the last element of an array or list, but got object of type \"{toReturn.Object.TypeName}\"!", executionContext, node.Value.StartPosition, node.Value.EndPosition));
                    return;
            }
        }

        RuntimeResult.SetReturnFlag(toReturn);
    }
}
