namespace GOLD.Internal
{
    internal class FAState
    {
        public FAState(Symbol accept)
        {
            this.Accept = accept;
            this.Edges = new FAEdgeList();
        }

        public FAState()
        {
            this.Accept = null;
            this.Edges = new FAEdgeList();
        }

        public Symbol Accept { get; set; }

        public FAEdgeList Edges { get; set; }
    }
}