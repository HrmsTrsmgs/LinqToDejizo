using Marimo.LinqToDejizo;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Marimo.LinqToDejizo
{
    public class DejizoSource
    {
        public IQueryable<DejizoItem> EJdict { get; set; } = new Query<DejizoItem>(new DejizoProvider());
    }

    public class DejizoItem
    {
        public string HeaderText { get; set; }
        public string BodyText { get; set; }
    }

    public class DejizoProvider : QueryProvider
    {
        public override object Execute(Expression expression)
        {
            SearchDicItemResult result = null;
            Task.Run(async () =>
            {
                var client = new HttpClient();

                string word = null;
                switch(expression)
                {
                    case MethodCallExpression m:
                        switch(m.Arguments[0])
                        {
                            case MethodCallExpression mm:
                                switch(mm.Arguments[1])
                                {
                                    case UnaryExpression u:
                                        switch(u.Operand)
                                        {
                                            case LambdaExpression l:
                                                switch(l.Body)
                                                {
                                                    case MethodCallExpression mmm:
                                                        
                                                        switch (mmm.Arguments[0])
                                                        {
                                                            case ConstantExpression c:
                                                                switch (c.Value)
                                                                {
                                                                    case string s:
                                                                        word = s;
                                                                        break;
                                                                }

                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                        }
                                        break;

                                }
                                break;
                        }
                        break;
                }

                var uri =
                    QueryHelpers.AddQueryString(
                        "http://public.dejizo.jp/NetDicV09.asmx/SearchDicItemLite",
                        new Dictionary<string, string>
                        {
                            {"Dic", "EJdict"},
                            {"Word", word},
                            {"Scope", "HEADWORD"},
                            {"Match", "STARTWITH"},
                            {"Merge", "AND"},
                            {"Prof", "XHTML"},
                            {"PageSize", "20"},
                            {"PageIndex", "0"}
                        });

                var response = await client.GetAsync(uri);

                var stream = await response.Content.ReadAsStreamAsync();

                var serializer = new DataContractSerializer(typeof(SearchDicItemResult));

                result = serializer.ReadObject(stream) as SearchDicItemResult;
            }).Wait();

            

            return result.TotalHitCount;
        }

        public override string GetQueryText(Expression expression)
        {
            return "query text";
        }
    }

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



        //public override string ToString()
        //{

        //    return this.provider.GetQueryText(this.expression);

        //}

    }

    public abstract class QueryProvider : IQueryProvider
    {

        protected QueryProvider()
        {

        }



        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {

            return new Query<S>(this, expression);

        }



        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {

            Type elementType = TypeSystem.GetElementType(expression.Type);

            try
            {

                return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });

            }

            catch (TargetInvocationException tie)
            {

                throw tie.InnerException;

            }

        }



        S IQueryProvider.Execute<S>(Expression expression)
        {

            return (S)this.Execute(expression);

        }



        object IQueryProvider.Execute(Expression expression)
        {

            return this.Execute(expression);

        }



        public abstract string GetQueryText(Expression expression);

        public abstract object Execute(Expression expression);

    }

    internal static class TypeSystem
    {

        internal static Type GetElementType(Type seqType)
        {

            Type ienum = FindIEnumerable(seqType);

            if (ienum == null) return seqType;

            return ienum.GetGenericArguments()[0];

        }

        private static Type FindIEnumerable(Type seqType)
        {

            if (seqType == null || seqType == typeof(string))

                return null;

            if (seqType.IsArray)

                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.GetTypeInfo().IsGenericType)
            {

                foreach (Type arg in seqType.GetGenericArguments())
                {

                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (ienum.IsAssignableFrom(seqType))
                    {

                        return ienum;

                    }

                }

            }

            Type[] ifaces = seqType.GetInterfaces();

            if (ifaces != null && ifaces.Length > 0)
            {

                foreach (Type iface in ifaces)
                {

                    Type ienum = FindIEnumerable(iface);

                    if (ienum != null) return ienum;

                }

            }

            if (seqType.GetTypeInfo().BaseType != null && seqType.GetTypeInfo().BaseType != typeof(object))
            {

                return FindIEnumerable(seqType.GetTypeInfo().BaseType);

            }

            return null;

        }

    }
}
