using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace Marimo.ExpressionParserCombinator
{
    public class MethodCallParser : ExpressionParser<MethodCallExpression>
    {
        public ExpressionParser[] Arguments { get; set; } = new ExpressionParser[] { null, null };

        protected override IEnumerable<(ExpressionParser, Func<MethodCallExpression, Expression>)> Children =>
            Arguments.Select<ExpressionParser, (ExpressionParser, Func<MethodCallExpression, Expression>)>((x, i) => (x, xx => xx.Arguments[i]));
        public MethodCallParser() { }
        public MethodCallParser(Func<MethodCallExpression, bool> condition) : base(condition) { }
    }
}
