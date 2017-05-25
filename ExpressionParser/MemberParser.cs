using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class MemberParser : ExpressionParser<MemberExpression>
    {
        public ExpressionParser Expression { get; set; }

        public MemberParser() { }
        public MemberParser(Func<MemberExpression, bool> condition) : base(condition) { }

        protected override IEnumerable<(ExpressionParser, Func<MemberExpression, Expression>)> Children =>
            new(ExpressionParser, Func<MemberExpression, Expression>)[]
            {
                (Expression, x => x.Expression),
            };
    }
}
