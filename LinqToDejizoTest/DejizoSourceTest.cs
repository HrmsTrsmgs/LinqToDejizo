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


            (from word in tested.RestEJdict
             where word.StartsWith("dict")
             select word)
            .Count().Is(11);
        }
    }
}
