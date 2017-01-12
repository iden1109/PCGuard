using com.gseo.browser.urlHistory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using com.gseo.browser.uri;
using com.gseo.cache;

namespace com.gseo.persistent.local
{
    /// <summary>
    /// 本地暫存用的DB
    /// </summary>
    public class LocalDB
    {
        private string _localDBPath; //DB file path
        private SQLiteConnection _connection;
        private ConcurrencyCache<string> _cache;
        
        //插入瀏覽記錄sql
        private string _historyInsertSQL = @"insert into history (url, title, browser_type, last_visit_time) 
                                                values (?,?,?,?);";
        //查找100筆瀏覽記錄sql
        private string _historySelectSQL = @"select * from history 
                                                order by num
                                                limit 100";
        //刪除瀏覽記錄sql
        private string _historyDeleteSQL = @"delete from history 
                                                where num <= ?";


        public LocalDB() : this("")
        {
            if(_cache == null)
                _cache = ConcurrencyCache<string>.GetInstance();
        }

        public LocalDB(string dbFile)
        {
            if (dbFile != null && !dbFile.Equals(""))
            {
                this._localDBPath = "PCGuardDB_" + dbFile + ".db";
            }
            else
            {
                this._localDBPath = "PCGuardDB.db";
            }

            if (_cache == null)
                _cache = ConcurrencyCache<string>.GetInstance();
        }


        /// <summary>
        /// 建立Table
        /// </summary>
        public void CreateTable()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS history (num INTEGER PRIMARY KEY AUTOINCREMENT, url LONGVARCHAR, title LONGVARCHAR, browser_type VARCHAR(10), last_visit_time INTEGER)";
            DoSQL(sql);
        }

        /// <summary>
        /// 寫入瀏覽記錄
        /// </summary>
        /// <param name="url">網址</param>
        /// <param name="title">標題</param>
        /// <param name="browserType">瀏覽器</param>
        /// <param name="visiTime">最後訪問時間</param>
        public void InsertHistory(string url, string title, BROWSER browserType, DateTime visiTime)
        {
            //Cache不存在此url才寫入local db
            if (!_cache.ContainKey(url))
            {
                _cache.Add(url, DateTime.Now);

                long time = visiTime.ToFileTimeUtc() / 10;

                InitConnection();
                SQLiteCommand command = new SQLiteCommand(_historyInsertSQL, _connection);
                command.Parameters.Add("@url", DbType.String).Value = url;
                command.Parameters.Add("@title", DbType.String).Value = title;
                command.Parameters.Add("@browserType", DbType.String).Value = browserType.ToString();
                command.Parameters.Add("@lastVisitTime", DbType.VarNumeric).Value = time;
                DoSQL(command);
            }
        }

        /// <summary>
        /// 大量寫入瀏覽記錄
        /// </summary>
        /// <param name="list">瀏覽記錄List</param>
        public void InsertBulkHistory(List<URL> list)
        {
            if (list == null)
                return;

            InitConnection();

            SQLiteCommand command;
            foreach (URL u in list)
            {
                //Cache不存在此url才寫入local db
                if (!_cache.ContainKey(u.URI))
                {
                    _cache.Add(u.URI, DateTime.Now);

                    long time = u.VisitedTime.ToFileTimeUtc() / 10;
                    command = new SQLiteCommand(_historyInsertSQL, _connection);
                    command.Parameters.Add("@url", DbType.String).Value = u.URI;
                    command.Parameters.Add("@title", DbType.String).Value = u.Title;
                    command.Parameters.Add("@browserType", DbType.String).Value = u.BrowserType.ToString();
                    command.Parameters.Add("@lastVisitTime", DbType.VarNumeric).Value = time;
                    command.ExecuteNonQuery();
                }
                
            }

            ReleaseConn();
        }

        /// <summary>
        /// 查詢現有瀏覽記錄
        /// </summary>
        /// <returns>瀏覽記錄List</returns>
        public List<URL> GetHistorys()
        {
            List<URL> list  = new List<URL>();

            InitConnection();
            try
            {
                SQLiteCommand command = new SQLiteCommand(_historySelectSQL, _connection);
                SQLiteDataReader reader = command.ExecuteReader();
                BROWSER browserType = BROWSER.IE; // default browser is IE
                DateTime lastVisitTime;
                URL u;
                while (reader.Read())
                {
                    switch (reader["browser_type"].ToString())
                    {
                        case "IE":
                            browserType = BROWSER.IE;
                            break;
                        case "CHROME":
                            browserType = BROWSER.CHROME;
                            break;
                        case "FIREFOX":
                            browserType = BROWSER.FIREFOX;
                            break;
                    }
                    long utcMicroSeconds = Convert.ToInt64(reader["last_visit_time"]);
                    lastVisitTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.FromFileTimeUtc(10 * utcMicroSeconds), TimeZoneInfo.Local);

                    u = new URL(reader["url"].ToString(), reader["title"].ToString(), browserType, lastVisitTime);
                    u.Num = Convert.ToInt64(reader["num"]);
                    //Debug.WriteLine("URL: " + reader["url"].ToString() + "\tTitle: " + reader["title"].ToString() + "\tBrowser: " + browserType);
                    list.Add(u);
                }
            }
            catch (SQLiteException ex)
            { 
            }
            
            ReleaseConn();
            return list;
        }

        /// <summary>
        /// 刪除比maxNum小的現有瀏覽記錄
        /// </summary>
        /// <param name="maxNum">要刪除的最大序號</param>
        public void Delete(long maxNum)
        {
            if (maxNum < 0)
                return;

            InitConnection();
            SQLiteCommand command = new SQLiteCommand(_historyDeleteSQL, _connection);
            command.Parameters.Add("@num", DbType.VarNumeric).Value = maxNum;

            DoSQL(command);
        }

        /// <summary>
        /// 執行SQL statement
        /// </summary>
        /// <param name="sql">SQL statement</param>
        private void DoSQL(string sql)
        {
            InitConnection();
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            DoSQL(command);
        }

        /// <summary>
        /// 執行SQL statement
        /// </summary>
        /// <param name="command">SQLiteCommand</param>
        private void DoSQL(SQLiteCommand command)
        {
            if (command == null)
                return;

            InitConnection();
            command.ExecuteNonQuery();
            ReleaseConn();
        }

        /// <summary>
        /// 連線初始化
        /// </summary>
        private void InitConnection()
        {
            if (_connection == null)
                _connection = new SQLiteConnection("Data Source=" + _localDBPath + ";Version=3;New=False;Compress=True;");
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        /// <summary>
        /// 連線釋放
        /// </summary>
        private void ReleaseConn()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }

        }

        private string Replace(string src)
        {
            if (src != null && !src.Equals(""))
            {
                src = src.Replace("'", "\'");
            }

            return src;
        }
    }
}
