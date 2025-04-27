using System;
using System.Collections.Generic;

namespace InterpreteCSharp{
    public enum TokenType{
        ASSIGN,
        BANG,
        COMMA,
        EOF,
        EQ,
        IF,
        ELSE,
        NOT_EQ,
        FOR,
        FUNCTION,
        IDENT,
        ILLEGAL,
        INT,
        LBRACE,
        LET,
        LPAREN,
        PLUS,
        MINUS,
        ASTERISK,
        SLASH,
        LT,
        GT,
        LE,
        GE,
        RBRACE,
        RPAREN,
        SEMICOLON,
        WHILE
    }

    public class Token{
        public TokenType Type { get; }
        public string Literal { get; }

        public Token(TokenType type, string literal){
            Type = type;
            Literal = literal;
        }

        public override string ToString() => $"Token({Type}, {Literal})";
    }

    public static class TokenLookup{
        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>{
            { "function", TokenType.FUNCTION },
            { "let",      TokenType.LET      },
            { "if",       TokenType.IF       },
            { "else",     TokenType.ELSE     },
            { "for",      TokenType.FOR      },
            { "while",    TokenType.WHILE    }
        };

        public static TokenType LookupTokenType(string literal){
            return keywords.TryGetValue(literal, out var type)
                ? type
                : TokenType.IDENT;
        }
    }

}