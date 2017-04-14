using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace Marimo.LinqToDejizo.Test
{
    public class DejizoSourceTest
    {
        [Fact]
        public void RestAPIでCountが取得できます()
        {
            var tested = new DejizoSource();

            var query =
                from word in tested.RestEJdict
                where word.StartsWith("dict")
                select word;

            query.Count().Is(11);
        }

        [Fact]
        public void RestAPIで検索する単語に合わせたCountが取得できます()
        {
            var tested = new DejizoSource();

            var query =
                from word in tested.RestEJdict
                where word.StartsWith("take")
                select word;

            query.Count().Is(6);
        }
    }
}
