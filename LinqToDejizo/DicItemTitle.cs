using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Marimo.LinqToDejizo
{
    [DataContract(Namespace = "http://btonic.est.co.jp/NetDic/NetDicV09")]
    public class DicItemTitle
    {
        [DataMember]
        public string ItemID { get; set; }
    }
}
