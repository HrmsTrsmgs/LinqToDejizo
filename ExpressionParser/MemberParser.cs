using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public class MemberParser : Parser<MemberExpression>
    {
        public MemberParser() { }
        public MemberParser(Func<MemberExpression, bool> condition) : base(condition) { }
    }
}
