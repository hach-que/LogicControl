namespace LogicControl
{
    using System;

    public class ConstantLogicExpression : TruthfulLogicExpression
    {
        public IComparable Value { get; set; }

        public ConstantLogicExpression(string str)
        {
            this.Value = str;
        }

        public ConstantLogicExpression(float num)
        {
            this.Value = num;
        }

        public override object Result(LogicExecutionState state)
        {
            return this.Value;
        }
    }
}