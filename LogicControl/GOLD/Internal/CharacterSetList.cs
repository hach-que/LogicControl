namespace GOLD.Internal
{
    using System.Collections;

    internal class CharacterSetList : ArrayList
    {
        public CharacterSetList()
        {
        }

        internal CharacterSetList(int size)
        {
            this.ReDimension(size);
        }

        public new CharacterSet this[int index]
        {
            get
            {
                return (CharacterSet)base[index];
            }

            set
            {
                base[index] = value;
            }
        }

        public int Add(ref CharacterSet item)
        {
            return this.Add(item);
        }

        internal void ReDimension(int size)
        {
            // Increase the size of the array to Size empty elements.
            int n;

            this.Clear();
            for (n = 0; n <= size - 1; n++)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                this.Add(null);
            }
        }
    }
}