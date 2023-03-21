using ezrSquared.General;

namespace ezrSquared.Nodes
{
    public abstract class node
    {
        public position startPos;
        public position endPos;

        public node(position startPos, position endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }

    public abstract class valueNode : node
    {
        public token valueToken;

        public valueNode(token valueToken, position startPos, position endPos) : base(startPos, endPos)
        { this.valueToken = valueToken; }
    }

    public abstract class arraylikeNode : node
    {
        public node[] elementNodes;

        public arraylikeNode(node[] elementNodes, position startPos, position endPos) : base(startPos, endPos)
        { this.elementNodes = elementNodes; }
    }

    public abstract class containerNode : node
    {
        public token? nameToken;
        public token[] argNameTokens;
        public node bodyNode;
        public bool shouldReturnNull;

        public containerNode(token? nameToken, token[] argNameTokens, node bodyNode, bool shouldReturnNull, position startPos, position endPos) : base(startPos, endPos)
        {
            this.nameToken = nameToken;
            this.argNameTokens = argNameTokens;
            this.bodyNode = bodyNode;
            this.shouldReturnNull = shouldReturnNull;
        }
    }

    public abstract class variableAssignmentNode : node
    {
        public token? operatorToken;
        public node valueNode;
        public bool isGlobal;

        public variableAssignmentNode(token? operatorToken, node valueNode, bool isGlobal, position startPos, position endPos) : base(startPos, endPos)
        {
            this.operatorToken = operatorToken;
            this.valueNode = valueNode;
            this.isGlobal = isGlobal;
        }
    }

    public class numberNode : valueNode
    {
        public numberNode(token valueToken, position startPos, position endPos) : base(valueToken, startPos, endPos) { }
    }

    public class stringNode : valueNode
    {
        public stringNode(token valueToken, position startPos, position endPos) : base(valueToken, startPos, endPos) { }
    }
    
    public class charListNode : valueNode
    {
        public charListNode(token valueToken, position startPos, position endPos) : base(valueToken, startPos, endPos) { }
    }

    public class arrayNode : arraylikeNode
    {
        public arrayNode(node[] elements, position startPos, position endPos) : base(elements, startPos, endPos) { }
    }

    public class listNode : arraylikeNode
    {
        public listNode(node[] elements, position startPos, position endPos) : base(elements, startPos, endPos) { }
    }

    public class dictionaryNode : node
    {
        public node[][] nodePairs;

        public dictionaryNode(node[][] nodePairs, position startPos, position endPos) : base(startPos, endPos)
        { this.nodePairs = nodePairs; }
    }

    public class variableAccessNode : valueNode
    {
        public bool isGlobal;
        public variableAccessNode(token valueToken, bool isGlobal, position startPos, position endPos) : base(valueToken, startPos, endPos)
        { this.isGlobal = isGlobal; }
    }

    public class simpleVariableAssignNode : variableAssignmentNode
    {
        public token nameToken;
        public simpleVariableAssignNode(token nameToken, token? operatorToken, node valueNode, bool isGlobal, position startPos, position endPos) : base(operatorToken, valueNode, isGlobal, startPos, endPos) { this.nameToken = nameToken; }
    }

    public class objectVariableAssignNode : variableAssignmentNode
    {
        public node accessNode;
        public token varName;

        public objectVariableAssignNode(node accessNode, token varName, token? operatorToken, node valueNode, position startPos, position endPos) : base(operatorToken, valueNode, false, startPos, endPos) { this.accessNode = accessNode; this.varName = varName; }
    }

    public class binaryOperationNode : node
    {
        public node leftNode;
        public node rightNode;
        public token operatorToken;

        public binaryOperationNode(node leftNode, node rightNode, token operatorToken, position startPos, position endPos) : base(startPos, endPos)
        {
            this.leftNode = leftNode;
            this.rightNode = rightNode;
            this.operatorToken = operatorToken;
        }
    }

    public class unaryOperationNode : node
    {
        public node operatedNode;
        public token operatorToken;

        public unaryOperationNode(node operatedNode, token operatorToken, position startPos, position endPos) : base(startPos, endPos)
        {
            this.operatedNode = operatedNode;
            this.operatorToken = operatorToken;
        }
    }

