namespace LogicControl
{
    using System.Collections.Generic;
    using System.Linq;

    public class LogicUnmappedScriptInstance
    {
        private readonly List<LogicStructure> m_Structures;

        private readonly List<LogicFunction> m_Functions;

        public LogicUnmappedScriptInstance(List<LogicStructure> logicStructures, List<LogicFunction> logicFunctions)
        {
            this.m_Structures = logicStructures;
            this.m_Functions = logicFunctions;
        }

        public Dictionary<string, object> Execute(string name, Dictionary<string, object> semanticInputs)
        {
            var executionState = new LogicExecutionState();
            executionState.Structures = this.m_Structures;
            executionState.Functions = this.m_Functions;

            var function = this.m_Functions.First(x => x.Name == name);

            foreach (var parameter in function.Parameters)
            {
                if (parameter.Semantic != null)
                {
                    // Map the semantic input to this parameter.
                    executionState.Variables[parameter.Name] = semanticInputs[parameter.Semantic];
                }
                else
                {
                    // Assume structure
                    // TODO: Validation
                    var structType = this.m_Structures.First(x => x.Name == parameter.Type);

                    var structObj = new LogicStructureInstance(structType);

                    foreach (var field in structType.Fields)
                    {
                        structObj.Fields[field] = semanticInputs[field.Semantic];
                    }

                    executionState.Variables[parameter.Name] = structObj;
                }
            }

            var result = function.Result(executionState);

            if (function.ReturnSemantic != null)
            {
                return new Dictionary<string, object>
                {
                    { function.ReturnSemantic, result }
                };
            }
            else
            {
                // TODO: Validation
                var results = new Dictionary<string, object>();
                var structResult = (LogicStructureInstance)result;
                foreach (var kv in structResult.Fields)
                {
                    results[kv.Key.Semantic] = kv.Value;
                }
                return results;
            }
        }
    }
}