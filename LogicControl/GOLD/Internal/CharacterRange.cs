namespace GOLD.Internal
{
    internal class CharacterRange
    {
        public CharacterRange(ushort start, ushort end)
        {
            this.Start = start;
            this.End = end;
        }

        public ushort End { get; set; }

        public ushort Start { get; set; }
    }
}