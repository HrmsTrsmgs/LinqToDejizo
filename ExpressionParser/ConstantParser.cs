using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class ConstantParser : ExpressionParser<ConstantExpression>
    {
        public override bool Parse(Expression expression)
        {
            return base.Parse(expression);
        }
    }
}
