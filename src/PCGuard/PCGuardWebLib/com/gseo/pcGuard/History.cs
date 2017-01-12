using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.gseo.pcGuard.dal;

namespace com.gseo.pcGuard
{
    public class History
    {
        private HistoryDAO _dao;

        public History()
        {
            if(_dao == null)
                _dao = new HistoryDAO();
        }

        /// <summary>
        /// 建立瀏覽歷史記錄
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="title">標題</param>
        /// <param name="browserType">瀏覽器型式</param>
        /// <param name="lastVisitDate">最後訪問時間</param>
        /// <param name="deptNo">部門代碼</param>
        /// <param name="empNo">員工編號</param>
        /// <returns>錯誤訊息</returns>
        public string Add(string url, string title, string browserType, DateTime? lastVisitDate, string deptNo, string empNo)
        {
            return _dao.Insert(url, title, browserType, lastVisitDate, deptNo, empNo);
        }
    }
}
