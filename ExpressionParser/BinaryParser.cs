using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class BinaryParser : ExpressionParser<BinaryExpression>
    {
        public ExpressionParser Right { get; set; }
        public ExpressionParser Left { get; set; }

        protected override IEnumerable<(ExpressionParser, Func<BinaryExpression, Expression>)> Children =>
            new(ExpressionParser, Func<BinaryExpression, Expression>)[]
            {
                (Right, x => x.Right),
                (Left, x => x.Left),
            };
    }
}
