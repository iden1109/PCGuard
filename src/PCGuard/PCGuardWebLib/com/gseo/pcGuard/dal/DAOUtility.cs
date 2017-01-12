using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.gseo.pcGuard.dal
{
    public class DAOUtility
    {
        /// <summary>
        /// 獲取InnerException錯誤資訊
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="sb">StringBuilder object</param>
        /// <returns>錯誤資訊</returns>
        public StringBuilder TakeInnerException(Exception ex, StringBuilder sb)
        {
            if (sb == null)
                return sb;

            if (ex.InnerException == null)
            {
                return sb;
            }
            sb.AppendFormat("InnerException: {0}", ex.InnerException.Message);
            return TakeInnerException(ex.InnerException, sb);
        }
    }
}
