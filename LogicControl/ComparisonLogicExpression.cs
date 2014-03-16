namespace LogicControl
{
    using System;

    public class ComparisonLogicExpression : LogicExpression
    {
        public ComparisonLogicExpression(
            LogicExpression leftHandExpression, 
            string op, 
            LogicExpression rightHandExpression)
        {
            this.LeftHandExpression = leftHandExpression;
            this.Op = op;
            this.RightHandExpression = rightHandExpression;
        }

        public LogicExpression LeftHandExpression { get; set; }

        public string Op { get; set; }

        public LogicExpression RightHandExpression { get; set; }

        public override object Result(LogicExecutionState state)
        {
            return this.Truthful(state);
        }

        public override bool Truthful(LogicExecutionState state)
        {
            var leftObj = this.LeftHandExpression.Result(state);
            var rightObj = this.RightHandExpression.Result(state);

            if (leftObj is IComparable && rightObj is IComparable)
            {
                var comparison = ((IComparable)leftObj).CompareTo(rightObj);

                switch (this.Op)
                {
                    case ">":
                        return comparison > 0;
                    case "<":
                        return comparison < 0;
                    case "<=":
                        return comparison >= 0;
                    case ">=":
                        return comparison <= 0;
                    case "==":
                        return comparison == 0;
                    case "!=":
                        return comparison != 0;
                }

                throw new InvalidOperationException();
            }

            throw new InvalidCastException();
        }
    }
}