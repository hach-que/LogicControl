namespace GOLD
{
    public enum SymbolType
    {
        Nonterminal = 0, 

        // Nonterminal 
        Content = 1, 

        // Passed to the parser
        Noise = 2, 

        // Ignored by the parser
        End = 3, 

        // End character (EOF)
        GroupStart = 4, 

        // Group start  
        GroupEnd = 5, 

        // Group end   
        // Note: There is no value 6. CommentLine was deprecated.
        Error = 7

        // Error symbol
    }
}