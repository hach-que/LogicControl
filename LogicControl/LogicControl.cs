using System;

namespace LogicControl
{
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using GOLD;

    public class LogicField
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Semantic { get; set; }
    }

    public class LogicStructure
    {
        public string Name { get; set; }

        public List<LogicField> Fields { get; set; } 
    }

    public class LogicFunction
    {
        public string Name { get; set; }

        public string ReturnType { get; set; }

        public string ReturnSemantic { get; set; }

        public List<LogicStatement> Statements { get; set; }

        public List<LogicParameter> Parameters { get; set; }
    }

    public class LogicParameter
    {
        public string Type { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public string Semantic { get; set; }
    }

    public abstract class LogicStatement
    {
        public abstract void Execute(LogicExecutionState state);
    }

    public class LogicExecutionState
    {
        public Dictionary<string, object> Variables { get; set; }

        public LogicExecutionState()
        {
            this.Variables = new Dictionary<string, object>();
        }

        public void Return(object result)
        {
            // TODO: Return value
        }
    }

    public class WhileLogicStatement : LogicStatement
    {
        public WhileLogicStatement(LogicExpression condition, LogicStatement statement)
        {
            this.Condition = condition;
            this.Statement = statement;
        }

        public LogicExpression Condition { get; set; }

        public LogicStatement Statement { get; set; }

        public override void Execute(LogicExecutionState state)
        {
            while (this.Condition.Truthful(state))
            {
                this.Statement.Execute(state);
            }
        }
    }

    public class IfLogicStatement : LogicStatement
    {
        public IfLogicStatement(LogicExpression condition, LogicStatement statement)
        {
            this.Condition = condition;
            this.Statement = statement;
        }

        public LogicExpression Condition { get; set; }

        public LogicStatement Statement { get; set; }

        public override void Execute(LogicExecutionState state)
        {
            if (this.Condition.Truthful(state))
            {
                this.Statement.Execute(state);
            }
        }
    }

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

    public class BlockLogicStatement : LogicStatement
    {
        public List<LogicStatement> Statements { get; set; } 

        public BlockLogicStatement(List<LogicStatement> statements)
        {
            this.Statements = statements;
        }

        public override void Execute(LogicExecutionState state)
        {
            foreach (var statement in this.Statements)
            {
                statement.Execute(state);
            }
        }
    }

    public class ReturnLogicStatement : LogicStatement
    {
        public LogicExpression Expression { get; set; }

        public ReturnLogicStatement(LogicExpression expression)
        {
            this.Expression = expression;
        }

        public override void Execute(LogicExecutionState state)
        {
            state.Return(this.Expression.Result(state));
        }
    }

    public class AssignLogicStatement : LogicStatement
    {
        public string Name { get; set; }

        public LogicExpression Expression { get; set; }

        public AssignLogicStatement(string name, LogicExpression expression)
        {
            this.Name = name;
            this.Expression = expression;
        }

        public override void Execute(LogicExecutionState state)
        {
            state.Variables[this.Name] = this.Expression.Result(state);
        }
    }

    public abstract class LogicExpression
    {
        public abstract bool Truthful(LogicExecutionState state);

        public abstract object Result(LogicExecutionState state);
    }

    public class LogicControl
    {
        private List<LogicStructure> m_Structures;

        private List<LogicFunction> m_Functions;

        public static void Main(string[] args)
        {
            new LogicControl();
        }

        public LogicControl()
        {
            this.m_Structures = new List<LogicStructure>();
            this.m_Functions = new List<LogicFunction>();

            var parser = new Parser();
            parser.LoadTables("Grammar.egt");

            var code = @"
struct Input
{
    vector3 Position : IN_Position;
    vector3 Velocity : IN_Velocity;
}

struct Output
{
    vector3 Velocity : OUT_Velocity;
}

vector3 ApplyGravity(InputValues input) : OUT_Velocity
{
    a = 5;

    while (a < 5)
        a = a - 1;

    return input.Velocity + vector3(0, 0.5, 0);
}
";

            parser.Open(ref code);

            Reduction root = null;
            string error = null;
            var done = false;
            while (!done)
            {
                var response = parser.Parse();

                switch (response)
                {
                    case GOLD.ParseMessage.LexicalError:
                        error = "Lexical Error:\n" +
                                      "Position: " + parser.CurrentPosition().Line + ", " + parser.CurrentPosition().Column + "\n" +
                                      "Read: " + parser.CurrentToken().Data;
                        done = true;
                        break;

                    case GOLD.ParseMessage.SyntaxError:
                        error = "Syntax Error:\n" +
                                      "Position: " + parser.CurrentPosition().Line + ", " + parser.CurrentPosition().Column + "\n" +
                                      "Read: " + parser.CurrentToken().Data + "\n" +
                                      "Expecting: " + parser.ExpectedSymbols().Text();
                        done = true;
                        break;

                    case GOLD.ParseMessage.Accept:
                        root = (GOLD.Reduction)parser.CurrentReduction;
                        done = true;
                        break;

                    case GOLD.ParseMessage.InternalError:
                        error = "Internal error";
                        done = true;
                        break;

                    case GOLD.ParseMessage.NotLoadedError:
                        error = "Tables not loaded";
                        done = true;
                        break;

                    case GOLD.ParseMessage.GroupError:
                        error = "Runaway group";
                        done = true;
                        break;
                }
            }

            if (error != null)
            {
                Console.Error.WriteLine(error);
                return;
            }

            this.TraverseNodes(root);
        }

        private void TraverseNodes(Reduction root)
        {
            while (true)
            {
                var globalStatement = (Reduction)root[0].Data;
                var declaration = (Reduction)globalStatement[0].Data;

                switch (declaration.Parent.Head().Name())
                {
                    case "StructureDeclaration":
                        this.TraverseStructureDeclaration(declaration);
                        break;
                    case "FunctionDeclaration":
                        this.TraverseFunctionDeclaration(declaration);
                        break;
                }

                if (root.Count() == 1)
                {
                    return;
                }

                root = (Reduction)root[1].Data;
            }
        }

        private void TraverseStructureDeclaration(Reduction structureDeclaration)
        {
            var root = structureDeclaration;

            var name = (string)root[1].Data;
            var fields = (Reduction)root[3].Data;

            var logicFields = new List<LogicField>();

            Reduction field;

            while (fields.Count() == 2)
            {
                field = (Reduction)fields[0].Data;

                logicFields.Add(new LogicField
                {
                    Name = (string)field[1].Data,
                    Type = (string)((Reduction)field[0].Data)[0].Data,
                    Semantic = (string)((Reduction)field[2].Data)[1].Data
                });

                fields = (Reduction)fields[1].Data;
            }

            field = (Reduction)fields[0].Data;

            logicFields.Add(new LogicField
            {
                Name = (string)field[1].Data,
                Type = (string)((Reduction)field[0].Data)[0].Data,
                Semantic = (string)((Reduction)field[2].Data)[1].Data
            });

            this.m_Structures.Add(new LogicStructure { Name = name, Fields = logicFields });
        }

        private void TraverseFunctionDeclaration(Reduction functionDeclaration)
        {
            var returnType = (string)((Reduction)functionDeclaration[0].Data)[0].Data;
            var name = (string)functionDeclaration[1].Data;
            var parameterDeclarations = (Reduction)functionDeclaration[3].Data;
            var optionalSemantic = (Reduction)functionDeclaration[5].Data;
            var statements = (Reduction)functionDeclaration[7].Data;

            this.m_Functions.Add(
                new LogicFunction
                {
                    Name = name,
                    ReturnSemantic = this.ParseOptionalSemantic(optionalSemantic),
                    ReturnType = returnType,
                    Parameters = this.ParseParameters(parameterDeclarations),
                    Statements = this.ParseStatements(statements)
                });
        }

        private List<LogicParameter> ParseParameters(Reduction parameterDeclarations)
        {
            var parameters = new List<LogicParameter>();

            while (true)
            {
                var parameter = (Reduction)parameterDeclarations[0].Data;

                var type = (string)((Reduction)parameter[0].Data)[0].Data;
                var name = (string)parameter[1].Data;
                var optionalSemantic = this.ParseOptionalSemantic((Reduction)parameter[2].Data);

                parameters.Add(new LogicParameter
                {
                    Index = parameters.Count,
                    Type = type,
                    Name = name,
                    Semantic = optionalSemantic
                });

                if (parameterDeclarations.Count() == 1)
                {
                    return parameters;
                }

                parameterDeclarations = (Reduction)parameterDeclarations[1].Data;
            }
        }

        private string ParseOptionalSemantic(Reduction optionalSemantic)
        {
            if (optionalSemantic.Count() == 0)
            {
                return null;
            }
            else
            {
                return (string)((Reduction)optionalSemantic[0].Data)[1].Data;
            }
        }

        private List<LogicStatement> ParseStatements(Reduction inputStatements)
        {
            var statements = new List<LogicStatement>();

            while (true)
            {
                var statement = (Reduction)inputStatements[0].Data;

                statements.Add(this.ParseStatement(statement));

                if (inputStatements.Count() == 1)
                {
                    return statements;
                }

                inputStatements = (Reduction)inputStatements[1].Data;
            }
        }

        private LogicStatement ParseBlockStatement(Reduction statement)
        {
            return new BlockLogicStatement(this.ParseStatements((Reduction)statement[1].Data));
        }

        private LogicStatement ParseReturnStatement(Reduction statement)
        {
            return new ReturnLogicStatement(this.ParseExpression((Reduction)statement[1].Data));
        }

        private LogicExpression ParseExpression(Reduction expression)
        {
            return null;
        }

        private LogicStatement ParseAssignStatement(Reduction statement)
        {
            return new AssignLogicStatement(
                (string)statement[0].Data,
                this.ParseExpression((Reduction)statement[2].Data));
        }

        private LogicStatement ParseIfStatement(Reduction statement)
        {
            if (statement.Count() == 7)
            {
                return new IfElseLogicStatement(
                    this.ParseExpression((Reduction)statement[2].Data),
                    this.ParseStatement((Reduction)statement[4].Data),
                    this.ParseStatement((Reduction)statement[6].Data));
            }

            return new IfLogicStatement(
                this.ParseExpression((Reduction)statement[2].Data),
                this.ParseStatement((Reduction)statement[4].Data));
        }

        private LogicStatement ParseStatement(Reduction statement)
        {
            if (statement.Parent.Head().Name() == "TerminatedStatement")
            {
                statement = (Reduction)statement[0].Data;
            }

            if (statement.Parent.Head().Name() == "Statement")
            {
                statement = (Reduction)statement[0].Data;
            }

            switch (statement.Parent.Head().Name())
            {
                case "WhileStatement":
                    return this.ParseWhileStatement(statement);
                case "IfStatement":
                    return this.ParseIfStatement(statement);
                case "AssignStatement":
                    return this.ParseAssignStatement(statement);
                case "ReturnStatement":
                    return this.ParseReturnStatement(statement);
                case "BlockStatement":
                    return this.ParseBlockStatement(statement);
            }

            throw new InvalidOperationException();
        }

        private LogicStatement ParseWhileStatement(Reduction statement)
        {
            return new WhileLogicStatement(
                this.ParseExpression((Reduction)statement[2].Data),
                this.ParseStatement((Reduction)statement[4].Data));
        }

        private void TraverseNodeList(Reduction root)
        {
            for (var i = 0; i < root.Count(); i++)
            {
                var data = root[i].Data as Reduction;

                if (data != null)
                {
                    this.TraverseNodes(data);
                }
            }
        }
    }
}