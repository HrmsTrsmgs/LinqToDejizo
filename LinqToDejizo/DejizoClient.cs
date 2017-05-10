using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Marimo.LinqToDejizo
{
    public class DejizoClient
    {
        public event EventHandler<DejizoRequestEventArgs> Requested;
        public event EventHandler<DejizoResponseEventArgs> Responsed;
        public TimeSpan Interval { get; set; } = new TimeSpan(0, 0, 0, 60);

        static DateTime lastGettingTime = DateTime.MinValue;

        static HttpClient client = new HttpClient();

        public async Task<SearchDicItemResult> SearchDicItemLite(SearchDicItemCondition condition)
        {
            await Wait();
            
            var uri =
                QueryHelpers.AddQueryString(
                    "http://public.dejizo.jp/NetDicV09.asmx/SearchDicItemLite",
                    new Dictionary<string, string>
                    {
                        {"Dic", "EJdict"},
                        {"Word", condition.Word},
                        {"Scope", condition.Scope},
                        {"Match", condition.Match},
                        {"Merge", "AND"},
                        {"Prof", "XHTML"},
                        {"PageSize", "100"},
                        {"PageIndex", "0"}
                    });

            Requested?.Invoke(this, new DejizoRequestEventArgs { Uri = new Uri(uri) });
            var response = await client.GetAsync(uri);

            Responsed?.Invoke(this, new DejizoResponseEventArgs { Uri = new Uri(uri), ResponseJon = await response.Content.ReadAsStringAsync() });

            var stream = await response.Content.ReadAsStreamAsync();
            
            var serializer = new DataContractSerializer(typeof(SearchDicItemResult));

            return serializer.ReadObject(stream) as SearchDicItemResult;
        }

        public async Task<GetDicItemResult> GetDicItemLite(string itemId)
        {
            await Wait();
            var uri =
                QueryHelpers.AddQueryString(
                    "http://public.dejizo.jp/NetDicV09.asmx/GetDicItemLite",
                    new Dictionary<string, string>
                    {
                            {"Dic", "EJdict"},
                            {"Item", itemId},
                            {"Loc", ""},
                            {"Prof", "XHTML"}
                    });
            Requested?.Invoke(this, new DejizoRequestEventArgs { Uri = new Uri(uri) });
            var response = await client.GetAsync(uri);
            
            Responsed?.Invoke(this, new DejizoResponseEventArgs { Uri = new Uri(uri), ResponseJon = await response.Content.ReadAsStringAsync() });
            var stream = await response.Content.ReadAsStreamAsync();

            var serializer = new DataContractSerializer(typeof(GetDicItemResult));

            return serializer.ReadObject(stream) as GetDicItemResult;
        }

        private async Task Wait()
        {
            var betweenLast = DateTime.Now - lastGettingTime;
            if (betweenLast < Interval)
            {
                await Task.Delay(betweenLast.Milliseconds);
            }
            lastGettingTime = DateTime.Now;
        }
    }
}
