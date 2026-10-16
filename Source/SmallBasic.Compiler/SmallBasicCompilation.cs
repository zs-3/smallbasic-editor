// <copyright file="SmallBasicCompilation.cs" company="MIT License">
// Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

namespace SmallBasic.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SmallBasic.Compiler.Binding;
    using SmallBasic.Compiler.Diagnostics;
    using SmallBasic.Compiler.Parsing;
    using SmallBasic.Compiler.Scanning;
    using SmallBasic.Compiler.Services;
    using SmallBasic.Utilities;

    public sealed class SmallBasicCompilation
    {
        private readonly DiagnosticBag diagnostics;
        private readonly bool isRunningOnDesktop;

        private readonly Scanner scanner;
        private readonly Parser parser;
        private readonly Binder binder;

        private readonly Lazy<RuntimeAnalysis> lazyAnalysis;

        public SmallBasicCompilation(string text, Func<string, string> fileResolver = null, string baseDirectory = "")
#if IsBuildingForDesktop
           : this(text, isRunningOnDesktop: true, fileResolver: fileResolver, baseDirectory: baseDirectory)
#else
           : this(text, isRunningOnDesktop: false, fileResolver: fileResolver, baseDirectory: baseDirectory)
#endif
        {
        }

        public SmallBasicCompilation(string text, bool isRunningOnDesktop, Func<string, string> fileResolver = null, string baseDirectory = "")
        {
            this.diagnostics = new DiagnosticBag();
            this.isRunningOnDesktop = isRunningOnDesktop;

            this.Text = ProcessImports(text, fileResolver, baseDirectory, new HashSet<string>(), this.diagnostics);

            this.scanner = new Scanner(this.Text, this.diagnostics);
            this.parser = new Parser(this.scanner.Tokens, this.diagnostics);
            this.binder = new Binder(this.parser.SyntaxTree, this.diagnostics, isRunningOnDesktop);

            this.lazyAnalysis = new Lazy<RuntimeAnalysis>(() => new RuntimeAnalysis(this));
        }

        private static string ProcessImports(string sourceText, Func<string, string> fileResolver, string baseDirectory, HashSet<string> visitedPaths, DiagnosticBag diagnostics)
        {
            if (string.IsNullOrEmpty(sourceText) || !sourceText.Contains("Import"))
            {
                return sourceText;
            }

            var lines = sourceText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var resultLines = new List<string>();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("Import ", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("Import\t", StringComparison.OrdinalIgnoreCase))
                {
                    string path = trimmed.Substring(6).Trim();
                    if (path.StartsWith("\"") && path.EndsWith("\"") && path.Length >= 2)
                    {
                        path = path.Substring(1, path.Length - 2);
                    }

                    string fullPath = System.IO.Path.IsPathRooted(path) ? path : System.IO.Path.Combine(baseDirectory ?? string.Empty, path);
                    string normalizedPath = fullPath;
                    try
                    {
                        normalizedPath = System.IO.Path.GetFullPath(fullPath);
                    }
                    catch
                    {
                    }

                    if (!visitedPaths.Contains(normalizedPath))
                    {
                        visitedPaths.Add(normalizedPath);

                        string importedContent = null;
                        if (!fileResolver.IsDefault())
                        {
                            importedContent = fileResolver(path);
                        }
                        else if (System.IO.File.Exists(fullPath))
                        {
                            importedContent = System.IO.File.ReadAllText(fullPath);
                        }

                        if (!importedContent.IsDefault())
                        {
                            string newBaseDir = System.IO.Path.GetDirectoryName(fullPath) ?? string.Empty;
                            string processed = ProcessImports(importedContent, fileResolver, newBaseDir, visitedPaths, diagnostics);
                            resultLines.Add(processed);
                        }
                    }
                }
                else
                {
                    resultLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, resultLines);
        }

        public string Text { get; private set; }

        public RuntimeAnalysis Analysis => this.lazyAnalysis.Value;

        public IReadOnlyList<Diagnostic> Diagnostics => this.diagnostics.Contents;

        internal BoundStatementBlock MainModule => this.binder.MainModule;

        internal IReadOnlyDictionary<string, BoundSubModule> SubModules => this.binder.SubModules;

        public MonacoCompletionItem[] ProvideCompletionItems(TextPosition position) => CompletionItemProvider.Provide(this.parser, this.binder, position);

        public string[] ProvideHover(TextPosition position) => HoverProvider.Provide(this.diagnostics, this.parser, position);
    }
}
