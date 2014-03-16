namespace LogicControl
{
    using System.Collections.Generic;

    public class LogicExecutionState
    {
        public LogicExecutionState()
        {
            this.Variables = new Dictionary<string, object>();
        }

        public List<LogicFunction> Functions { get; set; }

        public object Result { get; set; }

        public bool Finished { get; set; }

        public List<LogicStructure> Structures { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public void Return(object result)
        {
            this.Result = result;
            this.Finished = true;
        }
    }
}