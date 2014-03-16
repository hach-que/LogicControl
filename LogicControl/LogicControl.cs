namespace LogicControl
{
    public static class LogicControl
    {
        public static LogicScript LoadScript(string code)
        {
            return new LogicScript(code);
        }
    }
}