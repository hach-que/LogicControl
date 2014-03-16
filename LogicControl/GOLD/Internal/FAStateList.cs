namespace GOLD.Internal
{
    using System.Collections;

    internal class FAStateList : ArrayList
    {
        public FAStateList()
        {
            this.InitialState = 0;
            this.ErrorSymbol = null;
        }

        internal FAStateList(int size)
        {
            this.ReDimension(size);

            this.InitialState = 0;
            this.ErrorSymbol = null;
        }

        public Symbol ErrorSymbol { get; set; }

        public short InitialState { get; set; }

        public new FAState this[int index]
        {
            get
            {
                return (FAState)base[index];
            }

            set
            {
                base[index] = value;
            }
        }

        public int Add(FAState item)
        {
            return base.Add(item);
        }

        internal void ReDimension(int size)
        {
            // Increase the size of the array to Size empty elements.
            this.Clear();
            for (var n = 0; n <= size - 1; n++)
            {
                this.Add(null);
            }
        }
    }
}