namespace GOLD.Internal
{
    internal class LRAction
    {
        public LRAction(Symbol symbol, LRActionType type, short value)
        {
            this.Symbol = symbol;
            this.Type = type;
            this.Value = value;
        }

        public Symbol Symbol { get; set; }

        public LRActionType Type { get; set; }

        // shift to state, reduce rule, goto state
        public short Value { get; set; }
    }
}