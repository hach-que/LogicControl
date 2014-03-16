namespace GOLD
{
    using System;

    // ERROR: Not supported in C#: OptionDeclaration
    using System.ComponentModel;
    using System.IO;

    public class ParserException : System.Exception
    {


        public string Method;

        internal ParserException(string Message)
            : base(Message)
        {
            this.Method = "";
        }

        internal ParserException(string Message, Exception Inner, string Method)
            : base(Message, Inner)
        {
            this.Method = Method;
        }
    }

    //===== Parsing messages 
    public enum ParseMessage
    {
        TokenRead = 0,
        //A new token is read
        Reduction = 1,
        //A production is reduced
        Accept = 2,
        //Grammar complete
        NotLoadedError = 3,
        //The tables are not loaded
        LexicalError = 4,
        //Token not recognized
        SyntaxError = 5,
        //Token is not expected
        GroupError = 6,
        //Reached the end of the file inside a block
        InternalError = 7
        //Something is wrong, very wrong
    }


    public class GrammarProperties
    {

        private const int PropertyCount = 8;

        private enum PropertyIndex
        {
            Name = 0,

            Version = 1,

            Author = 2,

            About = 3,

            CharacterSet = 4,

            CharacterMapping = 5,

            GeneratedBy = 6,

            GeneratedDate = 7
        }


        private string[] m_Property = new string[PropertyCount + 1];

        internal GrammarProperties()
        {
            int n = 0;

            for (n = 0; n <= PropertyCount - 1; n++)
            {
                m_Property[n] = "";
            }
        }

        internal void SetValue(int Index, string Value)
        {
            if (Index >= 0 & Index < PropertyCount)
            {
                m_Property[Index] = Value;
            }
        }

        public string Name
        {
            get
            {
                return m_Property[(int)PropertyIndex.Name];
            }
        }

        public string Version
        {
            get
            {
                return m_Property[(int)PropertyIndex.Version];
            }
        }

        public string Author
        {
            get
            {
                return m_Property[(int)PropertyIndex.Author];
            }
        }

        public string About
        {
            get
            {
                return m_Property[(int)PropertyIndex.About];
            }
        }

        public string CharacterSet
        {
            get
            {
                return m_Property[(int)PropertyIndex.CharacterSet];
            }
        }

        public string CharacterMapping
        {
            get
            {
                return m_Property[(int)PropertyIndex.CharacterMapping];
            }
        }

        public string GeneratedBy
        {
            get
            {
                return m_Property[(int)PropertyIndex.GeneratedBy];
            }
        }

        public string GeneratedDate
        {
            get
            {
                return m_Property[(int)PropertyIndex.GeneratedDate];
            }
        }
    }


    public class Parser
    {
        //===================================================================
        // Class Name:
        //    Parser
        //
        // Purpose:
        //    This is the main class in the GOLD Parser Engine and is used to perform
        //    all duties required to the parsing of a source text string. This class
        //    contains the LALR(1) State Machine code, the DFA State Machine code,
        //    character table (used by the DFA algorithm) and all other structures and
        //    methods needed to interact with the developer.
        //
        //Author(s):
        //   Devin Cook
        //
        //Public Dependencies:
        //   Token, TokenList, Production, ProductionList, Symbol, SymbolList, Reduction, Position
        //
        //Private Dependencies:
        //   CGTReader, TokenStack, TokenStackQueue, FAStateList, CharacterRange, CharacterSet,
        //   CharacterSetList, LRActionTableList
        //
        //Revision History:    
        //  2011-10-06
        //      * Added 5.0 logic.
        //===================================================================


        private const string kVersion = "5.0";

        //===== Symbols recognized by the system

        private SymbolList m_SymbolTable = new SymbolList();

        //===== DFA
        private FAStateList m_DFA = new FAStateList();

        private CharacterSetList m_CharSetTable = new CharacterSetList();

        private string m_LookaheadBuffer;

        //===== Productions

        private ProductionList m_ProductionTable = new ProductionList();

        //===== LALR
        private LRStateList m_LRStates = new LRStateList();

        private int m_CurrentLALR;

        private TokenStack m_Stack = new TokenStack();

        //===== Used for Reductions & Errors
        //This ENTIRE list will available to the user
        private SymbolList m_ExpectedSymbols = new SymbolList();

        private bool m_HaveReduction;

        //NEW 12/2001
        private bool m_TrimReductions;

        //===== Private control variables
        private bool m_TablesLoaded;

        //Tokens to be analyzed - Hybred object!
        private TokenQueueStack m_InputTokens = new TokenQueueStack();


        private TextReader m_Source;

        //=== Line and column information. 
        //Internal - so user cannot mess with values
        private Position m_SysPosition = new Position();

        //Last read terminal
        private Position m_CurrentPosition = new Position();


        //===== The ParseLALR() function returns this value
        private enum ParseResult
        {
            Accept = 1,

            Shift = 2,

            ReduceNormal = 3,

            ReduceEliminated = 4,
            //Trim
            SyntaxError = 5,

            InternalError = 6
        }

        //===== Grammar Attributes

        private GrammarProperties m_Grammar = new GrammarProperties();

        //===== Lexical Groups
        private TokenStack m_GroupStack = new TokenStack();

        private GroupList m_GroupTable = new GroupList();

        public Parser()
        {
            Restart();
            m_TablesLoaded = false;

            //======= Default Properties
            m_TrimReductions = false;
        }

        [Description("Opens a string for parsing.")]
        public bool Open(ref string Text)
        {
            return Open(new StringReader(Text));
        }

        [Description("Opens a text stream for parsing.")]
        public bool Open(TextReader Reader)
        {
            Token Start = new Token();

            Restart();
            m_Source = Reader;

            //=== Create stack top item. Only needs state
            Start.State = m_LRStates.InitialState;
            m_Stack.Push(ref Start);

            return true;
        }

        [Description("When the Parse() method returns a Reduce, this method will contain the current Reduction.")]
        public object CurrentReduction
        {
            get
            {
                object functionReturnValue = null;
                if (m_HaveReduction)
                {
                    functionReturnValue = m_Stack.Top().Data;
                }
                else
                {
                    functionReturnValue = null;
                }
                return functionReturnValue;
            }
            set
            {
                if (m_HaveReduction)
                {
                    m_Stack.Top().Data = value;
                }
            }
        }

        [Description("Determines if reductions will be trimmed in cases where a production contains a single element.")]
        public bool TrimReductions
        {
            get
            {
                return m_TrimReductions;
            }
            set
            {
                m_TrimReductions = value;
            }
        }

        [Description("Returns information about the current grammar.")]
        public GrammarProperties Grammar()
        {
            return m_Grammar;
        }

        [Description("Current line and column being read from the source.")]
        public Position CurrentPosition()
        {
            return m_CurrentPosition;
        }

        [Description("If the Parse() function returns TokenRead, this method will return that last read token.")]
        public Token CurrentToken()
        {
            return m_InputTokens.Top();
        }

        [Description("Removes the next token from the input queue.")]
        public Token DiscardCurrentToken()
        {
            return m_InputTokens.Dequeue();
        }

        [Description("Added a token onto the end of the input queue.")]
        public void EnqueueInput(ref Token TheToken)
        {
            m_InputTokens.Enqueue(ref TheToken);
        }

        [Description("Pushes the token onto the top of the input queue. This token will be analyzed next.")]
        public void PushInput(ref Token TheToken)
        {
            m_InputTokens.Push(TheToken);
        }

        private string LookaheadBuffer(int Count)
        {
            //Return Count characters from the lookahead buffer. DO NOT CONSUME
            //This is used to create the text stored in a token. It is disgarded
            //separately. Because of the design of the DFA algorithm, count should
            //never exceed the buffer length. The If-Statement below is fault-tolerate
            //programming, but not necessary.

            if (Count > m_LookaheadBuffer.Length)
            {
                Count = Convert.ToInt32(this.m_LookaheadBuffer);
            }

            return m_LookaheadBuffer.Substring(0, Count);
        }

        private string Lookahead(int CharIndex)
        {
            //Return single char at the index. This function will also increase 
            //buffer if the specified character is not present. It is used 
            //by the DFA algorithm.

            int ReadCount = 0;
            int n = 0;

            //Check if we must read characters from the Stream
            if (CharIndex > m_LookaheadBuffer.Length)
            {
                ReadCount = CharIndex - m_LookaheadBuffer.Length;
                for (n = 1; n <= ReadCount; n++)
                {
                    m_LookaheadBuffer += (char)(m_Source.Read());
                }
            }

            //If the buffer is still smaller than the index, we have reached
            //the end of the text. In this case, return a null string - the DFA
            //code will understand.
            if (CharIndex <= m_LookaheadBuffer.Length)
            {
                return this.m_LookaheadBuffer[CharIndex - 1].ToString();
            }
            else
            {
                return "";
            }
        }

        [Description("Library name and version.")]
        public string About()
        {
            return "GOLD Parser Engine; Version " + kVersion;
        }

        internal void Clear()
        {
            m_SymbolTable.Clear();
            m_ProductionTable.Clear();
            m_CharSetTable.Clear();
            m_DFA.Clear();
            m_LRStates.Clear();

            m_Stack.Clear();
            m_InputTokens.Clear();

            m_Grammar = new GrammarProperties();

            m_GroupStack.Clear();
            m_GroupTable.Clear();

            Restart();
        }

        [Description("Loads parse tables from the specified filename. Only EGT (version 5.0) is supported.")]
        public bool LoadTables(string Path)
        {
            return LoadTables(new BinaryReader(File.Open(Path, FileMode.Open, FileAccess.Read)));
        }

        [Description("Loads parse tables from the specified BinaryReader. Only EGT (version 5.0) is supported.")]
        public bool LoadTables(BinaryReader Reader)
        {
            EGTReader EGT = new EGTReader();
            bool Success = false;
            EGTRecord RecType = default(EGTRecord);

            //try
            //{
                EGT.Open(Reader);

                Restart();
                Success = true;
                while (!(EGT.EndOfFile() || Success == false))
                {
                    EGT.GetNextRecord();

                    RecType = (EGTRecord)EGT.RetrieveByte();

                    switch (RecType)
                    {
                        case EGTRecord.Property:
                        {
                            //Index, Name, Value
                            int Index = 0;
                            string Name = null;

                            Index = EGT.RetrieveInt16();
                            Name = EGT.RetrieveString();
                            //Just discard
                            m_Grammar.SetValue(Index, EGT.RetrieveString());

                            break;
                        }
                        case EGTRecord.TableCounts:
                        {
                            //Symbol, CharacterSet, Rule, DFA, LALR
                            m_SymbolTable = new SymbolList(EGT.RetrieveInt16());
                            m_CharSetTable = new CharacterSetList(EGT.RetrieveInt16());
                            m_ProductionTable = new ProductionList(EGT.RetrieveInt16());
                            m_DFA = new FAStateList(EGT.RetrieveInt16());
                            m_LRStates = new LRStateList(EGT.RetrieveInt16());
                            m_GroupTable = new GroupList(EGT.RetrieveInt16());

                            break;
                        }
                        case EGTRecord.InitialStates:
                        {
                            //DFA, LALR
                            m_DFA.InitialState = (short)EGT.RetrieveInt16();
                            m_LRStates.InitialState = (short)EGT.RetrieveInt16();

                            break;
                        }
                        case EGTRecord.Symbol:
                        {
                            //#, Name, Kind
                            short Index = 0;
                            string Name = null;
                            SymbolType Type = default(SymbolType);

                            Index = (short)EGT.RetrieveInt16();
                            Name = EGT.RetrieveString();
                            Type = (SymbolType)EGT.RetrieveInt16();

                            m_SymbolTable[Index] = new Symbol(Name, Type, Index);

                            break;
                        }
                        case EGTRecord.Group:
                        {

                            //#, Name, Container#, Start#, End#, Tokenized, Open Ended, Reserved, Count, (Nested Group #...) 
                            Group G = new Group();
                            int Index = 0;
                            int Count = 0;
                            int n = 0;

                            var _with1 = G;
                            Index = EGT.RetrieveInt16();
                            //# 

                            _with1.Name = EGT.RetrieveString();
                            _with1.Container = SymbolTable()[EGT.RetrieveInt16()];
                            _with1.Start = SymbolTable()[EGT.RetrieveInt16()];
                            _with1.End = SymbolTable()[EGT.RetrieveInt16()];

                            _with1.Advance = (Group.AdvanceMode)EGT.RetrieveInt16();
                            _with1.Ending = (Group.EndingMode)EGT.RetrieveInt16();
                            EGT.RetrieveEntry();
                            //Reserved

                            Count = EGT.RetrieveInt16();
                            for (n = 1; n <= Count; n++)
                            {
                                _with1.Nesting.Add(EGT.RetrieveInt16());
                            }


                            //=== Link back
                            G.Container.Group = G;
                            G.Start.Group = G;
                            G.End.Group = G;

                            m_GroupTable[Index] = G;

                            break;
                        }
                        case EGTRecord.CharRanges:
                        {
                            //#, Total Sets, RESERVED, (Start#, End#  ...)
                            int Index = 0;
                            int Total = 0;

                            Index = EGT.RetrieveInt16();
                            EGT.RetrieveInt16();
                            //Codepage
                            Total = EGT.RetrieveInt16();
                            EGT.RetrieveEntry();
                            //Reserved

                            m_CharSetTable[Index] = new CharacterSet();
                            while (!(EGT.RecordComplete()))
                            {
                                m_CharSetTable[Index].Add(
                                    new CharacterRange((ushort)EGT.RetrieveInt16(), (ushort)EGT.RetrieveInt16()));
                            }

                            break;
                        }
                        case EGTRecord.Production:
                        {
                            //#, ID#, Reserved, (Symbol#,  ...)
                            int Index = 0;
                            int HeadIndex = 0;
                            int SymIndex = 0;

                            Index = EGT.RetrieveInt16();
                            HeadIndex = EGT.RetrieveInt16();
                            EGT.RetrieveEntry();
                            //Reserved

                            m_ProductionTable[Index] = new Production(m_SymbolTable[HeadIndex], (short)Index);

                            while (!(EGT.RecordComplete()))
                            {
                                SymIndex = EGT.RetrieveInt16();
                                m_ProductionTable[Index].Handle().Add(m_SymbolTable[SymIndex]);
                            }

                            break;
                        }
                        case EGTRecord.DFAState:
                        {
                            //#, Accept?, Accept#, Reserved (CharSet#, Target#, Reserved)...
                            int Index = 0;
                            bool Accept = false;
                            int AcceptIndex = 0;
                            int SetIndex = 0;
                            int Target = 0;

                            Index = EGT.RetrieveInt16();
                            Accept = EGT.RetrieveBoolean();
                            AcceptIndex = EGT.RetrieveInt16();
                            EGT.RetrieveEntry();
                            //Reserved

                            if (Accept)
                            {
                                m_DFA[Index] = new FAState(m_SymbolTable[AcceptIndex]);
                            }
                            else
                            {
                                m_DFA[Index] = new FAState();
                            }

                            //(Edge chars, Target#, Reserved)...
                            while (!(EGT.RecordComplete()))
                            {
                                SetIndex = EGT.RetrieveInt16();
                                //Char table index
                                Target = EGT.RetrieveInt16();
                                //Target
                                EGT.RetrieveEntry();
                                //Reserved

                                m_DFA[Index].Edges.Add(new FAEdge(m_CharSetTable[SetIndex], Target));
                            }

                            break;
                        }
                        case EGTRecord.LRState:
                        {
                            //#, Reserved (Symbol#, Action, Target#, Reserved)...
                            int Index = 0;
                            int SymIndex = 0;
                            int Action = 0;
                            int Target = 0;

                            Index = EGT.RetrieveInt16();
                            EGT.RetrieveEntry();
                            //Reserved

                            m_LRStates[Index] = new LRState();

                            //(Symbol#, Action, Target#, Reserved)...
                            while (!(EGT.RecordComplete()))
                            {
                                SymIndex = EGT.RetrieveInt16();
                                Action = EGT.RetrieveInt16();
                                Target = EGT.RetrieveInt16();
                                EGT.RetrieveEntry();
                                //Reserved

                                m_LRStates[Index].Add(
                                    new LRAction(m_SymbolTable[SymIndex], (LRActionType)Action, (short)Target));
                            }

                            break;
                        }
                        default:
                            //RecordIDComment
                            Success = false;
                            throw new ParserException(
                                "File Error. A record of type '" + (char)(RecType)
                                + "' was read. This is not a valid code.");
                    }
                }

                EGT.Close();

            /*}
            catch (Exception ex)
            {
                throw new ParserException(ex.Message, ex, "LoadTables");
            }*/

            m_TablesLoaded = Success;

            return Success;
        }

        [Description("Returns a list of Symbols recognized by the grammar.")]
        public SymbolList SymbolTable()
        {
            return m_SymbolTable;
        }

        [Description("Returns a list of Productions recognized by the grammar.")]
        public ProductionList ProductionTable()
        {
            return m_ProductionTable;
        }

        [Description(
            "If the Parse() method returns a SyntaxError, this method will contain a list of the symbols the grammar expected to see."
            )]
        public SymbolList ExpectedSymbols()
        {
            return m_ExpectedSymbols;
        }

        private ParseResult ParseLALR(ref Token NextToken)
        {
            //This function analyzes a token and either:
            //  1. Makes a SINGLE reduction and pushes a complete Reduction object on the m_Stack
            //  2. Accepts the token and shifts
            //  3. Errors and places the expected symbol indexes in the Tokens list
            //The Token is assumed to be valid and WILL be checked
            //If an action is performed that requires controlt to be returned to the user, the function returns true.
            //The Message parameter is then set to the type of action.

            short Index = 0;
            short n = 0;
            LRAction ParseAction = default(LRAction);
            Production Prod = default(Production);
            Token Head = default(Token);
            Reduction NewReduction = default(Reduction);
            ParseResult Result = default(ParseResult);

            ParseAction = m_LRStates[m_CurrentLALR][NextToken.Parent];

            // Work - shift or reduce
            if ((ParseAction != null))
            {
                m_HaveReduction = false;
                //Will be set true if a reduction is made
                //'Debug.WriteLine("Action: " & ParseAction.Text)

                switch (ParseAction.Type)
                {
                    case LRActionType.Accept:
                        m_HaveReduction = true;
                        Result = ParseResult.Accept;

                        break;
                    case LRActionType.Shift:
                        m_CurrentLALR = ParseAction.Value;
                        NextToken.State = (short)this.m_CurrentLALR;
                        m_Stack.Push(ref NextToken);
                        Result = ParseResult.Shift;

                        break;
                    case LRActionType.Reduce:
                        //Produce a reduction - remove as many tokens as members in the rule & push a nonterminal token
                        Prod = this.m_ProductionTable[ParseAction.Value];

                        //======== Create Reduction
                        if (m_TrimReductions && Prod.ContainsOneNonTerminal())
                        {
                            //The current rule only consists of a single nonterminal and can be trimmed from the
                            //parse tree. Usually we create a new Reduction, assign it to the Data property
                            //of Head and push it on the m_Stack. However, in this case, the Data property of the
                            //Head will be assigned the Data property of the reduced token (i.e. the only one
                            //on the m_Stack).
                            //In this case, to save code, the value popped of the m_Stack is changed into the head.

                            Head = m_Stack.Pop();
                            Head.Parent = Prod.Head();

                            Result = ParseResult.ReduceEliminated;
                            //Build a Reduction
                        }
                        else
                        {
                            m_HaveReduction = true;
                            NewReduction = new Reduction(Prod.Handle().Count());

                            var _with2 = NewReduction;
                            _with2.Parent = Prod;
                            for (n = (short)(Prod.Handle().Count() - 1); n >= 0; n += -1)
                            {
                                _with2[n] = m_Stack.Pop();
                            }

                            Head = new Token(Prod.Head(), NewReduction);
                            Result = ParseResult.ReduceNormal;
                        }

                        //========== Goto
                        Index = m_Stack.Top().State;

                        //========= If n is -1 here, then we have an Internal Table Error!!!!
                        n = m_LRStates[Index].IndexOf(Prod.Head());
                        if (n != -1)
                        {
                            m_CurrentLALR = m_LRStates[Index][n].Value;

                            Head.State = (short)this.m_CurrentLALR;
                            m_Stack.Push(ref Head);
                        }
                        else
                        {
                            Result = ParseResult.InternalError;
                        }
                        break;
                }

            }
            else
            {
                //=== Syntax Error! Fill Expected Tokens
                m_ExpectedSymbols.Clear();
                //.Count - 1
                foreach (LRAction Action in m_LRStates[m_CurrentLALR])
                {
                    switch (Action.Symbol.Type)
                    {
                        case SymbolType.Content:
                        case SymbolType.End:
                        case SymbolType.GroupStart:
                        case SymbolType.GroupEnd:
                            m_ExpectedSymbols.Add(Action.Symbol);
                            break;
                    }
                }
                Result = ParseResult.SyntaxError;
            }

            return Result;
            //Very important
        }

        [Description("Restarts the parser. Loaded tables are retained.")]
        public void Restart()
        {
            m_CurrentLALR = m_LRStates.InitialState;

            //=== Lexer
            m_SysPosition.Column = 0;
            m_SysPosition.Line = 0;
            m_CurrentPosition.Line = 0;
            m_CurrentPosition.Column = 0;

            m_HaveReduction = false;

            m_ExpectedSymbols.Clear();
            m_InputTokens.Clear();
            m_Stack.Clear();
            m_LookaheadBuffer = "";

            //==== V4
            m_GroupStack.Clear();
        }

        [Description("Returns true if parse tables were loaded.")]
        public bool TablesLoaded()
        {
            return m_TablesLoaded;
        }

        private Token LookaheadDFA()
        {
            //This function implements the DFA for th parser's lexer.
            //It generates a token which is used by the LALR state
            //machine.

            string Ch = null;
            int n = 0;
            int Target = 0;
            int CurrentDFA = 0;
            bool Found = false;
            bool Done = false;
            FAEdge Edge = default(FAEdge);
            int CurrentPosition = 0;
            int LastAcceptState = 0;
            int LastAcceptPosition = 0;
            Token Result = new Token();

            //===================================================
            //Match DFA token
            //===================================================

            Done = false;
            CurrentDFA = m_DFA.InitialState;
            CurrentPosition = 1;
            //Next byte in the input Stream
            LastAcceptState = -1;
            //We have not yet accepted a character string
            LastAcceptPosition = -1;

            Ch = Lookahead(1);
            //NO MORE DATA
            if (!(string.IsNullOrEmpty(Ch) | (int)(Ch[0]) == 65535))
            {
                while (!(Done))
                {
                    // This code searches all the branches of the current DFA state
                    // for the next character in the input Stream. If found the
                    // target state is returned.

                    Ch = Lookahead(CurrentPosition);
                    //End reached, do not match
                    if (string.IsNullOrEmpty(Ch))
                    {
                        Found = false;
                    }
                    else
                    {
                        n = 0;
                        Found = false;
                        while (n < m_DFA[CurrentDFA].Edges.Count & !Found)
                        {
                            Edge = m_DFA[CurrentDFA].Edges[n];

                            //==== Look for character in the Character Set Table
                            if (Edge.Characters.Contains((int)Ch[0]))
                            {
                                Found = true;
                                Target = Edge.Target;
                                //.TableIndex
                            }
                            n += 1;
                        }
                    }

                    // This block-if statement checks whether an edge was found from the current state. If so, the state and current
                    // position advance. Otherwise it is time to exit the main loop and report the token found (if there was one). 
                    // If the LastAcceptState is -1, then we never found a match and the Error Token is created. Otherwise, a new 
                    // token is created using the Symbol in the Accept State and all the characters that comprise it.

                    if (Found)
                    {
                        // This code checks whether the target state accepts a token.
                        // If so, it sets the appropiate variables so when the
                        // algorithm in done, it can return the proper token and
                        // number of characters.

                        //NOT is very important!
                        if ((m_DFA[Target].Accept != null))
                        {
                            LastAcceptState = Target;
                            LastAcceptPosition = CurrentPosition;
                        }

                        CurrentDFA = Target;
                        CurrentPosition += 1;

                        //No edge found
                    }
                    else
                    {
                        Done = true;
                        // Lexer cannot recognize symbol
                        if (LastAcceptState == -1)
                        {
                            Result.Parent = m_SymbolTable.GetFirstOfType(SymbolType.Error);
                            Result.Data = LookaheadBuffer(1);
                            // Create Token, read characters
                        }
                        else
                        {
                            Result.Parent = m_DFA[LastAcceptState].Accept;
                            Result.Data = LookaheadBuffer(LastAcceptPosition);
                            //Data contains the total number of accept characters
                        }
                    }
                    //DoEvents
                }

            }
            else
            {
                // End of file reached, create End Token
                Result.Data = "";
                Result.Parent = m_SymbolTable.GetFirstOfType(SymbolType.End);
            }

            //===================================================
            //Set the new token's position information
            //===================================================
            //Notice, this is a copy, not a linking of an instance. We don't want the user 
            //to be able to alter the main value indirectly.
            Result.Position().Copy(m_SysPosition);

            return Result;
        }

        private void ConsumeBuffer(int CharCount)
        {
            //Consume/Remove the characters from the front of the buffer. 

            int n = 0;

            if (CharCount <= m_LookaheadBuffer.Length)
            {
                // Count Carriage Returns and increment the internal column and line
                // numbers. This is done for the Developer and is not necessary for the
                // DFA algorithm.
                for (n = 0; n <= CharCount - 1; n++)
                {
                    switch (m_LookaheadBuffer[n])
                    {
                        case '\n':
                            m_SysPosition.Line += 1;
                            m_SysPosition.Column = 0;
                            break;
                        case '\r':
                            break;
                            //Ignore, LF is used to inc line to be UNIX friendly
                        default:
                            m_SysPosition.Column += 1;
                            break;
                    }
                }

                m_LookaheadBuffer = m_LookaheadBuffer.Remove(0, CharCount);
            }
        }

        private Token ProduceToken()
        {
            // ** VERSION 5.0 **
            //This function creates a token and also takes into account the current
            //lexing mode of the parser. In particular, it contains the group logic. 
            //
            //A stack is used to track the current "group". This replaces the comment
            //level counter. Also, text is appended to the token on the top of the 
            //stack. This allows the group text to returned in one chunk.

            Token Read = default(Token);
            Token Pop = default(Token);
            Token Top = default(Token);
            Token Result = default(Token);
            bool Done = false;
            bool NestGroup = false;

            Done = false;
            Result = null;
            Read = null;

            while (!Done)
            {
                Read = LookaheadDFA();

                //The logic - to determine if a group should be nested - requires that the top of the stack 
                //and the symbol's linked group need to be looked at. Both of these can be unset. So, this section
                //sets a Boolean and avoids errors. We will use this boolean in the logic chain below. 
                if (Read.Type() == SymbolType.GroupStart)
                {
                    if (m_GroupStack.Count == 0)
                    {
                        NestGroup = true;
                    }
                    else
                    {
                        NestGroup = m_GroupStack.Top().Group().Nesting.Contains(Read.Group().TableIndex);
                    }
                }
                else
                {
                    NestGroup = false;
                }

                //=================================
                // Logic chain
                //=================================

                if (NestGroup)
                {
                    ConsumeBuffer(((dynamic)Read.Data).Length);
                    m_GroupStack.Push(ref Read);

                }
                else if (m_GroupStack.Count == 0)
                {
                    //The token is ready to be analyzed.             
                    ConsumeBuffer(((dynamic)Read.Data).Length);
                    Result = Read;
                    Done = true;

                }
                else if ((object.ReferenceEquals(m_GroupStack.Top().Group().End, Read.Parent)))
                {
                    //End the current group
                    Pop = m_GroupStack.Pop();

                    //=== Ending logic
                    if (Pop.Group().Ending == Group.EndingMode.Closed)
                    {
                        Pop.Data += Read.Data;
                        //Append text
                        ConsumeBuffer(Read.Data.Length);
                        //Consume token
                    }

                    //We are out of the group. Return pop'd token (which contains all the group text)
                    if (m_GroupStack.Count == 0)
                    {
                        Pop.Parent = Pop.Group().Container;
                        //Change symbol to parent
                        Result = Pop;
                        Done = true;
                    }
                    else
                    {
                        m_GroupStack.Top().Data += Pop.Data;
                        //Append group text to parent
                    }

                }
                else if (Read.Type() == SymbolType.End)
                {
                    //EOF always stops the loop. The caller function (Parse) can flag a runaway group error.
                    Result = Read;
                    Done = true;

                }
                else
                {
                    //We are in a group, Append to the Token on the top of the stack.
                    //Take into account the Token group mode  
                    Top = m_GroupStack.Top();

                    if (Top.Group().Advance == Group.AdvanceMode.Token)
                    {
                        Top.Data += Read.Data;
                        // Append all text
                        ConsumeBuffer(Read.Data.Length);
                    }
                    else
                    {
                        Top.Data += Read.Data.Chars(0);
                        // Append one character
                        ConsumeBuffer(1);
                    }
                }
            }

            return Result;
        }

        [Description(
            "Performs a parse action on the input. This method is typically used in a loop until either grammar is accepted or an error occurs."
            )]
        public ParseMessage Parse()
        {
            ParseMessage Message = default(ParseMessage);
            bool Done = false;
            Token Read = default(Token);
            ParseResult Action = default(ParseResult);

            if (!m_TablesLoaded)
            {
                return ParseMessage.NotLoadedError;
            }

            //===================================
            //Loop until breakable event
            //===================================
            Done = false;
            while (!Done)
            {
                if (m_InputTokens.Count == 0)
                {
                    Read = ProduceToken();
                    m_InputTokens.Push(Read);

                    Message = ParseMessage.TokenRead;
                    Done = true;
                }
                else
                {
                    Read = m_InputTokens.Top();
                    m_CurrentPosition.Copy(Read.Position());
                    //Update current position

                    //Runaway group
                    if (m_GroupStack.Count != 0)
                    {
                        Message = ParseMessage.GroupError;
                        Done = true;
                    }
                    else if (Read.Type() == SymbolType.Noise)
                    {
                        //Just discard. These were already reported to the user.
                        m_InputTokens.Pop();

                    }
                    else if (Read.Type() == SymbolType.Error)
                    {
                        Message = ParseMessage.LexicalError;
                        Done = true;

                        //Finally, we can parse the token.
                    }
                    else
                    {
                        Action = ParseLALR(ref Read);
                        //SAME PROCEDURE AS v1

                        switch (Action)
                        {
                            case ParseResult.Accept:
                                Message = ParseMessage.Accept;
                                Done = true;

                                break;
                            case ParseResult.InternalError:
                                Message = ParseMessage.InternalError;
                                Done = true;

                                break;
                            case ParseResult.ReduceNormal:
                                Message = ParseMessage.Reduction;
                                Done = true;

                                break;
                            case ParseResult.Shift:
                                //ParseToken() shifted the token on the front of the Token-Queue. 
                                //It now exists on the Token-Stack and must be eliminated from the queue.
                                m_InputTokens.Dequeue();

                                break;
                            case ParseResult.SyntaxError:
                                Message = ParseMessage.SyntaxError;
                                Done = true;

                                break;
                            default:
                                break;
                                //Do nothing.
                        }
                    }
                }
            }

            return Message;
        }


    }
}