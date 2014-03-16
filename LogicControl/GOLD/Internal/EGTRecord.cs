namespace GOLD.Internal
{
    internal enum EGTRecord : byte
    {
        InitialStates = 73, 

        // I
        Symbol = 83, 

        // S
        Production = 82, 

        // R   R for Rule (related productions)
        DFAState = 68, 

        // D
        LRState = 76, 

        // L
        Property = 112, 

        // p
        CharRanges = 99, 

        // c 
        Group = 103, 

        // g
        TableCounts = 116

        // t   Table Counts
    }
}