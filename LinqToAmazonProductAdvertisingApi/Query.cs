using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Marimo.LinqToDejizo
{
    public class Query<T> : IQueryable<T>
    {

        QueryProvider provider;

        Expression expression;

        public Type ElementType => typeof(T);

        public Expression Expression => expression;

        public IQueryProvider Provider => provider;
        public IEnumerator<T> GetEnumerator() => ((IEnumerable)provider.Execute(expression)).Cast<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public Query(QueryProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            expression = Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (!typeof(IQueryable<T>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
            {

                throw new ArgumentOutOfRangeException(nameof(expression));

            }

            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            this.expression = expression;
        }

        public override string ToString()
        {

            return provider.GetQueryText(expression);

        }

    }
}
