using Marimo.LinqToDejizo.DejizoEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Marimo.LinqToDejizo
{
    public class DejizoItem
    {
        public string HeaderText { get; set; }
        public string BodyText { get; set; }

        public DejizoItem(GetDicItemResult result)
        {
            HeaderText = result.Head.Value.Trim();
            BodyText = result.Body.Value.Trim();
        }
    }
}