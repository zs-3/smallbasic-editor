// <copyright file="OtherInstructions.cs" company="MIT License">
// Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

namespace SmallBasic.Compiler.Runtime
{
    using System;
    using System.Threading.Tasks;
    using SmallBasic.Compiler.Scanning;

    internal sealed class InvokeSubModuleInstruction : BaseNonJumpInstruction
    {
        private readonly string subModuleName;
        private readonly bool expectsReturn;

        public InvokeSubModuleInstruction(string subModuleName, bool expectsReturn, TextRange range)
            : base(range)
        {
            this.subModuleName = subModuleName;
            this.expectsReturn = expectsReturn;
        }

        protected override void Execute(SmallBasicEngine engine)
        {
            var module = engine.Modules[this.subModuleName];
            int paramCount = module.Parameters.Count;
            var args = new BaseValue[paramCount];
            for (int i = paramCount - 1; i >= 0; i--)
            {
                args[i] = engine.EvaluationStack.Pop();
            }

            for (int i = 0; i < paramCount; i++)
            {
                engine.Memory[module.Parameters[i]] = args[i];
            }

            Frame frame = new Frame(module, expectsReturn: this.expectsReturn);
            engine.ExecutionStack.AddLast(frame);
        }
    }

    internal sealed class ReturnInstruction : BaseNonJumpInstruction
    {
        private readonly bool hasExpression;

        public ReturnInstruction(bool hasExpression, TextRange range)
            : base(range)
        {
            this.hasExpression = hasExpression;
        }

        protected override void Execute(SmallBasicEngine engine)
        {
            BaseValue returnValue = this.hasExpression ? engine.EvaluationStack.Pop() : StringValue.Empty;
            if (engine.ExecutionStack.Count > 0)
            {
                var frame = engine.ExecutionStack.Last.Value;
                bool expectsReturn = frame.ExpectsReturn;
                engine.ExecutionStack.RemoveLast();
                if (expectsReturn)
                {
                    engine.EvaluationStack.Push(returnValue);
                }
            }
        }
    }

    internal sealed class MethodInvocationInstruction : BaseAsyncNonJumpInstruction
    {
        private readonly string library;
        private readonly string method;

        public MethodInvocationInstruction(string library, string method, TextRange range)
            : base(range)
        {
            this.library = library;
            this.method = method;
        }

        protected override Task Execute(SmallBasicEngine engine)
        {
            return Libraries.Types[this.library].Methods[this.method].Execute(engine);
        }
    }

    internal sealed class StorePropertyInstruction : BaseAsyncNonJumpInstruction
    {
        private readonly string library;
        private readonly string property;

        public StorePropertyInstruction(string library, string property, TextRange range)
            : base(range)
        {
            this.library = library;
            this.property = property;
        }

        protected override Task Execute(SmallBasicEngine engine)
        {
            return Libraries.Types[this.library].Properties[this.property].Setter(engine);
        }
    }

    internal sealed class LoadPropertyInstruction : BaseAsyncNonJumpInstruction
    {
        private readonly string library;
        private readonly string property;

        public LoadPropertyInstruction(string library, string property, TextRange range)
            : base(range)
        {
            this.library = library;
            this.property = property;
        }

        protected override Task Execute(SmallBasicEngine engine)
        {
            return Libraries.Types[this.library].Properties[this.property].Getter(engine);
        }
    }

    internal sealed class SetEventCallBackInstruction : BaseNonJumpInstruction
    {
        private readonly string library;
        private readonly string eventName;
        private readonly string subModule;

        public SetEventCallBackInstruction(string library, string eventName, string subModule, TextRange range)
            : base(range)
        {
            this.library = library;
            this.eventName = eventName;
            this.subModule = subModule;
        }

        protected override void Execute(SmallBasicEngine engine)
        {
            engine.SetEventCallback(this.library, this.eventName, this.subModule);
        }
    }
}
