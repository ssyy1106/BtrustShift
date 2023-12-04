using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiSha.Enum.OrganizationManage
{
    public enum UserTypeEnum
    {
        [Description("学生")]
        A20 = 0,
        [Description("30小时")]
        A30 = 1,
        [Description("36小时")]
        A36 = 2,
        [Description("40小时")]
        A40 = 3,
        [Description("4小时")]
        A44 = 4,
        [Description("B")]
        B = 5,
        [Description("C")]
        C = 6,
        [Description("D")]
        D = 7
    }
}
