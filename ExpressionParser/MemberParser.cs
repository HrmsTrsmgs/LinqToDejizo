using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public class MemberParser : ExpressionParser<MemberExpression>
    {
        public MemberParser() { }
        public MemberParser(Func<MemberExpression, bool> condition) : base(condition) { }
    }
}
