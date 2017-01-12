using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGuardWebLib.Model
{
    /// <summary>
    /// 瀏覽記錄
    /// </summary>
    public class HistoryDTO
    {
        public string url { get; set; }
        public string title { get; set; }
        public string browser_type { get; set; }
        public long last_visit_time { get; set; }
        public string emp_no { get; set; }
        public string dept_no { get; set; }
    }
}
