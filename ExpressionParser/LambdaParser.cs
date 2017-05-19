using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public class LambdaParser : Parser<LambdaExpression>
    {
        public Parser Body { get; set; }

        protected override IEnumerable<(Parser, Func<LambdaExpression, Expression>)> Children =>
            new(Parser, Func<LambdaExpression, Expression>)[]
            {
                    (Body, x => x.Body),
            };
    }
}
