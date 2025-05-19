using System;
using System.Collections.Generic;
using InterpreteCSharp.AST;

namespace InterpreteCSharp.Objects
{
    public enum ObjectType
    {
        INTEGER,
        BOOLEAN,
        FUNCTION
    }

    public interface IObject
    {
        ObjectType Type();
        string Inspect();
    }

    public class IntegerObj : IObject
    {
        public int Value { get; }

        public IntegerObj(int value)
        {
            Value = value;
        }

        public ObjectType Type() => ObjectType.INTEGER;
        public string Inspect() => Value.ToString();
    }

    public class BooleanObj : IObject
    {
        public bool Value { get; }

        private BooleanObj(bool value)
        {
            Value = value;
        }

        public ObjectType Type() => ObjectType.BOOLEAN;
        public string Inspect() => Value ? "true" : "false";

        public static readonly BooleanObj True = new(true);
        public static readonly BooleanObj False = new(false);
    }

    public class FunctionObj : IObject
    {
        public List<Identifier> Parameters { get; }
        public BlockStatement Body { get; }
        public Environment.Env Env { get; }

        public FunctionObj(List<Identifier> parameters, BlockStatement body, Environment.Env env)
        {
            Parameters = parameters;
            Body = body;
            Env = env;
        }

        public ObjectType Type() => ObjectType.FUNCTION;

        public string Inspect()
        {
            var paramList = string.Join(", ", Parameters);
            return $"function({paramList}) {{ {Body} }}";
        }
    }
}
