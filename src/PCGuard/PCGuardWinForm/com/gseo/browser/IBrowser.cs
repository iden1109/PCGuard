using com.gseo.browser.uri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.gseo.browser
{
    /// <summary>
    /// Define Browser Interface
    /// </summary>
    public interface IBrowser
    {
        List<URL> URLs { get; set; } //歷史記錄集合

        IEnumerable<URL> GetHistory(); //抓歷史記錄

    }
}
