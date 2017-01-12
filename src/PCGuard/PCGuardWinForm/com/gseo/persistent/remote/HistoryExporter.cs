using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.gseo.browser.uri;
using System.Configuration;
using System.Diagnostics;
using System.Net;

namespace com.gseo.persistent.remote
{
    /// <summary>
    /// 調用遠端HistoryService，達成輸出歷史記綠
    /// </summary>
    public class HistoryExporter
    {
        private Uri _BASE_ADDRESS;
        private string _JSON_STRING = "application/json";
        private HttpClient _client;

        public HistoryExporter()
        {
            string webApiServer = "http://192.168.100.85:8090/";
            try
            {
                webApiServer = ConfigurationManager.AppSettings["PCGuardWebAPIServer"];
            }
            catch (Exception)
            {
            }
            _BASE_ADDRESS = new Uri(webApiServer);

            if (_client == null)
            {
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_JSON_STRING));
                _client.BaseAddress = _BASE_ADDRESS;
            }  
        }
        
        /// <summary>
        /// 新增一筆瀏覽歷史記綠
        /// </summary>
        /// <param name="his"></param>
        /// <param name="empNo"></param>
        /// <param name="deptNo"></param>
        /// <returns>success表示成功，否則是[失敗訊息]</returns>
        public string Export(URL his, string empNo, string deptNo)
        {
            if (his == null)
                return "";

            //準備json
            string json = JsonConvert.SerializeObject(new
            {
                url = his.URI,
                title = his.Title,
                browser_type = his.BrowserType.ToString(),
                last_visit_time = his.VisitedTime.ToFileTimeUtc(),
                emp_no = empNo,
                dept_no = deptNo
            });

            //呼叫WebAPI 
            return Post("api/PCGuardWebLib/PCGuardWebLib/HistoryService/Add", json);
        }

        /// <summary>
        /// 遠端服務是否存活著
        /// </summary>
        /// <returns>true:是 / false:否</returns>
        public bool IsAvailable()
        {
            string result = Post("api/PCGuardWebLib/PCGuardWebLib/HistoryService/Test", null);
            if (result != null && result.Equals("true"))
                return true;

            return false;
        }

        /// <summary>
        /// 送出Post請求
        /// </summary>
        /// <param name="uri">URI資源</param>
        /// <param name="json">送出參數</param>
        /// <returns>Post請求的回應, return null表示發生錯誤</returns>
        private string Post(string uri, string json)
        {
            StringContent sc = null;
            if(json != null && !json.Equals(""))
                sc = new StringContent(json, Encoding.UTF8, _JSON_STRING);

            try
            {
                HttpResponseMessage response = _client.PostAsync(uri, sc).Result;
                if (response == null || response.StatusCode != HttpStatusCode.OK)
                    return null;

                string responseBody = response.Content.ReadAsStringAsync().Result;
                if (responseBody == null)
                    return null;

                string result = JsonConvert.DeserializeObject<string>(responseBody);
                return result;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            catch (AggregateException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }
    }
}
