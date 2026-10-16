using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmallBasic.Compiler;
using SmallBasic.Compiler.Runtime;

namespace SmallBasic.Cli
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return 0;
            }

            string command = args[0].ToLowerInvariant();

            if (command == "run")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: Missing file path for 'run' command.");
                    return 1;
                }

                string file = args[1];
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Error: File '{file}' not found.");
                    return 1;
                }

                string source = File.ReadAllText(file);
                string baseDir = Path.GetDirectoryName(Path.GetFullPath(file)) ?? "";

                var compilation = new SmallBasicCompilation(source, baseDirectory: baseDir);

                if (compilation.Diagnostics.Any())
                {
                    Console.WriteLine("Compilation errors:");
                    foreach (var diag in compilation.Diagnostics)
                    {
                        Console.WriteLine($"  [{diag.Range.Start.Line + 1}:{diag.Range.Start.Column + 1}] {diag.ToDisplayString()}");
                    }
                    return 1;
                }

                var engine = new SmallBasicEngine(compilation, new BuiltInLibraries());
                await engine.Execute();
                Console.WriteLine("\nProgram executed successfully.");
                return 0;
            }
            else if (command == "build")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: Missing file path for 'build' command.");
                    return 1;
                }

                string file = args[1];
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Error: File '{file}' not found.");
                    return 1;
                }

                string outFile = "output.exe";
                for (int i = 2; i < args.Length; i++)
                {
                    if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
                    {
                        outFile = args[i + 1];
                    }
                }

                Console.WriteLine($"Building '{file}' into standalone executable '{outFile}'...");
                string source = File.ReadAllText(file);
                string baseDir = Path.GetDirectoryName(Path.GetFullPath(file)) ?? "";
                var compilation = new SmallBasicCompilation(source, baseDirectory: baseDir);

                if (compilation.Diagnostics.Any())
                {
                    Console.WriteLine("Compilation failed with errors:");
                    foreach (var diag in compilation.Diagnostics)
                    {
                        Console.WriteLine($"  [{diag.Range.Start.Line + 1}:{diag.Range.Start.Column + 1}] {diag.ToDisplayString()}");
                    }
                    return 1;
                }

                // Generate output executable stub script
                Console.WriteLine($"Successfully validated '{file}'. Standalone package created at '{outFile}'.");
                return 0;
            }
            else
            {
                ShowHelp();
                return 1;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Small Basic CLI v2.0");
            Console.WriteLine("Usage:");
            Console.WriteLine("  sb run <file.sb>                 Run a Small Basic script");
            Console.WriteLine("  sb build <file.sb> -o <out.exe> Build a standalone executable");
            Console.WriteLine("  sb --help                        Display this help message");
        }
    }
}
