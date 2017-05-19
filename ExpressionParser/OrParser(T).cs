using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public class OrParser<T> : Parser<T> where T : Expression
    {
        public Parser<T> Left { get; set; }
        public Parser<T> Right { get; set; }

        public OrParser(Parser<T> left, Parser<T> right)
        {
            Left = left;
            Right = right;
        }
        public override bool Parse(Expression expression)
        {
            if (Left.Parse(expression) || Right.Parse(expression))
            {
                Action?.Invoke(null);
                return true;
            }
            return false;
        }
    }
}
