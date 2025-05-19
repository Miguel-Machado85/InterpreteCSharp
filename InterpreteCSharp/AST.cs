using System;
using System.Collections.Generic;

namespace InterpreteCSharp.AST
{
    public interface INode
    {
        string TokenLiteral();
    }

    public interface IStatement : INode { }

    public interface IExpression : INode { }

    public class Script : INode
    {
        public List<IStatement> Statements { get; set; } = new();

        public string TokenLiteral()
        {
            return Statements.Count > 0 ? Statements[0].TokenLiteral() : "";
        }

        public override string ToString()
        {
            return string.Join("\n", Statements);
        }
    }

    public class LetStatement : IStatement
    {
        public Token Token;
        public Identifier Name;
        public IExpression Value;

        public LetStatement(Token token, Identifier name, IExpression value)
        {
            Token = token;
            Name = name;
            Value = value;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"{TokenLiteral()} {Name} = {Value};";
    }

    public class AssignStatement : IStatement
    {
        public Token Token;
        public Identifier Name;
        public IExpression Value;

        public AssignStatement(Token token, Identifier name, IExpression value)
        {
            Token = token;
            Name = name;
            Value = value;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"{Name} = {Value};";
    }

    public class ExpressionStatement : IStatement
    {
        public Token Token;
        public IExpression Expression;

        public ExpressionStatement(Token token, IExpression expression)
        {
            Token = token;
            Expression = expression;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => Expression.ToString();
    }

    public class Identifier : IExpression
    {
        public Token Token;
        public string Value;

        public Identifier(Token token, string value)
        {
            Token = token;
            Value = value;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => Value;
    }

    public class IntegerLiteral : IExpression
    {
        public Token Token;
        public int Value;

        public IntegerLiteral(Token token, int value)
        {
            Token = token;
            Value = value;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => Value.ToString();
    }

    public class PrefixExpression : IExpression
    {
        public Token Token;
        public string Operator;
        public IExpression Right;

        public PrefixExpression(Token token, string op, IExpression right)
        {
            Token = token;
            Operator = op;
            Right = right;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"({Operator}{Right})";
    }

    public class InfixExpression : IExpression
    {
        public Token Token;
        public IExpression Left;
        public string Operator;
        public IExpression Right;

        public InfixExpression(Token token, IExpression left, string op, IExpression right)
        {
            Token = token;
            Left = left;
            Operator = op;
            Right = right;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"({Left} {Operator} {Right})";
    }

    public class IfExpression : IExpression
    {
        public Token Token;
        public IExpression Condition;
        public BlockStatement Consequence;
        public BlockStatement Alternative;

        public IfExpression(Token token, IExpression condition, BlockStatement consequence, BlockStatement alternative)
        {
            Token = token;
            Condition = condition;
            Consequence = consequence;
            Alternative = alternative;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString()
        {
            var output = $"if {Condition} {Consequence}";
            if (Alternative != null)
                output += $" else {Alternative}";
            return output;
        }
    }

    public class BlockStatement : IStatement
    {
        public Token Token;
        public List<IStatement> Statements = new();

        public BlockStatement(Token token)
        {
            Token = token;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => "{ " + string.Join(" ", Statements) + " }";
    }

    public class WhileStatement : IStatement
    {
        public Token Token;
        public IExpression Condition;
        public BlockStatement Body;

        public WhileStatement(Token token, IExpression condition, BlockStatement body)
        {
            Token = token;
            Condition = condition;
            Body = body;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"while ({Condition}) {Body}";
    }

    public class ForStatement : IStatement
    {
        public Token Token;
        public IStatement Init;
        public IExpression Condition;
        public IStatement Post;
        public BlockStatement Body;

        public ForStatement(Token token, IStatement init, IExpression condition, IStatement post, BlockStatement body)
        {
            Token = token;
            Init = init;
            Condition = condition;
            Post = post;
            Body = body;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"for ({Init}; {Condition}; {Post}) {Body}";
    }

    public class FunctionLiteral : IExpression
    {
        public Token Token;
        public List<Identifier> Parameters;
        public BlockStatement Body;

        public FunctionLiteral(Token token, List<Identifier> parameters, BlockStatement body)
        {
            Token = token;
            Parameters = parameters;
            Body = body;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"{TokenLiteral()}({string.Join(", ", Parameters)}) {Body}";
    }

    public class CallExpression : IExpression
    {
        public Token Token;
        public IExpression Function;
        public List<IExpression> Arguments;

        public CallExpression(Token token, IExpression function, List<IExpression> arguments)
        {
            Token = token;
            Function = function;
            Arguments = arguments;
        }

        public string TokenLiteral() => Token.Literal;
        public override string ToString() => $"{Function}({string.Join(", ", Arguments)})";
    }
}