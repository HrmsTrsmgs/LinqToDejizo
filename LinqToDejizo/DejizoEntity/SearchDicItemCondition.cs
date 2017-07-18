using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.LinqToDejizo.DejizoEntity
{
    public class SearchDicItemCondition
    {
        public string Dic { get; set; } = "EJdict";
        public string ResultType { get; set; }
        public string Word { get; set; }
        public string Match { get; set; }
        public string Scope { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; }
        public string Merge { get; set; } = "AND";
        public string Prof { get; set; } = "XHTML";
    }
}
