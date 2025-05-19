using System;
using System.Collections.Generic;
using InterpreteCSharp.AST;

namespace InterpreteCSharp
{
    public class Parser
    {
        private readonly Lexer lexer;
        private Token currentToken;
        private Token peekToken;
        public List<string> Errors { get; } = new();

        private static readonly Dictionary<TokenType, int> PRECEDENCES = new()
        {
            [TokenType.EQ] = 2,
            [TokenType.NOT_EQ] = 2,
            [TokenType.LT] = 2,
            [TokenType.GT] = 2,
            [TokenType.LE] = 2,
            [TokenType.GE] = 2,
            [TokenType.PLUS] = 3,
            [TokenType.MINUS] = 3,
            [TokenType.SLASH] = 4,
            [TokenType.ASTERISK] = 4,
            [TokenType.LPAREN] = 5
        };

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            Advance();
            Advance();
        }

        private void Advance()
        {
            currentToken = peekToken;
            peekToken = lexer.NextToken();
        }

        public Script ParseProgram()
        {
            var program = new Script();

            while (currentToken.Type != TokenType.EOF)
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    program.Statements.Add(stmt);

                Advance();
            }

            return program;
        }

        private IStatement ParseStatement()
        {
            return currentToken.Type switch
            {
                TokenType.LET => ParseLetStatement(),
                TokenType.WHILE => ParseWhileStatement(),
                TokenType.FOR => ParseForStatement(),
                TokenType.IDENT when peekToken.Type == TokenType.ASSIGN => ParseAssignStatement(),
                _ => ParseExpressionStatement()
            };
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var token = currentToken;
            var expression = ParseExpression(0);

            if (peekToken.Type == TokenType.SEMICOLON)
                Advance();

            return new ExpressionStatement(token, expression);
        }

        private IExpression ParseExpression(int precedence)
        {
            var prefixFns = PrefixParseFns();
            if (!prefixFns.TryGetValue(currentToken.Type, out var prefix))
            {
                Errors.Add($"No prefix parse function for {currentToken.Type}");
                return null;
            }

            var left = prefix();

            while (peekToken.Type != TokenType.SEMICOLON && precedence < PeekPrecedence())
            {
                var infixFns = InfixParseFns();
                if (!infixFns.TryGetValue(peekToken.Type, out var infix))
                    return left;

                Advance();
                left = infix(left);
            }

            return left;
        }

        // === Prefix Parse Functions Map ===
        private Dictionary<TokenType, Func<IExpression>> PrefixParseFns() => new()
        {
            [TokenType.IDENT] = ParseIdentifier,
            [TokenType.INT] = ParseIntegerLiteral,
            [TokenType.BANG] = ParsePrefixExpression,
            [TokenType.MINUS] = ParsePrefixExpression,
            [TokenType.IF] = ParseIfExpression,
            [TokenType.FUNCTION] = ParseFunctionLiteral,
            [TokenType.LPAREN] = ParseGroupedExpression
        };

        // === Infix Parse Functions Map ===
        private Dictionary<TokenType, Func<IExpression, IExpression>> InfixParseFns() => new()
        {
            [TokenType.PLUS] = ParseInfixExpression,
            [TokenType.MINUS] = ParseInfixExpression,
            [TokenType.SLASH] = ParseInfixExpression,
            [TokenType.ASTERISK] = ParseInfixExpression,
            [TokenType.EQ] = ParseInfixExpression,
            [TokenType.NOT_EQ] = ParseInfixExpression,
            [TokenType.LT] = ParseInfixExpression,
            [TokenType.GT] = ParseInfixExpression,
            [TokenType.LE] = ParseInfixExpression,
            [TokenType.GE] = ParseInfixExpression,
            [TokenType.LPAREN] = ParseCallExpression
        };

        // === Expression Parsers ===
        private IExpression ParseIdentifier() =>
            new Identifier(currentToken, currentToken.Literal);

        private IExpression ParseIntegerLiteral()
        {
            if (!int.TryParse(currentToken.Literal, out var value))
            {
                Errors.Add($"Could not parse {currentToken.Literal} as integer.");
                return null;
            }

            return new IntegerLiteral(currentToken, value);
        }

        private IExpression ParsePrefixExpression()
        {
            var token = currentToken;
            var op = token.Literal;
            Advance();
            var right = ParseExpression(5);
            return new PrefixExpression(token, op, right);
        }

        private IExpression ParseInfixExpression(IExpression left)
        {
            var token = currentToken;
            var op = token.Literal;
            var precedence = CurrentPrecedence();
            Advance();
            var right = ParseExpression(precedence);
            return new InfixExpression(token, left, op, right);
        }

        private int PeekPrecedence() =>
            PRECEDENCES.TryGetValue(peekToken.Type, out var prec) ? prec : 0;

        private int CurrentPrecedence() =>
            PRECEDENCES.TryGetValue(currentToken.Type, out var prec) ? prec : 0;

        private bool ExpectPeek(TokenType type)
        {
            if (peekToken.Type == type)
            {
                Advance();
                return true;
            }

            Errors.Add($"Expected next token to be {type}, got {peekToken.Type} instead.");
            return false;
        }

        // === Statement Parsers ===

        private LetStatement ParseLetStatement()
        {
            var token = currentToken;
            if (!ExpectPeek(TokenType.IDENT)) return null;
            var name = new Identifier(currentToken, currentToken.Literal);
            if (!ExpectPeek(TokenType.ASSIGN)) return null;
            Advance();
            var value = ParseExpression(0);
            if (peekToken.Type == TokenType.SEMICOLON) Advance();
            return new LetStatement(token, name, value);
        }

        private AssignStatement ParseAssignStatement()
        {
            var name = new Identifier(currentToken, currentToken.Literal);
            if (!ExpectPeek(TokenType.ASSIGN)) return null;
            var token = currentToken;
            Advance();
            var value = ParseExpression(0);
            if (peekToken.Type == TokenType.SEMICOLON) Advance();
            return new AssignStatement(token, name, value);
        }

        private WhileStatement ParseWhileStatement()
        {
            var token = currentToken;
            if (!ExpectPeek(TokenType.LPAREN)) return null;
            Advance();
            var condition = ParseExpression(0);
            if (!ExpectPeek(TokenType.RPAREN)) return null;
            if (!ExpectPeek(TokenType.LBRACE)) return null;
            var body = ParseBlockStatement();
            return new WhileStatement(token, condition, body);
        }

        private ForStatement ParseForStatement()
        {
            var token = currentToken;
            if (!ExpectPeek(TokenType.LPAREN)) return null;
            Advance();

            IStatement init = null;
            if (currentToken.Type == TokenType.LET)
                init = ParseLetStatement();
            else if (currentToken.Type == TokenType.IDENT && peekToken.Type == TokenType.ASSIGN)
                init = ParseAssignStatement();
            else
            {
                Errors.Add($"Expected init statement in 'for', got {currentToken.Type}");
                return null;
            }

            if (!ExpectPeek(TokenType.SEMICOLON)) return null;
            Advance();
            var condition = ParseExpression(0);
            if (!ExpectPeek(TokenType.SEMICOLON)) return null;
            Advance();

            IStatement post = null;
            if (currentToken.Type == TokenType.LET)
                post = ParseLetStatement();
            else if (currentToken.Type == TokenType.IDENT && peekToken.Type == TokenType.ASSIGN)
                post = ParseAssignStatement();
            else
            {
                Errors.Add($"Expected post statement in 'for', got {currentToken.Type}");
                return null;
            }

            if (currentToken.Type != TokenType.RPAREN && !ExpectPeek(TokenType.RPAREN))
                return null;

            if (!ExpectPeek(TokenType.LBRACE)) return null;

            var body = ParseBlockStatement();
            return new ForStatement(token, init, condition, post, body);
        }

        private BlockStatement ParseBlockStatement()
        {
            var token = currentToken;
            var block = new BlockStatement(token);
            Advance();

            while (currentToken.Type != TokenType.RBRACE && currentToken.Type != TokenType.EOF)
            {
                var stmt = ParseStatement();
                if (stmt != null) block.Statements.Add(stmt);
                Advance();
            }

            return block;
        }

        private IExpression ParseIfExpression()
        {
            var token = currentToken;
            if (!ExpectPeek(TokenType.LPAREN)) return null;
            Advance();
            var condition = ParseExpression(0);
            if (!ExpectPeek(TokenType.RPAREN)) return null;
            if (!ExpectPeek(TokenType.LBRACE)) return null;
            var consequence = ParseBlockStatement();

            BlockStatement alternative = null;
            if (peekToken.Type == TokenType.ELSE)
            {
                Advance();
                if (peekToken.Type == TokenType.IF)
                {
                    Advance();
                    alternative = new BlockStatement(currentToken); // For simplicity
                }
                else if (ExpectPeek(TokenType.LBRACE))
                {
                    alternative = ParseBlockStatement();
                }
            }

            return new IfExpression(token, condition, consequence, alternative);
        }

        private IExpression ParseFunctionLiteral()
        {
            var token = currentToken;
            if (!ExpectPeek(TokenType.LPAREN)) return null;
            var parameters = ParseFunctionParameters();
            if (!ExpectPeek(TokenType.LBRACE)) return null;
            var body = ParseBlockStatement();
            return new FunctionLiteral(token, parameters, body);
        }

        private List<Identifier> ParseFunctionParameters()
        {
            var parameters = new List<Identifier>();
            if (peekToken.Type == TokenType.RPAREN)
            {
                Advance();
                return parameters;
            }

            Advance();
            parameters.Add(new Identifier(currentToken, currentToken.Literal));

            while (peekToken.Type == TokenType.COMMA)
            {
                Advance();
                Advance();
                parameters.Add(new Identifier(currentToken, currentToken.Literal));
            }

            if (!ExpectPeek(TokenType.RPAREN)) return null;

            return parameters;
        }

        private IExpression ParseCallExpression(IExpression function)
        {
            var token = currentToken;
            var arguments = ParseExpressionList(TokenType.RPAREN);
            return new CallExpression(token, function, arguments);
        }

        private List<IExpression> ParseExpressionList(TokenType end)
        {
            var args = new List<IExpression>();

            if (peekToken.Type == end)
            {
                Advance();
                return args;
            }

            Advance();
            args.Add(ParseExpression(0));

            while (peekToken.Type == TokenType.COMMA)
            {
                Advance();
                Advance();
                args.Add(ParseExpression(0));
            }

            if (!ExpectPeek(end)) return null;
            return args;
        }

        private IExpression ParseGroupedExpression()
        {
            Advance(); // consumir el '('
            var expr = ParseExpression(0);

            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }

            return expr;
        }
    }
}
