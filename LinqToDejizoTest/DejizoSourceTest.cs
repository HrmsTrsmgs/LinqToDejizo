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

        [Fact]
        public void 完全一致検索は変数で指定することもできます()
        {
            var tested = new DejizoSource();
            var word = "dictionary";
            var query =
                from item in tested.EJdict
                where item.HeaderText == word
                select item;

            query.Count().Is(1);
        }

        [Fact]
        public void 英和辞書で完全一致検索は左辺等辺がどちらでもできます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where "dictionary" == item.HeaderText
                select item;

            query.Count().Is(1);
        }

        [Fact]
        public void 英和辞書で取得したものに対してイテレーションが利きます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item;

            foreach(var item in query)
            {
                item.BodyText.Is("ｄｉｃｔａｔｉｏｎ	ｄｉｃｔａｔｏｒ	ｄｉｃｔｉｏｎａｒｙ");
                break;
            }
        }

        [Fact]
        public void 英和辞書で取得したものに対してFirstが使えます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item;
           
           query.First().BodyText.Is("ｄｉｃｔａｔｉｏｎ	ｄｉｃｔａｔｏｒ	ｄｉｃｔｉｏｎａｒｙ");
        }

        [Fact]
        public void Firstは項目がなかった場合にエラーとなります()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("xxx")
                select item;

            Assert.Throws<InvalidOperationException>(() =>query.First());
        }

        [Fact]
        public void 英和辞書で取得したものに対してSingleが使えます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText == "dictionary"
                select item;

            query.Single().BodyText.Is("『辞書』，辞典，字引き");
        }

        [Fact]
        public void Singleは項目がなかった場合にエラーとなります()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("xxx")
                select item;

            Assert.Throws<InvalidOperationException>(() => query.Single());
        }

        [Fact]
        public void Singleは項目が二つ以上あった場合にエラーとなります()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item;

            Assert.Throws<InvalidOperationException>(() => query.Single());
        }

        [Fact]
        public void SelectでBodyTextの選択ができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item.BodyText;

            query.First().Is("ｄｉｃｔａｔｉｏｎ	ｄｉｃｔａｔｏｒ	ｄｉｃｔｉｏｎａｒｙ");
        }

        [Fact]
        public void SelectでHeaderTextの選択ができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select item.HeaderText;

            query.First().Is("dict.");
        }

        [Fact]
        public void Selectでオブジェクトの組み立てができます()
        {
            var tested = new DejizoSource();

            var query =
                from item in tested.EJdict
                where item.HeaderText.StartsWith("dict")
                select new { item.HeaderText, item.BodyText };

            query.First().HeaderText.Is("dict.");
            query.First().BodyText.Is("ｄｉｃｔａｔｉｏｎ	ｄｉｃｔａｔｏｒ	ｄｉｃｔｉｏｎａｒｙ");
        }
    }
}
