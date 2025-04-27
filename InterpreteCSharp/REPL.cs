using System;
using InterpreteCSharp;

namespace InterpreteCSharp{
    class REPL{
        static void Main(string[] args){
            Console.WriteLine("Bienvenido al intérprete. Escribe 'salir' para terminar.");

            while (true){
                Console.Write("> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Trim().ToLower() == "salir")
                    break;

                var lexer = new Lexer(input);
                Token token;

                do{
                    token = lexer.NextToken();
                    Console.WriteLine(token);
                } while (token.Type != TokenType.EOF);

                Console.WriteLine();
            }
        }
    }
}