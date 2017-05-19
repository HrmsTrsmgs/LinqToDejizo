using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Reflection;

namespace Marimo.ExpressionParser
{
    public static class ParserCreaters
    {
        public static MethodCallParser MethodCall(IEnumerable<Parser> arguments)
           => new MethodCallParser
           {
               Arguments = arguments.ToArray()
           };

        public static MethodCallParser MethodCall(Func<MethodCallExpression, bool> condition, IEnumerable<Parser> arguments)
        => new MethodCallParser(condition)
        {
            Arguments = arguments.ToArray()
        };
        public static MethodCallParser _<TReceiver>(Expression<Func<TReceiver, object>> callExpression, IEnumerable<Parser> arguments)
            => MethodCall(m => m.Method.GetGenericMethodDefinition() == GetInfo(callExpression).GetGenericMethodDefinition(), arguments);

        public static MethodCallParser _<TReceiver1, TReceiver2>(Expression<Func<TReceiver1, TReceiver2, object>> callExpression, IEnumerable<Parser> arguments)
            => MethodCall(m => m.Method == GetInfo(callExpression), arguments);

        public static UnaryParser Unary(Parser operand)
            => new UnaryParser
            {
                Operand = operand
            };

        public static LambdaParser Lambda(Parser body = null)
            => new LambdaParser
            {
                Body = body
            };

        public static BinaryParser Binary(Parser left, Parser right)
            => new BinaryParser
            {
                Left = left,
                Right = right
            };

        public static MemberParser Member(Func<MemberExpression, bool> condition)
            => new MemberParser(condition);

        public static ConstantParser Constant()
            => new ConstantParser();

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
}
