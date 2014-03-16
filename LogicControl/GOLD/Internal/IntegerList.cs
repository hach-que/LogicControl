namespace GOLD.Internal
{
    using System.Collections;

    internal class IntegerList : ArrayList
    {
        public new int this[int index]
        {
            get
            {
                return (int)base[index];
            }

            set
            {
                base[index] = value;
            }
        }

        public int Add(int value)
        {
            return base.Add(value);
        }

        public bool Contains(int item)
        {
            return base.Contains(item);
        }
    }
}