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
            SearchDicItemCondition condition = new SearchDicItemCondition();

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
                select GetSelectLambda(condition)(new DejizoItem(item));

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

        private Func<DejizoItem, object> GetSelectLambda(SearchDicItemCondition condition) =>
            (Func<DejizoItem, object>)condition.SelectLambda?.Compile() ?? (x => x);

        private void ParseLinqRoot(Expression expression, SearchDicItemCondition condition)
        {
            var count = GetInfo<IQueryable<object>>(c => c.Count()).GetGenericMethodDefinition();
            var first = GetInfo<IQueryable<object>>(c => c.First()).GetGenericMethodDefinition();
            var single = GetInfo<IQueryable<object>>(c => c.Single()).GetGenericMethodDefinition();

            switch (expression)
            {
                case MethodCallExpression m when new[] { count, first, single }.Contains(m.Method.GetGenericMethodDefinition()):
                    condition.ResultType = m.Method.Name;
                    ParseSelectItems(m.Arguments[0], condition);
                    break;
                case MethodCallExpression m:
                    condition.ResultType = "SelectItems";
                    ParseSelectItems(m, condition);
                    break;

            }
        }

        private  void ParseSelectItems(Expression expression, SearchDicItemCondition condition)
        {
            var selectLambda = new LambdaParser();

            var unary = new UnaryParser { Operand = selectLambda };

            selectLambda.Action = l => condition.SelectLambda = l;
        
            var where = GetInfo<IQueryable<object>>(c => c.Where(x => true)).GetGenericMethodDefinition();
            var select = GetInfo<IQueryable<object>>(c => c.Select(x => x)).GetGenericMethodDefinition();

            switch (expression)
            {
                case MethodCallExpression m when m.Method.GetGenericMethodDefinition() == where:
                    ParseWherePart(condition, m);
                    break;
                case MethodCallExpression m when m.Method.GetGenericMethodDefinition() == select:
                    switch (m.Arguments[0])
                    {
                        case MethodCallExpression mm:
                            ParseWherePart(condition, mm);
                            break;
                    }
                    unary.Parse(m.Arguments[1]);
                    break;
            }
        }

        public abstract class ExpressionParser
        {
            protected abstract IEnumerable<ExpressionParser> Children { get; }
            public abstract bool Parse(Expression expression);
        }

        public abstract class ExpressionParser<T> : ExpressionParser where T : Expression
        {
            public Action<T> Action { get; set; }

            public override bool Parse(Expression expression)
            {
                switch (expression)
                {
                    case T t:
                        foreach(var child in Children)
                        {
                            if (!child.Parse(((UnaryExpression)(object)t).Operand))
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
        }

        public class UnaryParser : ExpressionParser<UnaryExpression>
        {
            public LambdaParser Operand { get; set; }

            protected override IEnumerable<ExpressionParser> Children => new[] { (ExpressionParser)Operand };
        }

        public class LambdaParser : ExpressionParser<LambdaExpression>
        {
            protected override IEnumerable<ExpressionParser> Children => new ExpressionParser[] { };
        }

        private MethodInfo GetInfo<TReceiver>(Expression<Func<TReceiver, object>> callExpression)
        {
            switch (callExpression)
            {
                case LambdaExpression l:
                    switch (l.Body)
                    {
                        case UnaryExpression u:
                            switch (u.Operand)
                            {
                                case MethodCallExpression m:
                                    return m.Method;
                                default:
                                    throw new Exception();
                            }
                        case MethodCallExpression m:
                            return m.Method;
                        default:
                            throw new Exception();
                    }
                default:
                    throw new Exception();
            }
        }

        private MethodInfo GetInfo<TReceiver, TIn>(Expression<Func<TReceiver, TIn, object>> callExpression)
        {
            switch (callExpression)
            {
                case LambdaExpression l:
                    switch (l.Body)
                    {
                        case UnaryExpression u:
                            switch (u.Operand)
                            {
                                case MethodCallExpression m:
                                    return m.Method;
                                default:
                                    throw new Exception();
                            }
                        default:
                            throw new Exception();
                    }
                default:
                    throw new Exception();
            }
        }

        private void ParseWherePart(SearchDicItemCondition condition, MethodCallExpression expression)
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
                                    if (m.Method == GetInfo<string, string>((s, p) => s.EndsWith(p)))
                                    {
                                        condition.Match = "ENDWITH";
                                        condition.Scope = "HEADWORD";
                                    }
                                    else if (m.Method == GetInfo<string, string>((s, p) => s.StartsWith(p)))
                                    {
                                        condition.Match = "STARTWITH";
                                        condition.Scope = "HEADWORD";
                                    }
                                    else if (m.Method == GetInfo<string, string>((s, p) => s.Contains(p)))
                                    {
                                        condition.Match = "CONTAIN";
                                        condition.Scope = "HEADWORD";
                                    }
                                    WordConstant(m.Arguments[0], condition);
                                    break;
                                case BinaryExpression b:
                                    condition.Match = "EXACT";
                                    switch(b.Right)
                                    {
                                        case ConstantExpression c:
                                            WordConstant(b.Right, condition);
                                            switch (b.Left)
                                            {
                                                case MemberExpression m:
                                                    if(m.Member.Name == "HeaderText")
                                                    {
                                                        condition.Scope = "HEADWORD"; 
                                                    }
                                                    break;
                                            }
                                            break;
                                        case MemberExpression m:
                                            WordConstant(b.Left, condition);
                                            switch (b.Right)
                                            {
                                                case MemberExpression mm:
                                                    if (mm.Member.Name == "HeaderText")
                                                    {
                                                        condition.Scope = "HEADWORD";
                                                    }
                                                    break;
                                            }
                                            break;
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
            return "query text";
        }
    }
}
