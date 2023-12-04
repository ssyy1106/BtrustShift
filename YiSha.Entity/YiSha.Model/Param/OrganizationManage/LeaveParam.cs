using System;
using System.Collections.Generic;
using YiSha.Entity.OrganizationManage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Model.Param.OrganizationManage
{
    public class LeaveListParam
    {
        public long? DepartmentId { get; set; }
        public string FromDay { get; set; }
        public string ToDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string UserName { get; set; }
    }
}
