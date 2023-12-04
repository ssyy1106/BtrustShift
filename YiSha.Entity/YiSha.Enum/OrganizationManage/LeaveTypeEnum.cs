using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum.OrganizationManage
{
    public enum LeaveTypeEnum
    {
        [Description("天")]
        Days = 0,

        [Description("时间段")]
        Time = 1
    }

    public enum LeaveKindEnum
    {
        [Description("事假")]
        Persanal = 0,

        [Description("年假")]
        Annual = 1,
        [Description("病假")]
        Sick = 2,
        [Description("其它")]
        Other = 3
    }
}
