using System;
using System.Collections.Generic;

namespace InterpreteCSharp{
    public class Lexer{
        private readonly string source;
        private char currentChar;
        private int position;
        private int readPosition;

        public Lexer(string source){
            this.source = source;
            position = 0;
            readPosition = 0;
            ReadChar();
        }

        public Token NextToken(){
            SkipWhitespace();

            Token token;
            switch (currentChar){
                case '=':
                    if (PeekChar() == '='){
                        ReadChar();
                        token = new Token(TokenType.EQ, "==");
                    }
                    else{
                        token = new Token(TokenType.ASSIGN, currentChar.ToString());
                    }
                    break;

                case '!':
                    if (PeekChar() == '='){
                        ReadChar();
                        token = new Token(TokenType.NOT_EQ, "!=");
                    }
                    else{
                        token = new Token(TokenType.BANG, currentChar.ToString());
                    }
                    break;

                case '+': token = new Token(TokenType.PLUS, currentChar.ToString()); break;
                case '-': token = new Token(TokenType.MINUS, currentChar.ToString()); break;
                case '*': token = new Token(TokenType.ASTERISK, currentChar.ToString()); break;
                case '/': token = new Token(TokenType.SLASH, currentChar.ToString()); break;

                case '<':
                    if (PeekChar() == '='){
                        ReadChar();
                        token = new Token(TokenType.LE, "<=");
                    }
                    else{
                        token = new Token(TokenType.LT, currentChar.ToString());
                    }
                    break;

                case '>':
                    if (PeekChar() == '='){
                        ReadChar();
                        token = new Token(TokenType.GE, ">=");
                    }
                    else{
                        token = new Token(TokenType.GT, currentChar.ToString());
                    }
                    break;

                case '(':
                    token = new Token(TokenType.LPAREN, currentChar.ToString());
                    break;
                case ')':
                    token = new Token(TokenType.RPAREN, currentChar.ToString());
                    break;
                case '{':
                    token = new Token(TokenType.LBRACE, currentChar.ToString());
                    break;
                case '}':
                    token = new Token(TokenType.RBRACE, currentChar.ToString());
                    break;
                case ',':
                    token = new Token(TokenType.COMMA, currentChar.ToString());
                    break;
                case ';':
                    token = new Token(TokenType.SEMICOLON, currentChar.ToString());
                    break;

                case '\0':
                    token = new Token(TokenType.EOF, string.Empty);
                    break;

                default:
                    if (IsDigit(currentChar)){
                        string number = ReadNumber();
                        return new Token(TokenType.INT, number);
                    }
                    else if (IsLetter(currentChar)){
                        string literal = ReadLiteral();
                        TokenType type = TokenLookup.LookupTokenType(literal);
                        return new Token(type, literal);
                    }
                    else{
                        token = new Token(TokenType.ILLEGAL, currentChar.ToString());
                    }
                    break;
            }

            ReadChar();
            return token;
        }

        private void SkipWhitespace(){
            while (char.IsWhiteSpace(currentChar)){
                ReadChar();
            }
        }

        private void ReadChar(){
            if (readPosition >= source.Length){
                currentChar = '\0';
            }
            else{
                currentChar = source[readPosition];
            }
            position = readPosition;
            readPosition++;
        }

        private char PeekChar(){
            if (readPosition >= source.Length){
                return '\0';
            }
            return source[readPosition];
        }

        private bool IsDigit(char c) => char.IsDigit(c);

        private bool IsLetter(char c) => char.IsLetter(c);

        private string ReadNumber(){
            int start = position;
            while (IsDigit(currentChar)){
                ReadChar();
            }
            return source[start..position];
        }

        private string ReadLiteral(){
            int start = position;
            while (IsLetter(currentChar) || IsDigit(currentChar)){
                ReadChar();
            }
            return source[start..position];
        }
    }
}