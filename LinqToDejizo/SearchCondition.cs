
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Marimo.LinqToDejizo
{
    public class SearchCondition
    {
        public string ResultType { get; set; }
        public string Word { get; set; }
        public string Match { get; set; }
        public List<(string Word, string Match)> Conditions { get; set; } = new List<(string Word, string Match)>();

        public string Scope { get; set; }
        public LambdaExpression SelectLambda { get; set; }
    }
}