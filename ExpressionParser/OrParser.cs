using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class OrParser : ExpressionParser
    {
        public ExpressionParser Left { get; set; }
        public ExpressionParser Right { get; set; }

        public OrParser(ExpressionParser left, ExpressionParser right)
        {
            Left = left;
            Right = right;
        }
        public override bool Parse(Expression expression)
        {
            if (Left.Parse(expression) || Right.Parse(expression))
            {
                Action?.Invoke();
                return true;
            }
            return false;
        }
    }
}
