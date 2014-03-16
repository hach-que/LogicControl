namespace GOLD.Internal
{
    using System.Collections;

    internal class GroupList : ArrayList
    {
        public GroupList()
        {
        }

        internal GroupList(int size)
        {
            this.ReDimension(size);
        }

        public new Group this[int index]
        {
            get
            {
                return (Group)base[index];
            }

            set
            {
                base[index] = value;
            }
        }

        public int Add(Group item)
        {
            return base.Add(item);
        }

        internal void ReDimension(int size)
        {
            // Increase the size of the array to Size empty elements.
            int n;

            this.Clear();
            for (n = 0; n <= size - 1; n++)
            {
                this.Add(null);
            }
        }
    }
}