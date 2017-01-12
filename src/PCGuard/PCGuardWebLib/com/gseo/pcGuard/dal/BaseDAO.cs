using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCGuardDB.DBModel;

namespace com.gseo.pcGuard.dal
{
    public class BaseDAO
    {
        protected PCGuardDBContext _dbContext = null;

        public BaseDAO()
        {
            if (_dbContext == null)
                _dbContext = new PCGuardDBContext();
        }
    }
}
