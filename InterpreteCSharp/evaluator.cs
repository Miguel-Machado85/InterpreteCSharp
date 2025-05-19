using System;
using System.Collections.Generic;
using InterpreteCSharp.AST;
using InterpreteCSharp.Objects;
using InterpreteCSharp.Environment;

namespace InterpreteCSharp
{
    public static class Evaluator
    {
        private static readonly Env GlobalEnv = new();

        public static IObject Eval(INode node, Env env = null)
        {
            env ??= GlobalEnv;

            return node switch
            {
                Script p => EvalScript(p, env),
                ExpressionStatement exprStmt => Eval(exprStmt.Expression, env),
                LetStatement letStmt => EvalLetStatement(letStmt, env),
                IntegerLiteral intLit => new IntegerObj(intLit.Value),
                PrefixExpression prefix => EvalPrefixExpression(prefix, env),
                InfixExpression infix => EvalInfixExpression(infix, env),
                IfExpression ifExpr => EvalIfExpression(ifExpr, env),
                BlockStatement block => EvalBlockStatement(block, env),
                Identifier ident => env.Get(ident.Value),
                WhileStatement whileStmt => EvalWhileStatement(whileStmt, env),
                ForStatement forStmt => EvalForStatement(forStmt, env),
                AssignStatement assign => EvalAssignStatement(assign, env),
                FunctionLiteral func => new FunctionObj(func.Parameters, func.Body, env),
                CallExpression call => EvalCallExpression(call, env),
                _ => null
            };
        }

        private static IObject EvalScript(Script program, Env env)
        {
            IObject result = null;
            foreach (var stmt in program.Statements)
            {
                result = Eval(stmt, env);
            }
            return result;
        }

        private static IObject EvalLetStatement(LetStatement stmt, Env env)
        {
            var value = Eval(stmt.Value, env);
            env.Set(stmt.Name.Value, value);
            return value;
        }

        private static IObject EvalAssignStatement(AssignStatement stmt, Env env)
        {
            var value = Eval(stmt.Value, env);
            env.Set(stmt.Name.Value, value);
            return value;
        }

        private static IObject EvalPrefixExpression(PrefixExpression prefix, Env env)
        {
            var right = Eval(prefix.Right, env);
            return prefix.Operator switch
            {
                "!" => EvalBangOperator(right),
                "-" when right.Type() == ObjectType.INTEGER => new IntegerObj(-((IntegerObj)right).Value),
                _ => null
            };
        }

        private static IObject EvalBangOperator(IObject obj)
        {
            return obj switch
            {
                BooleanObj b when b.Value => BooleanObj.False,
                BooleanObj b when !b.Value => BooleanObj.True,
                IntegerObj i when i.Value == 0 => BooleanObj.True,
                _ => BooleanObj.False
            };
        }

        private static IObject EvalInfixExpression(InfixExpression infix, Env env)
        {
            var left = Eval(infix.Left, env);
            var right = Eval(infix.Right, env);

            if (left.Type() == ObjectType.INTEGER && right.Type() == ObjectType.INTEGER)
                return EvalIntegerInfix(infix.Operator, (IntegerObj)left, (IntegerObj)right);

            if (infix.Operator == "==") return left.Equals(right) ? BooleanObj.True : BooleanObj.False;
            if (infix.Operator == "!=") return !left.Equals(right) ? BooleanObj.True : BooleanObj.False;

            return null;
        }

        private static IObject EvalIntegerInfix(string op, IntegerObj left, IntegerObj right)
        {
            return op switch
            {
                "+" => new IntegerObj(left.Value + right.Value),
                "-" => new IntegerObj(left.Value - right.Value),
                "*" => new IntegerObj(left.Value * right.Value),
                "/" => new IntegerObj(left.Value / right.Value),
                "<" => left.Value < right.Value ? BooleanObj.True : BooleanObj.False,
                ">" => left.Value > right.Value ? BooleanObj.True : BooleanObj.False,
                "<=" => left.Value <= right.Value ? BooleanObj.True : BooleanObj.False,
                ">=" => left.Value >= right.Value ? BooleanObj.True : BooleanObj.False,
                "==" => left.Value == right.Value ? BooleanObj.True : BooleanObj.False,
                "!=" => left.Value != right.Value ? BooleanObj.True : BooleanObj.False,
                _ => null
            };
        }

        private static IObject EvalIfExpression(IfExpression expr, Env env)
        {
            var condition = Eval(expr.Condition, env);
            if (IsTruthy(condition))
                return Eval(expr.Consequence, env);
            else if (expr.Alternative != null)
                return Eval(expr.Alternative, env);
            return null;
        }

        private static IObject EvalBlockStatement(BlockStatement block, Env env)
        {
            IObject result = null;
            foreach (var stmt in block.Statements)
            {
                result = Eval(stmt, env);
            }
            return result;
        }

        private static IObject EvalWhileStatement(WhileStatement stmt, Env env)
        {
            IObject result = null;
            while (IsTruthy(Eval(stmt.Condition, env)))
            {
                result = Eval(stmt.Body, env);
            }
            return result;
        }

        private static IObject EvalForStatement(ForStatement stmt, Env env)
        {
            Eval(stmt.Init, env);
            IObject result = null;

            while (IsTruthy(Eval(stmt.Condition, env)))
            {
                result = Eval(stmt.Body, env);
                Eval(stmt.Post, env);
            }

            return result;
        }

        private static bool IsTruthy(IObject obj)
        {
            return obj switch
            {
                BooleanObj b => b.Value,
                IntegerObj i => i.Value != 0,
                _ => obj != null
            };
        }

        private static IObject EvalCallExpression(CallExpression expr, Env env)
        {
            var function = Eval(expr.Function, env);
            var args = new List<IObject>();
            foreach (var arg in expr.Arguments)
                args.Add(Eval(arg, env));

            return ApplyFunction(function, args);
        }

        private static IObject ApplyFunction(IObject fn, List<IObject> args)
        {
            if (fn is FunctionObj func)
            {
                var extendedEnv = new Env(func.Env);
                for (int i = 0; i < func.Parameters.Count; i++)
                {
                    extendedEnv.Set(func.Parameters[i].Value, args[i]);
                }
                return Eval(func.Body, extendedEnv);
            }
            return null;
        }
    }
}