    public class ifNode : node
    {
        public node[][] cases;
        public node? elseCase;
        public bool shouldReturnNull;

        public ifNode(node[][] cases, node? elseCase, bool shouldReturnNull, position startPos, position endPos) : base(startPos, endPos)
        {
            this.cases = cases;
            this.elseCase = elseCase;
            this.shouldReturnNull = shouldReturnNull;
        }
    }

    public class countNode : node
    {
        public token? variableNameToken;
        public node? startValueNode;
        public node endValueNode;
        public node? stepValueNode;
        public node bodyNode;
        public bool shouldReturnNull;

        public countNode(token? variableNameToken, node? startValueNode, node endValueNode, node? stepValueNode, node bodyNode, bool shouldReturnNull, position startPos, position endPos) : base(startPos, endPos)
        {
            this.variableNameToken = variableNameToken;
            this.startValueNode = startValueNode;
            this.endValueNode = endValueNode;
            this.stepValueNode = stepValueNode;
            this.bodyNode = bodyNode;
            this.shouldReturnNull = shouldReturnNull;
        }
    }

    public class whileNode : node
    {
        public node conditionNode;
        public node bodyNode;
        public bool shouldReturnNull;

        public whileNode(node conditionNode, node bodyNode, bool shouldReturnNull, position startPos, position endPos) : base(startPos, endPos)
        {
            this.conditionNode = conditionNode;
            this.bodyNode = bodyNode;
            this.shouldReturnNull = shouldReturnNull;
        }
    }

    public class tryNode : node
    {
        public node bodyNode;
        public object?[][] catches;
        public bool shouldReturnNull;

        public tryNode(node bodyNode, object?[][] catches, bool shouldReturnNull, position startPos, position endPos) : base(startPos, endPos)
        {
            this.bodyNode = bodyNode;
            this.catches = catches;
            this.shouldReturnNull = shouldReturnNull;
        }
    }

    public class functionDefinitionNode : containerNode
    {
        public functionDefinitionNode(token? nameToken, token[] argNameTokens, node bodyNode, bool shouldReturnNull, position startPos, position endPos) : base(nameToken, argNameTokens, bodyNode, shouldReturnNull, startPos, endPos) { }
    }

    public class specialDefinitionNode : containerNode
    {
        public specialDefinitionNode(token nameToken, node bodyNode, bool shouldReturnNull, position startPos, position endPos) : base(nameToken, new token[0], bodyNode, shouldReturnNull, startPos, endPos) { }
    }

    public class objectDefinitionNode : containerNode
    {
        public token? inheritanceToken;

        public objectDefinitionNode(token nameToken, token? inheritFrom, token[] argNameTokens, node bodyNode, position startPos, position endPos) : base(nameToken, argNameTokens, bodyNode, true, startPos, endPos)
        { this.inheritanceToken = inheritFrom; }
    }

    public class callNode : node
    {
        public node nodeToCall;
        public node[] argNodes;

        public callNode(node nodeToCall, node[] argNodes, position startPos, position endPos) : base(startPos, endPos)
        {
            this.nodeToCall = nodeToCall;
            this.argNodes = argNodes;
        }
    }

    public class includeNode : node
    {
        public node fileNode;
        public node? classNode;
        public token? nicknameToken;
        public bool dumpAll;

        public includeNode(node fileNode, node? classNode, bool dumpAll, token? nicknameToken, position startPos, position endPos) : base(startPos, endPos)
        {
            this.fileNode = fileNode;
            this.classNode = classNode;
            this.dumpAll = dumpAll;
            this.nicknameToken = nicknameToken;
        }
    }

    public class returnNode : node
    {
        public node? nodeToReturn;

        public returnNode(node? nodeToReturn, position startPos, position endPos) : base(startPos, endPos)
        { this.nodeToReturn = nodeToReturn; }
    }

    public class skipNode : node
    {
        public skipNode(position startPos, position endPos) : base(startPos, endPos) { }
    }

    public class stopNode : node
    {
        public stopNode(position startPos, position endPos) : base(startPos, endPos) { }
    }
}