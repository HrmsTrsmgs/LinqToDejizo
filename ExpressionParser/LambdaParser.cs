using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class LambdaParser : ExpressionParser<LambdaExpression>
    {
        public ExpressionParser Body { get; set; }

        protected override IEnumerable<(ExpressionParser, Func<LambdaExpression, Expression>)> Children =>
            new(ExpressionParser, Func<LambdaExpression, Expression>)[]
            {
                    (Body, x => x.Body),
            };
    }
}
