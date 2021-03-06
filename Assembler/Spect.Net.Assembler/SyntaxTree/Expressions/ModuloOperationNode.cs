namespace Spect.Net.Assembler.SyntaxTree.Expressions
{
    /// <summary>
    /// This class represents the modulo operation
    /// </summary>
    public sealed class ModuloOperationNode : BinaryOperationNode
    {
        /// <summary>
        /// Calculates the result of the binary operation.
        /// </summary>
        /// <param name="evalContext">Evaluation context</param>
        /// <returns>Result of the operation</returns>
        public override ushort Calculate(IEvaluationContext evalContext)
        {
            var divider = RightOperand.Evaluate(evalContext);

            if (divider != 0) return (ushort) (LeftOperand.Evaluate(evalContext) % divider);

            EvaluationError = "Divide by zero error";
            return 0;
        }
    }
}