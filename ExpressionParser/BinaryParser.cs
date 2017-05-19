using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public class BinaryParser : Parser<BinaryExpression>
    {
        public Parser Right { get; set; }
        public Parser Left { get; set; }

        protected override IEnumerable<(Parser, Func<BinaryExpression, Expression>)> Children =>
            new(Parser, Func<BinaryExpression, Expression>)[]
            {
                (Right, x => x.Right),
                (Left, x => x.Left),
            };
    }
}
