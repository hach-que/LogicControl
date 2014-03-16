namespace GOLD.Internal
{
    using System.Collections;

    internal class FAEdgeList : ArrayList
    {
        public new FAEdge this[int index]
        {
            get
            {
                return (FAEdge)base[index];
            }

            set
            {
                base[index] = value;
            }
        }

        public int Add(FAEdge edge)
        {
            return base.Add(edge);
        }
    }
}