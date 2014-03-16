namespace LogicControl
{
    public class IfElseLogicStatement : LogicStatement
    {
        public IfElseLogicStatement(LogicExpression condition, LogicStatement statement, LogicStatement elseStatement)
        {
            this.Condition = condition;
            this.Statement = statement;
            this.ElseStatement = elseStatement;
        }

        public LogicExpression Condition { get; set; }

        public LogicStatement Statement { get; set; }

        public LogicStatement ElseStatement { get; set; }

        public override void Execute(LogicExecutionState state)
        {
            if (this.Condition.Truthful(state))
            {
                this.Statement.Execute(state);
            }
            else
            {
                this.ElseStatement.Execute(state);
            }
        }
    }
}