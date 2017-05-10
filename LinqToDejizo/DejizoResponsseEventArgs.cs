using System;
using System.Collections.Generic;
using System.Text;

namespace Marimo.LinqToDejizo
{
    public class DejizoResponseEventArgs : EventArgs
    {
        public Uri Uri { get; internal set; }
        public string ResponseJon { get; internal set; }
    }
}
