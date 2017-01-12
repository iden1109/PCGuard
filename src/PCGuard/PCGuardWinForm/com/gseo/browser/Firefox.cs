using com.gseo.browser.uri;
using com.gseo.browser.urlHistory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace com.gseo.browser
{
    /// <summary>
    /// Firefox的History cache抓取器
    /// </summary>
    public class Firefox : IBrowser
    {
        public List<URL> URLs { get; set; }

        public Firefox()
        {
            URLs = new List<URL>();
        }

        /// <summary>
        /// 抓歷史記錄
        /// </summary>
        /// <returns>URL集合</returns>
        public IEnumerable<URL> GetHistory()
        {
            // Get Current Users App Data
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Move to Firefox Data
            documentsFolder += "\\Mozilla\\Firefox\\Profiles\\";

            if (Directory.Exists(documentsFolder))
            {
                foreach (string folder in Directory.GetDirectories(documentsFolder))
                {
                    return ExtractUserHistory(folder);
                }
            }
            return null;
        }

        /// <summary>
        /// 截取歷史記綠
        /// </summary>
        /// <param name="folder">工作路徑</param>
        /// <returns></returns>
        private IEnumerable<URL> ExtractUserHistory(string folder)
        {
            DataTable historyDT = ExtractFromTable("moz_places", folder); //User history
            DataTable visitsDT = ExtractFromTable("moz_historyvisits", folder); //visit Time/Data
            
            foreach (DataRow r in historyDT.Rows)
            {
                var entryDate = (from dates in visitsDT.AsEnumerable()
                                 where dates["place_id"].ToString() == r["id"].ToString()
                                 select dates).LastOrDefault();
                if (entryDate != null)
                {
                    string url = r["Url"].ToString().Replace('\'', ' ');
                    string title = r["title"].ToString().Replace('\'', ' ');
                    URL u = new URL(url, title, BROWSER.FIREFOX);
                    URLs.Add(u);
                }
            }
            DeleteFromTable("moz_places", folder);
            DeleteFromTable("moz_historyvisits", folder);

            return URLs;
        }

        /// <summary>
        /// 清除歷史記錄
        /// </summary>
        /// <param name="table"></param>
        /// <param name="folder"></param>
        private void DeleteFromTable(string table, string folder)
        {
            string dbPath = folder + "\\places.sqlite";

            if (File.Exists(dbPath))
            {
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;New=False;Compress=True;");
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string sql = "delete from " + table;
                SQLiteCommand cmd = new SQLiteCommand(sql, connection);
                cmd.ExecuteNonQuery();

                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        /// <summary>
        /// 從places.sqlite抓資料
        /// </summary>
        /// <param name="table"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private DataTable ExtractFromTable(string table, string folder)
        {
            DataTable datatable = new DataTable();

            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            if (File.Exists(dbPath))
            {
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;New=False;Compress=True;");
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string sql = "select * from " + table;
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, connection);
                adapter.Fill(datatable);

                connection.Close();
                connection.Dispose();
                connection = null;
            }
            return datatable;
        }
    }
}
