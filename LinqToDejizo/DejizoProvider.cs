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

            var word = new ConstantParser();
            word.Action = x => condition.Word = (string)x.Value;

            var header = new MemberParser(x => x.Member.Name == "HeaderText");
            header.Action = x =>
            {
                condition.Scope = "HEADWORD";
            };
            var binary1 = new BinaryParser()
            {
                Left = word,
                Right = header
            };
            var binary2 = new BinaryParser()
            {
                Left = header,
                Right = word
            };

            var binary = new OrParser<BinaryExpression>(binary1, binary2);
            binary.Action = _ =>
            {
                condition.Match = "EXACT";
            };

            var call1 = new MethodCallParser(m => m.Method == GetInfo<string, string>((s, p) => s.EndsWith(p)))
            {
                Arguments = new[] { word }
            };

            call1.Action = m =>
            {
                condition.Match = "ENDWITH";
                condition.Scope = "HEADWORD";
            };
            var call2 = new MethodCallParser(m => m.Method == GetInfo<string, string>((s, p) => s.StartsWith(p)))
            {
                Arguments = new[] { word }
            };
            call2.Action = m =>
            {
                condition.Match = "STARTWITH";
                condition.Scope = "HEADWORD";
            };
            var call3 = new MethodCallParser(m => m.Method == GetInfo<string, string>((s, p) => s.Contains(p)))
            {
                Arguments = new[] { word }
            };
            call3.Action = m =>
            {
                condition.Match = "CONTAIN";
                condition.Scope = "HEADWORD";
            };

            var call = new OrParser<MethodCallExpression>(new OrParser<MethodCallExpression>(call1, call2), call3);

            var lambda = new LambdaParser
            {
                Body = new OrParser(binary, call)
            };

            var unary2= new UnaryParser
            {
                Operand = lambda
            };

            var whereMethod = new MethodCallParser(m => m.Method.GetGenericMethodDefinition() == where)
            {
                Arguments = new[] {null, unary2 }
            };
            var selectMethod = new MethodCallParser(m => m.Method.GetGenericMethodDefinition() == select);


            whereMethod.Parse(expression);

            switch (expression)
            {
                //case MethodCallExpression m when m.Method.GetGenericMethodDefinition() == where:
                //    unary2.Parse(m.Arguments[1]);
                //    break;
                case MethodCallExpression m when m.Method.GetGenericMethodDefinition() == select:
                    switch (m.Arguments[0])
                    {
                        case MethodCallExpression mm:
                            unary2.Parse(mm.Arguments[1]);
                            break;
                    }
                    unary.Parse(m.Arguments[1]);
                    break;
            }
        }

        public abstract class ExpressionParser
        {
            public Action Action { get; set; }
            public abstract bool Parse(Expression expression);
        }

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
                        foreach(var child in Children)
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
        }

        public class OrParser : ExpressionParser
        {
            public ExpressionParser Left { get; set; }
            public ExpressionParser Right { get; set; }

            public OrParser(ExpressionParser left, ExpressionParser right)
            {
                Left = left;
                Right = right;
            }
            public override bool Parse(Expression expression)
            {
                if (Left.Parse(expression) || Right.Parse(expression))
                {
                    Action?.Invoke();
                    return true;
                }
                return false;
            }
        }


        public class OrParser<T> : ExpressionParser<T> where T : Expression
        {
            public ExpressionParser<T> Left { get; set; }
            public ExpressionParser<T> Right { get; set; }

            public OrParser(ExpressionParser<T> left, ExpressionParser<T> right)
            {
                Left = left;
                Right = right;
            }
            public override bool Parse(Expression expression)
            {
                if(Left.Parse(expression) || Right.Parse(expression))
                {
                    Action?.Invoke(null);
                    return true;
                }
                return false;
            }
        }

        public class UnaryParser : ExpressionParser<UnaryExpression>
        {
            public LambdaParser Operand { get; set; }

            protected override IEnumerable<(ExpressionParser, Func<UnaryExpression, Expression>)> Children => 
                new(ExpressionParser, Func<UnaryExpression, Expression>)[] 
                {
                    (Operand, x => x.Operand)
                };
        }

        public class BinaryParser : ExpressionParser<BinaryExpression>
        {
            public ExpressionParser Right { get; set; }
            public ExpressionParser Left { get; set; }

            protected override IEnumerable<(ExpressionParser, Func<BinaryExpression, Expression>)> Children =>
                new(ExpressionParser, Func<BinaryExpression, Expression>)[]
                {
                    (Right, x => x.Right),
                    (Left, x => x.Left),
                };
        }

        public class LambdaParser : ExpressionParser<LambdaExpression>
        {
            public ExpressionParser Body { get; set; }

            protected override IEnumerable<(ExpressionParser, Func<LambdaExpression, Expression>)> Children =>
                new(ExpressionParser, Func<LambdaExpression, Expression>)[]
                {
                    (Body, x => x.Body),
                };
        }

        public class MethodCallParser : ExpressionParser<MethodCallExpression>
        {
            public ExpressionParser[] Arguments { get; set; } = new ExpressionParser[] { null, null };

            protected override IEnumerable<(ExpressionParser, Func<MethodCallExpression, Expression>)> Children =>
                new(ExpressionParser, Func<MethodCallExpression, Expression>)[]
                {
                    (Arguments.Any() ? Arguments[0] : null, x => x.Arguments.Any() ? x.Arguments[0] : null),
                    (2 <= Arguments.Count() ? Arguments[1] : null, x => 2 <= x.Arguments.Count ? x.Arguments[1] : null),
                };
            public MethodCallParser() { }
            public MethodCallParser(Func<MethodCallExpression, bool> condition) : base(condition) { }
        }

        public class ConstantParser : ExpressionParser<ConstantExpression>
        {
        }
        public class MemberParser : ExpressionParser<MemberExpression>
        {
            public MemberParser() { }
            public MemberParser(Func<MemberExpression, bool> condition) : base(condition) { }
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
        public override string GetQueryText(Expression expression)
        {
            return "query text";
        }
    }
}
