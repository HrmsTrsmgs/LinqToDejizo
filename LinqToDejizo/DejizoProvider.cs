﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using static Marimo.LinqToDejizo.ParserCreaters;
using static Marimo.LinqToDejizo.DejizoProvider;

namespace Marimo.LinqToDejizo
{
    public static class ParserCreaters
    {
        public static MethodCallParser MethodCall(Func<MethodCallExpression, bool> condition, IEnumerable<ExpressionParser> arguments)
        =>  new MethodCallParser(condition)
            {
                Arguments = arguments.ToArray()
            };
        public static MethodCallParser MethodCall<TReceiver>(Expression<Func<TReceiver, object>> callExpression, IEnumerable<ExpressionParser> arguments)
            => MethodCall(m => m.Method.GetGenericMethodDefinition() == GetInfo<TReceiver>(callExpression).GetGenericMethodDefinition(), arguments);



        private static MethodInfo GetInfo<TReceiver>(Expression<Func<TReceiver, object>> callExpression)
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

        private static MethodInfo GetInfo<TReceiver, TIn>(Expression<Func<TReceiver, TIn, object>> callExpression)
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

    }
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
            var where = GetInfo<IQueryable<object>>(c => c.Where(x => true)).GetGenericMethodDefinition();
            var select = GetInfo<IQueryable<object>>(c => c.Select(x => x)).GetGenericMethodDefinition();

            var selectLambda = new LambdaParser();

            var word = new ConstantParser();
            var header = new MemberParser(x => x.Member.Name == "HeaderText");

            var binary = new BinaryParser
            {
                Left = word,
                Right = header
            } | new BinaryParser
            {
                Left = header,
                Right = word
            };

            var call1 = new MethodCallParser(m => m.Method == GetInfo<string, string>((s, p) => s.EndsWith(p)))
            {
                Arguments = new[] { word }
            };
            var call2 = new MethodCallParser(m => m.Method == GetInfo<string, string>((s, p) => s.StartsWith(p)))
            {
                Arguments = new[] { word }
            };
            var call3 = new MethodCallParser(m => m.Method == GetInfo<string, string>((s, p) => s.Contains(p)))
            {
                Arguments = new[] { word }
            };
            var unary = new UnaryParser
            {
                Operand = new LambdaParser
                {
                    Body = binary | call1 | call2 | call3
                }
            };
            var query = new MethodCallParser(m => m.Method.GetGenericMethodDefinition() == where)
            {
                Arguments = new[] { null, unary }
            } 
            | new MethodCallParser(m => m.Method.GetGenericMethodDefinition() == select)
            {
                Arguments = new ExpressionParser[]
                {
                    new MethodCallParser
                    {
                        Arguments = new[]{null, unary}
                    },
                    new UnaryParser { Operand = selectLambda }
                }
            };
            var countCall = MethodCall<IQueryable<object>>(c => c.Count(), arguments: new[] { query });
            var firstCall = MethodCall<IQueryable<object>>(c => c.First(), arguments: new[] { query });
            var singleCall = MethodCall<IQueryable<object>>(c => c.Single(), arguments: new[] { query });
            var lastMethod = countCall | firstCall | singleCall;

            var wholeExtention = lastMethod | query;
            
            selectLambda.Action = l => condition.SelectLambda = l;
            word.Action = x => condition.Word = (string)x.Value;
            header.Action = x => condition.Scope = "HEADWORD";
            binary.Action = _ => condition.Match = "EXACT";
            call1.Action = m =>
            {
                condition.Match = "ENDWITH";
                condition.Scope = "HEADWORD";
            };
            call2.Action = m =>
            {
                condition.Match = "STARTWITH";
                condition.Scope = "HEADWORD";
            };
            call3.Action = m =>
            {
                condition.Match = "CONTAIN";
                condition.Scope = "HEADWORD";
            };
            countCall.Action = m => condition.ResultType = m.Method.Name;
            firstCall.Action = m => condition.ResultType = m.Method.Name;
            singleCall.Action = m => condition.ResultType = m.Method.Name;
            query.Action = _ => condition.ResultType = "SelectItems";

            wholeExtention.Parse(expression);
        }

        public abstract class ExpressionParser
        {
            public Action Action { get; set; }
            public abstract bool Parse(Expression expression);

            public static ExpressionParser operator|(ExpressionParser left, ExpressionParser right)
            {
                return new OrParser(left, right);
            }
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

            public static ExpressionParser<T> operator |(ExpressionParser<T> left, ExpressionParser<T> right)
            {
                return new OrParser<T>(left, right);
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
