using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;
using Marimo.LinqToDejizo.DejizoEntity;

namespace Marimo.LinqToDejizo
{
    public class TypicalHttpClient
    {
        private HttpClient baseClient { get; }

        public event EventHandler<RequestEventArgs> Requested;
        public event EventHandler<ResponseEventArgs> ReturnedResponse;

        public TypicalHttpClient(HttpClient client)
        {
            baseClient = client;
        }

        public async Task<TResult> GetAsync<TSend, TResult>(string apiPath, TSend sentObject)
        {
            var uri =
                new Uri(
                    QueryHelpers.AddQueryString(
                        apiPath,
                        sentObject.GetType().GetProperties().ToDictionary(
                            x => x.Name,
                            x => x.GetValue(sentObject).ToString())),
                    UriKind.Relative);
            
            Requested?.Invoke(this, new RequestEventArgs { Uri = uri });
            var response = await baseClient.GetAsync(uri);
            ReturnedResponse?.Invoke(this, new ResponseEventArgs { Uri = uri, Response = await response.Content.ReadAsStringAsync() });
            var stream = await response.Content.ReadAsStreamAsync();

            var serializer = new DataContractSerializer(typeof(TResult));

            return (TResult)serializer.ReadObject(stream);
        }
    }


    public static class HttpClientExtensions
    {
        public static async Task<TResult> GetAsync<TSend, TResult>(this HttpClient self, string apiPath, TSend data)
        {
            var client = new TypicalHttpClient(self);

            return await client.GetAsync<TSend, TResult>(apiPath, data);
        }
            
    }



    public class DejizoClient
    {
        public static int PageSize { get; set; } = 512;

        public event EventHandler<RequestEventArgs> Requested;
        public event EventHandler<ResponseEventArgs> ReturnedResponse;
        public TimeSpan Interval { get; set; } = new TimeSpan(0, 0, 0, 60);

        static DateTime lastGettingTime = DateTime.MinValue;

        static HttpClient client = new HttpClient { BaseAddress = new Uri("http://public.dejizo.jp/NetDicV09.asmx/") };

        public async Task<SearchDicItemResult> SearchDicItemLite(SearchDicItemCondition data)
        {
            await Wait();

            var typicalClient = new TypicalHttpClient(client);

            typicalClient.Requested += (sender, e) =>
            {
                Requested?.Invoke(this, e);
            };
            typicalClient.ReturnedResponse += (sender, e) =>
            {
                ReturnedResponse?.Invoke(this, e);
            };

            return await typicalClient.GetAsync<SearchDicItemCondition, SearchDicItemResult>(
                "SearchDicItemLite",
                data);
            
        }

        public async Task<GetDicItemResult> GetDicItemLite(string itemId)
        {
            await Wait();

            var typicalClient = new TypicalHttpClient(client);

            typicalClient.Requested += (sender, e) =>
            {
                Requested?.Invoke(this, e);
            };
            typicalClient.ReturnedResponse += (sender, e) =>
            {
                ReturnedResponse?.Invoke(this, e);
            };

            return await typicalClient.GetAsync<GetDicItemCondition, GetDicItemResult>(
                "GetDicItemLite",
                new GetDicItemCondition { Item = itemId });
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
