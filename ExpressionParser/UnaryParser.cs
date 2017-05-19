using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public class UnaryParser : Parser<UnaryExpression>
    {
        public Parser Operand { get; set; }

        protected override IEnumerable<(Parser, Func<UnaryExpression, Expression>)> Children =>
            new(Parser, Func<UnaryExpression, Expression>)[]
            {
                    (Operand, x => x.Operand)
            };
    }
}
