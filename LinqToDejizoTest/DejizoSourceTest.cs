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
        public void 英和辞書でCountが取得できます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item;

            query.Count().Is(11);
        }

        [Fact]
        public void 英和辞書で検索する単語に合わせたCountが取得できます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("take")
                select item;

            query.Count().Is(6);
        }

        [Fact]
        public void 英和辞書で前方一致検索ができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item;

            query.Count().Is(11);
        }

        [Fact]
        public void 英和辞書で後方一致検索ができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.EndsWith("dict")
                select item;

            query.Count().Is(10);
        }

        [Fact]
        public void 英和辞書であいまい検索ができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.Contains("dict")
                select item;

            query.Count().Is(45);
        }

        [Fact]
        public void 英和辞書で完全一致検索ができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText == "dictionary"
                select item;

            query.Count().Is(1);
        }
    }
}
