using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParserCombinator
{
    public abstract class ExpressionParser<T> : ExpressionParser where T : Expression
    {
        public ExpressionParser() { }

        public ExpressionParser(Func<T, bool> condition)
        {
            this.condition = condition;
        }

        Func<T, bool> condition;

        protected virtual IEnumerable<(ExpressionParser, Func<T, Expression>)> Children => new(ExpressionParser, Func<T, Expression>)[] { };
        public new Action<T> Action { get; set; }

        public override bool Parse(Expression expression)
        {
            switch (expression)
            {
                case T t when condition?.Invoke(t) ?? true:
                    foreach (var child in Children)
                    {
                        if (!child.Item1?.Parse(child.Item2(t)) ?? false)
                        {
                            return false;
                        }
                    }
                    Action?.Invoke(t);
                    return true;
                default:
                    return false;
            }
        }

        public static ExpressionParser<T> operator |(ExpressionParser<T> left, ExpressionParser<T> right)
        {
            return new OrParser<T>(left, right);
        }
    }
}
