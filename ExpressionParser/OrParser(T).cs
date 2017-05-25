using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class OrParser<T> : ExpressionParser<T> where T : Expression
    {
        public ExpressionParser<T> Left { get; set; }
        public ExpressionParser<T> Right { get; set; }

        public OrParser(ExpressionParser<T> left, ExpressionParser<T> right)
        {
            Left = left;
            Right = right;
        }
        public override bool Parse(Expression expression)
        {
            if (Left.Parse(expression) || Right.Parse(expression))
            {
                Action?.Invoke((T)expression);
                return true;
            }
            return false;
        }
    }
}
