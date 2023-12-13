using System;
using System.Collections.Generic;
using YiSha.Entity.OrganizationManage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Model.Param.OrganizationManage
{
    public class PunchParam
    {
        public long? DepartmentId { get; set; }

        public string PeriodBegin { get; set; }
    }
}
