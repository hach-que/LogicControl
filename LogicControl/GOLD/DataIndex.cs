namespace GOLD
{
    public class DataIndex
    {
        private readonly TokenList m_TokenList;

        public DataIndex(TokenList tokenList)
        {
            this.m_TokenList = tokenList;
        }

        public object this[int index]
        {
            get
            {
                return this.m_TokenList[index].Data;
            }

            set
            {
                this.m_TokenList[index].Data = value;
            }
        }
    }
}