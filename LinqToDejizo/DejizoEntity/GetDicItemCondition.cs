using System;
using System.Collections.Generic;
using System.Text;

namespace Marimo.LinqToDejizo.DejizoEntity
{
    public class GetDicItemCondition
    {
        public string Dic { get; } = "EJdict";
        public string Item { get; set; }
        public string Loc { get; } = "";
        public string Prof { get; } = "XHTML";
    }
}
