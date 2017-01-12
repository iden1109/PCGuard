using com.gseo.browser.uri;
using com.gseo.browser.urlHistory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.gseo.browser
{
    /// <summary>
    /// Chrome的History cache抓取器
    /// </summary>
    public class Chrome : IBrowser
    {
        private string _chromePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default"; //Chrome工作路徑
        private string _chromeHistoryFile = @"History"; //Chrome歷史檔 Sqlite格式
        
        public List<URL> URLs { get; set; }
        
        public Chrome()
        {
            URLs = new List<URL>();
        }

        /// <summary>
        /// 抓歷史記錄
        /// </summary>
        /// <returns>URL集合</returns>
        public IEnumerable<URL> GetHistory()
        {
            string hisFile = _chromePath + "\\" + _chromeHistoryFile;
            string tmpHisFile = hisFile + ".tmp";
            if (File.Exists(hisFile))
            {
                File.Copy(hisFile, tmpHisFile, true);

                SQLiteConnection connection = new SQLiteConnection("Data Source=" + tmpHisFile + ";Version=3;New=False;Compress=True;");
                if(connection.State != ConnectionState.Open)
                    connection.Open();

                long lastMonth = DateTime.Now.AddMonths(-1).ToFileTimeUtc()/10;
                
                DataSet dataset = new DataSet();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter("select * from urls where last_visit_time > "+ lastMonth +" order by last_visit_time desc", connection);
                adapter.Fill(dataset);
                if (dataset != null && dataset.Tables[0] != null && dataset.Tables.Count > 0)
                {
                    DataTable dt = dataset.Tables[0];
                    foreach (DataRow r in dt.Rows)
                    {
                        long utcMicroSeconds = Convert.ToInt64(r["last_visit_time"]);
                        DateTime gmtLastVisitTime = DateTime.FromFileTimeUtc(10 * utcMicroSeconds);
                        DateTime lastVisitTime = TimeZoneInfo.ConvertTimeFromUtc(gmtLastVisitTime, TimeZoneInfo.Local);

                        URL historyItem = new URL(Convert.ToString(r["url"]),
                                                    Convert.ToString(r["title"]),
                                                    BROWSER.CHROME,
                                                    lastVisitTime);
                        URLs.Add(historyItem);
                    }
                }
                adapter.Dispose();
                adapter = null;
                connection.Close();
                connection.Dispose();
                connection = null;

                File.Delete(tmpHisFile);

                return URLs;
            }
            return null;
        }


    }
}
