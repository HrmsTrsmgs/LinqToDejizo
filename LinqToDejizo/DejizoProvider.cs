using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace Marimo.LinqToDejizo
{
    public class DejizoProvider : QueryProvider
    {
        DejizoClient client = new DejizoClient();

        public override object Execute(Expression expression)
        {
            SearchDicItemResult itemsInfo = null;
            IEnumerable<GetDicItemResult> items = null;
            SearchDicItemCondition condition = new SearchDicItemCondition();

            ParseLinqRoot(expression, condition);

            Task.Run(async () =>
            {
                itemsInfo = await client.SearchDicItemLite(condition);

                IEnumerable<GetDicItemResult> GetResults()
                {
                    foreach (var item in itemsInfo.TitleList)
                    {
                        yield return client.GetDicItemLite(item.ItemID).GetAwaiter().GetResult();
                    }
                }

                items = GetResults();

            }).GetAwaiter().GetResult();

            var selectLambda = (Func<DejizoItem, object>)condition.SelectLambda?.Compile() ?? (x => x);

            var query =
                from item in items
                select selectLambda(new DejizoItem(item));

            switch (condition.ResultType)
            {
                case "Count":
                    return itemsInfo.TotalHitCount;
                case "First":
                    return query.First();
                case "Single":
                    return query.Single();
                case "SelectItems":
                    return query;
                default:
                    return null;
            }
        }

        private MethodInfo GetMethod<S,T>(Expression<Func<IEnumerable<S>,T>> method)
        {
            return null;
        }

        private void ParseLinqRoot(Expression expression, SearchDicItemCondition condition)
        {
            switch (expression)
            {
                case MethodCallExpression m when new[] { "Count", "First", "Single" }.Contains(m.Method.Name):
                    condition.ResultType = m.Method.Name;
                    ParseSelectItems(m.Arguments[0], condition);
                    break;
                case MethodCallExpression m:
                    condition.ResultType = "SelectItems";
                    ParseSelectItems(m, condition);
                    break;

            }
        }

        private static void ParseSelectItems(Expression expression, SearchDicItemCondition condition)
        {
            switch (expression)
            {
                case MethodCallExpression m when m.Method.Name == "Where":
                    ParseWherePart(condition, m);
                    break;
                case MethodCallExpression m when m.Method.Name == "Select":
                    switch(m.Arguments[0])
                    {
                        case MethodCallExpression mm:
                            ParseWherePart(condition, mm);
                            switch(m.Arguments[1])
                            {
                                case UnaryExpression u:
                                    switch (u.Operand)
                                    {
                                        case LambdaExpression l:
                                            condition.SelectLambda = l;
                                            break;
                                    }
                                            
                                break;
                            }
                            
                            break;
                    }
                    break;
            }
        }

        private static void ParseWherePart(SearchDicItemCondition condition, MethodCallExpression expression)
        {
            switch (expression.Arguments[1])
            {
                case UnaryExpression u:
                    switch (u.Operand)
                    {
                        case LambdaExpression l:
                            switch (l.Body)
                            {
                                case MethodCallExpression m:

                                    if (m.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                                    {
                                        condition.Match = "ENDWITH";
                                    }
                                    else if (m.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                                    {
                                        condition.Match = "STARTWITH";
                                    }
                                    else if (m.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                                    {
                                        condition.Match = "CONTAIN";
                                    }
                                    WordConstant(m.Arguments[0], condition);
                                    break;
                                case BinaryExpression b:
                                    condition.Match = "EXACT";
                                    WordConstant(b.Right, condition);
                                    break;
                            }
                            break;
                    }
                    break;

            }
        }

        private static void WordConstant(Expression expression, SearchDicItemCondition condition)
        {
            switch (expression)
            {
                case ConstantExpression c:
                    switch (c.Value)
                    {
                        case string s:
                            condition.Word = s;
                            break;
                    }

                    break;
            }
        }

        public override string GetQueryText(Expression expression)
        {
            return "query text";
        }
    }
}
