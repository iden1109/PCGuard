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
        private const string _DB_PREFIX = "LocalDB"; //Local資料庫檔前綴字元
        private string _localDBPath; //DB file path
        private SQLiteConnection _connection;
        private ConcurrencyCache<string> _cache;
        
        //插入瀏覽記錄sql
        private string _historyInsertSQL = @"insert into history (url, title, browser_type, last_visit_time) values (?,?,?,?);";
        //查找100筆瀏覽記錄sql
        private string _historySelectSQL = @"select * from history order by num limit 100;";
        //刪除瀏覽記錄sql
        private string _historyDeleteSQL = @"delete from history where num <= ?;";


        public LocalDB()
            : this("")
        {
            if (_cache == null)
                _cache = ConcurrencyCache<string>.GetInstance();
        }

        public LocalDB(string dbFile)
        {
            if (dbFile != null && !dbFile.Equals(""))
            {
                this._localDBPath = _DB_PREFIX + "_" + dbFile + ".db";
            }
            else
            {
                this._localDBPath = _DB_PREFIX + ".db";
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
            long time = visiTime.ToFileTimeUtc() / 10;

            //Cache不存在此url才寫入local db
            //KEY = URL + LastVisitTime
            if (!_cache.ContainKey(url + time.ToString()))
            {
                _cache.Add(url + time.ToString(), DateTime.Now);

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
                long time = u.VisitedTime.ToFileTimeUtc() / 10;

                //Cache不存在此url才寫入local db
                //KEY = URL + LastVisitTime
                if (!_cache.ContainKey(u.URI + time.ToString()))
                {
                    _cache.Add(u.URI + time.ToString(), DateTime.Now);
                    
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
                    browserType = (BROWSER) Enum.Parse(typeof (BROWSER), reader["browser_type"].ToString());
                    lastVisitTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.FromFileTimeUtc(10 * Convert.ToInt64(reader["last_visit_time"])), TimeZoneInfo.Local);

                    u = new URL(reader["url"].ToString(), reader["title"].ToString(), browserType, lastVisitTime);
                    u.Num = Convert.ToInt64(reader["num"]);

                    list.Add(u);
                }
            }
            catch (SQLiteException ex)
            {
                if (!ex.ToString().Contains("SQL logic error or missing database"))
                    Debug.WriteLine(ex.ToString());
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

        /// <summary>
        /// 寫入DB前的字元替換，避開不合適字元
        /// </summary>
        /// <param name="src">原始字串</param>
        /// <returns>已替換字串</returns>
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
