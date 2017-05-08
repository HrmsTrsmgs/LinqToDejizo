using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.LinqToDejizo
{
    public class SearchDicItemCondition
    {
        public string ResultType { get; set; }
        public string Word { get; set; }
        public string Match { get; set; }
        public LambdaExpression SelectLambda { get; set; }
    }
}
