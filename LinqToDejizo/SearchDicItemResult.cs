using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Marimo.LinqToDejizo
{
    [DataContract(Namespace ="http://btonic.est.co.jp/NetDic/NetDicV09")]
    public class SearchDicItemResult
    {
        public SearchDicItemResult()
        {
            TitleList = new List<DicItemTitle>();
        }

        [DataMember(Order = 1)]
        public int TotalHitCount { get; set; }

        [DataMember(Order = 2)]
        public List<DicItemTitle> TitleList { get; set; }
    }
    
}