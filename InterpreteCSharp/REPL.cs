using System;
using InterpreteCSharp;
using InterpreteCSharp.AST;
using InterpreteCSharp.Objects;
using InterpreteCSharp.Environment;

namespace InterpreteCSharp
{
    public static class REPL
    {
        public static void Start()
        {
            Console.WriteLine("Welcome to the InterpreteCSharp REPL");
            Console.WriteLine("Type 'exit' to quit");

            var env = new Env();

            while (true)
            {
                Console.Write(">> ");
                var input = Console.ReadLine();

                if (input == null || input.Trim().ToLower() == "exit")
                    break;

                try
                {
                    var lexer = new Lexer(input);
                    var parser = new Parser(lexer);
                    var program = parser.ParseProgram();

                    if (parser.Errors.Count > 0)
                    {
                        Console.WriteLine("Parser errors:");
                        foreach (var err in parser.Errors)
                        {
                            Console.WriteLine($"  ✖ {err}");
                        }
                        continue;
                    }

                    var result = Evaluator.Eval(program, env);
                    if (result != null)
                    {
                        Console.WriteLine(result.Inspect());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Runtime Error: {ex.Message}");
                }
            }
        }
    }
}
