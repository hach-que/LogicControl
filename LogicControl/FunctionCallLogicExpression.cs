namespace LogicControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    public class FunctionCallLogicExpression : TruthfulLogicExpression
    {
        public string Name { get; set; }

        public List<LogicExpression> Arguments { get; set; }

        public FunctionCallLogicExpression(string name, List<LogicExpression> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }

        public override object Result(LogicExecutionState state)
        {
            var structType = state.Structures.FirstOrDefault(x => x.Name == this.Name);

            if (structType != null)
            {
                return new LogicStructureInstance(structType);
            }

            var values = this.Arguments.Select(x => x.Result(state)).ToArray();

            switch (this.Name)
            {
                case "float":
                    return Convert.ToSingle(values[0]);
                case "string":
                    return Convert.ToString(values[0]);
            }

            throw new NotImplementedException();
        }
    }
}