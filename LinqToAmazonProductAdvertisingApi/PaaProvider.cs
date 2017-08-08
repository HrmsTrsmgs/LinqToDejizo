using AmazonProductAdvtApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Marimo.LinqToDejizo
{
    public class PaaProvider : QueryProvider
    {
        private string AwsAccessKeyID { get; }
        private string AwsSecretKey { get; }
        private string AssociateTag { get; }

        public PaaProvider(string awsAccessKeyID, string awsSecretKey, string associateTag)
        {
            AwsAccessKeyID = awsAccessKeyID;
            AwsSecretKey = awsSecretKey;
            AssociateTag = associateTag;
        }

        public override object Execute(Expression expression)
        {
            SignedRequestHelper helper = new SignedRequestHelper(AwsAccessKeyID, AwsSecretKey, "ecs.amazonaws.jp");

            /*
             * Here is an ItemLookup example where the request is stored as a dictionary.
             */
            IDictionary<string, string> r1 = new Dictionary<string, String>();
            r1["Service"] = "AWSECommerceService";
            r1["Operation"] = "ItemSearch";
            r1["AWSAccessKeyId"] = AwsAccessKeyID;
            r1["AssociateTag"] = AssociateTag;
            r1["SearchIndex"] = "All";
            r1["ResponseGroup"] = "Images,ItemAttributes,Offers";
            r1["Keywords"] = "\"初めてのRuby\"";


            var requestUrl = helper.Sign(r1);
            var title = FetchTitle(requestUrl);
            return title;
        }

        HttpClient client = new HttpClient();

        private string FetchTitle(string url)
        {
            string str = client.GetAsync(url).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XDocument xml = XDocument.Parse(str);

            var ns = xml.Root.Name.Namespace;
            var errorMessageNodes = xml.Descendants(ns + "Message").ToList();
            if (errorMessageNodes.Any())
            {
                var message = errorMessageNodes[0].Value;
                return "Error: " + message;
            }

            var title = xml.Descendants(ns + "Title").First();
            return title.Value;
        }

        public override string GetQueryText(Expression expression)
        {
            try
            {
                return "query text";
                return expression?.ToString() ?? "null";
            }
            catch
            {
                return "query text";
            }
        }
    }
}
