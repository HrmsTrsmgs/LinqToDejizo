using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using static Marimo.ExpressionParser.ParserCreaters;
using static Marimo.LinqToDejizo.DejizoProvider;
using Marimo.ExpressionParser;

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

        public override object Execute(Expression expression)
        {
            var condition = new SearchDicItemCondition();

            ParseLinqRoot(expression, condition);

            (var itemsInfo, var items) =
                Task.Run(async () =>
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

                }).GetAwaiter().GetResult();

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
                case "SelectItems":
                    return query;
                default:
                    return null;
            }
        }

        private void ParseLinqRoot(Expression expression, SearchDicItemCondition condition)
        {
            var selectLambda = Lambda();

            var word = Constant();

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

            var cast = 
                Unary(
                    operand: Lambda(equals | endsWith | startsWith | contains));

            var query =
                _((IQueryable<object> c) => c.Where(x => true),
                    arguments: new[] { null, cast }) 
                |
                _((IQueryable<object> c) => c.Select(x => x),
                    arguments: new Parser[]
                    {
                        MethodCall(
                            arguments:new[]{ null, cast }),
                        Unary(
                            operand: selectLambda)
                    });

            var count =
                _((IQueryable<object> c) => c.Count(),
                    arguments: new[] { query });

            var first =
                _((IQueryable<object> c) => c.First(),
                    arguments: new[] { query });

            var single =
                _((IQueryable<object> c) => c.Single(),
                    arguments: new[] { query });

            var lastMethod = count | first | single;

            var wholeExtention = lastMethod | query;

            selectLambda.Action = l => condition.SelectLambda = l;
            word.Action = x => condition.Word = (string)x.Value;
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
            count.Action = m => condition.ResultType = m.Method.Name;
            first.Action = m => condition.ResultType = m.Method.Name;
            single.Action = m => condition.ResultType = m.Method.Name;
            query.Action = _ => condition.ResultType = "SelectItems";

            wholeExtention.Parse(expression);
        }

        public override string GetQueryText(Expression expression)
        {
            return "query text";
        }
    }
}
