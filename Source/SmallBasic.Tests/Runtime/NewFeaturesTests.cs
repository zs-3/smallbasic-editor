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
a = 10
b = 20
x = 30");
        }

        [Fact]
        public Task ItEvaluatesSubroutine()
        {
            return new SmallBasicCompilation(@"
Sub Multiply
    res = a * b
EndSub

a = 5
b = 24
Multiply()
").VerifyRealRuntime(@"
a = 5
b = 24
res = 120");
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
x = 6
ans = 36");
        }

        [Fact]
        public Task ItEvaluatesSelectCaseStatement()
        {
            return new SmallBasicCompilation(@"
val = 2
res = """"
Select Case val
    Case 1
        res = ""one""
    Case 2
        res = ""two""
    Else
        res = ""other""
EndSelect
").VerifyRealRuntime(@"
val = 2
res = two");
        }

        [Fact]
        public Task ItEvaluatesTryCatchStatement()
        {
            return new SmallBasicCompilation(@"
a = 10
b = 20
Try
    c = a + b
Catch ex
    c = 0
EndTry
").VerifyRealRuntime(@"
a = 10
b = 20
c = 30");
        }

        [Fact]
        public void ItProvidesCompletionsForNewKeywords()
        {
            var compilation = new SmallBasicCompilation(@"F");
            var items = compilation.ProvideCompletionItems((0, 1));
            FluentAssertions.AssertionExtensions.Should(items.Any(item => item.label == "Function")).BeTrue();
            FluentAssertions.AssertionExtensions.Should(items.Any(item => item.label == "Fn")).BeTrue();
        }
    }
}
