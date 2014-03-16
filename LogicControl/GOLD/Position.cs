namespace GOLD
{
    public class Position
    {
        internal Position()
        {
            this.Line = 0;
            this.Column = 0;
        }

        public int Column { get; set; }

        public int Line { get; set; }

        internal void Copy(Position pos)
        {
            this.Column = pos.Column;
            this.Line = pos.Line;
        }
    }
}