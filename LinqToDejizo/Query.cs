﻿using System;
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

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)this.provider.Execute(this.expression)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();



        public Query(QueryProvider provider)
        {

            if (provider == null)
            {

                throw new ArgumentNullException(nameof(provider));

            }

            this.provider = provider;

            this.expression = Expression.Constant(this);

        }



        public Query(QueryProvider provider, Expression expression)
        {

            if (provider == null)
            {

                throw new ArgumentNullException(nameof(provider));

            }

            if (expression == null)
            {

                throw new ArgumentNullException(nameof(expression));

            }

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {

                throw new ArgumentOutOfRangeException(nameof(expression));

            }

            this.provider = provider;

            this.expression = expression;

        }



        public override string ToString()
        {

            return this.provider.GetQueryText(this.expression);

        }

    }
}
