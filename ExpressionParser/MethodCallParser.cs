using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace Marimo.ExpressionParser
{
    public class MethodCallParser : Parser<MethodCallExpression>
    {
        public Parser[] Arguments { get; set; } = new Parser[] { null, null };

        protected override IEnumerable<(Parser, Func<MethodCallExpression, Expression>)> Children =>
            Arguments.Select<Parser, (Parser, Func<MethodCallExpression, Expression>)>((x, i) => (x, xx => xx.Arguments[i]));
        public MethodCallParser() { }
        public MethodCallParser(Func<MethodCallExpression, bool> condition) : base(condition) { }
    }
}
