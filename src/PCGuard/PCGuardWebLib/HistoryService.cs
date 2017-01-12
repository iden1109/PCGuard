using GSEO.Dummies.ServerComponent;
using PCGuardWebLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.gseo.pcGuard.dal;
using com.gseo.pcGuard;

namespace PCGuardWebLib
{
    [AuthorizedSystemGroups("", "")]
    public class HistoryService : GSEOServerComponent
    {

        /// <summary>
        /// 建立瀏覽歷史記錄
        /// </summary>
        /// <param name="his">瀏覽記錄</param>
        /// <returns>錯誤訊息, success表示成功</returns>
        [WriteLog(UseLogType.ToFileSystem)]
        [WriteExceptionLog(UseLogType.ToEventLog)]
        [ErrorLevel(LevelType.Fatal)]
        [EnabledAnonymous(true), ExposeWebAPI(true)]
        public string Add(HistoryDTO his)
        {
            DateTime? lastVisitTime = null;
            if (his.last_visit_time > 0)
            {
                DateTime gmtLastVisitTime = DateTime.FromFileTimeUtc(his.last_visit_time);
                lastVisitTime = TimeZoneInfo.ConvertTimeFromUtc(gmtLastVisitTime, TimeZoneInfo.Local);
            }
            
            History history = new History();
            string msg = history.Add(his.url, his.title, his.browser_type, lastVisitTime, his.dept_no, his.emp_no);
            if (msg != null && !msg.Equals(""))
                return msg;

            return "success";
        }

        /// <summary>
        /// 是否存活
        /// </summary>
        /// <returns></returns>
        [WriteLog(UseLogType.ToFileSystem)]
        [WriteExceptionLog(UseLogType.ToEventLog)]
        [ErrorLevel(LevelType.Fatal)]
        [EnabledAnonymous(true), ExposeWebAPI(true)]
        public string Test()
        {
            return "true";
        }
    }
}
