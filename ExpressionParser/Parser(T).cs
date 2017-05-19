using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.ExpressionParser
{
    public abstract class Parser<T> : Parser where T : Expression
    {
        public Parser() { }

        public Parser(Func<T, bool> condition)
        {
            this.condition = condition;
        }

        Func<T, bool> condition;

        protected virtual IEnumerable<(Parser, Func<T, Expression>)> Children => new(Parser, Func<T, Expression>)[] { };
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

        public static Parser<T> operator |(Parser<T> left, Parser<T> right)
        {
            return new OrParser<T>(left, right);
        }
    }
}
