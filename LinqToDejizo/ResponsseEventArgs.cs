using System;
using System.Collections.Generic;
using System.Text;

namespace Marimo.LinqToDejizo
{
    public class ResponseEventArgs : EventArgs
    {
        public Uri Uri { get; internal set; }
        public string Response { get; internal set; }
    }
}
