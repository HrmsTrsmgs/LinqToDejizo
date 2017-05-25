using Marimo.ExpressionParserCombinator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using static Marimo.ExpressionParserCombinator.ParserCreaters;

namespace Marimo.LinqToDejizo
{
    public class DejizoProvider : QueryProvider
    {
        public DejizoProvider()
        {
            client = new DejizoClient();
            client.Requested += (sender, e) =>
            {
                Requested?.Invoke(this, e);
            };
            client.Responsed += (sender, e) =>
            {
                Responsed?.Invoke(this, e);
            };
        }

        public event EventHandler<DejizoRequestEventArgs> Requested;
        public event EventHandler<DejizoResponseEventArgs> Responsed;


        DejizoClient client;

        async Task<(SearchDicItemResult itemsInfo, IEnumerable<GetDicItemResult> items)> GetItems(SearchDicItemCondition condition)
        {
            var titles = await client.SearchDicItemLite(condition);

            IEnumerable<GetDicItemResult> GetResults()
            {
                foreach (var item in titles.TitleList)
                {
                    yield return client.GetDicItemLite(item.ItemID).GetAwaiter().GetResult();
                }
            }

            return (titles, GetResults());
        }

        public override object Execute(Expression expression)
        {
            var condition = ParseLinqRoot(expression);
            
            (var itemsInfo, var items) = Task.Run(async () => await GetItems(condition)).GetAwaiter().GetResult();

            var query =
                from item in items
                select ((Func<DejizoItem, object>)condition.SelectLambda?.Compile() ?? (x => x))(
                    new DejizoItem(item));

            switch (condition.ResultType)
            {
                case "Count":
                    return itemsInfo.TotalHitCount;
                case "First":
                    return query.First();
                case "Single":
                    return query.Single();
                case "FirstOrDefault":
                    return query.FirstOrDefault();
                case "SelectItems":
                    return query;
                default:
                    return null;
            }
        }

        private T GetValue<T>(MemberExpression memberExpression, PropertyInfo propertyInfo)
        {
            return (T)propertyInfo.GetValue(GetValue<object>(memberExpression), null);
        }
        private T GetValue<T>(MemberExpression memberExpression)
        {
            var childExpression = memberExpression.Expression;
            if (childExpression.NodeType == ExpressionType.MemberAccess)
            {
                return (T)GetValue<object>(childExpression as MemberExpression, memberExpression.Member as PropertyInfo);
            }
            else
            {
                var constant = childExpression as ConstantExpression;
                var fieldInfo = memberExpression.Member as FieldInfo;
                return (T)fieldInfo.GetValue(constant.Value);
            }
        }

        private static T GetValue<T>(ConstantExpression constantExpression)
        {
            return (T)constantExpression.Value;
        }

        private SearchDicItemCondition ParseLinqRoot(Expression expression)
        {
            var condition = new SearchDicItemCondition();

            var selectLambda = Lambda();

            var constWord = Constant();

            var valiableWord = Member();

            var word = constWord | valiableWord;

            var header = Member(x => x.Member.Name == "HeaderText");

            var equals = Binary(word, header) | Binary(header, word);

            var endsWith = 
                _((string s, string p) => s.EndsWith(p),
                    arguments: new[] { word });

            var startsWith =
                _((string s, string p) => s.StartsWith(p),
                    arguments: new[] { word });

            var contains = 
                _((string s, string p) => s.Contains(p),
                    arguments: new[] { word });

            var whereFunc = 
                Unary(
                    operand: Lambda(equals | endsWith | startsWith | contains));

            var query =
                _((IQueryable<object> c) => c.Where(x => true),
                    arguments: new[] { null, whereFunc }) 
                |
                _((IQueryable<object> c) => c.Select(x => x),
                    arguments: new ExpressionParser[]
                    {
                        MethodCall(
                            arguments:new[]{ null, whereFunc }),
                        Unary(
                            operand: selectLambda)
                    });

            var count =
                _((IQueryable<object> c) => c.Count(),
                    arguments: new[] { query });

            var first =
                _((IQueryable<object> c) => c.First(),
                    arguments: new[] { query });

            var firstOrDefault =
                _((IQueryable<object> c) => c.FirstOrDefault(),
                    arguments: new[] { query });

            var single =
                _((IQueryable<object> c) => c.Single(),
                    arguments: new[] { query });

            var lastMethod = count | first | single | firstOrDefault;

            var wholeExtention = lastMethod | query;

            selectLambda.Action = l => condition.SelectLambda = l;
            constWord.Action = x => condition.Word = GetValue<string>(x);
            valiableWord.Action = x => condition.Word = GetValue<string>(x);
            
            header.Action = x => condition.Scope = "HEADWORD";
            equals.Action = _ => condition.Match = "EXACT";
            endsWith.Action = m =>
            {
                condition.Match = "ENDWITH";
                condition.Scope = "HEADWORD";
            };
            startsWith.Action = m =>
            {
                condition.Match = "STARTWITH";
                condition.Scope = "HEADWORD";
            };
            contains.Action = m =>
            {
                condition.Match = "CONTAIN";
                condition.Scope = "HEADWORD";
            };
            lastMethod.Action = m => condition.ResultType = m.Method.Name;
            query.Action = _ => condition.ResultType = "SelectItems";

            wholeExtention.Parse(expression);

            return condition;
        }

        public override string GetQueryText(Expression expression)
        {
            return expression?.ToString() ?? "null";
        }
    }
}
