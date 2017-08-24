namespace AntlrZ80Asm.SyntaxTree.Pragmas
{
    /// <summary>
    /// This class represents the DEFM pragma
    /// </summary>
    public sealed class DefmPragma : PragmaLine
    {
        /// <summary>
        /// The message to define
        /// </summary>
        public string Message { get; set; }
    }
}