// <copyright file="Program.cs" company="MIT License">
// Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

namespace SmallBasic.Cli
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using SmallBasic.Compiler;
    using SmallBasic.Compiler.Runtime;

    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return 0;
            }

            string command = args[0].ToUpperInvariant();

            if (command == "RUN")
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
                string baseDir = Path.GetDirectoryName(Path.GetFullPath(file)) ?? string.Empty;

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
                await engine.Execute().ConfigureAwait(false);
                Console.WriteLine("\nProgram executed successfully.");
                return 0;
            }
            else if (command == "BUILD")
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
                string baseDir = Path.GetDirectoryName(Path.GetFullPath(file)) ?? string.Empty;
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

                string escapedSource = SymbolDisplay.FormatLiteral(source, true);

                string csharpCode = $@"
using System;
using System.Threading.Tasks;
using SmallBasic.Compiler;
using SmallBasic.Compiler.Runtime;

namespace SmallBasicRunner
{{
    public static class Program
    {{
        public static async Task Main(string[] args)
        {{
            string code = {escapedSource};
            var compilation = new SmallBasicCompilation(code);
            var engine = new SmallBasicEngine(compilation, new BuiltInLibraries());
            await engine.Execute();
        }}
    }}
}}";

                var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
                var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode, parseOptions);

                var references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(GetAssemblyLocation(typeof(object))),
                    MetadataReference.CreateFromFile(GetAssemblyLocation(typeof(Console))),
                    MetadataReference.CreateFromFile(GetAssemblyLocation(typeof(Task))),
                    MetadataReference.CreateFromFile(GetAssemblyLocation(Assembly.Load("netstandard"))),
                    MetadataReference.CreateFromFile(GetAssemblyLocation(Assembly.Load("System.Runtime"))),
                    MetadataReference.CreateFromFile(GetAssemblyLocation(typeof(SmallBasicCompilation)))
                };

                var csharpCompilation = CSharpCompilation.Create(
                    Path.GetFileNameWithoutExtension(outFile),
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

                var emitResult = csharpCompilation.Emit(outFile);

                if (!emitResult.Success)
                {
                    Console.WriteLine("Failed to generate executable:");
                    foreach (var diagnostic in emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine($"  {diagnostic.GetMessage()}");
                    }

                    return 1;
                }

                // Copy compiler & utilities assemblies next to output so it runs out-of-the-box
                string outDir = Path.GetDirectoryName(Path.GetFullPath(outFile)) ?? ".";
                string compLoc = GetAssemblyLocation(typeof(SmallBasicCompilation));
                string utilLoc = GetAssemblyLocation(typeof(SmallBasic.Utilities.ExceptionUtilities));
                if (File.Exists(compLoc))
                {
                    File.Copy(compLoc, Path.Combine(outDir, Path.GetFileName(compLoc)), overwrite: true);
                }

                if (File.Exists(utilLoc))
                {
                    File.Copy(utilLoc, Path.Combine(outDir, Path.GetFileName(utilLoc)), overwrite: true);
                }

                string runtimeConfig = Path.Combine(outDir, Path.GetFileNameWithoutExtension(outFile) + ".runtimeconfig.json");
                string jsonContent = @"{
  ""runtimeOptions"": {
    ""tfm"": ""net8.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""8.0.0""
    }
  }
}";
                File.WriteAllText(runtimeConfig, jsonContent);

                Console.WriteLine($"Successfully built executable at '{outFile}'.");
                return 0;
            }
            else
            {
                ShowHelp();
                return 1;
            }
        }

        private static string GetAssemblyLocation(Type type)
        {
            string loc = type.Assembly.Location;
            if (string.IsNullOrEmpty(loc))
            {
                loc = Path.Combine(AppContext.BaseDirectory, type.Assembly.GetName().Name + ".dll");
            }

            return loc;
        }

        private static string GetAssemblyLocation(Assembly assembly)
        {
            string loc = assembly.Location;
            if (string.IsNullOrEmpty(loc))
            {
                loc = Path.Combine(AppContext.BaseDirectory, assembly.GetName().Name + ".dll");
            }

            return loc;
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
