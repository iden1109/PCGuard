using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using com.gseo.browser.urlHistory;

namespace com.gseo.browser.uri
{
    /// <summary>
    /// 一筆瀏覽過的歷史資源
    /// </summary>
    public class URL
    {
        public string URI { get; set; }
        public string Title { get; set; }
        public BROWSER BrowserType { get; set; }
        public DateTime VisitedTime { get; set; }
        public long Num { get; set; }

        public URL(string url, string title, BROWSER browserType) : this(url, title, browserType, DateTime.Now)
        {
        }

        public URL(string url, string title, BROWSER browserType, DateTime visiTime)
        {
            this.URI = url;
            this.Title = title;
            this.BrowserType = browserType;
            this.VisitedTime = visiTime;
        }
    }
}
