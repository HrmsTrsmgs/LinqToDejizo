using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace Marimo.LinqToDejizo.DejizoEntity
{
    [DataContract(Namespace = "http://btonic.est.co.jp/NetDic/NetDicV09")]
    public class GetDicItemResult
    {
        [DataMember(Order =0)]
        public XElement Head { get; set; }

        [DataMember(Order =1)]
        public XElement Body { get; set; }
    }
}
