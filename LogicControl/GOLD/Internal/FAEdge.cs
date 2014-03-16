namespace GOLD.Internal
{
    internal class FAEdge
    {
        // Characters to advance on	
        public CharacterSet Characters { get; set; }

        // FAState
        public int Target { get; set; }

        public FAEdge(CharacterSet charSet, int target)
        {
            this.Characters = charSet;
            this.Target = target;
        }

        public FAEdge()
        {
            // Nothing for now
        }
    }
}