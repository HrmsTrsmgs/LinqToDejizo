using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace Marimo.LinqToDejizo
{
    internal static class TypeExtensions
    {

        internal static Type ElementType(this Type self)
        {
            return self?.ToIEnumerable()?.GetGenericArguments().First() ?? self;
        }

        private static Type ToIEnumerable(this Type self)
        {
            if (self == null || self == typeof(string))
                return null;

            if (self.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(self.GetElementType());

            if (self.GetTypeInfo().IsGenericType)
            {
                foreach (var arg in self.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (ienum.IsAssignableFrom(self))
                    {
                        return ienum;
                    }
                }
            }

            var ifaces = self.GetInterfaces();

            if (ifaces != null && ifaces.Any())
            {
                foreach (var iface in ifaces)
                {
                    var ienum = ToIEnumerable(iface);

                    if (ienum != null) return ienum;
                }
            }

            if (self.GetTypeInfo().BaseType != null && self.GetTypeInfo().BaseType != typeof(object))
            {
                return ToIEnumerable(self.GetTypeInfo().BaseType);
            }

            return null;
        }
    }
}