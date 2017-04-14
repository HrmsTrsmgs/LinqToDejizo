using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Marimo.LinqToDejizo
{
    [DataContract(Namespace ="http://btonic.est.co.jp/NetDic/NetDicV09")]
    public class SearchDicItemResult
    {
        [DataMember]
        public int TotalHitCount { get; set; }
    }
}