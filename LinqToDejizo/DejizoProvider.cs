using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Marimo.LinqToDejizo
{
    public class DejizoProvider : QueryProvider
    {
        DejizoClient client = new DejizoClient();

        public override object Execute(Expression expression)
        {
            SearchDicItemResult result = null;
            IEnumerable<GetDicItemResult> results = null;
            SearchDicItemCondition condition = new SearchDicItemCondition();
            Task.Run(async () =>
            {
                switch (expression)
                {
                    case MethodCallExpression m when m.Method.Name == "Count":
                        condition.ResultType = "Count";
                        SelectItems(m.Arguments[0], condition);
                        break;
                    case MethodCallExpression m:
                        condition.ResultType = "SelectItems";
                        SelectItems(m, condition);
                        break;

                }
                result = await client.SearchDicItemLite(condition);

                IEnumerable<GetDicItemResult> GetResults()
                {
                    foreach(var item in result.TitleList)
                    {
                        yield return client.GetDicItemLite(item.ItemID).GetAwaiter().GetResult();
                    }
                }

                results = GetResults();

            }).GetAwaiter().GetResult();

            switch(condition.ResultType)
            {
                case "Count":
                    return result.TotalHitCount;
                case "SelectItems":
                    return 
                        from item in results
                        select new DejizoItem { HeaderText = item.Head.Value.Trim(), BodyText = item.Body.Value.Trim() };
                default:
                    return null;
            }
            
        }

        private static void SelectItems(Expression expression, SearchDicItemCondition condition)
        {
            switch (expression)
            {
                case MethodCallExpression mm:
                    switch (mm.Arguments[1])
                    {
                        case UnaryExpression u:
                            switch (u.Operand)
                            {
                                case LambdaExpression l:
                                    switch (l.Body)
                                    {
                                        case MethodCallExpression mmm:

                                            if (mmm.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                                            {
                                                condition.Match = "ENDWITH";
                                            }
                                            else if (mmm.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                                            {
                                                condition.Match = "STARTWITH";
                                            }
                                            else if (mmm.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                                            {
                                                condition.Match = "CONTAIN";
                                            }
                                            WordConstant(mmm.Arguments[0], condition);
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
            return expression.ToString();
        }
    }
}
