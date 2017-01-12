using com.gseo.browser.uri;
using com.gseo.browser.urlHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.gseo.browser
{
    /// <summary>
    /// IE的History cache抓取器
    /// </summary>
    public class InternetExplorer : IBrowser
    {
        // List of URL objects
        public List<URL> URLs { get; set; }

        public InternetExplorer()
        {
            URLs = new List<URL>();
        }

        /// <summary>
        /// 抓歷史記錄
        /// </summary>
        /// <returns>URL集合</returns>
        public IEnumerable<URL> GetHistory()
        {
            UrlHistoryWrapper urlhistory = new UrlHistoryWrapper();
            // Enumerate URLs in HistoryExporter
            UrlHistoryWrapper.STATURLEnumerator enumerator = urlhistory.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string url = enumerator.Current.URL.Replace('\'', ' ');
                string title = enumerator.Current.Title != null ? enumerator.Current.Title.Replace('\'', ' ') : "";
                if (url.StartsWith("http"))
                {
                    URLs.Add(new URL(url, title, BROWSER.IE));
                }
                
            }
            enumerator.Reset();
            urlhistory.ClearHistory();

            return URLs;
        }

    }
}
