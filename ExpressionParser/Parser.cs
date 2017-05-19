using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public abstract class Parser
    {
        public Action Action { get; set; }
        public abstract bool Parse(Expression expression);

        public static Parser operator |(Parser left, Parser right)
        {
            return new OrParser(left, right);
        }
    }
}
