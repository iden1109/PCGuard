using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCGuardDB.DBModel;
using PCGuardWebLib.Model;
using System.Data.Entity;
using System.Security.Policy;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace com.gseo.pcGuard.dal
{
    public class HistoryDAO : BaseDAO
    {
        private string _ERROR = "Error:";

        /// <summary>
        /// 取資料
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public history Get(string title)
        {
            var query = (from data in _dbContext.history
                where data.title.Contains(title)
                select data);

            history obj = query.FirstOrDefault();
            return obj;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="title">標題</param>
        /// <param name="browserType">瀏覽器型式</param>
        /// <param name="lastVisitDate">最後訪問時間</param>
        /// <param name="deptNo">部門代碼</param>
        /// <param name="empNo">員工編號</param>
        /// <returns>錯誤訊息</returns>
        public string Insert(string url, string title, string browserType, DateTime? lastVisitDate, string deptNo, string empNo)
        {
            using (DbContextTransaction trx = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    history h = new history();
                    h.url = url;
                    h.title = title;
                    h.browser_type = browserType;
                    h.last_visit_time = lastVisitDate;
                    h.dept_no = deptNo;
                    h.emp_no = empNo;

                    _dbContext.history.Add(h);
                    _dbContext.SaveChanges();

                    trx.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    trx.Rollback();

                    var entityError =
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
                    var getFullMessage = string.Join("; ", entityError);
                    var exceptionMessage = string.Concat(ex.Message, "errors are: ", getFullMessage);
                    return _ERROR + exceptionMessage;
                }
                catch (DbUpdateException ex)
                {
                    trx.Rollback();
                    StringBuilder sb = new StringBuilder("A DbUpdateException was caught while saving changes. ");
                    try
                    {
                        foreach (var result in ex.Entries)
                        {
                            sb.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
                        }
                    }
                    catch (Exception e)
                    {
                        sb.Append("Error parsing DbUpdateException: " + e.ToString());
                    }

                    DAOUtility util = new DAOUtility();
                    sb = util.TakeInnerException(ex, sb);

                    return _ERROR + sb.ToString();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    return _ERROR + ex.Message + " : " + ex.InnerException.Message + "   " + ex.ToString();
                }
            }
            return "";
        }
    }
}
