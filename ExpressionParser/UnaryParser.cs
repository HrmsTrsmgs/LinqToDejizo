using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class UnaryParser : ExpressionParser<UnaryExpression>
    {
        public ExpressionParser Operand { get; set; }

        protected override IEnumerable<(ExpressionParser, Func<UnaryExpression, Expression>)> Children =>
            new(ExpressionParser, Func<UnaryExpression, Expression>)[]
            {
                    (Operand, x => x.Operand)
            };
    }
}
