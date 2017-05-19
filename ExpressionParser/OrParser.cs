using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public class OrParser : Parser
    {
        public Parser Left { get; set; }
        public Parser Right { get; set; }

        public OrParser(Parser left, Parser right)
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
