// <copyright file="NewFeaturesTests.cs" company="MIT License">
// Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

namespace SmallBasic.Tests.Runtime
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SmallBasic.Compiler;
    using SmallBasic.Compiler.Scanning;
    using SmallBasic.Utilities;
    using Xunit;

    public sealed class NewFeaturesTests : IClassFixture<CultureFixture>
    {
        [Fact]
        public Task ItEvaluatesFunctionWithParametersAndReturn()
        {
            return new SmallBasicCompilation(@"
Function Add(a, b)
    Return a + b
EndFunction

x = Add(10, 20)
").VerifyRealRuntime(@"
x = 30");
        }

        [Fact]
        public Task ItEvaluatesRecursiveFunction()
        {
            return new SmallBasicCompilation(@"
Fn Factorial(n)
    If n <= 1 Then
        Return 1
    EndIf
    Return n * Factorial(n - 1)
EndFn

result = Factorial(5)
").VerifyRealRuntime(@"
result = 120");
        }

        [Fact]
        public Task ItEvaluatesImportWithFileResolver()
        {
            var files = new Dictionary<string, string>
            {
                {
                    "math_helper.sb", @"
Function Square(x)
    Return x * x
EndFunction
"
                }
            };

            var compilation = new SmallBasicCompilation(@"
Import ""math_helper.sb""
ans = Square(6)
", fileResolver: path => files.ContainsKey(path) ? files[path] : null);

            return compilation.VerifyRealRuntime(@"
ans = 36");
        }

        [Fact]
        public void ItProvidesCompletionsForNewKeywords()
        {
            var compilation = new SmallBasicCompilation(@"F");
            var items = compilation.ProvideCompletionItems((0, 1));
            Assert.Contains(items, item => item.Label == "Function");
            Assert.Contains(items, item => item.Label == "Fn");
        }
    }
}
