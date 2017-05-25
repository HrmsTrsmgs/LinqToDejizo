using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public abstract class ExpressionParser
    {
        public Action Action { get; set; }
        public abstract bool Parse(Expression expression);

        public static ExpressionParser operator |(ExpressionParser left, ExpressionParser right) =>
            new OrParser(left, right);
    }
}
