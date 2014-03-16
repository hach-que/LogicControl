namespace GOLD.Internal
{
    internal enum LRActionType
    {
        Shift = 1, 

        // Shift a symbol and goto a state
        Reduce = 2, 

        // Reduce by a specified rule
        Goto = 3, 

        // Goto to a state on reduction
        Accept = 4, 

        // Input successfully parsed
        Error = 5

        // Programmars see this often!
    }
}